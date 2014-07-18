// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------


namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions.DSC
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Language;

    public static class ConfigurationParsingHelper
    {
        private static readonly ConcurrentDictionary<string, string> _resourceName2ModuleNameCache = 
            new ConcurrentDictionary<string, string>();

        private static bool IsParameterName(CommandElementAst ast, string name)
        {
            CommandParameterAst constantAst = ast as CommandParameterAst;
            if (constantAst == null)
            {
                return false;
            }
            return String.Equals(constantAst.ParameterName, name, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCommandImportDscResource(Ast ast)
        {
            CommandAst commandAst = ast as CommandAst;
            if (commandAst == null)
            {
                return false;
            }
            if (commandAst.CommandElements.Count == 0)
            {
                return false;
            }
            StringConstantExpressionAst constantExpressionAst = commandAst.CommandElements[0] as StringConstantExpressionAst;
            if (constantExpressionAst != null && 
                String.Compare(constantExpressionAst.Value, "Import-DscResource", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        private static List<string> GetTopLevelParametersFromAst(CommandAst ast, string parameterName)
        {
            List<string> parameters = new List<string>();
            IEnumerable<CommandParameterAst> commandElement =
                ast.CommandElements.Where(x => IsParameterName(x, parameterName)).OfType<CommandParameterAst>();
            foreach (var commandElementAst in commandElement)
            {
                ArrayLiteralAst arrayLiteralAst = commandElementAst.Argument as ArrayLiteralAst;
                if (arrayLiteralAst != null)
                {
                    parameters.AddRange(arrayLiteralAst.Elements.OfType<StringConstantExpressionAst>().Select(x => x.Value));
                }
            }
            return parameters;
        }

        private static List<string> GetNodeLevelRequiredModules(Ast ast)
        {
            IEnumerable<CommandAst> importAsts = ast.FindAll(IsCommandImportDscResource, true).OfType<CommandAst>();
            List<string> modules = new List<string>();
            foreach (CommandAst importAst in importAsts)
            {
                StaticBindingResult bindingResult = StaticParameterBinder.BindCommand(importAst);
                foreach (KeyValuePair<string, ParameterBindingResult> parameter in bindingResult.BoundParameters)
                {
                    if (String.Equals(parameter.Key, "Name", StringComparison.OrdinalIgnoreCase))
                    {
                        StringConstantExpressionAst resourceName = parameter.Value.Value as StringConstantExpressionAst;
                        if (resourceName != null)
                        {
                            modules.Add(GetModuleNameForDscResource(resourceName.Value));
                        }
                    }
                    else if (String.Equals(parameter.Key, "ModuleName", StringComparison.OrdinalIgnoreCase))
                    {
                        StringConstantExpressionAst moduleName = parameter.Value.Value as StringConstantExpressionAst;
                        if (moduleName != null)
                        {
                            modules.Add(GetModuleNameForDscResource(moduleName.Value));
                        }
                    }
                }
            }
            return modules;
        }

        public static string GetModuleNameForDscResource(string resourceName)
        {
            string moduleName;
            if (!_resourceName2ModuleNameCache.TryGetValue(resourceName, out moduleName))
            {
                using (PowerShell powershell = PowerShell.Create())
                {
                    powershell.AddCommand("Get-DscResource").AddParameter("Name", resourceName).
                        AddCommand("Foreach-Object").AddParameter("MemberName", "Module").
                        AddCommand("Foreach-Object").AddParameter("MemberName", "Name");
                    moduleName = powershell.Invoke<string>().First();
                }
                _resourceName2ModuleNameCache.TryAdd(resourceName, moduleName);
            }
            return moduleName;
        }

        private static List<string> GetRequiredModulesFromAst(CommandAst ast)
        {
            List<string> modules = new List<string>();

            // There are two place where 'Import-DscResource' keyword can appear:
            // 1) 
            // Configuration Foo {
            //   Import-DscResource ....
            //   Node Bar {...}
            // }
            // 2)
            // Configuration Foo {
            //   Node Bar {
            //     Import-DscResource ....
            //     ...
            //   }
            // }
            // Parser produce different ASTs for these two cases, here we handle first one.
            
            // Example: Import-DscResource -Module xPSDesiredStateConfiguration
            modules.AddRange(GetTopLevelParametersFromAst(ast, "ModuleDefinition"));
            // Example: Import-DscResource -Name MSFT_xComputer
            modules.AddRange(GetTopLevelParametersFromAst(ast, "ResourceDefinition").Select(GetModuleNameForDscResource));
            
            // And here second one.
            modules.AddRange(GetNodeLevelRequiredModules(ast));

            return modules.Distinct().ToList();
        }

        private static List<string> Visit(Ast ast)
        {
            RequiredModulesAstVisitor visitor = new RequiredModulesAstVisitor();
            ast.Visit(visitor);
            return visitor.Modules.Distinct().ToList();
        }

        private class RequiredModulesAstVisitor : AstVisitor
        {
            public List<string> Modules { get; private set; }

            public RequiredModulesAstVisitor()
            {
                Modules = new List<string>();
            }

            public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
            {
                CommandAst commandParentAst = commandParameterAst.Parent as CommandAst;
                if (commandParentAst != null && 
                    String.Equals(commandParentAst.GetCommandName(), "Import-DscResource", StringComparison.OrdinalIgnoreCase))
                {
                    // Resource can be specified by name, without a module.
                    if (String.Equals(commandParameterAst.ParameterName, "Name", StringComparison.OrdinalIgnoreCase))
                    {
                        ArrayLiteralAst arrayLiteralAst = commandParameterAst.Argument as ArrayLiteralAst;
                        if (arrayLiteralAst != null)
                        {
                            IEnumerable<string> resourceNames = arrayLiteralAst.Elements.OfType<StringConstantExpressionAst>().Select(x => x.Value);
                            foreach (string resourceName in resourceNames)
                            {
                                
                            }
                        }
                    }
                    // Or with ModuleDefinition parameter
                    else if (String.Equals(commandParameterAst.ParameterName, "ModuleDefinition", StringComparison.OrdinalIgnoreCase))
                    {
                        ArrayLiteralAst arrayLiteralAst = commandParameterAst.Argument as ArrayLiteralAst;
                        if (arrayLiteralAst != null)
                        {
                            Modules.AddRange(arrayLiteralAst.Elements.OfType<StringConstantExpressionAst>().Select(x => x.Value));
                        }
                    }
                }
                return base.VisitCommandParameter(commandParameterAst);
            }
        }

        private static bool IsAstConfiguration(Ast node)
        {
            CommandAst commandNode = node as CommandAst;
            if (commandNode == null)
            {
                return false;
            }
            // TODO: Add case when configuration name is not a StringConstant, but a variable.
            StringConstantExpressionAst commandParameter = (commandNode.CommandElements[0] as StringConstantExpressionAst);
            if (commandParameter == null)
            {
                return false;
            }
            // Find the AST nodes defining configurations. These nodes will be CommandAst nodes
            // with 7 or 8 command elements (8 if the configuration requires any custom modules.)
            return
                commandNode.CommandElements.Count >= 7 &&
                String.Equals(commandParameter.Extent.Text, "configuration", StringComparison.OrdinalIgnoreCase) &&
                String.Equals(commandParameter.Value, @"PSDesiredStateConfiguration\Configuration",
                    StringComparison.OrdinalIgnoreCase);
        }

        public static ConfigurationParseResult ExtractConfigurationNames(string path)
        {
            // Get the resolved script path. This will throw an exception if the file is not found.
            string fullPath = Path.GetFullPath(path);
            Token[] tokens;
            ParseError[] errors;
            // Parse the script into an AST, capturing parse errors. Note - even with errors, the
            // file may still successfully define one or more configurations.
            ScriptBlockAst ast = Parser.ParseFile(fullPath, out tokens, out errors);
            IEnumerable<CommandAst> configs = ast.FindAll(IsAstConfiguration, true).Select(x => (CommandAst)x);

            List<string> requiredModules = configs.Select(GetRequiredModulesFromAst).SelectMany(x => x).Distinct().ToList();

            return new ConfigurationParseResult()
            {
                Path = fullPath,
                Errors = errors,
                RequiredModules = requiredModules,
            };
        }
    }
}

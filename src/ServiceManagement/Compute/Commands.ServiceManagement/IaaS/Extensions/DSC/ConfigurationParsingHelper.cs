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

        private static List<string> GetLegacyTopLevelParametersFromAst(CommandAst ast, string parameterName)
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
            List<string> resources = new List<string>();
            foreach (CommandAst importAst in importAsts)
            {
                // TODO: refactor code to avoid calling a script, just use StaticBindingResult directly,
                // once System.Management.Automation.dll version will be updated from 3.0.0.0.

                using (PowerShell powerShell = PowerShell.Create()) 
                {
                    powerShell.AddScript(
                     @"function BindArguments($ast, $outModules, $outResources) 
                        {
                            function GetListFromBinding($binding) 
                            {
                                if ($binding.Value.Value.Elements)
                                # ArrayAst case
                                { 
                                    foreach ($x in $binding.Value.Value.Elements) { Write-Output $x } 
                                }
                                else 
                                { 
                                    Write-Output $binding.Value.Value.Value
                                }
                            }

                            $dic = ([System.Management.Automation.Language.StaticParameterBinder]::BindCommand($ast)).BoundParameters 
                            $modulePresent = $false
                            foreach ($binding in $dic.GetEnumerator()) 
                            {
                                if ($binding.Key -like ""[M]*"") { $modulePresent = $true; break; } 
                            }
                            foreach ($binding in $dic.GetEnumerator()) 
                            {
                                # ModuleName case
                                if ($binding.Key -like ""[M]*"") 
                                { 
                                    GetListFromBinding($binding) | %{$outModules.Add( $_ )}
                                }
                                else 
                                { 
                                    # Name case, ignore if module specified
                                    if ( (-not $modulePresent) -and ($binding.Key -like ""[N]*"") ) 
                                    { 
                                        GetListFromBinding($binding) | %{$outResources.Add( $_ )}
                                    } 
                                }
                            }
                        }");
                    powerShell.Invoke();
                    powerShell.Commands.Clear();
                    powerShell.AddCommand("BindArguments")
                        .AddParameter("ast", importAst)
                        .AddParameter("outModules", modules)
                        .AddParameter("outResources", resources);
                    powerShell.Invoke();
                }
            }
            modules.AddRange(resources.Select(GetModuleNameForDscResource));
            return modules;
        }

        public static string GetModuleNameForDscResource(string resourceName)
        {
            string moduleName;
            if (!_resourceName2ModuleNameCache.TryGetValue(resourceName, out moduleName))
            {
                try
                {
                    using (PowerShell powershell = PowerShell.Create())
                    {
                        powershell.AddCommand("Get-DscResource").AddParameter("Name", resourceName).
                            AddCommand("Foreach-Object").AddParameter("MemberName", "Module").
                            AddCommand("Foreach-Object").AddParameter("MemberName", "Name");
                        moduleName = powershell.Invoke<string>().First();
                    }
                } 
                catch (InvalidOperationException e) 
                {
                    throw new GetDscResourceException(resourceName, e);
                }
                _resourceName2ModuleNameCache.TryAdd(resourceName, moduleName);
            }
            return moduleName;
        }

        private static List<string> GetRequiredModulesFromAst(Ast ast)
        {
            List<string> modules = new List<string>();

            // We use System.Management.Automation.Language.Parser to extract required modules from ast, 
            // but format of ast is a bit tricky and have changed in time.
            //
            // There are two place where 'Import-DscResource' keyword can appear:
            // 1) 
            // Configuration Foo {
            //   Import-DscResource ....  # outside node
            //   Node Bar {...}
            // }
            // 2)
            // Configuration Foo {
            //   Node Bar {
            //     Import-DscResource .... # inside node
            //     ...
            //   }
            // }
            // 
            // The old version of System.Management.Automation.Language.Parser produces slightly different AST for the first case.
            // In new version, Configuration corresponds to ConfigurationDefinitionAst.
            // In old version is's a generic CommandAst with specific commandElements which capture top-level Imports (case 1).
            // In new version all imports correspond to their own CommandAsts, same for case 2 in old version. 

            // Old version, case 1:
            IEnumerable<CommandAst> legacyConfigurationAsts = ast.FindAll(IsLegacyAstConfiguration, true).Select(x => (CommandAst)x);
            foreach (var legacyConfigurationAst in legacyConfigurationAsts)
            {
                // Note: these two sequences are translated to same AST:
                //
                // Import-DscResource -Module xComputerManagement; Import-DscResource -Name xComputer
                // Import-DscResource -Module xComputerManagement -Name xComputer
                //
                // We cannot distinguish different imports => cannot ignore resource names for imports with specified modules.
                // So we process everything: ModuleDefinition and ResourceDefinition.

                // Example: Import-DscResource -Module xPSDesiredStateConfiguration
                modules.AddRange(GetLegacyTopLevelParametersFromAst(legacyConfigurationAst, "ModuleDefinition"));
                // Example: Import-DscResource -Name MSFT_xComputer
                modules.AddRange(GetLegacyTopLevelParametersFromAst(legacyConfigurationAst, "ResourceDefinition").Select(GetModuleNameForDscResource));    
            }
            
            // Both cases in new version and 2 case in old version:
            modules.AddRange(GetNodeLevelRequiredModules(ast));

            return modules.Distinct().ToList();
        }

        private static bool IsLegacyAstConfiguration(Ast node)
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

        public static ConfigurationParseResult ParseConfiguration(string path)
        {
            // Get the resolved script path. This will throw an exception if the file is not found.
            string fullPath = Path.GetFullPath(path);
            Token[] tokens;
            ParseError[] errors;
            // Parse the script into an AST, capturing parse errors. Note - even with errors, the
            // file may still successfully define one or more configurations.
            ScriptBlockAst ast = Parser.ParseFile(fullPath, out tokens, out errors);
            List<string> requiredModules = GetRequiredModulesFromAst(ast).Distinct().ToList();

            return new ConfigurationParseResult()
            {
                Path = fullPath,
                Errors = errors,
                RequiredModules = requiredModules,
            };
        }
    }
}

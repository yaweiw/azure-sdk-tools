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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Language;

    public static class ConfigurationNameHelper
    {
        private static bool IsParameterName(CommandElementAst ast, string name)
        {
            CommandParameterAst constantAst = ast as CommandParameterAst;
            if (constantAst == null)
            {
                return false;
            }
            return String.Equals(constantAst.ParameterName, name, StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> GetRequiredModulesFromAst(CommandAst ast)
        {
            List<string> modules = new List<string>();
            IEnumerable<CommandParameterAst> commandElement =
                ast.CommandElements.Where(x => IsParameterName(x, "ModuleDefinition")).OfType<CommandParameterAst>();
            foreach (var commandElementAst in commandElement)
            {
                ArrayLiteralAst arrayLiteralAst = commandElementAst.Argument as ArrayLiteralAst;
                if (arrayLiteralAst != null)
                {
                    modules.AddRange(arrayLiteralAst.Elements.OfType<StringConstantExpressionAst>().Select(x => x.Value));
                }
            }
            return modules;
        }

        private static string GetConfigurationNameFromAst(CommandAst ast)
        {
            CommandElementAst commandElement = ast.CommandElements.FirstOrDefault(x => IsParameterName(x, "Name"));
            CommandParameterAst commandParameter = commandElement as CommandParameterAst;
            if (commandParameter != null)
            {
                // TODO: Add case when configuration name is not a StringConstant, but a variable.
                StringConstantExpressionAst stringConstantExpression =
                    commandParameter.Argument as StringConstantExpressionAst;
                if (stringConstantExpression != null)
                {
                    return stringConstantExpression.Value;
                }
            }
            return null;
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

            List<string> configurations = configs.Select(GetConfigurationNameFromAst).Where(x => x != null).ToList();
            List<string> requiredModules = configs.Select(GetRequiredModulesFromAst).SelectMany(x => x).Distinct().ToList();

            return new ConfigurationParseResult()
            {
                Path = fullPath,
                Configurations = configurations,
                Errors = errors,
                RequiredModules = requiredModules,
            };
        }
    }
}

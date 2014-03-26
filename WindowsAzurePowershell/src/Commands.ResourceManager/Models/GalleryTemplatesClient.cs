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

using Microsoft.Azure.Commands.ResourceManager.Properties;
using Microsoft.Azure.Gallery;
using Microsoft.Azure.Gallery.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Common.OData;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Text;

namespace Microsoft.Azure.Commands.ResourceManager.Models
{
    public class GalleryTemplatesClient
    {
        public IGalleryClient GalleryClient { get; set; }

        public GalleryTemplatesClient(WindowsAzureSubscription subscription)
            : this(subscription.CreateGalleryClientFromGalleryEndpoint<GalleryClient>())
        {

        }

        public GalleryTemplatesClient(IGalleryClient galleryClient)
        {
            GalleryClient = galleryClient;
        }

        /// <summary>
        /// Parameterless constructor for mocking
        /// </summary>
        public GalleryTemplatesClient()
        {

        }

        /// <summary>
        /// Gets the uri of the specified template name.
        /// </summary>
        /// <param name="templateIdentity">The fully qualified template name</param>
        /// <returns>The template uri</returns>
        public virtual string GetGalleryTemplateFile(string templateIdentity)
        {
            try
            {
                DefinitionTemplates definitionTemplates = GalleryClient.Items.Get(templateIdentity).Item.DefinitionTemplates;
                return definitionTemplates.DeploymentTemplateFileUrls[definitionTemplates.DefaultDeploymentTemplateId];
            }
            catch (CloudException)
            {
                throw new ArgumentException(string.Format(Resources.InvalidTemplateIdentity, templateIdentity));
            }
        }

        /// <summary>
        /// Filters gallery templates based on the passed options.
        /// </summary>
        /// <param name="options">The filter options</param>
        /// <returns>The filtered list</returns>
        public virtual List<GalleryItem> FilterGalleryTemplates(FilterGalleryTemplatesOptions options)
        {
            List<string> filterStrings = new List<string>();
            ItemListParameters parameters = null;
            List<GalleryItem> result = new List<GalleryItem>();

            if (!string.IsNullOrEmpty(options.Identity))
            {
                result.Add(GalleryClient.Items.Get(options.Identity).Item);
            }
            else
            {
                result.AddRange(QueryGalleryTemplates(options, filterStrings, parameters));
            }

            return result;
        }

        /// <summary>
        /// Downloads a gallery template file into specific directory.
        /// </summary>
        /// <param name="identity">The gallery template file identity</param>
        /// <param name="outputPath">The file output path</param>
        /// <param name="overwrite">Overrides existing file</param>
        /// <param name="confirmAction">The confirmation action</param>
        /// <returns>The file path</returns>
        public virtual string DownloadGalleryTemplateFile(string identity, string outputPath, bool overwrite, Action<bool, string, string, string, Action> confirmAction)
        {
            string fileUri = GetGalleryTemplateFile(identity);
            StringBuilder finalOutputPath = new StringBuilder();
            string contents = GeneralUtilities.DownloadFile(fileUri);

            if (!FileUtilities.IsValidDirectoryPath(outputPath))
            {
                // Try create the directory if it does not exist.
                new FileInfo(outputPath).Directory.Create();
            }

            if (FileUtilities.IsValidDirectoryPath(outputPath))
            {
                finalOutputPath.Append(Path.Combine(outputPath, identity + ".json"));
            }
            else
            {
                finalOutputPath.Append(outputPath);
                if (!outputPath.EndsWith(".json"))
                {
                    finalOutputPath.Append(".json");
                }
            }

            Action saveFile = () => File.WriteAllText(finalOutputPath.ToString(), contents);

            if (File.Exists(finalOutputPath.ToString()) && confirmAction != null)
            {
                confirmAction(
                    overwrite,
                    string.Format(Resources.FileAlreadyExists, finalOutputPath.ToString()),
                    Resources.OverrdingFile,
                    finalOutputPath.ToString(),
                    saveFile);
            }
            else
            {
                saveFile();
            }

            return finalOutputPath.ToString();
        }

        /// <summary>
        /// Gets the parameters for a given gallery template.
        /// </summary>
        /// <param name="templateIdentity">The gallery template name</param>
        /// <param name="templateParameterObject">Existing template parameter object</param>
        /// <param name="templateParameterFilePath">Path to the template parameter file if present</param>
        /// <param name="staticParameters">The existing PowerShell cmdlet parameters</param>
        /// <returns>The template parameters</returns>
        public virtual RuntimeDefinedParameterDictionary GetTemplateParametersFromGallery(string templateIdentity, Hashtable templateParameterObject, string templateParameterFilePath, string[] staticParameters)
        {
            RuntimeDefinedParameterDictionary dynamicParameters = new RuntimeDefinedParameterDictionary();
            string templateContent = null;

            templateContent = GeneralUtilities.DownloadFile(GetGalleryTemplateFile(templateIdentity));

            dynamicParameters = ParseTemplateAndExtractParameters(templateContent, templateParameterObject, templateParameterFilePath, staticParameters);
            return dynamicParameters;
        }

        /// <summary>
        /// Gets the parameters for a given template file.
        /// </summary>
        /// <param name="templateFilePath">The gallery template path (local or remote)</param>
        /// <param name="templateParameterObject">Existing template parameter object</param>
        /// <param name="templateParameterFilePath">Path to the template parameter file if present</param>
        /// <param name="staticParameters">The existing PowerShell cmdlet parameters</param>
        /// <returns>The template parameters</returns>
        public virtual RuntimeDefinedParameterDictionary GetTemplateParametersFromFile(string templateFilePath, Hashtable templateParameterObject, string templateParameterFilePath, string[] staticParameters)
        {
            RuntimeDefinedParameterDictionary dynamicParameters = new RuntimeDefinedParameterDictionary();
            string templateContent = null;

            if (templateFilePath != null)
            {
                if (Uri.IsWellFormedUriString(templateFilePath, UriKind.Absolute))
                {
                    templateContent = GeneralUtilities.DownloadFile(templateFilePath);
                }
                else if (File.Exists(templateFilePath))
                {
                    templateContent = File.ReadAllText(templateFilePath);
                }
            }

            dynamicParameters = ParseTemplateAndExtractParameters(templateContent, templateParameterObject, templateParameterFilePath, staticParameters);

            return dynamicParameters;
        }

        private RuntimeDefinedParameterDictionary ParseTemplateAndExtractParameters(string templateContent, Hashtable templateParameterObject, string templateParameterFilePath, string[] staticParameters)
        {
            RuntimeDefinedParameterDictionary dynamicParameters = new RuntimeDefinedParameterDictionary();

            if (!string.IsNullOrEmpty(templateContent))
            {
                TemplateFile templateFile = null;

                try
                {
                    templateFile = JsonConvert.DeserializeObject<TemplateFile>(templateContent);
                }
                catch
                {
                    // Can't parse the template file, do not generate dynamic parameters
                    return dynamicParameters;
                }

                foreach (KeyValuePair<string, TemplateFileParameter> parameter in templateFile.Parameters)
                {
                    RuntimeDefinedParameter dynamicParameter = ConstructDynamicParameter(staticParameters, parameter);
                    dynamicParameters.Add(dynamicParameter.Name, dynamicParameter);
                }
            }
            if (templateParameterObject != null)
            {
                UpdateParametersWithObject(dynamicParameters, templateParameterObject);
            }
            if (templateParameterFilePath != null && File.Exists(templateParameterFilePath))
            {
                var parametersFromFile = JsonConvert.DeserializeObject<Dictionary<string, TemplateFileParameter>>(File.ReadAllText(templateParameterFilePath));
                UpdateParametersWithObject(dynamicParameters, new Hashtable(parametersFromFile));
            }
            return dynamicParameters;
        }

        private void UpdateParametersWithObject(RuntimeDefinedParameterDictionary dynamicParameters, Hashtable templateParameterObject)
        {
            if (templateParameterObject != null)
            {
                foreach (KeyValuePair<string, RuntimeDefinedParameter> dynamicParameter in dynamicParameters)
                {
                    try
                    {
                        foreach (string key in templateParameterObject.Keys)
                        {
                            if (key.Equals(dynamicParameter.Key, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (templateParameterObject[key] is TemplateFileParameter)
                                {
                                    dynamicParameter.Value.Value = (templateParameterObject[key] as TemplateFileParameter).Value;
                                }
                                else
                                {
                                    dynamicParameter.Value.Value = templateParameterObject[key];
                                }
                                dynamicParameter.Value.IsSet = true;
                                ((ParameterAttribute)dynamicParameter.Value.Attributes[0]).Mandatory = false;
                            }
                        }
                    }
                    catch
                    {
                        throw new ArgumentException(string.Format(Resources.FailureParsingTemplateParameterObject,
                                                                  dynamicParameter.Key,
                                                                  templateParameterObject[dynamicParameter.Key]));
                    }
                }
            }
        }

        private Type GetParameterType(string resourceParameterType)
        {
            Debug.Assert(!string.IsNullOrEmpty(resourceParameterType));
            const string stringType = "string";
            const string intType = "int";
            const string boolType = "bool";
            const string secureStringType = "SecureString";
            Type typeObject = typeof(object);

            if (resourceParameterType.Equals(stringType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(string);
            }
            else if (resourceParameterType.Equals(intType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(int);
            }
            else if (resourceParameterType.Equals(secureStringType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(SecureString);
            }
            else if (resourceParameterType.Equals(boolType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(bool);
            }

            return typeObject;
        }

        internal RuntimeDefinedParameter ConstructDynamicParameter(string[] staticParameters, KeyValuePair<string, TemplateFileParameter> parameter)
        {
            const string duplicatedParameterSuffix = "FromTemplate";
            string name = parameter.Key;
            object defaultValue = parameter.Value.DefaultValue;

            RuntimeDefinedParameter runtimeParameter = new RuntimeDefinedParameter()
            {
                // For duplicated template parameter names, add a sufix FromTemplate to distingush them from the cmdlet parameter.
                Name = staticParameters.Any(n => n.StartsWith(name, StringComparison.OrdinalIgnoreCase)) 
                    ? name + duplicatedParameterSuffix : name,
                ParameterType = GetParameterType(parameter.Value.Type),
                Value = defaultValue
            };
            runtimeParameter.Attributes.Add(new ParameterAttribute()
            {
                Mandatory = defaultValue == null ? true : false,
                ValueFromPipelineByPropertyName = true,
                // Rely on the HelpMessage property to detect the original name for the dynamic parameter.
                HelpMessage = name
            });

            if (parameter.Value.AllowedValues != null && parameter.Value.AllowedValues.Count > 0)
            {
                runtimeParameter.Attributes.Add(new ValidateSetAttribute(parameter.Value.AllowedValues.ToArray())
                {
                    IgnoreCase = true,
                });
            }

            if (!string.IsNullOrEmpty(parameter.Value.MinLength) &&
                !string.IsNullOrEmpty(parameter.Value.MaxLength))
            {
                runtimeParameter.Attributes.Add(new ValidateLengthAttribute(int.Parse(parameter.Value.MinLength), int.Parse(parameter.Value.MaxLength)));
            }

            return runtimeParameter;
        }

        private List<GalleryItem> QueryGalleryTemplates(FilterGalleryTemplatesOptions options, List<string> filterStrings, ItemListParameters parameters)
        {
            if (!string.IsNullOrEmpty(options.Publisher))
            {
                filterStrings.Add(FilterString.Generate<ItemListFilter>(f => f.Publisher == options.Publisher));
            }

            if (!string.IsNullOrEmpty(options.Category))
            {
                filterStrings.Add(FilterString.Generate<ItemListFilter>(f => f.CategoryIds.Contains(options.Category)));
            }

            if (filterStrings.Count > 0)
            {
                parameters = new ItemListParameters() { Filter = string.Join(" and ", filterStrings) };
            }

            return GalleryClient.Items.List(parameters).Items.ToList();
        }
    }
}
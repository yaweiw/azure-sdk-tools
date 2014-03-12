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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Commands.ResourceManagement.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Newtonsoft.Json;

namespace Microsoft.Azure.Commands.ResourceManagement
{
    public abstract class ResourceWithParameterBaseCmdlet : ResourceBaseCmdlet
    {
        protected const string BaseParameterSetName = "Default";
        protected const string GalleryTemplateParameterObjectParameterSetName = "Deployment via Gallery and parameters object";
        protected const string GalleryTemplateParameterFileParameterSetName = "Deployment via Gallery and parameters file";
        protected const string GalleryTemplateDynamicParametersParameterSetName = "Deployment via Gallery and inline parameters";
        protected const string TemplateFileParameterObjectParameterSetName = "Deployment via template file and parameters object";
        protected const string TemplateFileParameterFileParameterSetName = "Deployment via template file and parameters file";
        protected const string ParameterlessTemplateFileParameterSetName = "Deployment via template file without parameters";
        protected const string ParameterlessGalleryTemplateParameterSetName = "Deployment via Gallery without parameters";
        protected RuntimeDefinedParameterDictionary dynamicParameters;

        private string galleryTemplateName;

        private string templateFile;

        protected ResourceWithParameterBaseCmdlet()
        {
            dynamicParameters = new RuntimeDefinedParameterDictionary();
            galleryTemplateName = null;
        }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents the parameters.")]
        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents the parameters.")]
        public Hashtable ParameterObject { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A file that has the template parameters.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "A file that has the template parameters.")]
        [ValidateNotNullOrEmpty]
        public string ParameterFile { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [Parameter(ParameterSetName = GalleryTemplateDynamicParametersParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [Parameter(ParameterSetName = ParameterlessGalleryTemplateParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [ValidateNotNullOrEmpty]
        public string GalleryTemplateName { get; set; }

        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Path to the template file, local or remote.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Path to the template file, local or remote.")]
        [Parameter(ParameterSetName = ParameterlessTemplateFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [ValidateNotNullOrEmpty]
        public string TemplateFile { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content version of the template.")]
        [ValidateNotNullOrEmpty]
        public string TemplateVersion { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content hash of the template.")]
        [ValidateNotNullOrEmpty]
        public string TemplateHash { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The algorithm used to hash the template content.")]
        [ValidateNotNullOrEmpty]
        public string TemplateHashAlgorithm { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The storage account which the cmdlet to upload the template file to. If not specified, the current storage account of the subscription will be used.")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName { get; set; }

        public object GetDynamicParameters()
        {
            if (!string.IsNullOrEmpty(GalleryTemplateName) &&
                !GalleryTemplateName.Equals(galleryTemplateName, StringComparison.OrdinalIgnoreCase))
            {
                galleryTemplateName = GalleryTemplateName;
                dynamicParameters = ResourceClient.GetTemplateParametersFromGallery(
                    GalleryTemplateName,
                    ParameterObject,
                    ParameterFile,
                    MyInvocation.MyCommand.Parameters.Keys.ToArray());
            }
            else if (!string.IsNullOrEmpty(TemplateFile) &&
                !TemplateFile.Equals(templateFile, StringComparison.OrdinalIgnoreCase))
            {
                templateFile = TemplateFile;
                dynamicParameters = ResourceClient.GetTemplateParametersFromFile(
                    this.TryResolvePath(TemplateFile),
                    ParameterObject,
                    ParameterFile,
                    MyInvocation.MyCommand.Parameters.Keys.ToArray());
            }

            return dynamicParameters;
        }

        protected Hashtable GetParameterObject(Hashtable parameterObject)
        {
            parameterObject = parameterObject ?? new Hashtable();

            // Load parameters from the file
            string parameterFilePath = this.TryResolvePath(ParameterFile);
            if (parameterFilePath != null && File.Exists(parameterFilePath))
            {
                var parametersFromFile = JsonConvert.DeserializeObject<Dictionary<string, TemplateFileParameter>>(File.ReadAllText(parameterFilePath));
                parametersFromFile.ForEach(dp => parameterObject[GeneralUtilities.ToUpperFirstLetter(dp.Key)] = dp.Value.Value);
            }

            // Load dynamic parameters
            IEnumerable<RuntimeDefinedParameter> parameters = GeneralUtilities.GetUsedDynamicParameters(dynamicParameters, MyInvocation);
            if (parameters.Any())
            {
                parameters.ForEach(dp => parameterObject[GeneralUtilities.ToUpperFirstLetter(dp.Name)] = dp.Value);
            }

            return parameterObject;
        }
    }
}

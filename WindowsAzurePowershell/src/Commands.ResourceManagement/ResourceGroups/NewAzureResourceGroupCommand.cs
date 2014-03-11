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

using Microsoft.Azure.Commands.ResourceManagement.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ResourceManagement
{
    /// <summary>
    /// Creates a new resource group.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureResourceGroup", DefaultParameterSetName = BaseParameterSetName), OutputType(typeof(PSResourceGroup))]
    public class NewAzureResourceGroupCommand : ResourceBaseCmdlet, IDynamicParameters
    {
        internal const string BaseParameterSetName = "Default";
        internal const string GalleryTemplateParameterObjectParameterSetName = "Deployment via Gallery and parameters object";
        internal const string GalleryTemplateParameterFileParameterSetName = "Deployment via Gallery and parameters file";
        internal const string GalleryTemplateDynamicParametersParameterSetName = "Deployment via Gallery and inline parameters";
        internal const string TemplateFileParameterObjectParameterSetName = "Deployment via template file and parameters object";
        internal const string TemplateFileParameterFileParameterSetName = "Deployment via template file and parameters file";
        internal const string ParameterlessTemplateFileParameterSetName = "Deployment via template file without parameters";
        internal const string ParameterlessGalleryTemplateParameterSetName = "Deployment via Gallery without parameters";
        
        private RuntimeDefinedParameterDictionary dynamicParameters;
        
        private string galleryTemplateName;
        
        private string templateFile;

        public NewAzureResourceGroupCommand()
        {
            dynamicParameters = new RuntimeDefinedParameterDictionary();
            galleryTemplateName = null;
        }

        [Alias("ResourceGroupName")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group location.")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the deployment it's going to create. Only valid when a template is used. When a template is used, if the user doesn't specify a deployment name, use the current time, like \"20131223140835\".")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName { get; set; }

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

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                ResourceGroupName = Name,
                Location = Location,
                Name = DeploymentName,
                GalleryTemplateName = GalleryTemplateName,
                TemplateFile = this.TryResolvePath(TemplateFile),
                ParameterObject = GetParameterObject(ParameterObject),
                ParameterFile = this.TryResolvePath(ParameterFile),
                TemplateVersion = TemplateVersion,
                TemplateHash = TemplateHash,
                TemplateHashAlgorithm = TemplateHashAlgorithm,
                StorageAccountName = StorageAccountName,
                Force = Force.IsPresent,
                ConfirmAction = ConfirmAction
            };

            WriteObject(ResourceClient.CreatePSResourceGroup(parameters));
        }

        private Hashtable GetParameterObject(Hashtable parameterObject)
        {
            IEnumerable<RuntimeDefinedParameter> parameters = GeneralUtilities.GetUsedDynamicParameters(dynamicParameters, MyInvocation);

            if (parameters.Count() > 0)
            {
                parameterObject = parameterObject ?? new Hashtable();
                parameters.ForEach(dp => parameterObject.Add(dp.Name, dp.Value));
            }

            return parameterObject;
        }

        public object GetDynamicParameters()
        {
            if (!string.IsNullOrEmpty(GalleryTemplateName) && 
                !GalleryTemplateName.Equals(galleryTemplateName, StringComparison.OrdinalIgnoreCase))
            {
                galleryTemplateName = GalleryTemplateName;
                dynamicParameters = ResourceClient.GetTemplateParameters(
                    GalleryTemplateName,
                    MyInvocation.MyCommand.Parameters.Keys.ToArray(),
                    GalleryTemplateDynamicParametersParameterSetName);
            }

            if (!string.IsNullOrEmpty(TemplateFile) &&
                !TemplateFile.Equals(templateFile, StringComparison.OrdinalIgnoreCase))
            {
                templateFile = TemplateFile;
                dynamicParameters = ResourceClient.GetTemplateParameters(
                    this.TryResolvePath(TemplateFile),
                    MyInvocation.MyCommand.Parameters.Keys.ToArray(),
                    GalleryTemplateDynamicParametersParameterSetName);
            }
            
            return dynamicParameters;
        }
    }
}

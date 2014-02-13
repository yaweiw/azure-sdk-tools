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
using System.Management.Automation;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure;

namespace Microsoft.Azure.Commands.ResourceManagement.ResourceGroups
{
    /// <summary>
    /// Creates a new resource group.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureResourceGroup", DefaultParameterSetName = BaseParameterSetName), OutputType(typeof(ResourceGroup))]
    public class NewAzureResourceGroup : ResourceBaseCmdlet
    {
        internal const string BaseParameterSetName = "basic";
        internal const string GalleryTemplateParameterObjectParameterSetName = "galery-template-parameter-object";
        internal const string TemplateFileParameterObjectParameterSetName = "template-file-parameter-object";
        internal const string GalleryTemplateParameterFileParameterSetName = "galery-template-parameter-file";
        internal const string TemplateFileParameterFileParameterSetName = "template-file-parameter-file";

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the resource group")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The location of the resource group")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the deployment it's going to create. Only valid when a template is used. When a template is used, if the user doesn't specify a deployment name, use the current time, like \"20131223140835\".")]
        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the deployment it's going to create. Only valid when a template is used. When a template is used, if the user doesn't specify a deployment name, use the current time, like \"20131223140835\".")]
        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the deployment it's going to create. Only valid when a template is used. When a template is used, if the user doesn't specify a deployment name, use the current time, like \"20131223140835\".")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the deployment it's going to create. Only valid when a template is used. When a template is used, if the user doesn't specify a deployment name, use the current time, like \"20131223140835\".")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the template in the gallery.")]
        [ValidateNotNullOrEmpty]
        public string GalleryTemplateName { get; set; }

        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Path to the template file, local or remote.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Path to the template file, local or remote.")]
        [ValidateNotNullOrEmpty]
        public string TemplateFile { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents the parameters.")]
        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents the parameters.")]
        public Hashtable ParameterObject { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A file that has the template parameters.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A file that has the template parameters.")]
        [ValidateNotNullOrEmpty]
        public string ParameterFile { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content version of the template.")]
        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content version of the template.")]
        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content version of the template.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content version of the template.")]
        [ValidateNotNullOrEmpty]
        public string TemplateVersion { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content hash of the template.")]
        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content hash of the template.")]
        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content hash of the template.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The expect content hash of the template.")]
        [ValidateNotNullOrEmpty]
        public string TemplateHash { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The algorithm used to hash the template content.")]
        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The algorithm used to hash the template content.")]
        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The algorithm used to hash the template content.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The algorithm used to hash the template content.")]
        [ValidateNotNullOrEmpty]
        public string TemplateHashAlgorithm { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The storage account which the cmdlet to upload the template file to. If not specified, the current storage account of the subscription will be used.")]
        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The storage account which the cmdlet to upload the template file to. If not specified, the current storage account of the subscription will be used.")]
        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The storage account which the cmdlet to upload the template file to. If not specified, the current storage account of the subscription will be used.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The storage account which the cmdlet to upload the template file to. If not specified, the current storage account of the subscription will be used.")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName { get; set; }

        [Parameter(ParameterSetName = GalleryTemplateParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "By default the command will wait until the resource group is created. Using this switch parameter will make it return immediately.")]
        [Parameter(ParameterSetName = TemplateFileParameterObjectParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "By default the command will wait until the resource group is created. Using this switch parameter will make it return immediately.")]
        [Parameter(ParameterSetName = GalleryTemplateParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "By default the command will wait until the resource group is created. Using this switch parameter will make it return immediately.")]
        [Parameter(ParameterSetName = TemplateFileParameterFileParameterSetName,
            Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "By default the command will wait until the resource group is created. Using this switch parameter will make it return immediately.")]
        public SwitchParameter Async { get; set; }

        public override void ExecuteCmdlet()
        {
            PSCreateResourceGroupParameters parameters = new PSCreateResourceGroupParameters()
            {
                Name = Name,
                Location = Location,
                DeploymentName = DeploymentName,
                GalleryTemplateName = GalleryTemplateName,
                TemplateFile = TemplateFile,
                ParameterObject = ParameterObject,
                ParameterFile = ParameterFile,
                TemplateVersion = TemplateVersion,
                TemplateHash = TemplateHash,
                TemplateHashAlgorithm = TemplateHashAlgorithm,
                StorageAccountName = StorageAccountName,
                Async = Async
            };
            WriteObject(ResourceClient.CreatePSResourceGroup(parameters));
        }
    }
}

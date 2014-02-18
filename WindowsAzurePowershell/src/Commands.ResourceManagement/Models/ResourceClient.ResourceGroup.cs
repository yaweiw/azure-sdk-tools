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
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Microsoft.Azure.Commands.ResourceManagement.Properties;
using Microsoft.Azure.Commands.ResourceManagement.ResourceGroups;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Azure.Management.Resources;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Newtonsoft.Json;
using System.Linq;
using System.Management.Automation;
using Microsoft.CSharp.RuntimeBinder;
using System.Diagnostics;
using System.Security;
using System.Threading;
using Microsoft.Azure.Gallery;
using Microsoft.Azure.Gallery.Models;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public partial class ResourcesClient
    {
        public virtual PSResourceGroup CreatePSResourceGroup(CreatePSResourceGroupParameters parameters)
        {
            // Validate that parameter group doesn't already exist
            if (ResourceManagementClient.ResourceGroups.Exists(parameters.Name).Exists)
            {
                throw new ArgumentException(Resources.ResourceGroupAlreadyExists);
            }

            ResourceGroup resourceGroup = CreateResourceGroup(parameters.Name, parameters.Location);

            CreateDeployment(resourceGroup.Name, parameters);
            List<Resource> resources = FilterResources(new FilterResourcesOptions() { ResourceGroup = resourceGroup.Name } );

            return new PSResourceGroup()
            {
                Name = resourceGroup.Name,
                Location = resourceGroup.Location,
                Resources = resources,
                ResourcesTable = ConstructResourcesTable(resources)
            };
        }

        public virtual List<Resource> FilterResources(FilterResourcesOptions options)
        {
            List<Resource> resources = new List<Resource>();

            if (!string.IsNullOrEmpty(options.ResourceGroup) && !string.IsNullOrEmpty(options.Name))
            {
                resources.Add(ResourceManagementClient.Resources.Get(
                    new ResourceParameters() {
                        ResourceGroupName = options.ResourceGroup,
                        ResourceName = options.Name }).Resource);
            }
            else if (!string.IsNullOrEmpty(options.ResourceGroup) && !string.IsNullOrEmpty(options.ResourceType))
            {
                resources.AddRange(ResourceManagementClient.Resources.ListForResourceGroup(
                    options.ResourceGroup,
                    new ResourceListParameters() { ResourceType = options.ResourceType }).Resources);
            }
            else if (!string.IsNullOrEmpty(options.ResourceGroup))
            {
                resources.AddRange(ResourceManagementClient.Resources
                    .ListForResourceGroup(options.ResourceGroup, new ResourceListParameters()).Resources);
            }
            else if (!string.IsNullOrEmpty(options.ResourceType))
            {
                resources.AddRange(ResourceManagementClient.Resources
                    .List(new ResourceListParameters() { ResourceType = options.ResourceType }).Resources);
            }

            return resources;
        }

        public virtual DeploymentProperties CreateDeployment(string resourceGroup, CreatePSDeploymentParameters parameters)
        {
            DeploymentProperties result = null;
            bool createDeployment = !string.IsNullOrEmpty(parameters.GalleryTemplateName) || !string.IsNullOrEmpty(parameters.TemplateFile);

            if (createDeployment)
            {
                BasicDeployment deployment = new BasicDeployment()
                {
                    Mode = DeploymentMode.Incremental,
                    TemplateLink = new TemplateLink()
                    {
                        Uri = GetTemplateUri(parameters.TemplateFile, parameters.StorageAccountName),
                        ContentVersion = parameters.TemplateVersion,
                        ContentHash = GetTemplateContentHash(parameters.TemplateHash, parameters.TemplateHashAlgorithm)
                    },
                    Parameters = GetDeploymentParameters(parameters.ParameterFile, parameters.ParameterObject)
                };

                result = ResourceManagementClient.Deployments.Create(resourceGroup, parameters.DeploymentName, deployment).Properties;
            }

            return result;
        }

        public virtual RuntimeDefinedParameterDictionary GetTemplateParameters(string templateName, string[] parameters, params string[] parameterSetNames)
        {
            const string duplicatedParameterSuffix = "FromTemplate";
            RuntimeDefinedParameterDictionary dynamicParameters = new RuntimeDefinedParameterDictionary();

            string templateContest = General.DownloadFile(GetGallaryTemplateFile(templateName));
            Dictionary<string, dynamic> template = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(templateContest);

            foreach (var parameter in template["parameters"])
            {
                string name = General.ToUpperFirstLetter(parameter.Name);
                RuntimeDefinedParameter runtimeParameter = new RuntimeDefinedParameter()
                {
                    Name = parameters.Contains(name) ? name + duplicatedParameterSuffix : name,
                    ParameterType = GetParameterType((string)parameter.Value.type)
                };
                foreach (string parameterSetName in parameterSetNames)
                {
                    runtimeParameter.Attributes.Add(new ParameterAttribute()
                    {
                        ParameterSetName = parameterSetName,
                        Mandatory = false,
                        ValueFromPipelineByPropertyName = true,
                        HelpMessage = "dynamically generated template parameter",
                    });
                }

                dynamicParameters.Add(runtimeParameter.Name, runtimeParameter);
            }

            return dynamicParameters;
        }

        public virtual IEnumerable<ResourceGroup> GetResourceGroups(GetAzureResourceGroupCommand parameters)
        {
            // Get all resource groups
            if (parameters.Name == null)
            {
                return ResourceManagementClient.ResourceGroups.List(null).ResourceGroups;
            }
            // Get one group by name 
            else if (parameters.Name != null)
            {
                return new[] { ResourceManagementClient.ResourceGroups.Get(parameters.Name).ResourceGroup };
            }
            return null;
        }
    }
}

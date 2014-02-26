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

using Microsoft.Azure.Commands.ResourceManagement.Properties;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public partial class ResourcesClient
    {
        /// <summary>
        /// Creates a new resource group and deployment using the passed template file option which
        /// can be user customized or from gallery tenplates.
        /// </summary>
        /// <param name="parameters">The create parameters</param>
        /// <returns>The created resource group</returns>
        public virtual PSResourceGroup CreatePSResourceGroup(CreatePSResourceGroupParameters parameters)
        {
            if (ResourceManagementClient.ResourceGroups.CheckExistence(parameters.Name).Exists)
            {
                throw new ArgumentException(Resources.ResourceGroupAlreadyExists);
            }

            ResourceGroup resourceGroup = CreateResourceGroup(parameters.Name, parameters.Location);
            CreatePSResourceGroupDeployment(resourceGroup.Name, parameters);

            return resourceGroup.ToPSResourceGroup(this);
        }

        /// <summary>
        /// Filters a given resource group resources.
        /// </summary>
        /// <param name="options">The filtering options</param>
        /// <returns>The filtered set of resources matching the filter criteria</returns>
        public virtual List<Resource> FilterResources(FilterResourcesOptions options)
        {
            List<Resource> resources = new List<Resource>();

            if (!string.IsNullOrEmpty(options.ResourceGroup) && !string.IsNullOrEmpty(options.Name))
            {
                resources.Add(ResourceManagementClient.Resources.Get(options.ResourceGroup,
                    new ResourceIdentity() { ResourceName = options.Name }).Resource);
            }
            else
            {
                ResourceListResult result = ResourceManagementClient.Resources.List(new ResourceListParameters()
                {
                    ResourceGroupName = options.ResourceGroup,
                    ResourceType = options.ResourceType
                });

                resources.AddRange(result.Resources);

                while (!string.IsNullOrEmpty(result.NextLink))
                {
                    result = ResourceManagementClient.Resources.ListNext(result.NextLink);
                    resources.AddRange(result.Resources);
                };
            }

            return resources;
        }

        /// <summary>
        /// Creates new deployment using the passed template file which can be user customized or
        /// from gallery templates.
        /// </summary>
        /// <param name="resourceGroup">The resource group name</param>
        /// <param name="parameters">The create deployment parameters</param>
        /// <returns>The created deployment instance</returns>
        public virtual PSResourceGroupDeployment CreatePSResourceGroupDeployment(string resourceGroup, CreatePSResourceGroupDeploymentParameters parameters)
        {
            DeploymentOperationsCreateResult result = null;
            bool createDeployment = !string.IsNullOrEmpty(parameters.GalleryTemplateName) || !string.IsNullOrEmpty(parameters.TemplateFile);

            if (createDeployment)
            {
                parameters.DeploymentName = string.IsNullOrEmpty(parameters.DeploymentName) ? Guid.NewGuid().ToString() : parameters.DeploymentName;
                BasicDeployment deployment = new BasicDeployment()
                {
                    Mode = DeploymentMode.Incremental,
                    TemplateLink = new TemplateLink()
                    {
                        Uri = GetTemplateUri(parameters.TemplateFile, parameters.GalleryTemplateName, parameters.StorageAccountName),
                        ContentVersion = parameters.TemplateVersion,
                        ContentHash = GetTemplateContentHash(parameters.TemplateHash, parameters.TemplateHashAlgorithm)
                    },
                    Parameters = GetDeploymentParameters(parameters.ParameterFile, parameters.ParameterObject)
                };

                ValidateDeployment(resourceGroup, deployment);
                
                result = ResourceManagementClient.Deployments.Create(resourceGroup, parameters.DeploymentName, deployment);
                WriteProgress(string.Format("Create template deployment '{0}' using template {1}.", parameters.DeploymentName, deployment.TemplateLink.Uri));
                ProvisionDeploymentStatus(resourceGroup, parameters.DeploymentName);
            }

            return result.ToPSResourceGroupDeployment();
        }

        /// <summary>
        /// Gets the parameters for a given gallery template.
        /// </summary>
        /// <param name="templateName">The gallery template name</param>
        /// <param name="parameters">The existing PowerShell cmdlet parameters</param>
        /// <param name="parameterSetNames">The parameter set which the dynamic parameters should be added to</param>
        /// <returns>The template parameters</returns>
        public virtual RuntimeDefinedParameterDictionary GetTemplateParameters(string templateName, string[] parameters, params string[] parameterSetNames)
        {
            RuntimeDefinedParameterDictionary dynamicParameters = new RuntimeDefinedParameterDictionary();
            string templateContent = null;
            
            if (Uri.IsWellFormedUriString(templateName, UriKind.Absolute))
            {
                templateContent = General.DownloadFile(GetGalleryTemplateFile(templateName));
            }
            else if (File.Exists(templateName))
            {
                templateContent = File.ReadAllText(templateName);
            }
            else
            {
                throw new ArgumentException("templateName");
            }
            
            TemplateFile templateFile = JsonConvert.DeserializeObject<TemplateFile>(templateContent);

            foreach (KeyValuePair<string, TemplateFileParameter> parameter in templateFile.Parameters)
            {
                RuntimeDefinedParameter dynamicParameter = ConstructDynamicParameter(parameters, parameterSetNames, parameter);
                dynamicParameters.Add(dynamicParameter.Name, dynamicParameter);
            }
            
            return dynamicParameters;
        }

        /// <summary>
        /// Filters the subscription's resource groups.
        /// </summary>
        /// <param name="name">The resource group name.</param>
        /// <returns>The filtered resource groups</returns>
        public virtual List<PSResourceGroup> FilterResourceGroups(string name)
        {
            List<PSResourceGroup> result = new List<PSResourceGroup>();
            if (string.IsNullOrEmpty(name))
            {
                result.AddRange(ResourceManagementClient.ResourceGroups.List(null).ResourceGroups
                    .Select(rg => rg.ToPSResourceGroup(this)));
            }
            else
            {
                result.Add(ResourceManagementClient.ResourceGroups.Get(name).ResourceGroup.ToPSResourceGroup(this));
            }

            return result;
        }

        /// <summary>
        /// Deletes a given resource group
        /// </summary>
        /// <param name="name">The resource group name</param>
        public virtual void DeleteResourceGroup(string name)
        {
            ResourceManagementClient.ResourceGroups.Delete(name);
        }

        /// <summary>
        /// Filters resource group deployments for a subscription
        /// </summary>
        /// <param name="resourceGroup">The resource group name</param>
        /// <param name="name">The deployment name</param>
        /// <param name="provisioningState">The provisioning state</param>
        /// <returns>The deployments that match the search criteria</returns>
        public virtual List<PSResourceGroupDeployment> FilterResourceGroupDeployments(
            string resourceGroup,
            string name,
            string provisioningState)
        {
            List<PSResourceGroupDeployment> deployments = new List<PSResourceGroupDeployment>();

            if (!string.IsNullOrEmpty(resourceGroup) && !string.IsNullOrEmpty(name))
            {
                deployments.Add(ResourceManagementClient.Deployments.Get(resourceGroup, name).ToPSResourceGroupDeployment());
            }
            else if (!string.IsNullOrEmpty(resourceGroup))
            {
                DeploymentListResult result = ResourceManagementClient.Deployments.List(
                    new DeploymentListParameters()
                    {
                        ResourceGroupName = resourceGroup,
                        ProvisioningState = provisioningState
                    });

                deployments.AddRange(result.Deployments.Select(d => d.ToPSResourceGroupDeployment()));

                while (!string.IsNullOrEmpty(result.NextLink))
                {
                    result = ResourceManagementClient.Deployments.ListNext(result.NextLink);
                    deployments.AddRange(result.Deployments.Select(d => d.ToPSResourceGroupDeployment()));
                };
            }

            return deployments;
        }
    }
}

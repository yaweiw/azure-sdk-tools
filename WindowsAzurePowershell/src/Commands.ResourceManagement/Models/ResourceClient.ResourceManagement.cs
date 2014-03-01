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
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public partial class ResourcesClient
    {
        /// <summary>
        /// Creates a new resource.
        /// </summary>
        /// <param name="parameters">The create parameters</param>
        /// <returns>The created resource</returns>
        public virtual PSResource CreatePSResource(CreatePSResourceParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.ResourceType))
            {
                throw new ArgumentNullException("ResourceType");
            }

            string[] resourceType = parameters.ResourceType.Split('/');
            if (resourceType.Length != 2)
            {
                throw new ArgumentException(Resources.ResourceTypeFormat);
            }

            ResourceIdentity resourceIdentity = new ResourceIdentity
                {
                    ParentResourcePath = parameters.ParentResourceName,
                    ResourceName = parameters.Name,
                    ResourceProviderNamespace = resourceType[0],
                    ResourceType = resourceType[1]
                };

            bool resourceExists = ResourceManagementClient.Resources.CheckExistence(parameters.ResourceGroupName, resourceIdentity).Exists;
            
            if (resourceExists)
            {
                throw new ArgumentException(Resources.ResourceAlreadyExists);
            }

            if (ResourceManagementClient.ResourceGroups.CheckExistence(parameters.ResourceGroupName).Exists)
            {
                WriteProgress(string.Format("{0, -10} Resource group \"{1}\" is found.", "[Info]",
                                            parameters.ResourceGroupName));
            }
            else
            {
                throw new ArgumentException(Resources.ResourceGroupDoesntExists);
            }

            WriteProgress(string.Format("{0, -10} Creating resource \"{1}\".", "[Start]", parameters.Name));
            
            ResourceCreateOrUpdateResult createOrUpdateResult = ResourceManagementClient.Resources.CreateOrUpdate(parameters.ResourceGroupName, resourceIdentity, 
                new ResourceCreateOrUpdateParameters
                {
                    ValidationMode = ResourceValidationMode.NameValidation,
                    Resource = new BasicResource
                        {
                            Location = parameters.Location,
                            Properties = SerializeHashtable(parameters.PropertyObject, addValueLayer: false)
                        }
                });

            if (createOrUpdateResult.Resource != null)
            {
                WriteProgress(string.Format("{0, -10} Creating resource \"{1}\".", "[Complete]", parameters.Name));
            }

            ResourceGetResult getResult = ResourceManagementClient.Resources.Get(parameters.ResourceGroupName, resourceIdentity);

            return getResult.Resource.ToPSResource(this);
        }

        /// <summary>
        /// Updates an existing resource.
        /// </summary>
        /// <param name="parameters">The update parameters</param>
        /// <returns>The updated resource</returns>
        public virtual PSResource UpdatePSResource(UpdatePSResourceParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.ResourceType))
            {
                throw new ArgumentNullException("ResourceType");
            }

            string[] resourceType = parameters.ResourceType.Split('/');
            if (resourceType.Length != 2)
            {
                throw new ArgumentException(Resources.ResourceTypeFormat);
            }

            ResourceIdentity resourceIdentity = new ResourceIdentity
            {
                ParentResourcePath = parameters.ParentResourceName,
                ResourceName = parameters.Name,
                ResourceProviderNamespace = resourceType[0],
                ResourceType = resourceType[1]
            };

            ResourceGetResult getResource;

            try
            {
                getResource = ResourceManagementClient.Resources.Get(parameters.ResourceGroupName,
                                                                     resourceIdentity);
            }
            catch (CloudException)
            {
                throw new ArgumentException(Resources.ResourceDoesntExists);
            }

            ResourceManagementClient.Resources.CreateOrUpdate(parameters.ResourceGroupName, resourceIdentity,
                new ResourceCreateOrUpdateParameters
                {
                    ValidationMode = ResourceValidationMode.NameValidation,
                    Resource = new BasicResource
                    {
                        Location = getResource.Resource.Location,
                        Properties = SerializeHashtable(parameters.PropertyObject, addValueLayer: false)
                    }
                });

            ResourceGetResult getResult = ResourceManagementClient.Resources.Get(parameters.ResourceGroupName, resourceIdentity);

            return getResult.Resource.ToPSResource(this);
        }

        /// <summary>
        /// Get an existing resource or resources.
        /// </summary>
        /// <param name="parameters">The get parameters</param>
        /// <returns>List of resources</returns>
        public virtual List<PSResource> FilterPSResources(GetPSResourceParameters parameters)
        {
            List<PSResource> resources = new List<PSResource>();

            if (!string.IsNullOrEmpty(parameters.Name))
            {
                if (string.IsNullOrEmpty(parameters.ResourceType))
                {
                    throw new ArgumentNullException("ResourceType");
                }

                string[] resourceType = parameters.ResourceType.Split('/');
                if (resourceType.Length != 2)
                {
                    throw new ArgumentException(Resources.ResourceTypeFormat);
                }

                ResourceGetResult getResult = ResourceManagementClient.Resources.Get(parameters.ResourceGroupName, new ResourceIdentity
                    {
                        ResourceName = parameters.Name,
                        ParentResourcePath = parameters.ParentResourceName,
                        ResourceProviderNamespace = resourceType[0],
                        ResourceType = resourceType[1]
                    });

                resources.Add(getResult.Resource.ToPSResource(this));
            }
            else
            {
                ResourceListResult listResult = ResourceManagementClient.Resources.List(new ResourceListParameters
                    {
                        ResourceGroupName = parameters.ResourceGroupName,
                        ResourceType = parameters.ResourceType
                    });

                if (listResult.Resources != null)
                {
                    resources.AddRange(listResult.Resources.Select(r => r.ToPSResource(this)));
                }
            }
            return resources;
        }

        /// <summary>
        /// Creates a new resource group and deployment using the passed template file option which
        /// can be user customized or from gallery tenplates.
        /// </summary>
        /// <param name="parameters">The create parameters</param>
        /// <returns>The created resource group</returns>
        public virtual PSResourceGroup CreatePSResourceGroup(CreatePSResourceGroupParameters parameters)
        {
            if (ResourceManagementClient.ResourceGroups.CheckExistence(parameters.ResourceGroupName).Exists)
            {
                throw new ArgumentException(Resources.ResourceGroupAlreadyExists);
            }

            ResourceGroup resourceGroup = CreateResourceGroup(parameters.ResourceGroupName, parameters.Location);
            CreatePSResourceGroupDeployment(parameters);

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
                }
            }

            return resources;
        }

        /// <summary>
        /// Creates new deployment using the passed template file which can be user customized or
        /// from gallery templates.
        /// </summary>
        /// <param name="parameters">The create deployment parameters</param>
        /// <returns>The created deployment instance</returns>
        public virtual PSResourceGroupDeployment CreatePSResourceGroupDeployment(CreatePSResourceGroupDeploymentParameters parameters)
        {
            DeploymentOperationsCreateResult result = null;
            bool createDeployment = !string.IsNullOrEmpty(parameters.GalleryTemplateName) || !string.IsNullOrEmpty(parameters.TemplateFile);
            string resourceGroup = parameters.ResourceGroupName;

            RegisterResourceProviders();

            if (createDeployment)
            {
                parameters.DeploymentName = string.IsNullOrEmpty(parameters.DeploymentName) ? Guid.NewGuid().ToString() : parameters.DeploymentName;
                BasicDeployment deployment = CreateBasicDeployment(parameters);
                List<ResourceManagementError> errors = CheckBasicDeploymentErrors(resourceGroup, deployment);

                if (errors.Count != 0)
                {
                    int counter = 1;
                    string errorFormat = "Error {0}: Code={1}; Message={2}; Target={3}\r\n";
                    StringBuilder errorsString = new StringBuilder();
                    errors.ForEach(e => errorsString.AppendFormat(errorFormat, counter++, e.Code, e.Message, e.Target));
                    throw new ArgumentException(errors.ToString());
                }

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
                templateContent = General.DownloadFile(templateName);
            }
            else if (File.Exists(templateName))
            {
                templateContent = File.ReadAllText(templateName);
            }
            else
            {
                templateContent = General.DownloadFile(GetGalleryTemplateFile(templateName));
            }

            if (string.IsNullOrEmpty(templateContent))
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
                }
            }

            return deployments;
        }

        /// <summary>
        /// Cancels the active deployment.
        /// </summary>
        /// <param name="resourceGroup">The resource group name</param>
        public virtual void CancelDeployment(string resourceGroup)
        {
            List<PSResourceGroupDeployment> deployments = FilterResourceGroupDeployments(resourceGroup);

            foreach (PSResourceGroupDeployment deployment in deployments)
            {
                if (deployment.ProvisioningState != ProvisioningState.Failed && 
                    deployment.ProvisioningState != ProvisioningState.Succeeded)
                {
                    ResourceManagementClient.Deployments.Cancel(resourceGroup, deployment.DeploymentName);
                    break;
                }
            }
        }

        /// <summary>
        /// Validates a given deployment.
        /// </summary>
        /// <param name="parameters">The deployment create options</param>
        /// <returns>True if valid, false otherwise.</returns>
        public virtual List<ResourceManagementError> ValidatePSResourceGroupDeployment(ValidatePSResourceGroupDeploymentParameters parameters)
        {
            List<ResourceManagementError> errors = new List<ResourceManagementError>();

            BasicDeployment deployment = CreateBasicDeployment(parameters);
            errors.AddRange(CheckBasicDeploymentErrors(parameters.ResourceGroupName, deployment));

            return errors;
        }
    }
}

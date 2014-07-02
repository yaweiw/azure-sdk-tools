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

using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProjectResources = Microsoft.Azure.Commands.Resources.Properties.Resources;

namespace Microsoft.Azure.Commands.Resources.Models
{
    public partial class ResourcesClient
    {
        public const string ResourceGroupTypeName = "ResourceGroup";

        public static List<string> KnownLocations = new List<string>()
        {
            "East Asia", "South East Asia", "East US", "West US", "North Central US", 
            "South Central US", "Central US", "North Europe", "West Europe"
        };

        internal static List<string> KnownLocationsNormalized = KnownLocations
            .Select(loc => loc.ToLower().Replace(" ", "")).ToList();

        /// <summary>
        /// Creates a new resource.
        /// </summary>
        /// <param name="parameters">The create parameters</param>
        /// <returns>The created resource</returns>
        public virtual PSResource CreatePSResource(CreatePSResourceParameters parameters)
        {
            ResourceIdentity resourceIdentity = parameters.ToResourceIdentity();

            if (ResourceManagementClient.ResourceGroups.CheckExistence(parameters.ResourceGroupName).Exists)
            {
                WriteVerbose(string.Format("Resource group \"{0}\" is found.", parameters.ResourceGroupName));
            }
            else
            {
                parameters.ConfirmAction(parameters.Force,
                                             ProjectResources.ResourceGroupDoesntExistsAdd,
                                             ProjectResources.AddingResourceGroup,
                                             parameters.Name,
                                             () => CreateResourceGroup(parameters.ResourceGroupName, parameters.Location));

                if (!ResourceManagementClient.ResourceGroups.CheckExistence(parameters.ResourceGroupName).Exists)
                {
                    throw new ArgumentException(ProjectResources.ResourceGroupDoesntExists);
                }
            }

            bool resourceExists = ResourceManagementClient.Resources.CheckExistence(parameters.ResourceGroupName, resourceIdentity).Exists;

            Action createOrUpdateResource = () =>
                {
                    WriteVerbose(string.Format("Creating resource \"{0}\" started.", parameters.Name));

                    ResourceCreateOrUpdateResult createOrUpdateResult = ResourceManagementClient.Resources.CreateOrUpdate(parameters.ResourceGroupName, 
                        resourceIdentity,
                        new BasicResource
                            {
                                Location = parameters.Location,
                                Properties = SerializeHashtable(parameters.PropertyObject, addValueLayer: false)
                            });

                    if (createOrUpdateResult.Resource != null)
                    {
                        WriteVerbose(string.Format("Creating resource \"{0}\" complete.", parameters.Name));
                    }
                };
            
            if (resourceExists && !parameters.Force)
            {
                parameters.ConfirmAction(parameters.Force,
                                         ProjectResources.ResourceAlreadyExists,
                                         ProjectResources.NewResourceMessage,
                                         parameters.Name,
                                         createOrUpdateResource);
            }
            else
            {
                createOrUpdateResource();
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
            ResourceIdentity resourceIdentity = parameters.ToResourceIdentity();

            ResourceGetResult getResource;

            try
            {
                getResource = ResourceManagementClient.Resources.Get(parameters.ResourceGroupName,
                                                                     resourceIdentity);
            }
            catch (CloudException)
            {
                throw new ArgumentException(ProjectResources.ResourceDoesntExists);
            }

            string newProperty = SerializeHashtable(parameters.PropertyObject,
                                                    addValueLayer: false);

            ResourceManagementClient.Resources.CreateOrUpdate(parameters.ResourceGroupName, resourceIdentity,
                        new BasicResource
                            {
                                Location = getResource.Resource.Location,
                                Properties = newProperty
                            });

            ResourceGetResult getResult = ResourceManagementClient.Resources.Get(parameters.ResourceGroupName, resourceIdentity);

            return getResult.Resource.ToPSResource(this);
        }

        /// <summary>
        /// Get an existing resource or resources.
        /// </summary>
        /// <param name="parameters">The get parameters</param>
        /// <returns>List of resources</returns>
        public virtual List<PSResource> FilterPSResources(BasePSResourceParameters parameters)
        {
            List<PSResource> resources = new List<PSResource>();

            if (!string.IsNullOrEmpty(parameters.ResourceGroupName))
            {
                if (!ResourceManagementClient.ResourceGroups.CheckExistence(parameters.ResourceGroupName).Exists)
                {
                    throw new ArgumentException(ProjectResources.ResourceGroupDoesntExists);
                }
            }

            if (!string.IsNullOrEmpty(parameters.Name))
            {
                ResourceIdentity resourceIdentity = parameters.ToResourceIdentity();

                ResourceGetResult getResult;

                try
                {
                    getResult = ResourceManagementClient.Resources.Get(parameters.ResourceGroupName, resourceIdentity);
                }
                catch (CloudException)
                {
                    throw new ArgumentException(ProjectResources.ResourceDoesntExists);
                }

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
            bool createDeployment = !string.IsNullOrEmpty(parameters.GalleryTemplateIdentity) || !string.IsNullOrEmpty(parameters.TemplateFile);
            bool resourceExists = ResourceManagementClient.ResourceGroups.CheckExistence(parameters.ResourceGroupName).Exists;

            ResourceGroup resourceGroup = null;
            Action createOrUpdateResourceGroup = () =>
            {
                resourceGroup = CreateResourceGroup(parameters.ResourceGroupName, parameters.Location);

                if (createDeployment)
                {
                    ExecuteDeployment(parameters);
                }
            };

            if (resourceExists && !parameters.Force)
            {
                parameters.ConfirmAction(parameters.Force,
                    ProjectResources.ResourceGroupAlreadyExists,
                    ProjectResources.NewResourceGroupMessage,
                    parameters.DeploymentName,
                    createOrUpdateResourceGroup);
                resourceGroup = ResourceManagementClient.ResourceGroups.Get(parameters.ResourceGroupName).ResourceGroup;
            }
            else
            {
                createOrUpdateResourceGroup();
            }

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
        public virtual PSResourceGroupDeployment ExecuteDeployment(CreatePSResourceGroupDeploymentParameters parameters)
        {
            parameters.DeploymentName = GenerateDeploymentName(parameters);
            BasicDeployment deployment = CreateBasicDeployment(parameters);
            List<ResourceManagementError> errors = CheckBasicDeploymentErrors(parameters.ResourceGroupName, parameters.DeploymentName, deployment);

            if (errors.Count != 0)
            {
                int counter = 1;
                string errorFormat = "Error {0}: Code={1}; Message={2}\r\n";
                StringBuilder errorsString = new StringBuilder();
                errors.ForEach(e => errorsString.AppendFormat(errorFormat, counter++, e.Code, e.Message));
                throw new ArgumentException(errorsString.ToString());
            }
            else
            {
                WriteVerbose(ProjectResources.TemplateValid);
            }

            ResourceManagementClient.Deployments.CreateOrUpdate(parameters.ResourceGroupName, parameters.DeploymentName, deployment);
            WriteVerbose(string.Format("Create template deployment '{0}' using template {1}.", parameters.DeploymentName, deployment.TemplateLink.Uri));
            Deployment result = ProvisionDeploymentStatus(parameters.ResourceGroupName, parameters.DeploymentName, deployment);

            return result.ToPSResourceGroupDeployment(parameters.ResourceGroupName);
        }

        private string GenerateDeploymentName(CreatePSResourceGroupDeploymentParameters parameters)
        {
            if (!string.IsNullOrEmpty(parameters.DeploymentName))
            {
                return parameters.DeploymentName;
            }
            else if (!string.IsNullOrEmpty(parameters.TemplateFile))
            {
                return Path.GetFileNameWithoutExtension(parameters.TemplateFile);
            }
            else if (!string.IsNullOrEmpty(parameters.GalleryTemplateIdentity))
            {
                return parameters.GalleryTemplateIdentity;
            }
            else
            {
                return Guid.NewGuid().ToString();
            }
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
                try
                {
                    result.Add(ResourceManagementClient.ResourceGroups.Get(name).ResourceGroup.ToPSResourceGroup(this));
                }
                catch (CloudException)
                {
                    throw new ArgumentException(ProjectResources.ResourceGroupDoesntExists);
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes a given resource
        /// </summary>
        /// <param name="parameters">The resource identification</param>
        public virtual void DeleteResource(BasePSResourceParameters parameters)
        {
            ResourceIdentity resourceIdentity = parameters.ToResourceIdentity();

            if (!ResourceManagementClient.Resources.CheckExistence(parameters.ResourceGroupName, resourceIdentity).Exists)
            {
                throw new ArgumentException(ProjectResources.ResourceDoesntExists);
            }

            ResourceManagementClient.Resources.Delete(parameters.ResourceGroupName, resourceIdentity);
        }

        /// <summary>
        /// Deletes a given resource group
        /// </summary>
        /// <param name="name">The resource group name</param>
        public virtual void DeleteResourceGroup(string name)
        {
            if (!ResourceManagementClient.ResourceGroups.CheckExistence(name).Exists)
            {
                throw new ArgumentException(ProjectResources.ResourceGroupDoesntExists);
            }

            ResourceManagementClient.ResourceGroups.Delete(name);
        }

        /// <summary>
        /// Filters the resource group deployments
        /// </summary>
        /// <param name="options">The filtering options</param>
        /// <returns>The filtered list of deployments</returns>
        public virtual List<PSResourceGroupDeployment> FilterResourceGroupDeployments(FilterResourceGroupDeploymentOptions options)
        {
            List<PSResourceGroupDeployment> deployments = new List<PSResourceGroupDeployment>();
            string resourceGroup = options.ResourceGroupName;
            string name = options.DeploymentName;
            List<string> excludedProvisioningStates = options.ExcludedProvisioningStates ?? new List<string>();
            List<string> provisioningStates = options.ProvisioningStates ?? new List<string>();

            if (!string.IsNullOrEmpty(resourceGroup) && !string.IsNullOrEmpty(name))
            {
                deployments.Add(ResourceManagementClient.Deployments.Get(resourceGroup, name).ToPSResourceGroupDeployment(options.ResourceGroupName));
            }
            else if (!string.IsNullOrEmpty(resourceGroup))
            {
                DeploymentListParameters parameters = new DeploymentListParameters();

                if (provisioningStates.Count == 1)
                {
                    parameters.ProvisioningState = provisioningStates.First();
                }

                DeploymentListResult result = ResourceManagementClient.Deployments.List(resourceGroup, parameters);

                deployments.AddRange(result.Deployments.Select(d => d.ToPSResourceGroupDeployment(options.ResourceGroupName)));

                while (!string.IsNullOrEmpty(result.NextLink))
                {
                    result = ResourceManagementClient.Deployments.ListNext(result.NextLink);
                    deployments.AddRange(result.Deployments.Select(d => d.ToPSResourceGroupDeployment(options.ResourceGroupName)));
                }
            }

            if (provisioningStates.Count > 1)
            {
                return deployments.Where(d => provisioningStates
                    .Any(s => s.Equals(d.ProvisioningState, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            else if (provisioningStates.Count == 0 && excludedProvisioningStates.Count > 0)
            {
                return deployments.Where(d => excludedProvisioningStates
                    .All(s => !s.Equals(d.ProvisioningState, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            else
            {
                return deployments;
            }
        }

        /// <summary>
        /// Cancels the active deployment.
        /// </summary>
        /// <param name="resourceGroup">The resource group name</param>
        /// <param name="deploymentName">Deployment name</param>
        public virtual void CancelDeployment(string resourceGroup, string deploymentName)
        {
            FilterResourceGroupDeploymentOptions options = new FilterResourceGroupDeploymentOptions()
            {
                DeploymentName = deploymentName,
                ResourceGroupName = resourceGroup
            };

            if (string.IsNullOrEmpty(deploymentName))
            {
                options.ExcludedProvisioningStates = new List<string>()
                {
                    ProvisioningState.Failed,
                    ProvisioningState.Succeeded
                };
            }

            List<PSResourceGroupDeployment> deployments = FilterResourceGroupDeployments(options);

            if (deployments.Count == 0)
            {
                if (string.IsNullOrEmpty(deploymentName))
                {
                    throw new ArgumentException(string.Format("There is no deployment called '{0}' to cancel", deploymentName));
                }
                else
                {
                    throw new ArgumentException(string.Format("There are no running deployemnts under resource group '{0}'", resourceGroup));
                }
            }
            else if (deployments.Count == 1)
            {
                ResourceManagementClient.Deployments.Cancel(resourceGroup, deployments.First().DeploymentName);
            }
            else
            {
                throw new ArgumentException("There are more than one running deployment please specify one");
            }
        }

        /// <summary>
        /// Validates a given deployment.
        /// </summary>
        /// <param name="parameters">The deployment create options</param>
        /// <returns>True if valid, false otherwise.</returns>
        public virtual List<PSResourceManagerError> ValidatePSResourceGroupDeployment(ValidatePSResourceGroupDeploymentParameters parameters)
        {
            BasicDeployment deployment = CreateBasicDeployment(parameters);
            List<ResourceManagementError> errors = CheckBasicDeploymentErrors(parameters.ResourceGroupName, Guid.NewGuid().ToString(), deployment);

            if (errors.Count == 0)
            {
                WriteVerbose(ProjectResources.TemplateValid);
            }
            return errors.Select(e => e.ToPSResourceManagerError()).ToList();
        }

        /// <summary>
        /// Gets available locations for the specified resource type.
        /// </summary>
        /// <param name="resourceTypes">The resource types</param>
        /// <returns>Mapping between each resource type and its available locations</returns>
        public virtual List<PSResourceProviderType> GetLocations(params string[] resourceTypes)
        {
            if (resourceTypes == null)
            {
                resourceTypes = new string[0];
            }
            List<string> providerNames = resourceTypes.Select(r => r.Split('/').First()).ToList();
            List<PSResourceProviderType> result = new List<PSResourceProviderType>();
            List<Provider> providers = new List<Provider>();

            if (resourceTypes.Length == 0 || resourceTypes.Any(r => r.Equals(ResourcesClient.ResourceGroupTypeName, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(new ProviderResourceType()
                {
                    Name = ResourcesClient.ResourceGroupTypeName,
                    Locations = ResourcesClient.KnownLocations
                }.ToPSResourceProviderType(null));
            }

            if (resourceTypes.Length > 0)
            {
                providers.AddRange(ListResourceProviders()
                    .Where(p => providerNames.Any(pn => pn.Equals(p.Namespace, StringComparison.OrdinalIgnoreCase))));
            }
            else
            {
                providers.AddRange(ListResourceProviders());
            }

            result.AddRange(providers.SelectMany(p => p.ResourceTypes
                .Select(r => r.ToPSResourceProviderType(p.Namespace)))
                .Where(r => r.Locations != null && r.Locations.Count > 0));

            return result;
        }
    }
}

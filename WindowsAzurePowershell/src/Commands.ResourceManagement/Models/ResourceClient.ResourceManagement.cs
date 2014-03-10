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

            bool resourceExists = ResourceManagementClient.Resources.CheckExistence(parameters.ResourceGroupName, resourceIdentity).Exists;
            
            if (resourceExists)
            {
                throw new ArgumentException(Resources.ResourceAlreadyExists);
            }

            if (ResourceManagementClient.ResourceGroups.CheckExistence(parameters.ResourceGroupName).Exists)
            {
                WriteProgress(string.Format("Resource group \"{0}\" is found.", parameters.ResourceGroupName));
            }
            else
            {
                throw new ArgumentException(Resources.ResourceGroupDoesntExists);
            }

            WriteProgress(string.Format("Creating resource \"{0}\" started.", parameters.Name));
            
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
                WriteProgress(string.Format("Creating resource \"{0}\" complete.", parameters.Name));
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
                throw new ArgumentException(Resources.ResourceDoesntExists);
            }

            string newProperty = SerializeHashtable(parameters.PropertyObject,
                                                    addValueLayer: false);

            if (parameters.Mode == SetResourceMode.Update)
            {
                newProperty = JsonUtilities.Patch(getResource.Resource.Properties, newProperty);
            }
            ResourceManagementClient.Resources.CreateOrUpdate(parameters.ResourceGroupName, resourceIdentity,
                        new ResourceCreateOrUpdateParameters
                            {
                                ValidationMode = ResourceValidationMode.NameValidation,
                                    Resource = new BasicResource
                                        {
                                            Location = getResource.Resource.Location,
                                            Properties = newProperty
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
        public virtual List<PSResource> FilterPSResources(BasePSResourceParameters parameters)
        {
            List<PSResource> resources = new List<PSResource>();

            if (!string.IsNullOrEmpty(parameters.Name))
            {
                ResourceIdentity resourceIdentity = parameters.ToResourceIdentity();

                ResourceGetResult getResult = ResourceManagementClient.Resources.Get(parameters.ResourceGroupName, resourceIdentity);

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
            bool createDeployment = !string.IsNullOrEmpty(parameters.GalleryTemplateName) || !string.IsNullOrEmpty(parameters.TemplateFile);

            if (ResourceManagementClient.ResourceGroups.CheckExistence(parameters.ResourceGroupName).Exists)
            {
                throw new ArgumentException(Resources.ResourceGroupAlreadyExists);
            }

            if (createDeployment)
            {
                ValidateStorageAccount(parameters);
            }

            ResourceGroup resourceGroup = CreateResourceGroup(parameters.ResourceGroupName, parameters.Location);

            if (createDeployment)
            {
                CreatePSResourceGroupDeployment(parameters);
            }

            return resourceGroup.ToPSResourceGroup(this);
        }

        /// <summary>
        /// Verify Storage account has been specified. 
        /// </summary>
        /// <param name="parameters"></param>
        private void ValidateStorageAccount(CreatePSResourceGroupParameters parameters)
        {
            GetStorageAccountName(parameters.StorageAccountName);
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
            RegisterResourceProviders();

            parameters.Name = string.IsNullOrEmpty(parameters.Name) ? Guid.NewGuid().ToString() : parameters.Name;
            BasicDeployment deployment = CreateBasicDeployment(parameters);
            List<ResourceManagementError> errors = CheckBasicDeploymentErrors(parameters.ResourceGroupName, deployment);

            if (errors.Count != 0)
            {
                int counter = 1;
                string errorFormat = "Error {0}: Code={1}; Message={2}; Target={3}\r\n";
                StringBuilder errorsString = new StringBuilder();
                errors.ForEach(e => errorsString.AppendFormat(errorFormat, counter++, e.Code, e.Message, e.Target));
                throw new ArgumentException(errors.ToString());
            }

            DeploymentOperationsCreateResult result = ResourceManagementClient.Deployments.CreateOrUpdate(parameters.ResourceGroupName, parameters.Name, deployment);
            WriteProgress(string.Format("Create template deployment '{0}' using template {1}.", parameters.Name, deployment.TemplateLink.Uri));
            ProvisionDeploymentStatus(parameters.ResourceGroupName, parameters.Name);

            return result.ToPSResourceGroupDeployment();
        }

        /// <summary>
        /// Gets the parameters for a given gallery template.
        /// </summary>
        /// <param name="templateName">The gallery template name</param>
        /// <param name="parameters">The existing PowerShell cmdlet parameters</param>
        /// <param name="parameterSetNames">The parameters set which the dynamic parameters should be added to</param>
        /// <returns>The template parameters</returns>
        public virtual RuntimeDefinedParameterDictionary GetTemplateParameters(string templateName, string[] parameters, params string[] parameterSetNames)
        {
            RuntimeDefinedParameterDictionary dynamicParameters = new RuntimeDefinedParameterDictionary();
            string templateContent = null;
            
            if (Uri.IsWellFormedUriString(templateName, UriKind.Absolute))
            {
                templateContent = GeneralUtilities.DownloadFile(templateName);
            }
            else if (File.Exists(templateName))
            {
                templateContent = File.ReadAllText(templateName);
            }
            else
            {
                templateContent = GeneralUtilities.DownloadFile(GetGalleryTemplateFile(templateName));
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
        /// Deletes a given resource
        /// </summary>
        /// <param name="parameters">The resource identification</param>
        public virtual void DeleteResource(BasePSResourceParameters parameters)
        {
            ResourceIdentity resourceIdentity = parameters.ToResourceIdentity();

            if (!ResourceManagementClient.Resources.CheckExistence(parameters.ResourceGroupName, resourceIdentity).Exists)
            {
                throw new ArgumentException(Resources.ResourceDoesntExists);
            }

            ResourceManagementClient.Resources.Delete(parameters.ResourceGroupName, resourceIdentity);
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
                deployments.Add(ResourceManagementClient.Deployments.Get(resourceGroup, name).ToPSResourceGroupDeployment());
            }
            else if (!string.IsNullOrEmpty(resourceGroup))
            {
                DeploymentListParameters parameters = new DeploymentListParameters();

                if (provisioningStates.Count == 1)
                {
                    parameters.ProvisioningState = provisioningStates.First();
                }

                DeploymentListResult result = ResourceManagementClient.Deployments.List(resourceGroup, parameters);

                deployments.AddRange(result.Deployments.Select(d => d.ToPSResourceGroupDeployment()));

                while (!string.IsNullOrEmpty(result.NextLink))
                {
                    result = ResourceManagementClient.Deployments.ListNext(result.NextLink);
                    deployments.AddRange(result.Deployments.Select(d => d.ToPSResourceGroupDeployment()));
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
        public virtual List<ResourceManagementError> ValidatePSResourceGroupDeployment(ValidatePSResourceGroupDeploymentParameters parameters)
        {
            List<ResourceManagementError> errors = new List<ResourceManagementError>();

            BasicDeployment deployment = CreateBasicDeployment(parameters);
            errors.AddRange(CheckBasicDeploymentErrors(parameters.ResourceGroupName, deployment));

            return errors;
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

            result.AddRange(providers.SelectMany(p => p.ResourceTypes.Select(r => r.ToPSResourceProviderType(p.Namespace))));

            return result;
        }
    }
}

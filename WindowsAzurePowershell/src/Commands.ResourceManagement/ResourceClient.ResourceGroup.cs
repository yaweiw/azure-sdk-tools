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

namespace Microsoft.Azure.Commands.ResourceManagement
{
    public partial class ResourceClient
    {
        private static string DeploymentTemplateStorageContainerName = "deployment-templates";

        public ResourceGroup CreateOrUpdateResourceGroup(CreateResourceGroupParameters parameters)
        {
            // Validate that parameter group doesn't already exist
            if (ResourceManagementClient.ResourceGroups.Exists(parameters.Name).Exists)
            {
                throw new ArgumentException(Resources.ResourceGroupAlreadyExists);
            }

            // Create resource group and deploy a template from file
            if (parameters.TemplateFile != null)
            {
                Uri templateFilePath = null;

                // If local file - upload it to storage, if remote file - pass the string over as-is
                if (parameters.TemplateFile.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    parameters.TemplateFile.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    templateFilePath = new Uri(parameters.TemplateFile);
                }
                else
                {
                    var storageAccountName = GetStorageAccountNameOrThrowException(parameters);
                    templateFilePath = StorageClientWrapper.UploadFileToBlob(new BlobUploadParameters
                        {
                            StorageName = storageAccountName,
                            FileLocalPath = parameters.TemplateFile,
                            FileRemoteName = Path.GetFileNameWithoutExtension(parameters.TemplateFile),
                            OverrideIfExists = true,
                            ContainerPublic = true,
                            ContainerName = DeploymentTemplateStorageContainerName
                        });
                }

                var resourceGroupCreateOrUpdateResult =
                    ResourceManagementClient.ResourceGroups.CreateOrUpdate(parameters.Name,
                    new BasicResourceGroup
                        {
                            Location = parameters.Location
                        });

                var templateDeployment = new BasicDeployment()
                    {
                        Mode = DeploymentMode.Incremental,
                        TemplateLink = new TemplateLink
                            {
                                Uri = templateFilePath
                            }
                    };

                if (parameters.ParameterObject != null)
                {
                    var paramDictionary = parameters.ParameterObject.ToMultidimentionalDictionary();
                    var serializedParamDictionary = JsonConvert.SerializeObject(paramDictionary, new JsonSerializerSettings
                        {
                            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                            TypeNameHandling = TypeNameHandling.None
                        });
                    templateDeployment.Parameters = serializedParamDictionary;
                }
                
                ResourceManagementClient.Deployments.Create(parameters.Name,
                                                Path.GetFileName(templateFilePath.ToString()),
                                                templateDeployment);

                return resourceGroupCreateOrUpdateResult.ResourceGroup;
            }
                // Create just resource group
            else
            {
                return CreateResourceGroup(parameters).ResourceGroup;
            }
        }

        private string GetStorageAccountNameOrThrowException(CreateResourceGroupParameters parameters)
        {
            string subscriptionStorageAccountName = null;
            if (WindowsAzureProfile.Instance.CurrentSubscription != null)
            {
                subscriptionStorageAccountName =
                    WindowsAzureProfile.Instance.CurrentSubscription.CurrentStorageAccountName;
            }
            var storageName = parameters.StorageAccountName ?? subscriptionStorageAccountName;
            if (string.IsNullOrEmpty(storageName))
            {
                throw new ArgumentException(Resources.StorageAccountNameNeedsToBeSpecified);
            }
            return storageName;
        }

        private ResourceGroupCreateOrUpdateResult CreateResourceGroup(CreateResourceGroupParameters parameters)
        {
            var result = ResourceManagementClient.ResourceGroups.CreateOrUpdate(parameters.Name,
                new BasicResourceGroup
                    {
                        Location = parameters.Location
                    });
            return result;
        }

        public IEnumerable<ResourceGroup> GetResourceGroups(GetAzureResourceGroup parameters)
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

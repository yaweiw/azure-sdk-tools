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

namespace Microsoft.Azure.Commands.ResourceManagement
{
    public partial class ResourceClient
    {
        private static string DeploymentTemplateStorageContainerName = "deployment-templates";

        private string GetDeploymentParameters(PSCreateResourceGroupParameters parameters)
        {
            string deploymentParameters = null;

            if (parameters.ParameterObject != null)
            {
                Dictionary<string, object> parametersDictionary = parameters.ParameterObject.ToMultidimentionalDictionary();
                deploymentParameters = JsonConvert.SerializeObject(parametersDictionary, new JsonSerializerSettings
                {
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                    TypeNameHandling = TypeNameHandling.None
                });

            }
            else
            {
                deploymentParameters = File.ReadAllText(parameters.ParameterFile);
            }

            return deploymentParameters;
        }

        private Uri GetTemplateUri(PSCreateResourceGroupParameters parameters)
        {
            Uri templateFileUri;

            if (!string.IsNullOrEmpty(parameters.TemplateFile))
            {
                if (Uri.IsWellFormedUriString(parameters.TemplateFile, UriKind.Absolute))
                {
                    templateFileUri = new Uri(parameters.TemplateFile);
                }
                else
                {
                    string storageAccountName = GetStorageAccountName(parameters);
                    templateFileUri = StorageClientWrapper.UploadFileToBlob(new BlobUploadParameters
                    {
                        StorageName = storageAccountName,
                        FileLocalPath = parameters.TemplateFile,
                        FileRemoteName = Path.GetFileName(parameters.TemplateFile),
                        OverrideIfExists = true,
                        ContainerPublic = true,
                        ContainerName = DeploymentTemplateStorageContainerName
                    });
                }
            }
            else
            {
                // To do: get the actual GalleryTemplateName uri
                // templateFileUri = ResourceManagementClient.Gallary.GetTemplateFile(parameters.GalleryTemplateName).TemplateFileUri
                templateFileUri = new Uri("http://microsoft.com");
            }

            return templateFileUri;
        }

        private string GetStorageAccountName(PSCreateResourceGroupParameters parameters)
        {
            string currentStorageName = null;
            if (WindowsAzureProfile.Instance.CurrentSubscription != null)
            {
                currentStorageName = WindowsAzureProfile.Instance.CurrentSubscription.CurrentStorageAccountName;
            }

            string storageName = parameters.StorageAccountName ?? currentStorageName;

            if (string.IsNullOrEmpty(storageName))
            {
                throw new ArgumentException(Resources.StorageAccountNameNeedsToBeSpecified);
            }

            return storageName;
        }

        private ContentHash GetTemplateContentHash(PSCreateResourceGroupParameters parameters)
        {
            ContentHash contentHash = null;

            if (!string.IsNullOrEmpty(parameters.TemplateHash))
            {
                contentHash.Value = parameters.TemplateHash;
                contentHash.Algorithm = string.IsNullOrEmpty(parameters.TemplateHashAlgorithm) ? ContentHashAlgorithm.Sha256 :
                    (ContentHashAlgorithm)Enum.Parse(typeof(ContentHashAlgorithm), parameters.TemplateHashAlgorithm);
            }

            return contentHash;
        }

        private ResourceGroup CreateResourceGroup(string name, string location)
        {
            var result = ResourceManagementClient.ResourceGroups.CreateOrUpdate(name,
                new BasicResourceGroup
                {
                    Location = location
                });

            return result.ResourceGroup;
        }

        public PSResourceGroup CreatePSResourceGroup(PSCreateResourceGroupParameters parameters)
        {
            // Validate that parameter group doesn't already exist
            if (ResourceManagementClient.ResourceGroups.Exists(parameters.Name).Exists)
            {
                throw new ArgumentException(Resources.ResourceGroupAlreadyExists);
            }

            ResourceGroup resourceGroup = CreateResourceGroup(parameters.Name, parameters.Location);
            DeploymentProperties properties = null;
            bool createDeployment = !string.IsNullOrEmpty(parameters.GalleryTemplateName) || !string.IsNullOrEmpty(parameters.TemplateFile);

            if (createDeployment)
            {
                BasicDeployment deployment = new BasicDeployment()
                {
                    Mode = DeploymentMode.Incremental,
                    TemplateLink = new TemplateLink() {
                        Uri = GetTemplateUri(parameters),
                        ContentVersion = parameters.TemplateVersion,
                        ContentHash = GetTemplateContentHash(parameters)
                    },
                    Parameters = GetDeploymentParameters(parameters)
                };
                
                properties = ResourceManagementClient.Deployments.Create(parameters.Name, parameters.DeploymentName, deployment).Properties;
            }

            List<Resource> resources = ResourceManagementClient.Resources.ListForResourceGroup(resourceGroup.Name, new ResourceListParameters()).Resources.ToList();
            return new PSResourceGroup() { Name = resourceGroup.Name, Location = resourceGroup.Location, Resources = resources };
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

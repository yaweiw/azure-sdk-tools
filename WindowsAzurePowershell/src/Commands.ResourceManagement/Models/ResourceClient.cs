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
using Microsoft.Azure.Gallery;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Microsoft.WindowsAzure.Management.Storage;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Text;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public partial class ResourcesClient
    {
        public IResourceManagementClient ResourceManagementClient { get; set; }
        public IStorageClientWrapper StorageClientWrapper { get; set; }

        public IGalleryClient GalleryClient { get; set; }

        /// <summary>
        /// Creates new ResourceManagementClient
        /// </summary>
        /// <param name="subscription">Subscription containing resources to manipulate</param>
        public ResourcesClient(WindowsAzureSubscription subscription) : this (
            subscription.CreateCloudServiceClient<ResourceManagementClient>(),
            new StorageClientWrapper(subscription.CreateClient<StorageManagementClient>()),
            subscription.CreateGalleryClient<GalleryClient>())
        {
            
        }

        /// <summary>
        /// Creates new ResourcesClient instance
        /// </summary>
        /// <param name="resourceManagementClient">The IResourceManagementClient instance</param>
        /// <param name="storageClientWrapper">The IStorageClientWrapper instance</param>
        /// <param name="galleryClient">The IGalleryClient instance</param>
        public ResourcesClient(
            IResourceManagementClient resourceManagementClient,
            IStorageClientWrapper storageClientWrapper,
            IGalleryClient galleryClient)
        {
            ResourceManagementClient = resourceManagementClient;
            StorageClientWrapper = storageClientWrapper;
            GalleryClient = galleryClient;
        }

        /// <summary>
        /// Parameterless constructor for mocking
        /// </summary>
        public ResourcesClient()
        {

        }

        private static string DeploymentTemplateStorageContainerName = "deployment-templates";

        private string GetDeploymentParameters(string parameterFile, Hashtable parameterObject)
        {
            string deploymentParameters = null;

            if (parameterObject != null)
            {
                Dictionary<string, object> parametersDictionary = parameterObject.ToMultidimentionalDictionary();
                deploymentParameters = JsonConvert.SerializeObject(parametersDictionary, new JsonSerializerSettings
                {
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                    TypeNameHandling = TypeNameHandling.None
                });

            }
            else
            {
                deploymentParameters = File.ReadAllText(parameterFile);
            }

            return deploymentParameters;
        }

        private Uri GetTemplateUri(string templateFile, string galleryTemplateName, string storageAccountName)
        {
            Uri templateFileUri;

            if (!string.IsNullOrEmpty(templateFile))
            {
                if (Uri.IsWellFormedUriString(templateFile, UriKind.Absolute))
                {
                    templateFileUri = new Uri(templateFile);
                }
                else
                {
                    storageAccountName = GetStorageAccountName(storageAccountName);
                    templateFileUri = StorageClientWrapper.UploadFileToBlob(new BlobUploadParameters
                    {
                        StorageName = storageAccountName,
                        FileLocalPath = templateFile,
                        FileRemoteName = Path.GetFileName(templateFile),
                        OverrideIfExists = true,
                        ContainerPublic = true,
                        ContainerName = DeploymentTemplateStorageContainerName
                    });
                }
            }
            else
            {
                templateFileUri = GetGalleryTemplateFile(galleryTemplateName);
            }

            return templateFileUri;
        }

        private string GetStorageAccountName(string storageAccountName)
        {
            string currentStorageName = null;
            if (WindowsAzureProfile.Instance.CurrentSubscription != null)
            {
                currentStorageName = WindowsAzureProfile.Instance.CurrentSubscription.CurrentStorageAccountName;
            }

            string storageName = string.IsNullOrEmpty(storageAccountName) ? currentStorageName : storageAccountName;

            if (string.IsNullOrEmpty(storageName))
            {
                throw new ArgumentException(Resources.StorageAccountNameNeedsToBeSpecified);
            }

            return storageName;
        }

        private ContentHash GetTemplateContentHash(string templateHash, string templateHashAlgorithm)
        {
            ContentHash contentHash = null;

            if (!string.IsNullOrEmpty(templateHash))
            {
                contentHash = new ContentHash();
                contentHash.Value = templateHash;
                contentHash.Algorithm = string.IsNullOrEmpty(templateHashAlgorithm) ? ContentHashAlgorithm.Sha256 :
                    (ContentHashAlgorithm)Enum.Parse(typeof(ContentHashAlgorithm), templateHashAlgorithm);
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

        private Type GetParameterType(string resourceParameterType)
        {
            Debug.Assert(!string.IsNullOrEmpty(resourceParameterType));
            const string stringType = "string";
            const string intType = "int";
            const string secureStringType = "SecureString";
            Type typeObject = typeof(object);

            if (resourceParameterType.Equals(stringType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(string);
            }
            else if (resourceParameterType.Equals(intType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(int);
            }
            else if (resourceParameterType.Equals(secureStringType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(SecureString);
            }

            return typeObject;
        }

        private string ConstructResourcesTable(List<Resource> resources)
        {
            StringBuilder resourcesTable = new StringBuilder();

            if (resources.Count > 0)
            {
                string rowFormat = "{0, -15}  {1, -25}  {2, -10}\r\n";
                resourcesTable.AppendLine();
                resourcesTable.AppendFormat(rowFormat, "Name", "Type", "Location");
                resourcesTable.AppendFormat(rowFormat, GenerateSeparator(15, "="), GenerateSeparator(25, "="), GenerateSeparator(10, "="));

                foreach (Resource resource in resources)
                {
                    resourcesTable.AppendFormat(rowFormat, resource.Name, resource.Type, resource.Location);
                }
            }

            return resourcesTable.ToString();
        }

        private string GenerateSeparator(int amount, string separator)
        {
            string result = string.Empty;
            while (amount-- != 0) result += separator;
            return result;
        }
    }
}

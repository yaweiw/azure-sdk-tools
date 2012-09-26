// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.CloudService.Services
{
    using System;
    using System.Globalization;
    using System.IO;
    using StorageClient;
    using System.Collections.Generic;

    public static class AzureBlob
    {
        public static readonly string BlobEndpointTemplate = "https://{0}.blob.core.windows.net/";
        private const string ContainerName = "azpsnode122011";

        /// <summary>
        /// Checks if a container exists.
        /// </summary>
        /// <param name="container">Container to check for</param>
        /// <returns>Flag indicating the existence of the container</returns>
        private static bool Exists(CloudBlobContainer container)
        {
            try
            {
                container.FetchAttributes();
                return true;
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                {
                    return false;
                }

                throw;
            }
        }

        private static CloudBlobContainer GetCloudBlobContainer(IServiceManagement channel, string storageName, string subscriptionId)
        {
            StorageService storageService = channel.GetStorageKeys(subscriptionId, storageName);
            string storageKey = storageService.StorageServiceKeys.Primary;
            string baseAddress = string.Format(CultureInfo.InvariantCulture, BlobEndpointTemplate, storageName);
            StorageCredentialsAccountAndKey credentials = new StorageCredentialsAccountAndKey(storageName, storageKey);
            CloudBlobClient client = new CloudBlobClient(baseAddress, credentials);
            
            return client.GetContainerReference(ContainerName);
        }

        /// <summary>
        /// Deletes all the blobs in a deployment container.
        /// </summary>
        /// <param name="channel">The HTTP channel</param>
        /// <param name="storageName">The service storage account name</param>
        /// <param name="subscriptionId">The user subscription id</param>
        public static void CleanContainer(IServiceManagement channel, string storageName, string subscriptionId)
        {
            CloudBlobContainer container = GetCloudBlobContainer(channel, storageName, subscriptionId);
            List<IListBlobItem> blobs = new List<IListBlobItem>(container.ListBlobs());

            foreach (IListBlobItem blob in blobs)
            {
                CloudBlob cloudBlob = container.GetBlobReference(blob.Uri.AbsoluteUri);
                cloudBlob.Delete();
            }
        }

        /// <summary>
        /// Uploads the deployment package to the specified container. The package name is a generated GUID.
        /// </summary>
        /// <param name="channel">The HTTP channel</param>
        /// <param name="storageName">The associated service storage account name</param>
        /// <param name="subscriptionId">The service subscription id</param>
        /// <param name="packagePath">The path of the deployment package</param>
        /// <param name="blobRequestOptions">The blob request options</param>
        /// <returns>The blob URI of the uploaded package</returns>
        public static Uri UploadPackageToBlob(IServiceManagement channel, string storageName, string subscriptionId, string packagePath, BlobRequestOptions blobRequestOptions)
        {
            StorageService storageService = channel.GetStorageKeys(subscriptionId, storageName);
            string storageKey = storageService.StorageServiceKeys.Primary;

            return UploadFile(storageName, storageKey, packagePath, blobRequestOptions);
        }

        /// <summary>
        /// Uploads a file to azure store.
        /// </summary>
        /// <param name="storageName">Store which file will be uploaded to</param>
        /// <param name="storageKey">Store access key</param>
        /// <param name="filePath">Path to file which will be uploaded</param>
        /// <param name="blobRequestOptions">The request options for blob uploading.</param>
        /// <returns>Uri which holds locates the uploaded file</returns>
        /// <remarks>The uploaded file name will be guid</remarks>
        public static Uri UploadFile(string storageName, string storageKey, string filePath, BlobRequestOptions blobRequestOptions)
        {
            var baseAddress = string.Format(CultureInfo.InvariantCulture, BlobEndpointTemplate, storageName);
            var credentials = new StorageCredentialsAccountAndKey(storageName, storageKey);
            var client = new CloudBlobClient(baseAddress, credentials);
            string blobName = Guid.NewGuid().ToString();

            CloudBlobContainer container = client.GetContainerReference(ContainerName);
            container.CreateIfNotExist();
            CloudBlob blob = container.GetBlobReference(blobName);

            using (FileStream readStream = File.OpenRead(filePath))
            {
                blob.UploadFromStream(readStream, blobRequestOptions);
            }

            return new Uri(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}{2}{3}",
                    client.BaseUri,
                    ContainerName,
                    client.DefaultDelimiter,
                    blobName));
        }
    }
}
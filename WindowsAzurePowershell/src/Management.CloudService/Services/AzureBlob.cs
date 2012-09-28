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

    public static class AzureBlob
    {
        public static readonly string BlobEndpointTemplate = "https://{0}.blob.core.windows.net/";
        private const string ContainerName = "azpsnode122011";

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

        /// <summary>
        /// Removes uploaded package from storage account.
        /// </summary>
        /// <param name="channel">Channel to use for REST calls</param>
        /// <param name="storageName">Store which has the package</param>
        /// <param name="subscriptionId">Subscription which has the store</param>
        public static void RemovePackageFromBlob(IServiceManagement channel, string storageName, string subscriptionId)
        {
            StorageService storageService = channel.GetStorageKeys(subscriptionId, storageName);
            string storageKey = storageService.StorageServiceKeys.Primary;

            RemoveFile(storageName, storageKey);
        }

        /// <summary>
        /// Removes file from storage account
        /// </summary>
        /// <param name="storageName">Store which has file to remove</param>
        /// <param name="storageKey">Store access key</param>
        private static void RemoveFile(string storageName, string storageKey)
        {
            var baseAddress = string.Format(CultureInfo.InvariantCulture, BlobEndpointTemplate, storageName);
            var credentials = new StorageCredentialsAccountAndKey(storageName, storageKey);
            var client = new CloudBlobClient(baseAddress, credentials);

            CloudBlobContainer container = client.GetContainerReference(ContainerName);
            if (Exists(container))
            {
                container.Delete();
            }
        }

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
    }
}
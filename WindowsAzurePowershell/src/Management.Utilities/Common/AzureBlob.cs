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

namespace Microsoft.WindowsAzure.Management.Utilities.Common
{
    using System;
    using System.Globalization;
    using System.IO;
    using ServiceManagement;
    using Storage;
    using Storage.Auth;
    using Storage.Blob;


    public static class AzureBlob
    {
        private const string BlobEndpointIdentifier = ".blob.";
        private const string ContainerName = "mydeployments";

        public static Uri UploadPackageToBlob(IServiceManagement channel, string storageName, string subscriptionId, string packagePath, BlobRequestOptions blobRequestOptions)
        {
            string storageKey;
            string blobEndpointUri;

            StorageService storageService = channel.GetStorageKeys(subscriptionId, storageName);
            storageKey = storageService.StorageServiceKeys.Primary;
            storageService = channel.GetStorageService(subscriptionId, storageName);
            blobEndpointUri = storageService.StorageServiceProperties.Endpoints[0];

            return UploadFile(storageName, blobEndpointUri, storageKey, packagePath, blobRequestOptions);
        }

        public static void DeletePackageFromBlob(IServiceManagement channel, string storageName, string subscriptionId, Uri packageUri)
        {
            var storageService = channel.GetStorageKeys(subscriptionId, storageName);
            var storageKey = storageService.StorageServiceKeys.Primary;
            storageService = channel.GetStorageService(subscriptionId, storageName);
            var blobStorageEndpoint = new Uri(storageService.StorageServiceProperties.Endpoints.Find(p => p.Contains(BlobEndpointIdentifier)));
            var credentials = new StorageCredentials(storageName, storageKey);
            var client = new CloudBlobClient(blobStorageEndpoint, credentials);
            ICloudBlob blob = client.GetBlobReferenceFromServer(packageUri);
            blob.DeleteIfExists();
        }

        public static Uri UploadFile(string storageName, string blobEndpointUri, string storageKey, string filePath, BlobRequestOptions blobRequestOptions)
        {
            StorageCredentials credentials = new StorageCredentials(storageName, storageKey);
            CloudBlobClient client = new CloudBlobClient(new Uri(blobEndpointUri), credentials);
            string blobName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1}",
                DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture),
                Path.GetFileName(filePath));

            CloudBlobContainer container = client.GetContainerReference(ContainerName);
            container.CreateIfNotExists();
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            using (FileStream readStream = File.OpenRead(filePath))
            {
                blob.UploadFromStream(readStream, AccessCondition.GenerateEmptyCondition(), blobRequestOptions);
            }

            return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}", client.BaseUri, ContainerName, client.DefaultDelimiter, blobName));
        }
    }
}
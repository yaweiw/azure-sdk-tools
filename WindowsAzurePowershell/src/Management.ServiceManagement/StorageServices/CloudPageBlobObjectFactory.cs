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


namespace Microsoft.WindowsAzure.Management.ServiceManagement.StorageServices
{
    using System;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Sync.Download;
    using Microsoft.WindowsAzure.Sync.Upload;

    public class CloudPageBlobObjectFactory : ICloudPageBlobObjectFactory
    {
        private IServiceManagement channel;
        private readonly string subscriptionId;
        private readonly TimeSpan delayBetweenRetries = TimeSpan.FromSeconds(10);
        private TimeSpan operationTimeout;

        public CloudPageBlobObjectFactory(IServiceManagement channel, string subscriptionId, TimeSpan operationTimeout)
        {
            this.operationTimeout = operationTimeout;
            this.channel = channel;
            this.subscriptionId = subscriptionId;
        }

        public CloudPageBlob Create(BlobUri destination)
        {
            if(String.IsNullOrEmpty(destination.QueryString))
            {
                StorageService sService = this.channel.GetStorageKeys(subscriptionId, destination.StorageAccountName);
                return new CloudPageBlob(new Uri(destination.BlobPath), new StorageCredentials(destination.StorageAccountName, sService.StorageServiceKeys.Primary));
            }
            return new CloudPageBlob(new Uri(destination.BlobPath), new StorageCredentials(destination.Uri.Query));
        }

        public bool CreateContainer(BlobUri destination)
        {
            if (String.IsNullOrEmpty(destination.Uri.Query))
            {
                var destinationBlob = Create(destination);
                return destinationBlob.Container.CreateIfNotExists(this.CreateRequestOptions());
            }
            return true;
        }

        public BlobRequestOptions CreateRequestOptions()
        {
            return new BlobRequestOptions
                       {
                           ServerTimeout = this.operationTimeout,
                           RetryPolicy = new LinearRetry(delayBetweenRetries, 5)
                       };
        }
    }
}
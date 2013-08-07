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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using ServiceManagement;

    public class SubscriptionData
    {
        public string SubscriptionName { get; set; }

        public string SubscriptionId { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public string ServiceEndpoint { get; set; }

        public string SqlAzureServiceEndpoint { get; set; }

        public string CurrentStorageAccount { get; set; }

        public bool IsDefault { get; set; }

        public CloudStorageAccount CurrentCloudStorageAccount { get; set; }

        /// <summary>
        /// Gets current storage account using current subscription.
        /// </summary>
        /// <returns>The current storage account</returns>
        public CloudStorageAccount GetCurrentStorageAccount()
        {
            Binding serviceBinding = ConfigurationConstants.WebHttpBinding(0);
            string serviceEndpoint = string.IsNullOrEmpty(ServiceEndpoint) ?
                ConfigurationConstants.ServiceManagementEndpoint :
                ServiceEndpoint;
            IServiceManagement channel = ChannelHelper.CreateServiceManagementChannel<IServiceManagement>(
                serviceBinding,
                new Uri(ServiceEndpoint),
                Certificate);

            return GetCurrentStorageAccount(channel);
        }

        public CloudStorageAccount GetCurrentStorageAccount(IServiceManagement channel)
        {
            return GetCurrentCloudStorageAccount(channel, this);
        }

        public static CloudStorageAccount GetCurrentCloudStorageAccount(
            IServiceManagement channel,
            SubscriptionData subscriptionData)
        {
            if (string.IsNullOrEmpty(subscriptionData.CurrentStorageAccount))
            {
                return null;
            }

            if (subscriptionData.CurrentCloudStorageAccount != null)
            {
                return subscriptionData.CurrentCloudStorageAccount;
            }

            SetCurrentCloudStorageAccount(channel, subscriptionData);
            
            return subscriptionData.CurrentCloudStorageAccount;
        }

        private static void SetCurrentCloudStorageAccount(IServiceManagement channel, SubscriptionData subscriptionData)
        {
            CloudStorageAccount currentStorage = null;
            using (new OperationContextScope((IContextChannel)channel))
            {
                var storageService = channel.GetStorageService(
                    subscriptionData.SubscriptionId,
                    subscriptionData.CurrentStorageAccount);
                var storageServiceKeys = channel.GetStorageKeys(
                    subscriptionData.SubscriptionId,
                    subscriptionData.CurrentStorageAccount);
                
                if (storageService != null && storageServiceKeys != null)
                {
                    currentStorage = new CloudStorageAccount(new StorageCredentials(
                        storageService.ServiceName,
                        storageServiceKeys.StorageServiceKeys.Primary),
                        General.CreateHttpsEndpoint(storageService.StorageServiceProperties.Endpoints[0]),
                        General.CreateHttpsEndpoint(storageService.StorageServiceProperties.Endpoints[1]),
                        General.CreateHttpsEndpoint(storageService.StorageServiceProperties.Endpoints[2]));
                }
            }

            subscriptionData.CurrentCloudStorageAccount = currentStorage;
        }

        public void NullCurrentStorageAccount()
        {
            CurrentCloudStorageAccount = null;
        }
    }
}
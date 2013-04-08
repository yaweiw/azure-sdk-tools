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
    using System.ServiceModel;
    using ServiceManagement;
    using Storage;

    public class CloudStorageAccountFactory
    {
        public static CloudStorageAccount GetCurrentCloudStorageAccount(IServiceManagement channel, SubscriptionData subscriptionData)
        {
            if (String.IsNullOrEmpty(subscriptionData.CurrentStorageAccount))
            {
                return null;
            }

            if (subscriptionData.CurrentCloudStorageAccount != null)
            {
                return subscriptionData.CurrentCloudStorageAccount;
            }

            CloudStorageAccount currentStorage = null;
            using (new OperationContextScope(channel.ToContextChannel()))
            {
                var storageService = channel.GetStorageService(subscriptionData.SubscriptionId, subscriptionData.CurrentStorageAccount);
                var storageServiceKeys = channel.GetStorageKeys(subscriptionData.SubscriptionId, subscriptionData.CurrentStorageAccount);
                if (storageService != null && storageServiceKeys != null)
                {
                    string connectionString = General.BuildConnectionString("https", storageService.ServiceName, storageServiceKeys.StorageServiceKeys.Primary, storageService.StorageServiceProperties.Endpoints[0].Replace("http://", "https://"), storageService.StorageServiceProperties.Endpoints[2].Replace("http://", "https://"), storageService.StorageServiceProperties.Endpoints[1].Replace("http://", "https://"));
                    currentStorage = CloudStorageAccount.Parse(connectionString);
                }
            }

            subscriptionData.CurrentCloudStorageAccount = currentStorage;
            return currentStorage;
        }
    }
}
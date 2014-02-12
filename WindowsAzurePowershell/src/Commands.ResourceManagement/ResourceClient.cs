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

using Microsoft.Azure.Management.Resources;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Microsoft.WindowsAzure.Management.Storage;

namespace Microsoft.Azure.Commands.ResourceManagement
{
    public partial class ResourceClient
    {
        public IResourceManagementClient ResourceManagementClient { get; set; }
        public IStorageClientWrapper StorageClientWrapper { get; set; }

        /// <summary>
        /// Creates new ResourceManagementClient
        /// </summary>
        /// <param name="subscription">Subscription containing resources to manipulate</param>
        public ResourceClient(WindowsAzureSubscription subscription)
        {
            ResourceManagementClient = subscription.CreateCloudServiceClient<ResourceManagementClient>();
            var storageManagementClient = subscription.CreateClient<StorageManagementClient>();
            StorageClientWrapper = new StorageClientWrapper(storageManagementClient);
        }
    }
}

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

namespace Microsoft.WindowsAzure.Commands.Utilities.Store.Contract
{
    using ResourceModel;

    public static class StoreExtensionMethods
    {
        public static CloudServiceList ListCloudServices(this IStoreManagement proxy, string subscriptionId)
        {
            return proxy.EndListCloudServices(proxy.BeginListCloudServices(subscriptionId, null, null));
        }

        public static void DeleteResource(
            this IStoreManagement proxy,
            string subscriptionId,
            string cloudServiceName,
            string resourceProviderNamespace,
            string resourceType,
            string resourceName)
        {
            proxy.EndDeleteResource(proxy.BeginDeleteResource(
                subscriptionId,
                cloudServiceName,
                resourceProviderNamespace,
                resourceType,
                resourceName,
                null,
                null));
        }

        public static void CreateCloudService(
            this IStoreManagement proxy,
            string subscriptionId,
            string cloudServiceName,
            CloudService cloudService)
        {
            proxy.EndCreateCloudService(proxy.BeginCreateCloudService(
                subscriptionId,
                cloudServiceName,
                cloudService,
                null,
                null));
        }

        public static void CreateResource(
            this IStoreManagement proxy,
            string subscriptionId,
            string cloudServiceName,
            string resourceProviderNamespace,
            string resourceType,
            string resourceName,
            Resource resource)
        {
            proxy.EndCreateResource(proxy.BeginCreateResource(
                subscriptionId,
                cloudServiceName,
                resourceProviderNamespace,
                resourceType,
                resourceName,
                resource,
                null,
                null));
        }
    }
}

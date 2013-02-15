/**
* Copyright Microsoft Corporation 2012
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Microsoft.WindowsAzure.ServiceManagement
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    public partial interface IServiceManagement
    {
        #region CreateStorageService
        
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/storageservices")]
        IAsyncResult BeginCreateStorageService(string subscriptionId, CreateStorageServiceInput input, AsyncCallback callback, object state);
        void EndCreateStorageService(IAsyncResult asyncResult);
        
        #endregion

        #region UpdateStorageService
        
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionId}/services/storageservices/{serviceName}")]
        IAsyncResult BeginUpdateStorageService(string subscriptionId, string serviceName, UpdateStorageServiceInput input, AsyncCallback callback, object state);
        void EndUpdateStorageService(IAsyncResult asyncResult);
        
        #endregion

        #region DeleteStorageService
        
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/services/storageservices/{serviceName}")]
        IAsyncResult BeginDeleteStorageService(string subscriptionId, string serviceName, AsyncCallback callback, object state);
        void EndDeleteStorageService(IAsyncResult asyncResult);
        
        #endregion

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/storageservices")]
        IAsyncResult BeginListStorageServices(string subscriptionId, AsyncCallback callback, object state);
        StorageServiceList EndListStorageServices(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/storageservices/{serviceName}")]
        IAsyncResult BeginGetStorageService(string subscriptionId, string serviceName, AsyncCallback callback, object state);
        StorageService EndGetStorageService(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/storageservices/{serviceName}/keys")]
        IAsyncResult BeginGetStorageKeys(string subscriptionId, string serviceName, AsyncCallback callback, object state);
        StorageService EndGetStorageKeys(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/storageservices/{serviceName}/keys?action=regenerate")]
        IAsyncResult BeginRegenerateStorageServiceKeys(string subscriptionId, string serviceName, RegenerateKeys regenerateKeys, AsyncCallback callback, object state);
        StorageService EndRegenerateStorageServiceKeys(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/services/storageservices/operations/isavailable/{serviceName}")]
        IAsyncResult BeginIsStorageServiceAvailable(string subscriptionID, string serviceName, AsyncCallback callback, object state);
        AvailabilityResponse EndIsStorageServiceAvailable(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static void CreateStorageService(this IServiceManagement proxy, string subscriptionId, CreateStorageServiceInput input)
        {
            proxy.EndCreateStorageService(proxy.BeginCreateStorageService(subscriptionId, input, null, null));
        }

        public static void UpdateStorageService(this IServiceManagement proxy, string subscriptionId, string serviceName, UpdateStorageServiceInput input)
        {
            proxy.EndUpdateStorageService(proxy.BeginUpdateStorageService(subscriptionId, serviceName, input, null, null));
        }

        public static void DeleteStorageService(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            proxy.EndDeleteStorageService(proxy.BeginDeleteStorageService(subscriptionId, serviceName, null, null));
        }

        public static StorageServiceList ListStorageServices(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListStorageServices(proxy.BeginListStorageServices(subscriptionId, null, null));
        }

        public static StorageService GetStorageService(this IServiceManagement proxy, string subscriptionId, string name)
        {
            return proxy.EndGetStorageService(proxy.BeginGetStorageService(subscriptionId, name, null, null));
        }

        public static StorageService GetStorageKeys(this IServiceManagement proxy, string subscriptionId, string name)
        {
            return proxy.EndGetStorageKeys(proxy.BeginGetStorageKeys(subscriptionId, name, null, null));
        }

        public static StorageService RegenerateStorageServiceKeys(this IServiceManagement proxy, string subscriptionId, string name, RegenerateKeys regenerateKeys)
        {
            return proxy.EndRegenerateStorageServiceKeys(proxy.BeginRegenerateStorageServiceKeys(subscriptionId, name, regenerateKeys, null, null));
        }

        public static AvailabilityResponse IsStorageServiceAvailable(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            return proxy.EndIsStorageServiceAvailable(proxy.BeginIsStorageServiceAvailable(subscriptionId, serviceName, null, null));
        }
    }
}

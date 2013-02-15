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
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    public partial interface IServiceManagement
    {
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionId}/services/networking/media")]        
        IAsyncResult BeginSetNetworkConfiguration(string subscriptionId, Stream networkConfiguration, AsyncCallback callback, object state);
        void EndSetNetworkConfiguration(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/networking/media")]
        IAsyncResult BeginGetNetworkConfiguration(string subscriptionId, AsyncCallback callback, object state);
        Stream EndGetNetworkConfiguration(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/networking/virtualnetwork")]
        IAsyncResult BeginListVirtualNetworkSites(string subscriptionId, AsyncCallback callback, object state);
        VirtualNetworkSiteList EndListVirtualNetworkSites(IAsyncResult asyncResult);

    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static void SetNetworkConfiguration(this IServiceManagement proxy, string subscriptionID, Stream networkConfiguration)
        {
            proxy.EndSetNetworkConfiguration(proxy.BeginSetNetworkConfiguration(subscriptionID, networkConfiguration, null, null));
        }

        public static Stream GetNetworkConfiguration(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndGetNetworkConfiguration(proxy.BeginGetNetworkConfiguration(subscriptionID, null, null));
        }

        public static VirtualNetworkSiteList ListVirtualNetworkSites(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndListVirtualNetworkSites(proxy.BeginListVirtualNetworkSites(subscriptionID, null, null));
        }

    }
}

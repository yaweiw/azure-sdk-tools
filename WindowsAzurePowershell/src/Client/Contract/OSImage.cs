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
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/services/images")]
        IAsyncResult BeginListOSImages(string subscriptionID, AsyncCallback callback, object state);
        OSImageList EndListOSImages(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionID}/services/images")]
        IAsyncResult BeginCreateOSImage(string subscriptionID, OSImage image, AsyncCallback callback, object state);
        OSImage EndCreateOSImage(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/services/images/{imageName}")]
        IAsyncResult BeginGetOSImage(string subscriptionID, string imageName, AsyncCallback callback, object state);
        OSImage EndGetOSImage(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionID}/services/images/{imageName}")]
        IAsyncResult BeginUpdateOSImage(string subscriptionID, string imageName, OSImage image, AsyncCallback callback, object state);
        OSImage EndUpdateOSImage(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionID}/services/images/{imageName}")]
        IAsyncResult BeginDeleteOSImage(string subscriptionID, string imageName, AsyncCallback callback, object state);
        void EndDeleteOSImage(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionID}/services/images/{imageName}?comp={comp}")]
        IAsyncResult BeginDeleteOSImageEx(string subscriptionID, string imageName, string comp, AsyncCallback callback, object state);
        void EndDeleteOSImageEx(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionID}/services/images/{imageName}/replicate")]
        IAsyncResult BeginReplicateOSImage(string subscriptionID, string imageName, ReplicationInput replicationInput, AsyncCallback callback, object state);
        string EndReplicateOSImage(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionID}/services/images/{imageName}/share?permission={permission}")]
        IAsyncResult BeginShareOSImage(string subscriptionID, string imageName, string permission, AsyncCallback callback, object state);
        bool EndShareOSImage(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionID}/services/images/{imageName}/unreplicate")]
        IAsyncResult BeginUnReplicateOSImage(string subscriptionID, string imageName, AsyncCallback callback, object state);
        void EndUnReplicateOSImage(IAsyncResult asyncResult);


        [OperationContract(AsyncPattern = true)]
        [WebGet ( UriTemplate = @"{subscriptionID}/services/images/query?location={location}&publisher={publisher}" )]
        IAsyncResult BeginQueryOSImages(string subscriptionID, string location, string publisher, AsyncCallback callback, object state);
        OSImageList EndQueryOSImages(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/services/images/{imageName}/details")]
        IAsyncResult BeginGetOSImageWithDetails(string subscriptionID, string imageName, AsyncCallback callback, object state);
        OSImageDetails EndGetOSImageWithDetails(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static OSImageList ListOSImages(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndListOSImages(proxy.BeginListOSImages(subscriptionID, null, null));
        }

        public static OSImage CreateOSImage(this IServiceManagement proxy, string subscriptionID, OSImage image)
        {
            return proxy.EndCreateOSImage(proxy.BeginCreateOSImage(subscriptionID, image, null, null));
        }

        public static OSImage UpdateOSImage(this IServiceManagement proxy, string subscriptionID, string imageName, OSImage image)
        {
            return proxy.EndUpdateOSImage(proxy.BeginUpdateOSImage(subscriptionID, imageName, image, null, null));
        }

        public static OSImage GetOSImage(this IServiceManagement proxy, string subscriptionID, string imageName)
        {
            return proxy.EndGetOSImage(proxy.BeginGetOSImage(subscriptionID, imageName, null, null));
        }

        public static void DeleteOSImage(this IServiceManagement proxy, string subscriptionID, string imageName)
        {
            proxy.EndDeleteOSImage(proxy.BeginDeleteOSImage(subscriptionID, imageName, null, null));
        }

        public static void DeleteOSImageEx(this IServiceManagement proxy, string subscriptionID, string imageName, string comp)
        {
            proxy.EndDeleteOSImageEx(proxy.BeginDeleteOSImageEx(subscriptionID, imageName, comp, null, null));
        }

        public static string ReplicateOSImage(this IServiceManagement proxy, string subscriptionId, string imageName, ReplicationInput replicationInput)
        {
            return proxy.EndReplicateOSImage(proxy.BeginReplicateOSImage(subscriptionId, imageName, replicationInput, null, null));
        }

        public static void UnReplicateOSImage(this IServiceManagement proxy, string subscriptionId, string imageName)
        {
            proxy.EndUnReplicateOSImage(proxy.BeginUnReplicateOSImage(subscriptionId, imageName, null, null));
        }

        public static bool ShareOSImage(this IServiceManagement proxy, string subscriptionId, string imageName, string permission)
        {
            return proxy.EndShareOSImage(proxy.BeginShareOSImage(subscriptionId, imageName, permission, null, null));
        }

        public static OSImageDetails GetOSImagWithDetails(this IServiceManagement proxy, string subscriptionId, string imageName)
        {
            return proxy.EndGetOSImageWithDetails(proxy.BeginGetOSImageWithDetails(subscriptionId, imageName, null, null));
        }
    }
}

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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    public partial interface IServiceManagement
    {
        ////List the disks associated with a given subscription.
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/services/disks")]
        IAsyncResult BeginListDisks(string subscriptionID, AsyncCallback callback, object state);

        DiskList EndListDisks(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionID}/services/disks")]
        IAsyncResult BeginCreateDisk(string subscriptionID, Disk disk, AsyncCallback callback, object state);

        Disk EndCreateDisk(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/services/disks/{diskName}")]
        IAsyncResult BeginGetDisk(string subscriptionID, string diskName, AsyncCallback callback, object state);

        Disk EndGetDisk(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionID}/services/disks/{diskName}")]
        IAsyncResult BeginUpdateDisk(string subscriptionID, string diskName, Disk disk, AsyncCallback callback, object state);

        Disk EndUpdateDisk(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionID}/services/disks/{diskName}")]
        IAsyncResult BeginDeleteDisk(string subscriptionID, string diskName, AsyncCallback callback, object state);

        void EndDeleteDisk(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static DiskList ListDisks(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndListDisks(proxy.BeginListDisks(subscriptionID, null, null));
        }

        public static Disk CreateDisk(this IServiceManagement proxy, string subscriptionID, Disk disk)
        {
            return proxy.EndCreateDisk(proxy.BeginCreateDisk(subscriptionID, disk, null, null));
        }

        public static Disk UpdateDisk(this IServiceManagement proxy, string subscriptionID, string diskName, Disk disk)
        {
            return proxy.EndUpdateDisk(proxy.BeginUpdateDisk(subscriptionID, diskName, disk, null, null));
        }

        public static Disk GetDisk(this IServiceManagement proxy, string subscriptionID, string diskName)
        {
            return proxy.EndGetDisk(proxy.BeginGetDisk(subscriptionID, diskName, null, null));
        }

        public static void DeleteDisk(this IServiceManagement proxy, string subscriptionID, string diskName)
        {
            proxy.EndDeleteDisk(proxy.BeginDeleteDisk(subscriptionID, diskName, null, null));
        }
    }
}
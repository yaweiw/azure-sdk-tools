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
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/affinitygroups")]
        IAsyncResult BeginCreateAffinityGroup(string subscriptionId, CreateAffinityGroupInput input, AsyncCallback callback, object state);
        void EndCreateAffinityGroup(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/affinitygroups/{affinityGroupName}")]
        IAsyncResult BeginDeleteAffinityGroup(string subscriptionId, string affinityGroupName, AsyncCallback callback, object state);
        void EndDeleteAffinityGroup(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionId}/affinitygroups/{affinityGroupName}")]
        IAsyncResult BeginUpdateAffinityGroup(string subscriptionId, string affinityGroupName, UpdateAffinityGroupInput input, AsyncCallback callback, object state);
        void EndUpdateAffinityGroup(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/affinitygroups")]
        IAsyncResult BeginListAffinityGroups(string subscriptionId, AsyncCallback callback, object state);
        AffinityGroupList EndListAffinityGroups(IAsyncResult asyncResult);

        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/affinitygroups/{affinityGroupName}")]
        IAsyncResult BeginGetAffinityGroup(string subscriptionId, string affinityGroupName, AsyncCallback callback, object state);
        AffinityGroup EndGetAffinityGroup(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static void CreateAffinityGroup(this IServiceManagement proxy, string subscriptionId, CreateAffinityGroupInput input)
        {
            proxy.EndCreateAffinityGroup(proxy.BeginCreateAffinityGroup(subscriptionId, input, null, null));
        }

        public static void DeleteAffinityGroup(this IServiceManagement proxy, string subscriptionId, string affinityGroupName)
        {
            proxy.EndDeleteAffinityGroup(proxy.BeginDeleteAffinityGroup(subscriptionId, affinityGroupName, null, null));
        }

        public static void UpdateAffinityGroup(this IServiceManagement proxy, string subscriptionId, string affinityGroupName, UpdateAffinityGroupInput input)
        {
            proxy.EndUpdateAffinityGroup(proxy.BeginUpdateAffinityGroup(subscriptionId, affinityGroupName, input, null, null));
        }

        public static AffinityGroupList ListAffinityGroups(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListAffinityGroups(proxy.BeginListAffinityGroups(subscriptionId, null, null));
        }

        public static AffinityGroup GetAffinityGroup(this IServiceManagement proxy, string subscriptionId, string affinityGroupName)
        {
            return proxy.EndGetAffinityGroup(proxy.BeginGetAffinityGroup(subscriptionId, affinityGroupName, null, null));
        }
    }
}

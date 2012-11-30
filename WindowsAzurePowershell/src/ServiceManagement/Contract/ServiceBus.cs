// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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
    using Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel;

    /// <summary>
    /// The service bus-related part of the API
    /// </summary>
    public partial interface IServiceManagement
    {
        /// <summary>
        /// Gets a service bus namespace.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [GetNamespaceBehavior]
        [WebGet(UriTemplate = @"{subscriptionId}/services/servicebus/namespaces/{name}")]
        IAsyncResult BeginGetNamespace(string subscriptionId, string name, AsyncCallback callback, object state);

        Namespace EndGetNamespace(IAsyncResult asyncResult);

        /// <summary>
        /// Gets service bus namespaces associated with a subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [ListNamespacesBehavior]
        [WebGet(UriTemplate = @"{subscriptionId}/services/servicebus/namespaces")]
        IAsyncResult BeginListNamespaces(string subscriptionId, AsyncCallback callback, object state);

        NamespaceList EndListNamespaces(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static Namespace GetNamespace(this IServiceManagement proxy, string subscriptionId, string name)
        {
            return proxy.EndGetNamespace(proxy.BeginGetNamespace(subscriptionId, name, null, null));
        }

        public static NamespaceList ListNamespaces(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListNamespaces(proxy.BeginListNamespaces(subscriptionId, null, null));
        }
    }
}

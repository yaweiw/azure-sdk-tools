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

    /// <summary>
    /// The operating-system-specific interface of the resource model service.
    /// </summary>
    public partial interface IServiceManagement
    {
        #region List Operating Systems

        /// <summary>
        /// Lists all available operating systems.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/operatingsystems")]
        IAsyncResult BeginListOperatingSystems(string subscriptionId, AsyncCallback callback, object state);

        OperatingSystemList EndListOperatingSystems(IAsyncResult asyncResult);

        #endregion

        #region List Operating Systems Families

        /// <summary>
        /// Lists all available operating system families and their operating systems.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/operatingsystemfamilies")]
        IAsyncResult BeginListOperatingSystemFamilies(string subscriptionId, AsyncCallback callback, object state);

        OperatingSystemFamilyList EndListOperatingSystemFamilies(IAsyncResult asyncResult);

        #endregion
    }

    /// <summary>
    /// Extensions of the IServiceManagement interface that allows clients to call operations synchronously.
    /// </summary>
    public static partial class ServiceManagementExtensionMethods
    {
        public static OperatingSystemList ListOperatingSystems(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListOperatingSystems(proxy.BeginListOperatingSystems(subscriptionId, null, null));
        }

        public static OperatingSystemFamilyList ListOperatingSystemFamilies(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListOperatingSystemFamilies(proxy.BeginListOperatingSystemFamilies(subscriptionId, null, null));
        }
    }
}

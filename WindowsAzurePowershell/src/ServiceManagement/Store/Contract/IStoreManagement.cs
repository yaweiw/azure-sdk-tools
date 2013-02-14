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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement.Store.Contract
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Store.ResourceModel;

    [ServiceContract]
    public interface IStoreManagement
    {
        /// <summary>
        /// Lists the cloud services associated with a given subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/CloudServices")]
        IAsyncResult BeginListCloudServices(string subscriptionId, AsyncCallback callback, object state);

        CloudServiceList EndListCloudServices(IAsyncResult asyncResult);
        
        /// <summary>
        /// Removes given resource from the Cloud Service.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/CloudServices/{cloudServiceName}/resources/{resourceProviderNamespace}/{resourceType}/{resourceName}")]
        IAsyncResult BeginDeleteResource(
            string subscriptionId,
            string cloudServiceName,
            string resourceProviderNamespace,
            string resourceType,
            string resourceName,
            AsyncCallback callback,
            object state);

        void EndDeleteResource(IAsyncResult asyncResult);
    }
}

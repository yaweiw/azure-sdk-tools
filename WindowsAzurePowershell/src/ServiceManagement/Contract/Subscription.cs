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

    public partial interface IServiceManagement
    {
        /// <summary>
        ///// Returns information about a subscription
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}")]
        IAsyncResult BeginGetSubscription(string subscriptionID, AsyncCallback callback, object state);

        Subscription EndGetSubscription(IAsyncResult asyncResult);

        /// <summary>
        ///// Returns a list of subscription operations
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionID}/operations?starttime={starttime}&endtime={endtime}&objectidfilter={objectidfilter}&operationresultfilter={operationresultfilter}&continuationtoken={continuationtoken}")]
        IAsyncResult BeginListSubscriptionOperations(string subscriptionID, string startTime, string endTime, string objectIdFilter, string operationResultFilter, string continuationToken, AsyncCallback callback, object state);

        SubscriptionOperationCollection EndListSubscriptionOperations(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static Subscription GetSubscription(this IServiceManagement proxy, string subscriptionID)
        {
            return proxy.EndGetSubscription(proxy.BeginGetSubscription(subscriptionID, null, null));
        }

        public static SubscriptionOperationCollection ListSubscriptionOperations(this IServiceManagement proxy, string subscriptionID, string startTime, string endTime, string objectIdFilter, string operationResultFilter, string continuationToken)
        {
            return proxy.EndListSubscriptionOperations(proxy.BeginListSubscriptionOperations(subscriptionID, startTime, endTime, objectIdFilter, operationResultFilter, continuationToken, null, null));
        }
    }
}

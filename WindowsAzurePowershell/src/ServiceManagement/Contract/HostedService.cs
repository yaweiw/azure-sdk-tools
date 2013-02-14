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
    /// The hosted services related part of the Service Management API
    /// </summary>
    public partial interface IServiceManagement
    {
        #region CreateHostedService
        /// <summary>
        /// Creates a hosted service
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/hostedservices")]
        IAsyncResult BeginCreateHostedService(string subscriptionId, CreateHostedServiceInput input, AsyncCallback callback, object state);

        void EndCreateHostedService(IAsyncResult asyncResult);
        #endregion

        #region UpdateHostedService
        /// <summary>
        /// Updates a hosted service 
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}")]
        IAsyncResult BeginUpdateHostedService(string subscriptionId, string serviceName, UpdateHostedServiceInput input, AsyncCallback callback, object state);

        void EndUpdateHostedService(IAsyncResult asyncResult);
        #endregion

        #region DeleteHostedService
        /// <summary>
        /// Deletes a hosted service
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}")]
        IAsyncResult BeginDeleteHostedService(string subscriptionId, string serviceName, AsyncCallback callback, object state);

        void EndDeleteHostedService(IAsyncResult asyncResult);
        #endregion

        /// <summary>
        /// Lists the hosted services associated with a given subscription.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/hostedservices")]
        IAsyncResult BeginListHostedServices(string subscriptionId, AsyncCallback callback, object state);

        HostedServiceList EndListHostedServices(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the properties for the specified hosted service.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}")]
        IAsyncResult BeginGetHostedService(string subscriptionId, string serviceName, AsyncCallback callback, object state);

        HostedService EndGetHostedService(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the detailed properties for the specified hosted service. 
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/hostedservices/{serviceName}?embed-detail={embedDetail}")]
        IAsyncResult BeginGetHostedServiceWithDetails(string subscriptionId, string serviceName, bool embedDetail, AsyncCallback callback, object state);

        HostedService EndGetHostedServiceWithDetails(IAsyncResult asyncResult);

        /// <summary>
        /// List the locations supported by a given subscription. 
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/locations")]
        IAsyncResult BeginListLocations(string subscriptionId, AsyncCallback callback, object state);

        LocationList EndListLocations(IAsyncResult asyncResult);

        /// <summary>
        /// Checks if DNS is available
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/hostedservices/operations/isavailable/{serviceName}")]
        IAsyncResult BeginIsDNSAvailable(string subscriptionId, string serviceName, AsyncCallback callback, object state);

        AvailabilityResponse EndIsDNSAvailable(IAsyncResult asyncResult);
    }

    public static partial class ServiceManagementExtensionMethods
    {
        public static void CreateHostedService(this IServiceManagement proxy, string subscriptionId, CreateHostedServiceInput input)
        {
            proxy.EndCreateHostedService(proxy.BeginCreateHostedService(subscriptionId, input, null, null));
        }

        public static void UpdateHostedService(this IServiceManagement proxy, string subscriptionId, string serviceName, UpdateHostedServiceInput input)
        {
            proxy.EndUpdateHostedService(proxy.BeginUpdateHostedService(subscriptionId, serviceName, input, null, null));
        }

        public static void DeleteHostedService(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            proxy.EndDeleteHostedService(proxy.BeginDeleteHostedService(subscriptionId, serviceName, null, null));
        }

        public static HostedServiceList ListHostedServices(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListHostedServices(proxy.BeginListHostedServices(subscriptionId, null, null));
        }

        public static HostedServiceList ListHostedServicesWithDetails(this IServiceManagement proxy, string subscriptionId, ref string continuationToken)
        {
            if (continuationToken != null)
            {
                WebOperationContext.Current.OutgoingRequest.Headers["x-ms-continuation-token"] = continuationToken;
            }
            else
            {
                WebOperationContext.Current.OutgoingRequest.Headers["x-ms-continuation-token"] = "All";
            }

            HostedServiceList hsList = proxy.EndListHostedServices(proxy.BeginListHostedServices(subscriptionId, null, null));
            continuationToken = WebOperationContext.Current.IncomingResponse.Headers["x-ms-continuation-token"];

            return hsList;
        }

        public static HostedService GetHostedService(this IServiceManagement proxy, string subscriptionId, string serviceName)
        {
            return proxy.EndGetHostedService(proxy.BeginGetHostedService(subscriptionId, serviceName, null, null));
        }

        public static HostedService GetHostedServiceWithDetails(this IServiceManagement proxy, string subscriptionId, string serviceName, bool embedDetail)
        {
            return proxy.EndGetHostedServiceWithDetails(proxy.BeginGetHostedServiceWithDetails(subscriptionId, serviceName, embedDetail, null, null));
        }

        public static LocationList ListLocations(this IServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndListLocations(proxy.BeginListLocations(subscriptionId, null, null));
        }

        public static AvailabilityResponse IsDNSAvailable(this IServiceManagement proxy, string subscriptionID, string dnsname)
        {
            return proxy.EndIsDNSAvailable(proxy.BeginIsDNSAvailable(subscriptionID, dnsname, null, null));
        }
    }
}

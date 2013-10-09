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

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using Commands.Test.Utilities.Common;
    using Commands.Utilities.ServiceBus.Contract;
    using Commands.Utilities.ServiceBus.ResourceModel;
    using VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Simple implementation of the IServiceBusManagement interface that can be
    /// used for mocking basic interactions without involving Azure directly.
    /// </summary>
    public class SimpleServiceBusManagement : IServiceBusManagement
    {
        /// <summary>
        /// Gets or sets a value indicating whether the thunk wrappers will
        /// throw an exception if the thunk is not implemented.  This is useful
        /// when debugging a test.
        /// </summary>
        public bool ThrowsIfNotImplemented { get; set; }

        /// <summary>
        /// Initializes a new instance of the SimpleServiceBusManagement class.
        /// </summary>
        public SimpleServiceBusManagement()
        {
            ThrowsIfNotImplemented = true;
        }

        public IAsyncResult BeginGetServiceBusNamespace(string subscriptionId, string name, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["name"] = name;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            
            return result;
        }

        public Func<SimpleServiceManagementAsyncResult, ServiceBusNamespace> GetNamespaceThunk { get; set; }
        public ServiceBusNamespace EndGetServiceBusNamespace(IAsyncResult asyncResult)
        {
            ServiceBusNamespace serviceBusNamespace = new ServiceBusNamespace();

            if (GetNamespaceThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                serviceBusNamespace = GetNamespaceThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("GetNamespaceThunk is not implemented!");
            }

            return serviceBusNamespace;
        }

        public Func<SimpleServiceManagementAsyncResult, List<ServiceBusNamespace>> ListNamespacesThunk { get; set; }
        public IAsyncResult BeginListServiceBusNamespaces(string subscriptionId, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["callback"] = callback;
            result.Values["state"] = state;

            return result;
        }

        public List<ServiceBusNamespace> EndListServiceBusNamespaces(IAsyncResult asyncResult)
        {
            List<ServiceBusNamespace> serviceBusNamespace = new List<ServiceBusNamespace>();

            if (ListNamespacesThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                serviceBusNamespace = ListNamespacesThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("ListNamespacesThunk is not implemented!");
            }

            return serviceBusNamespace;
        }

        public Func<SimpleServiceManagementAsyncResult, List<ServiceBusRegion>> ListServiceBusRegionsThunk { get; set; }
        public IAsyncResult BeginListServiceBusRegions(string subscriptionId, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["callback"] = callback;
            result.Values["state"] = state;

            return result;
        }

        public List<ServiceBusRegion> EndListServiceBusRegions(IAsyncResult asyncResult)
        {
            List<ServiceBusRegion> serviceBusNamespace = new List<ServiceBusRegion>();

            if (ListServiceBusRegionsThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                serviceBusNamespace = ListServiceBusRegionsThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("ListServiceBusRegionsThunk is not implemented!");
            }

            return serviceBusNamespace;
        }

        public Func<SimpleServiceManagementAsyncResult, ServiceBusNamespace> CreateServiceBusNamespaceThunk { get; set; }
        public IAsyncResult BeginCreateServiceBusNamespace(string subscriptionId, ServiceBusNamespace namespaceDescription, string name, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["namespaceDescription"] = namespaceDescription;
            result.Values["name"] = name;
            result.Values["callback"] = callback;
            result.Values["state"] = state;

            return result;
        }

        public ServiceBusNamespace EndCreateServiceBusNamespace(IAsyncResult asyncResult)
        {
            ServiceBusNamespace serviceBusNamespace = new ServiceBusNamespace();

            if (CreateServiceBusNamespaceThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                serviceBusNamespace = CreateServiceBusNamespaceThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("CreateServiceBusNamespaceThunk is not implemented!");
            }

            return serviceBusNamespace;
        }

        public Action<SimpleServiceManagementAsyncResult> DeleteServiceBusNamespaceThunk { get; set; }
        public IAsyncResult BeginDeleteServiceBusNamespace(string subscriptionId, string name, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["name"] = name;
            result.Values["callback"] = callback;
            result.Values["state"] = state;

            return result;
        }

        public void EndDeleteServiceBusNamespace(IAsyncResult asyncResult)
        {
            if (DeleteServiceBusNamespaceThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                DeleteServiceBusNamespaceThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("DeleteServiceBusNamespaceThunk is not implemented!");
            }
        }

        public Func<SimpleServiceManagementAsyncResult, ServiceBusNamespaceAvailabilityResponse> IsServiceBusNamespaceAvailableThunk { get; set; }
        public IAsyncResult BeginIsServiceBusNamespaceAvailable(string subscriptionId, string serviceName, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["serviceName"] = serviceName;
            result.Values["callback"] = callback;
            result.Values["state"] = state;

            return result;
        }

        public ServiceBusNamespaceAvailabilityResponse EndIsServiceBusNamespaceAvailable(IAsyncResult asyncResult)
        {
            ServiceBusNamespaceAvailabilityResponse availabilityResponse = new ServiceBusNamespaceAvailabilityResponse();

            if (IsServiceBusNamespaceAvailableThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                availabilityResponse = IsServiceBusNamespaceAvailableThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("IsServiceBusNamespaceAvailableThunk is not implemented!");
            }

            return availabilityResponse;
        }
    }
}
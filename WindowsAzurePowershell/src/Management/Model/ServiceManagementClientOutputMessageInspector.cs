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
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    public class ServiceManagementClientOutputMessageInspector : IClientMessageInspector, IEndpointBehavior
    {
        public const string UserAgentHeaderName = "User-Agent";
        public const string UserAgentHeaderContent = "Windows Azure Powershell/v.0.6.11";
        public const string VSDebuggerCausalityDataHeaderName = "VSDebuggerCausalityData";

        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref Message reply, object correlationState) { }
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                var property = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];

                // Remove VSDebuggerCausalityData header which is added by WCF.
                if (property.Headers[VSDebuggerCausalityDataHeaderName] != null)
                {
                    property.Headers.Remove(VSDebuggerCausalityDataHeaderName);
                }

                if (property.Headers[Constants.VersionHeaderName] == null)
                {
                    property.Headers.Add(Constants.VersionHeaderName, Constants.VersionHeaderContent20120301);
                }

                if (property.Headers[UserAgentHeaderName] == null)
                {
                    property.Headers.Add(UserAgentHeaderName, UserAgentHeaderContent);
                }
            }

            return null;
        }

        #endregion

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        public void Validate(ServiceEndpoint endpoint) { }

        #endregion
    }
}
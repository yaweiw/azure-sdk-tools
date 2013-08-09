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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Services
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using Commands.Utilities.Common;

    internal class ClientOutputMessageInspector : IClientMessageInspector, IEndpointBehavior
    {
        private string requestSessionId;

        internal ClientOutputMessageInspector(string requestSessionId)
        {
            this.requestSessionId = requestSessionId;
        }

        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState) 
        { 
        }

        /// <summary>
        /// Adds additional trace information and agent identification values to the web request header
        /// </summary>
        /// <param name="request">The web request to be sent</param>
        /// <param name="channel">The channel being used.</param>
        /// <returns>always null</returns>
        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            if (request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                var property = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];

                // Add the version to the header.
                if (property.Headers[Constants.VersionHeaderName] == null)
                {
                    property.Headers.Add(Constants.VersionHeaderName, Constants.VersionHeaderContent);
                }

                // Add the client session ID to the header
                if (property.Headers[Constants.ClientSessionIdHeaderName] == null)
                {
                    property.Headers.Add(
                        Constants.ClientSessionIdHeaderName, 
                        SqlDatabaseManagementCmdletBase.clientSessionId);
                }

                // Add the request session id to the header
                if (property.Headers[Constants.ClientRequestIdHeaderName] == null)
                {
                    property.Headers.Add(Constants.ClientRequestIdHeaderName, this.requestSessionId);
                }

                // Add the appropriate headers to indicate which user agent sent the request.
                if (property.Headers[ServiceManagementClientOutputMessageInspector.UserAgentHeaderName] == null)
                {
                    property.Headers.Add(
                        ServiceManagementClientOutputMessageInspector.UserAgentHeaderName, 
                        ServiceManagementClientOutputMessageInspector.UserAgentHeaderContent);
                }
            }

            return null;
        }

        #endregion

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) 
        { 
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) 
        { 
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}
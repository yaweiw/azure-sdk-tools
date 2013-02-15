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

namespace Microsoft.WindowsAzure.Management.Model
{
    using System;
    using System.Management.Automation;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using Microsoft.WindowsAzure.Management.Utilities;
    using GeneralSM = Microsoft.WindowsAzure.ServiceManagement.Utilities.General;

    public class HttpRestMessageInspector : IClientMessageInspector, IEndpointBehavior
    {
        private Action<string> logger;

        public HttpRestMessageInspector(Action<string> logger)
        {
            this.logger = logger;
        }

        private string MessageHeadersToString(WebHeaderCollection headers)
        {
            string[] keys = headers.AllKeys;
            StringBuilder result = new StringBuilder();

            foreach (string key in keys)
            {
                result.AppendLine(string.Format(
                    "{0,-30}: {1}",
                    key,
                    General.ArrayToString<string>(headers.GetValues(key), ",")));
            }

            return result.ToString();
        }

        #region IClientMessageInspector

        public virtual void AfterReceiveReply(ref Message reply, object correlationState)
        {
            HttpResponseMessageProperty responseProperties = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
            StringBuilder httpResponseLog = new StringBuilder();
            string body = GeneralSM.ReadBody(ref reply);

            httpResponseLog.AppendLine(string.Format("============================ HTTP RESPONSE ============================{0}", Environment.NewLine));
            httpResponseLog.AppendLine(string.Format("Status Code:{0}{1}{0}", Environment.NewLine, responseProperties.StatusCode.ToString()));
            httpResponseLog.AppendLine(string.Format("Headers:{0}{1}", Environment.NewLine, MessageHeadersToString(responseProperties.Headers)));
            httpResponseLog.AppendLine(string.Format("Body:{0}{1}{0}", Environment.NewLine, body));
            logger(httpResponseLog.ToString());
        }

        public virtual object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            HttpRequestMessageProperty requestProperties = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
            StringBuilder httpRequestLog = new StringBuilder();
            string body = GeneralSM.ReadBody(ref request);

            httpRequestLog.AppendLine(string.Format("============================ HTTP REQUEST ============================{0}", Environment.NewLine));
            httpRequestLog.AppendLine(string.Format("HTTP Method:{0}{1}{0}", Environment.NewLine, requestProperties.Method));
            httpRequestLog.AppendLine(string.Format("Absolute Uri:{0}{1}{0}", Environment.NewLine, request.Headers.To.AbsoluteUri));
            httpRequestLog.AppendLine(string.Format("Headers:{0}{1}", Environment.NewLine, MessageHeadersToString(requestProperties.Headers)));
            httpRequestLog.AppendLine(string.Format("Body:{0}{1}{0}", Environment.NewLine, body));
            logger(httpRequestLog.ToString());

            return request;
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

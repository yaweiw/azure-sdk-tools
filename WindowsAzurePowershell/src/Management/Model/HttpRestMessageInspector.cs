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
    using System.Xml;
    using Microsoft.WindowsAzure.Management.Utilities;

    public class HttpRestMessageInspector : IClientMessageInspector, IEndpointBehavior
    {
        private PSCmdlet cmdlet;

        public HttpRestMessageInspector(PSCmdlet cmdlet)
        {
            this.cmdlet = cmdlet;
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

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (cmdlet.SessionState != null)
            {
                HttpResponseMessageProperty responseProperties = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
                StringBuilder httpResponse = new StringBuilder();
                string body = ReadBody(ref reply);

                httpResponse.AppendLine("============================ HTTP RESPONSE ============================" + Environment.NewLine);
                httpResponse.AppendLine("Status Code:\n" + responseProperties.StatusCode.ToString() + Environment.NewLine);
                httpResponse.AppendLine("Headers:\n" + MessageHeadersToString(responseProperties.Headers));
                httpResponse.AppendLine("Body:\n" + General.Beautify(body) + Environment.NewLine);
                cmdlet.WriteDebug(httpResponse.ToString());
            }
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (cmdlet.SessionState != null)
            {
                HttpRequestMessageProperty requestProperties = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
                StringBuilder httpRequest = new StringBuilder();
                string body = ReadBody(ref request);

                httpRequest.AppendLine("============================ HTTP REQUEST ============================" + Environment.NewLine);
                httpRequest.AppendLine("HTTP Method:\n" + requestProperties.Method + Environment.NewLine);
                httpRequest.AppendLine("Absolute Uri:\n" + request.Headers.To.AbsoluteUri + Environment.NewLine);
                httpRequest.AppendLine("Headers:\n" + MessageHeadersToString(requestProperties.Headers));
                httpRequest.AppendLine("Body:\n" + General.Beautify(body) + Environment.NewLine);
                cmdlet.WriteDebug(httpRequest.ToString());
            }

            return request;
        }

        private string ReadBody(ref Message originalMessage)
        {
            StringBuilder strBuilder = new StringBuilder();

            using (MessageBuffer messageBuffer = originalMessage.CreateBufferedCopy(int.MaxValue))
            {
                Message message = messageBuffer.CreateMessage();
                XmlWriter writer = XmlWriter.Create(strBuilder);
                using (XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer))
                {
                    message.WriteBodyContents(dictionaryWriter);   
                }

                originalMessage = messageBuffer.CreateMessage();
            }

            return strBuilder.ToString();
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

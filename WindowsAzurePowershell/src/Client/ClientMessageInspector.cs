/**
* Copyright Microsoft Corporation 2012
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Microsoft.WindowsAzure.ServiceManagement
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    /// <summary>
    /// Injects custom headers into the Service Management Requests and pulls pulls errors from the replies.
    /// </summary>
    internal sealed class ClientMessageInspector : IClientMessageInspector, IEndpointBehavior
    {
        /// <summary>
        /// The value of the version header passed to the Service Management API.
        /// </summary>
        /// <remarks> You can change this value to simulate older versions of the Service Management client.</remarks>
        private const string VersionHeaderString = Constants.VersionHeaderContentLatest;

        private const string ComponentTraceName = "ClientMessageInspector";

        private string _userAgent = null;
        private Func<string> _clientRequestIdGenerator = ClientMessageInspector.DefaultRequestIdGenerator;

        private TraceSourceHelper _logger = null;

        /// <summary>
        /// Constructs a new instance of ClientOutputMessageInspector.
        /// </summary>
        /// <param name="userAgent">Optional, user agent string to supply with requests.</param>
        /// <param name="clientRequestIdGenerator">Optional, function delegate for returning a x-ms-clientid value.</param>
        /// <param name="logger">Optional, .Net TraceSource to use for logging.</param>
        /// <param name="errorEventId">Optional, ID for the event to use when logging errors.</param>
        public ClientMessageInspector(string userAgent, Func<string> clientRequestIdGenerator, TraceSource logger, int errorEventId)
        {
            if (!string.IsNullOrEmpty(userAgent))
            {
                this._userAgent = userAgent;
            }

            if (clientRequestIdGenerator != null)
            {
                this._clientRequestIdGenerator = clientRequestIdGenerator;
            }

            this._logger = new TraceSourceHelper(logger, errorEventId, ClientMessageInspector.ComponentTraceName);
        }

        #region IClientMessageInspector Members

        /// <summary>
        /// Checks the respons message for errors and throws an exception if necessary.
        /// </summary>
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState) 
        {
            this._logger.LogDebugInformation("ClientOutputMessageInspector.AfterReceiveReply has been invoked.");

            if (correlationState != null)
            {
                this._logger.LogDebugInformation("Processing reply for correlation state: {0}.", correlationState);
            }

            if (reply != null) 
            {
                HttpResponseMessageProperty responseProperty = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
                if (responseProperty != null)
                {
                    // Get the operation tracking ID from the response header
                    string operationId = string.Empty;
                    if (responseProperty.Headers != null)
                    {
                        operationId = responseProperty.Headers[Constants.OperationTrackingIdHeader];
                    }

                    ServiceManagementClientException smError = null;
                    if (ClientMessageInspector.IsRestFault(responseProperty.StatusCode))
                    {
                        this._logger.LogDebugInformation("An error occurred calling the Service Management API.");

                        this._logger.LogDebugInformation("Operation ID of the failed operation: {0}.", string.IsNullOrEmpty(operationId) ? "<NONE>" : operationId);

                        // Create a copy of the message and try to read the ServiceManagementError object from the stream
                        ServiceManagementError errorDetails = null;
                        using (MessageBuffer buffer = reply.CreateBufferedCopy(int.MaxValue))
                        {
                            try
                            {
                                this._logger.LogDebugInformation("Attempting to parse error details.");
                                using (XmlDictionaryReader reader = buffer.CreateMessage().GetReaderAtBodyContents())
                                {
                                    try
                                    {
                                        DataContractSerializer ser = new DataContractSerializer(typeof(ServiceManagementError));
                                        errorDetails = (ServiceManagementError)ser.ReadObject(reader, true);
                                        this._logger.LogDebugInformation("Error details processed successfully.");
                                    }
                                    catch (SerializationException ex)
                                    {
                                        this._logger.LogError("Could not deserialize error object into ServiceManagementError: {0}", ex);

                                        // Swallow the exception and continue with less information
                                    }
                                }
                            }
                            catch (ObjectDisposedException)
                            {
                                // When using .Net 4.0, there are certain error codes { 404, 415, 503, 504 and sometimes 400 } which can make it difficult to get the body of the 
                                // response due to a closed stream. This issue does not affect any other framework versions.
                                this._logger.LogError("Could not process body response because the stream has been closed.");

                                // Swallow the exception and continue with less information
                            }
                            catch (InvalidOperationException ex)
                            {
                                this._logger.LogError("Could not create XmlDictionaryReader: {0}.", ex);

                                // Swallow the exception and continue with less information
                            }

                            // Throw the ServiceManagementClientException with the HTTP status and the detailed error information and operation ID if set
                            smError = new ServiceManagementClientException(responseProperty.StatusCode, errorDetails, operationId, responseProperty.Headers);
                            this._logger.LogError("Error details: {0}", smError);
                        }
                    }

                    // The information below will be used by Microsoft to debug any issues using this API. Please do not remove or change it
                    string smErrorCode = "<NONE>";
                    string smErrorMesage = "<NONE>";
                    if ((smError != null) && (smError.ErrorDetails != null))
                    {
                        if (!string.IsNullOrEmpty(smError.ErrorDetails.Code))
                        {
                            smErrorCode = smError.ErrorDetails.Code;
                        }

                        if (!string.IsNullOrEmpty(smError.ErrorDetails.Message))
                        {
                            smErrorMesage = smError.ErrorDetails.Message;
                        }
                    }

                    this._logger.LogInformation("RESPONSE, {0}, {1}, {2}, {3}, {4}", responseProperty.StatusCode, smErrorCode, smErrorMesage, operationId, correlationState);

                    if (smError != null)
                    {
                        throw smError;
                    }
                }
            }                   
        }

        /// <summary>
        /// Adds custom headers to outgoing response.
        /// </summary>
        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            HttpRequestMessageProperty requestProperty = null;
            if ((request != null) && (request.Properties != null))
            {
                requestProperty = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
            }

            if ((requestProperty == null) || (request.Headers == null))
            {
                throw new InvalidOperationException(Resources.CouldNotSetRequestHeaderError);
            }

            // Add the version header
            // This could be set in the operation context and could be used to override the version header.
            if (string.IsNullOrEmpty(requestProperty.Headers[Constants.VersionHeaderName]))
            {
                this._logger.LogDebugInformation("Adding version header {0} to request.", ClientMessageInspector.VersionHeaderString);
                requestProperty.Headers[Constants.VersionHeaderName] = ClientMessageInspector.VersionHeaderString;
            }
            else
            {
                this._logger.LogDebugInformation("Version header of the request is {0}.", requestProperty.Headers[Constants.VersionHeaderName]);
            }

            // Add the UserAgent header
            if (!string.IsNullOrEmpty(this._userAgent))
            {
                this._logger.LogDebugInformation("Adding UserAgent header {0} to request.", this._userAgent);
                requestProperty.Headers[HttpRequestHeader.UserAgent] = this._userAgent;
            }

            string clientRequestId = null;
            if (this._clientRequestIdGenerator != null)
            {
                clientRequestId = this._clientRequestIdGenerator();
                if (!string.IsNullOrEmpty(clientRequestId))
                {
                    this._logger.LogDebugInformation("Adding {0} header {1} to request.", Constants.ClientRequestIdHeader, clientRequestId);
                    requestProperty.Headers[Constants.ClientRequestIdHeader] = clientRequestId;
                }
            }

            // Return the correlation state
            Guid correlationState = Guid.NewGuid();
            this._logger.LogDebugInformation("Setting correlation state of request to {0}.", Constants.ClientRequestIdHeader, correlationState);

            // The information below will be used by Microsoft to debug any issues using this API. Please do not remove or change it
            string clientReqIdLog = (string.IsNullOrEmpty(clientRequestId) ? "<NONE>" : clientRequestId);
            string userAgentLog = (string.IsNullOrEmpty(this._userAgent) ? "<NONE>" : this._userAgent);
            this._logger.LogInformation("REQUEST, {0}, {1}, {2}, {3}, {4}", requestProperty.Method, request.Headers.To.ToString(), clientReqIdLog, userAgentLog,
                correlationState);

            return correlationState;
        }

        #endregion

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // Empty
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) 
        {
            // Empty
        }

        public void Validate(ServiceEndpoint endpoint) 
        { 
            // Empty
        }

        #endregion

        #region Private Methods
        
        private static bool IsRestFault(HttpStatusCode statusCode)
        {
            int code = (int)statusCode;
            return ((code >= 400) && (code < 600));
        }

        private static string DefaultRequestIdGenerator()
        {
            return Guid.NewGuid().ToString();
        }

        #endregion

    }
}

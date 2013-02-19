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
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Xml;

    /// <summary>
    /// Implements a channel proxy to wrap the built-in channel.
    /// </summary>
    /// <remarks>
    /// The proxy is used to trap exceptions along the WCF channel that cannot be caught using the IClientMessageInspector hook.
    /// </remarks>
    public sealed class ServiceManagementChannelProxy : RealProxy
    {
        #region Member Variables

        private IServiceManagement _sink;

        private TraceSourceHelper _logger = null;
        private const string ComponentTraceName = "ServiceManagemenChannelProxy";

        #endregion

        /// <summary>
        /// Creates a new instance of ServiceManagementChannelProxy given a channel to the object.
        /// </summary>
        /// <param name="sink">Channel to IServiceManagement interface.</param>
        /// <param name="logger">.Net TraceSource to use for logging.</param>
        /// <param name="errorEventId">ID for the event to use when logging errors.</param>
        /// <exception cref="ArgumentNullException">Thrown if sink is null.</exception>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public ServiceManagementChannelProxy(IServiceManagement sink, TraceSource logger, int errorEventId)
            : base(typeof(IServiceManagement))
        {
            ArgumentValidator.CheckIfNull("sink", sink);
            
            this._sink = sink;
            this._logger = new TraceSourceHelper(logger, errorEventId, ServiceManagementChannelProxy.ComponentTraceName);
        }

        /// <summary>
        /// Invokes the Service Management operation.
        /// </summary>
        /// <param name="msg">Message containing the SM information.</param>
        /// <returns>The response message from the Service Management operation.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override System.Runtime.Remoting.Messaging.IMessage Invoke(System.Runtime.Remoting.Messaging.IMessage msg)
        {
            MethodCallMessageWrapper messageCaller = new MethodCallMessageWrapper((IMethodCallMessage)msg);
            MethodInfo methodInformation = (MethodInfo)messageCaller.MethodBase;

            try
            {
                this._logger.LogDebugInformation("Attempting to invoke {0} method on Service Management API.", methodInformation.Name);
                object result = methodInformation.Invoke(this._sink, messageCaller.Args);
                return new ReturnMessage(result, messageCaller.Args, messageCaller.Args.Length, messageCaller.LogicalCallContext, messageCaller);
            }
            catch (TargetInvocationException ex)
            {
                this._logger.LogDebugInformation("An error occurred calling the Service Management API.");
                
                // Handle HTTP responses and rethrow anything unrelated
                if (ex.InnerException == null)
                {
                    this._logger.LogError("Exception occurred while executing Service Management API operation: {0}.", ex);
                    throw;
                }

                CommunicationException cex = ex.InnerException as CommunicationException;
                if (cex == null)
                {
                    this._logger.LogError("Exception occurred while executing Service Management API operation: {0}.", ex.InnerException);
                    throw ex.InnerException;
                }

                Exception restEx = this.GetRestFaultFromComEx(cex);
                this._logger.LogError("Exception occurred while executing Service Management API operation: {0}.", restEx);
                throw restEx;
            }
        }

        /// <summary>
        /// Gets an interface to the proxy.
        /// </summary>
        /// <typeparam name="T">The interface type requested.</typeparam>
        /// <param name="proxy">The proxy instance.</param>
        /// <returns>Interface of T to the proxy.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static T GetInterface<T>(object proxy)
        {
            ServiceManagementChannelProxy realProxy = (ServiceManagementChannelProxy)RemotingServices.GetRealProxy(proxy);
            return (T)realProxy._sink;
        }

        #region Private Methods

        private Exception GetRestFaultFromComEx(CommunicationException cex)
        {
            // Make sure the exception is a REST fault
            WebException wex = cex.InnerException as WebException;
            if (wex == null)
            {
                return cex;
            }

            // Make sure there is a response to parse
            HttpWebResponse response = wex.Response as HttpWebResponse;
            if (response == null)
            {
                return cex;
            }
            
            ServiceManagementError errorDetails = null;
            string operationId = string.Empty;

            // Get the operation tracking ID from the response header
            if (response.Headers != null)
            {
                operationId = response.Headers[Constants.OperationTrackingIdHeader];
            }
            this._logger.LogDebugInformation("Operation ID of the failed operation: {0}.", string.IsNullOrEmpty(operationId) ? "<NONE>" : operationId);

            try
            {
                // Try to read the ServiceManagementError object from the stream
                Stream s = response.GetResponseStream();
                if (s.Length > 0)
                {
                    try
                    {
                        this._logger.LogDebugInformation("Attempting to parse error details.");
                        
                        // This using statement will also clean up the underlying stream
                        using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(s, new XmlDictionaryReaderQuotas())) 
                        {
                            DataContractSerializer ser = new DataContractSerializer(typeof(ServiceManagementError));
                            errorDetails = (ServiceManagementError)ser.ReadObject(reader, true);
                            this._logger.LogDebugInformation("Error details processed successfully.");
                        }
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
            ServiceManagementClientException smError = new ServiceManagementClientException(response.StatusCode, errorDetails, operationId, response.Headers);

            string smErrorCode = "<NONE>";
            string smErrorMesage = "<NONE>";
            if (smError.ErrorDetails != null)
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

            // The information below will be used by Microsoft to debug any issues using this API. Please do not remove or change it
            this._logger.LogInformation("PROXYERROR, {0}, {1}, {2}, {3}", (int)response.StatusCode, smErrorCode, smErrorMesage, 
                string.IsNullOrEmpty(operationId) ? "<NONE>" : operationId);

            this._logger.LogError("Error details: {0}", smError);
            throw smError;
        }

        #endregion
    }
}

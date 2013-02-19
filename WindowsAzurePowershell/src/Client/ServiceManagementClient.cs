/**
* Copyright Microsoft Corporation  2012
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
    using System.Globalization;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Client for calling Windows Azure Service Management API.
    /// </summary>
    public partial class ServiceManagementClient : IDisposable
    {
        public static readonly Uri ServiceManagementUri = new Uri("https://management.core.windows.net");

        private const string ComponentTraceName = "ServiceManagementClient";
        private const int PollAsyncOperationIntervalSeconds = 30;

        #region Member Variables

        private bool _disposed = false;
        private IServiceManagement _service = null;
        private WebChannelFactory<IServiceManagement> _factory = null;
        private X509Certificate2 _clientCert = null;

        private TraceSourceHelper _logger = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance of the IServiceManagement interface used for calling the ServiceManagement API.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the ServiceManagementClient has already been disposed.</exception>
        public IServiceManagement Service 
        {
            get
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(Resources.AccessDisposedClientError);
                }
                
                return this._service;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of the ServiceManagementClient.
        /// </summary>
        /// <param name="clientCert">The client certificate used to authenticate against the Service Management API.</param>
        /// <param name="clientOptions">The options for constructing this client. Object must not be null, but any of the individual members can be.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        [SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public ServiceManagementClient(X509Certificate2 clientCert, ServiceManagementClientOptions clientOptions)
        {
            ArgumentValidator.CheckIfNull("clientCert", clientCert);
            ArgumentValidator.CheckIfNull("clientOptions", clientOptions);

            this._factory = new WebChannelFactory<IServiceManagement>();
            this._clientCert = clientCert;

            this.Initialize(clientOptions);
        }

        /// <summary>
        /// Creates an instance of the ServiceManagementClient.
        /// </summary>
        /// <param name="endpoint">The endpoint representing the Service Management service.</param>
        /// <param name="clientCert">The client certificate used to authenticate against the Service Management API.</param>
        /// <param name="clientOptions">The options for constructing this client. Object must not be null, but any of the induvidual members can be.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        [SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public ServiceManagementClient(ServiceEndpoint endpoint, X509Certificate2 clientCert, ServiceManagementClientOptions clientOptions)
        {
            ArgumentValidator.CheckIfNull("endpoint", endpoint);
            ArgumentValidator.CheckIfNull("clientCert", clientCert);
            ArgumentValidator.CheckIfNull("clientOptions", clientOptions);

            this._factory = new WebChannelFactory<IServiceManagement>(endpoint);
            this._clientCert = clientCert;
            
            this.Initialize(clientOptions);
        }

        /// <summary>
        /// Creates an instance of the ServiceManagementClient.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the Service Management service endpoint configuration element.</param>
        /// <param name="clientCert">The client certificate used to authenticate against the Service Management API.</param>
        /// <param name="clientOptions">The options for constructing this client. Object must not be null, but any of the induvidual members can be.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required string parameters are empty.</exception>
        [SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public ServiceManagementClient(string endpointConfigurationName, X509Certificate2 clientCert, ServiceManagementClientOptions clientOptions)
        {
            ArgumentValidator.CheckIfNull("endpointConfigurationName", endpointConfigurationName);
            ArgumentValidator.CheckIfEmptyString("endpointConfigurationName", endpointConfigurationName);
            ArgumentValidator.CheckIfNull("clientCert", clientCert);
            ArgumentValidator.CheckIfNull("clientOptions", clientOptions);

            this._factory = new WebChannelFactory<IServiceManagement>(endpointConfigurationName);
            this._clientCert = clientCert;
                        
            this.Initialize(clientOptions);
        }

        /// <summary>
        /// Creates an instance of the ServiceManagementClient.
        /// </summary>
        /// <param name="endpointConfigurationName">The name of the Service Management service endpoint configuration element.</param>
        /// <param name="remoteUri">The remote address of the service endpoint.</param>
        /// <param name="clientCert">The client certificate used to authenticate against the Service Management API.</param>
        /// <param name="clientOptions">The options for constructing this client. Object must not be null, but any of the induvidual members can be.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required string parameters are empty.</exception>
        [SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public ServiceManagementClient(string endpointConfigurationName, Uri remoteUri, X509Certificate2 clientCert, ServiceManagementClientOptions clientOptions)
        {
            ArgumentValidator.CheckIfNull("endpointConfigurationName", endpointConfigurationName);
            ArgumentValidator.CheckIfEmptyString("endpointConfigurationName", endpointConfigurationName);
            ArgumentValidator.CheckIfNull("remoteUri", remoteUri);
            ArgumentValidator.CheckIfNull("clientCert", clientCert);
            ArgumentValidator.CheckIfNull("clientOptions", clientOptions);

            this._factory = new WebChannelFactory<IServiceManagement>(endpointConfigurationName, remoteUri);
            this._clientCert = clientCert;
            
            this.Initialize(clientOptions);
        }

        /// <summary>
        /// Creates an instance of the ServiceManagementClient.
        /// </summary>
        /// <param name="binding">The binding representing the ServiceManagement service.</param>
        /// <param name="remoteUri">The remote address of the service endpoint.</param>
        /// <param name="clientCert">The client certificate used to authenticate against the Service Management API.</param>
        /// <param name="clientOptions">The options for constructing this client. Object must not be null, but any of the induvidual members can be.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        [SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public ServiceManagementClient(Binding binding, Uri remoteUri, X509Certificate2 clientCert, ServiceManagementClientOptions clientOptions)
        {
            ArgumentValidator.CheckIfNull("binding", binding);
            ArgumentValidator.CheckIfNull("remoteUri", remoteUri);
            ArgumentValidator.CheckIfNull("clientCert", clientCert);
            ArgumentValidator.CheckIfNull("clientOptions", clientOptions);

            this._factory = new WebChannelFactory<IServiceManagement>(binding, remoteUri);
            this._clientCert = clientCert;

            this.Initialize(clientOptions);
        }        

        #endregion

        /// <summary>
        /// Runs any Service Management operation and returns the tracking ID.
        /// </summary>
        /// <param name="smApiMethod">Action delegate for the Service Management operation.</param>
        /// <returns>The operation ID or an empty string if the operation executed synchronously.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        public string RunRequestAndGetTrackingId(Action smApiMethod)
        {
            ArgumentValidator.CheckIfNull("smApiMethod", smApiMethod);

            string operationTrackingId = string.Empty;

            IContextChannel currentContext = this.Service.ToContextChannel();
            using (OperationContextScope scope = new OperationContextScope(currentContext))
            {
                this._logger.LogInformation("Attempting to run async method on {0}.", currentContext.RemoteAddress);
                smApiMethod();

                if ((WebOperationContext.Current != null) &&
                    (WebOperationContext.Current.IncomingResponse != null) &&
                    (WebOperationContext.Current.IncomingResponse.Headers != null))
                {
                    operationTrackingId = WebOperationContext.Current.IncomingResponse.Headers[Constants.OperationTrackingIdHeader];
                }
            }

            this._logger.LogInformation("Operation started with ID {0}.", operationTrackingId);
            return operationTrackingId;
        }

        /// <summary>
        /// Waits for an executing Service Management operation to complete.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID associated with the Service Management API call.</param>
        /// <param name="operationTrackingId">The tracking ID associated with the running operation.</param>
        /// <param name="waitTimeout">The TimeSpan to wait for success before timing out.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required string parameters are empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the Service Management service returns a value that this client can't process.</exception>
        /// <exception cref="ServiceManagementClientException">Thrown if the Service Management operation did not complete successfully.</exception>
        /// <exception cref="TimeoutException">Thrown if the timeout period is exceeded without the operation completing.</exception>
        public void WaitForOperationToComplete(string subscriptionId, string operationTrackingId, TimeSpan waitTimeout)
        {
            ArgumentValidator.CheckIfNull("subscriptionId", subscriptionId);
            ArgumentValidator.CheckIfEmptyString("subscriptionId", subscriptionId);
            ArgumentValidator.CheckIfNull("operationTrackingId", operationTrackingId);
            ArgumentValidator.CheckIfEmptyString("operationTrackingId", operationTrackingId);

            DateTimeOffset waitEndTime = DateTimeOffset.UtcNow.Add(waitTimeout);
            this._logger.LogInformation("Waiting for operation {0} to complete until {1}.", operationTrackingId, waitEndTime.ToUniversalTime());
            Operation oper = null;
            do
            {
                this._logger.LogInformation("Getting status for operation {0}.", operationTrackingId);
                oper = this.Service.GetOperationStatus(subscriptionId, operationTrackingId);
                if (oper != null)
                {
                    this._logger.LogInformation("Operation {0}: {1}.", operationTrackingId, oper.Status);
                    if (!oper.Status.Equals(OperationState.InProgress, StringComparison.InvariantCultureIgnoreCase))
                    {
                        this._logger.LogInformation("Operation is {0} is no longer in progress.", operationTrackingId);
                        break;
                    }
                }


                this._logger.LogInformation("Operation {0} is still running. Waiting for an additional {1} seconds.", operationTrackingId, 
                    ServiceManagementClient.PollAsyncOperationIntervalSeconds);
                Thread.Sleep(TimeSpan.FromSeconds(ServiceManagementClient.PollAsyncOperationIntervalSeconds));

            } while ((oper.Status.Equals(OperationState.InProgress, StringComparison.InvariantCultureIgnoreCase)) && (DateTimeOffset.UtcNow < waitEndTime));

            if ((oper == null) || (oper.Status.Equals(OperationState.InProgress, StringComparison.InvariantCultureIgnoreCase)))
            {
                this._logger.LogError("Operation {0} did not complete within {1} seconds.", operationTrackingId, waitTimeout.TotalSeconds);
                throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, Resources.ServiceManagementTimeoutError, oper.OperationTrackingId, waitTimeout.TotalSeconds));
            }
            else if (oper.Status.Equals(OperationState.Failed, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!Enum.IsDefined(typeof(HttpStatusCode), oper.HttpStatusCode))
                {
                    // This case should never hit unless new HTTP status code methods are added on the server but not in .Net
                    this._logger.LogError("Operation {0} returned an unrecognized HTTP status: {1}.", operationTrackingId, oper.HttpStatusCode);
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.UnknownHttpStatusError, oper.OperationTrackingId, oper.HttpStatusCode));
                }
                HttpStatusCode errorHttpStatus = (HttpStatusCode)Enum.ToObject(typeof(HttpStatusCode), oper.HttpStatusCode);
                if (oper.Error == null)
                {
                    this._logger.LogError("Operation {0} did not complete successfully. HTTP status: {1}.", operationTrackingId, errorHttpStatus);
                }
                else
                {
                    this._logger.LogError("Operation {0} did not complete successfully. HTTP status: {1}. Service Management error code: {2}. Service Management error message: {3}.", 
                        operationTrackingId, 
                        errorHttpStatus, 
                        oper.Error.Code == null ? "<NONE>" : oper.Error.Code,
                        oper.Error.Message == null ? "<NONE>" : oper.Error.Message);
                }
                throw new ServiceManagementClientException(errorHttpStatus, oper.Error, oper.OperationTrackingId);
            }            
            else
            {
                // Do nothing if the operation has completed successfully
                if (!oper.Status.Equals(OperationState.Succeeded, StringComparison.InvariantCultureIgnoreCase))
                {
                    this._logger.LogError("Operation {0} returned an unrecognized operation status: {1}.", operationTrackingId, oper.Status);
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.UnknownOperationStatus, oper.OperationTrackingId, oper.Status));
                }

                this._logger.LogInformation("Operation {0} completed successfully.", operationTrackingId);
            }
        }

        /// <summary>
        /// Runs an asychronous Service Management operation and waits for it to complete.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID associated with the Service Management API call.</param>
        /// <param name="waitTimeout">The TimeSpan to wait for success before timing out.</param>
        /// <param name="smApiMethod">Action delegate for the Service Management operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required string parameters are empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the Service Management service returns a value that this client can't procsess.</exception>
        /// <exception cref="ServiceManagementClientException">Thrown if the Service Management operation did not complete successfully.</exception>
        /// <exception cref="TimeoutException">Thrown if the timeout period is exceeded without the operation completing.</exception>
        public void RunAsyncRequestAndWaitForCompletion(string subscriptionId, TimeSpan waitTimeout, Action smApiMethod)
        {
            ArgumentValidator.CheckIfNull("subscriptionId", subscriptionId);
            ArgumentValidator.CheckIfEmptyString("subscriptionId", subscriptionId);
            ArgumentValidator.CheckIfNull("smApiMethod", smApiMethod);

            this._logger.LogInformation("Running operation and getting tracking ID.");
            string operationTrackingId = this.RunRequestAndGetTrackingId(smApiMethod);

            if (!string.IsNullOrEmpty(operationTrackingId))
            {
                this.WaitForOperationToComplete(subscriptionId, operationTrackingId, waitTimeout);
            }
        }

        /// <summary>
        /// Encodes a string to base 64.
        /// </summary>
        /// <param name="original">String to encode.</param>
        /// <returns>Base 64 version of the string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required string parameters are empty.</exception>
        public static string EncodeToBase64String(string original)
        {
            ArgumentValidator.CheckIfNull("original", original);
            ArgumentValidator.CheckIfEmptyString("original", original);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(original));
        }

        /// <summary>
        /// Decodes a base 64 string into readable text.
        /// </summary>
        /// <param name="original">Base 64 string.</param>
        /// <returns>Decoded version of the string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required string parameters are empty.</exception>
        public static string DecodeFromBase64String(string original)
        {
            ArgumentValidator.CheckIfNull(original, "original");
            ArgumentValidator.CheckIfEmptyString(original, "original");

            return Encoding.UTF8.GetString(Convert.FromBase64String(original));
        }

        /// <summary>
        /// Closes the WCF communication channel to the service and sets the instance of ServiceManagementClient.Service to null.
        /// </summary>
        /// <remarks>
        /// This should always be called after receiving a WCF communication error and a new client should be created otherwise there is risk of future failure,
        /// from calling a channel in a faulted state.
        /// </remarks>
        public void CloseChannel()
        {
            this.Dispose(true);
        }

        #region Private Methods

        private void Initialize(ServiceManagementClientOptions clientOptions)
        {
            if (this._disposed)
            {
                throw new InvalidOperationException(Resources.InitDisposedClientError);
            }

            this._logger = new TraceSourceHelper(clientOptions.Logger, clientOptions.ErrorEventId, ServiceManagementClient.ComponentTraceName);
            this._logger.LogDebugInformation("Logger has been successfully initialized for the ServiceManagementClient.");

            this._logger.LogDebugInformation("Adding custom ClientOutputMessageInspector.");
            this._factory.Endpoint.Behaviors.Add(new ClientMessageInspector(clientOptions.UserAgentString, clientOptions.ClientRequestIdGenerator,
                clientOptions.Logger, clientOptions.ErrorEventId));

            if (clientOptions.EndpointBehaviors.Count > 0)
            {
                this._logger.LogDebugInformation("Adding custom EndpointBehaviors.");
                foreach (var endpointBehavior in clientOptions.EndpointBehaviors)
                {
                    this._factory.Endpoint.Behaviors.Add(endpointBehavior);
                }
            }

            this._logger.LogInformation("Using client certificate with thumbprint {0}.", this._clientCert.Thumbprint);
            this._factory.Credentials.ClientCertificate.Certificate = this._clientCert;

            this._logger.LogDebugInformation("Creating custom ServiceManagemenChannelProxy.");
            ServiceManagementChannelProxy proxy = new ServiceManagementChannelProxy(this._factory.CreateChannel(), clientOptions.Logger, clientOptions.ErrorEventId);

            this._logger.LogDebugInformation("Returning custom proxy channel to client.");
            this._service = (IServiceManagement)proxy.GetTransparentProxy();

            this._logger.LogInformation("Please use the following guidelines for understanding the log statements:");
            this._logger.LogInformation("\tREQUEST, HTTP verb, request URL, x-ms-client-id header, UserAgent header, WCF correlation state.");
            this._logger.LogInformation("\tRESPONSE, HTTP status code, Service Management error code, Service Management error message, x-ms-request-id header, WCF correlation state.");
            this._logger.LogInformation("\tPROXYERROR, HTTP status code, Service Management error code, Service Management error message, x-ms-request-id header");
        }       

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            this._logger.LogDebugInformation("Cleaning up Service Management client.");
            if ((!this._disposed) && (disposing))
            {
                if (this.Service != null)
                {
                    IContextChannel channel = this.Service.ToContextChannel();
                    if (channel != null)
                    {
                        if ((channel.State != CommunicationState.Closed) && (channel.State != CommunicationState.Closing))
                        {
                            if (channel.State == CommunicationState.Faulted)
                            {
                                channel.Abort();
                            }
                            else
                            {
                                channel.Close();
                            }
                        }
                    }

                    this._service = null;
                }

                if (this._factory != null)
                {
                    if ((this._factory.State != CommunicationState.Closed) && (this._factory.State != CommunicationState.Closing))
                    {
                        if (this._factory.State != CommunicationState.Faulted)
                        {
                            this._factory.Abort();
                        }
                        else
                        {
                            this._factory.Close();
                        }
                    }
                    this._factory = null;
                }

                this._disposed = true;
                this._clientCert = null;
            }
        }

        #endregion
    }

    public static partial class ServiceManagementExtensions
    {
        /// <summary>
        /// Gets the IContextChannel interface on the IServiceManagement object.
        /// </summary>
        /// <param name="client">Instance of the IServiceManagement proxy.</param>
        /// <returns>IContextChannel representation of IServiceManagement proxy.</returns>
        [SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static IContextChannel ToContextChannel(this IServiceManagement client)
        {
            return ServiceManagementChannelProxy.GetInterface<IContextChannel>(client);
        }

        public static HostedServiceList ListHostedServicesWithDetails(this IServiceManagement proxy, string subscriptionId, ref string continuationToken)
        {
            WebOperationContext context = WebOperationContext.Current;
            OperationContextScope scope = null;
            if (context == null)
            {
                scope = new OperationContextScope(proxy.ToContextChannel());
            }

            try
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
            finally
            {
                if (scope != null)
                {
                    scope.Dispose();
                }
            }
        }
    }
}

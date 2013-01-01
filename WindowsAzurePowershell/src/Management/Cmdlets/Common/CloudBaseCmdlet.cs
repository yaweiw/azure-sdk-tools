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

namespace Microsoft.WindowsAzure.Management.Cmdlets.Common
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Web;
    using System.Threading;
    using Extensions;
    using Microsoft.WindowsAzure.Management.Service;
    using Microsoft.WindowsAzure.Management.Services;
    using Model;
    using Properties;
    using Samples.WindowsAzure.ServiceManagement;
    using Service.Gateway;
    using Utilities;
    using ServiceManagementHelper = Samples.WindowsAzure.ServiceManagement.ServiceManagementHelper2;

    public abstract class CloudBaseCmdlet<T> : CmdletBase
        where T : class
    {
        private SubscriptionData _currentSubscription;

        public string CurrentServiceEndpoint { get; set; }

        public Binding ServiceBinding
        {
            get;
            set;
        }

        public string ServiceEndpoint
        {
            get;
            set;
        }

        protected T Channel
        {
            get;
            set;
        }

        public SubscriptionData CurrentSubscription
        {
            get
            {
                if (_currentSubscription == null)
                {
                    _currentSubscription = this.GetCurrentSubscription();
                }

                return _currentSubscription;
            }

            set
            {
                if (_currentSubscription != value)
                {
                    _currentSubscription = value;

                    // Recreate the channel if necessary
                    if (!ShareChannel)
                    {
                        InitChannelCurrentSubscription(true);
                    }
                }
            }
        }

        public int MaxStringContentLength
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the current subscription to the passed subscription name. If null, no changes.
        /// </summary>
        /// <param name="subscriptionName">The subscription name</param>
        public void SetCurrentSubscription(string subscriptionName)
        {
            if (!string.IsNullOrEmpty(subscriptionName))
            {
                GlobalComponents globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory);
                CurrentSubscription = globalComponents.Subscriptions.Values.First(sub => sub.SubscriptionName == subscriptionName);
            }
        }

        protected void InitChannelCurrentSubscription()
        {
            InitChannelCurrentSubscription(false);
        }

        protected virtual void InitChannelCurrentSubscription(bool force)
        {
            if (CurrentSubscription == null)
            {
                throw new ArgumentException(Resources.InvalidCurrentSubscription);
            }

            if (CurrentSubscription.Certificate == null)
            {
                throw new ArgumentException(Resources.InvalidCurrentSuscriptionCertificate);
            }

            if (String.IsNullOrEmpty(CurrentSubscription.SubscriptionId))
            {
                throw new ArgumentException(Resources.InvalidCurrentSubscriptionId);
            }

            if (Channel == null || force)
            {
                Channel = CreateChannel();
            }
        }

        protected virtual void OnProcessRecord()
        {
            // Intentionally left blank
        }

        protected override void ProcessRecord()
        {
            try
            {
                Validate.ValidateInternetConnection();
                InitChannelCurrentSubscription();
                ExecuteCmdlet();
                OnProcessRecord();
            }
            catch (CommunicationException ex)
            {
                WriteErrorDetails(ex);
            }
            catch (Exception ex)
            {
                WriteExceptionError(ex);
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether CreateChannel should share
        /// the command's current Channel when asking for a new one.  This is
        /// only used for testing.
        /// </summary>
        public bool ShareChannel { get; set; }

        protected virtual T CreateChannel()
        {
            // If ShareChannel is set by a unit test, use the same channel that
            // was passed into out constructor.  This allows the test to submit
            // a mock that we use for all network calls.
            if (ShareChannel)
            {
                return Channel;
            }

            if (ServiceBinding == null)
            {
                ServiceBinding = Microsoft.WindowsAzure.Management.Utilities.ConfigurationConstants.WebHttpBinding(MaxStringContentLength);
            }

            if (!string.IsNullOrEmpty(CurrentServiceEndpoint))
            {
                ServiceEndpoint = CurrentServiceEndpoint;
            }
            else if (!string.IsNullOrEmpty(CurrentSubscription.ServiceEndpoint))
            {
                ServiceEndpoint = CurrentSubscription.ServiceEndpoint;
            }
            else
            {
                // Use default endpoint
                ServiceEndpoint = Microsoft.WindowsAzure.Management.Utilities.ConfigurationConstants.ServiceManagementEndpoint;
            }

            return ServiceManagementHelper.CreateServiceManagementChannel<T>(ServiceBinding, new Uri(ServiceEndpoint), CurrentSubscription.Certificate);
        }

        protected void RetryCall(Action<string> call)
        {
            RetryCall(CurrentSubscription.SubscriptionId, call);
        }

        protected void RetryCall(string subsId, Action<string> call)
        {
            try
            {
                call(subsId);
            }
            catch (MessageSecurityException ex)
            {
                var webException = ex.InnerException as WebException;

                if (webException == null)
                {
                    throw;
                }

                var webResponse = webException.Response as HttpWebResponse;

                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    WriteError(new ErrorRecord(new Exception(Resources.CommunicationCouldNotBeEstablished, ex), string.Empty, ErrorCategory.InvalidData, null));
                }
                else
                {
                    throw;
                }
            }
        }

        protected TResult RetryCall<TResult>(Func<string, TResult> call)
        {
            return RetryCall(CurrentSubscription.SubscriptionId, call);
        }

        protected TResult RetryCall<TResult>(string subsId, Func<string, TResult> call)
        {
            try
            {
                return call(subsId);
            }
            catch (MessageSecurityException ex)
            {
                var webException = ex.InnerException as WebException;

                if (webException == null)
                {
                    throw;
                }

                var webResponse = webException.Response as HttpWebResponse;

                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    WriteError(new ErrorRecord(new Exception(Resources.CommunicationCouldNotBeEstablished, ex), string.Empty, ErrorCategory.InvalidData, null));
                    throw;
                }

                throw;
            }
        }

        protected Operation WaitForGatewayOperation(string opdesc)
        {
            Operation operation = null;
            String operationId = RetrieveOperationId();
            SubscriptionData currentSubscription = this.GetCurrentSubscription();
            try
            {
                IGatewayServiceManagement channel = (IGatewayServiceManagement)Channel;
                operation = RetryCall(s => channel.GetGatewayOperation(currentSubscription.SubscriptionId, operationId));

                var activityId = new Random().Next(1, 999999);
                var progress = new ProgressRecord(activityId, opdesc, "Operation Status: " + operation.Status);
                while (string.Compare(operation.Status, OperationState.Succeeded, StringComparison.OrdinalIgnoreCase) != 0 &&
                        string.Compare(operation.Status, OperationState.Failed, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    WriteProgress(progress);
                    Thread.Sleep(1 * 1000);
                    operation = RetryCall(s => channel.GetGatewayOperation(currentSubscription.SubscriptionId, operationId));
                }

                if (string.Compare(operation.Status, OperationState.Failed, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var errorMessage = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", operation.Status, operation.Error.Message);
                    var exception = new Exception(errorMessage);
                    WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
                }
            }
            catch (CommunicationException ex)
            {
                WriteErrorDetails(ex);
            }

            return operation;
        }

        protected virtual Operation WaitForOperation(string opdesc)
        {
            return WaitForOperation(opdesc, false);
        }

        protected virtual Operation WaitForOperation(string opdesc, bool silent)
        {
            string operationId = RetrieveOperationId();
            Operation operation = null;

            if (!string.IsNullOrEmpty(operationId))
            {
                try
                {
                    SubscriptionData currentSubscription = this.GetCurrentSubscription();

                    operation = RetryCall(s => GetOperationStatus(currentSubscription.SubscriptionId, operationId));

                    var activityId = new Random().Next(1, 999999);
                    var progress = new ProgressRecord(activityId, opdesc, "Operation Status: " + operation.Status);

                    while (string.Compare(operation.Status, OperationState.Succeeded, StringComparison.OrdinalIgnoreCase) != 0 &&
                            string.Compare(operation.Status, OperationState.Failed, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (silent == false)
                        {
                            WriteProgress(progress);
                        }

                        Thread.Sleep(1 * 1000);
                        operation = RetryCall(s => GetOperationStatus(currentSubscription.SubscriptionId, operationId));
                    }

                    if (string.Compare(operation.Status, OperationState.Failed, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var errorMessage = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", operation.Status, operation.Error.Message);
                        var exception = new Exception(errorMessage);
                        WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
                    }

                    if (silent == false)
                    {
                        progress = new ProgressRecord(activityId, opdesc, "Operation Status: " + operation.Status);
                        WriteProgress(progress);
                    }
                }
                catch (CommunicationException ex)
                {
                    WriteErrorDetails(ex);
                }
            }
            else
            {
                operation = new Operation
                {
                    OperationTrackingId = string.Empty,
                    Status = OperationState.Failed
                };
            }

            return operation;
        }

        /// <summary>
        /// Invoke the given operation within an OperationContextScope if the
        /// channel supports it.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        protected void InvokeInOperationContext(Action action)
        {
            IContextChannel contextChannel = Channel as IContextChannel;
            if (contextChannel != null)
            {
                using (new OperationContextScope(contextChannel))
                {
                    action();
                }
            }
            else
            {
                action();
            }
        }

        protected void ExecuteClientAction(object input, string operationDescription, Action<string> action, Func<string, Operation> waitOperation)
        {
            if (input != null)
            {
                this.WriteVerboseOutputForObject(input);
            }

            RetryCall(action);
            Operation operation = waitOperation(operationDescription);
            var context = new ManagementOperationContext
            {
                OperationDescription = operationDescription,
                OperationId = operation.OperationTrackingId,
                OperationStatus = operation.Status
            };

            WriteObject(context, true);
        }

        protected void ExecuteClientActionInOCS(object input, string operationDescription, Action<string> action, Func<string, Operation> waitOperation)
        {
            if (input != null)
            {
                this.WriteVerboseOutputForObject(input);
            }

            IContextChannel contextChannel = Channel as IContextChannel;
            if (contextChannel != null)
            {
                using (new OperationContextScope(contextChannel))
                {
                    try
                    {
                        RetryCall(action);
                        Operation operation = waitOperation(operationDescription);
                        var context = new ManagementOperationContext
                        {
                            OperationDescription = operationDescription,
                            OperationId = operation.OperationTrackingId,
                            OperationStatus = operation.Status
                        };

                        WriteObject(context, true);
                    }
                    catch (CommunicationException ex)
                    {
                        WriteErrorDetails(ex);
                    }
                }
            }
            else
            {
                RetryCall(action);
            }
        }

        protected void ExecuteClientActionInOCS<TResult>(object input, string operationDescription, Func<string, TResult> action, Func<string, Operation> waitOperation, Func<Operation, TResult, object> contextFactory) where TResult : class
        {
            if (input != null)
            {
                this.SafeWriteVerboseOutputForObject(input);
            }

            IContextChannel contextChannel = Channel as IContextChannel;
            if (contextChannel != null)
            {
                using (new OperationContextScope(contextChannel))
                {
                    try
                    {
                        TResult result = RetryCall(action);
                        Operation operation = waitOperation(operationDescription);
                        if (result != null)
                        {
                            object context = contextFactory(operation, result);
                            WriteObject(context, true);
                        }
                    }
                    catch (CommunicationException ex)
                    {
                        WriteErrorDetails(ex);
                    }
                }
            }
            else
            {
                TResult result = RetryCall(action);
                if (result != null)
                {
                    WriteObject(result, true);
                }
            }
        }

        protected TResult ExecuteClientGetAction<TResult, TChannelResult>(Func<string, TChannelResult> channelCall, Func<TChannelResult, TResult> resultFactory, out Operation operation)
        {
            operation = null;

            IContextChannel contextChannel = Channel as IContextChannel;
            if (contextChannel != null)
            {
                try
                {
                    using (new OperationContextScope(contextChannel))
                    {
                        var deployment = RetryCall(channelCall);

                        operation = WaitForOperation(CommandRuntime.ToString());

                        return resultFactory(deployment);
                    }
                }
                catch (CommunicationException ex)
                {
                    if (ex is EndpointNotFoundException && !IsVerbose())
                    {
                        return default(TResult);
                    }

                    WriteErrorDetails(ex);
                }
            }
            else
            {
                var deployment = RetryCall(channelCall);
                return resultFactory(deployment);
            }

            return default(TResult);
        }

        protected virtual Operation GetOperationStatus(string subscriptionId, string operationId)
        {
            var channel = (IServiceManagement)Channel;
            return channel.GetOperationStatus(subscriptionId, operationId);
        }

        protected virtual void WriteErrorDetails(CommunicationException exception)
        {
            ServiceManagementError error;
            ErrorRecord errorRecord = null;

            string operationId;
            SMErrorHelper.TryGetExceptionDetails(exception, out error, out operationId);
            if (error == null)
            {
                errorRecord = new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null);
            }
            else
            {
                string errorDetails = string.Format(
                    CultureInfo.InvariantCulture,
                    "HTTP Status Code: {0} - HTTP Error Message: {1}\nOperation ID: {2}",
                    error.Code,
                    error.Message,
                    operationId);

                errorRecord = new ErrorRecord(new CommunicationException(errorDetails), string.Empty, ErrorCategory.CloseError, null);
            }

            if (CommandRuntime != null)
            {
                WriteError(errorRecord);
            }
        }

        /// <summary>
        /// Wrap the base Cmdlet's WriteError call so that it will not throw
        /// a NotSupportedException when called without a CommandRuntime (i.e.,
        /// when not called from within Powershell).
        /// </summary>
        /// <param name="errorRecord">The error to write.</param>
        protected void WriteWindowsAzureError(ErrorRecord errorRecord)
        {
            // If the exception is an Azure Service Management error, pull the
            // Azure message out to the front instead of the generic response.
            errorRecord = AzureServiceManagementException.WrapExistingError(errorRecord);
        }

        protected static string RetrieveOperationId()
        {
            var operationId = string.Empty;

            if ((WebOperationContext.Current != null) && (WebOperationContext.Current.IncomingResponse != null))
            {
                operationId = WebOperationContext.Current.IncomingResponse.Headers[Microsoft.Samples.WindowsAzure.ServiceManagement.Constants.OperationTrackingIdHeader];
            }

            return operationId;
        }
    }
}

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

namespace Microsoft.WindowsAzure.Management.Cmdlets.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using Microsoft.WindowsAzure.Management.Utilities;
    using Model;
    using ServiceManagement;

    public abstract class ServiceManagementBaseCmdlet : CloudBaseCmdlet<IServiceManagement>
    {
        protected override IServiceManagement CreateChannel()
        {
            // If ShareChannel is set by a unit test, use the same channel that
            // was passed into out constructor.  This allows the test to submit
            // a mock that we use for all network calls.
            if (ShareChannel)
            {
                return Channel;
            }

            var messageInspectors = new List<IClientMessageInspector>
            {
                new ServiceManagementClientOutputMessageInspector(),
                new HttpRestMessageInspector(this.WriteDebug)
            };

            var clientOptions = new ServiceManagementClientOptions(null, null, null, 0, RetryPolicy.NoRetryPolicy, ServiceManagementClientOptions.DefaultOptions.WaitTimeForOperationToComplete, messageInspectors);
            //var smClient = new ServiceManagementClient(this.ServiceBinding, new Uri(this.ServiceEndpoint), CurrentSubscription.Certificate, clientOptions);
            var smClient = new ServiceManagementClient(new Uri(this.ServiceEndpoint), CurrentSubscription.SubscriptionId, CurrentSubscription.Certificate, clientOptions);
            return smClient.Service;
        }

        /// <summary>
        /// Invoke the given operation within an OperationContextScope if the
        /// channel supports it.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        protected override void InvokeInOperationContext(Action action)
        {
            IContextChannel contextChannel = ToContextChannel();
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

        protected virtual IContextChannel ToContextChannel()
        {
            try
            {
                return Channel.ToContextChannel();
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void ExecuteClientAction(object input, string operationDescription, Action<string> action, Func<string, Operation> waitOperation)
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

        protected void ExecuteClientActionInOCS(object input, string operationDescription, Action<string> action, Func<string, Operation> waitOperation)
        {
            IContextChannel contextChannel = null;
            try
            {
                contextChannel = Channel.ToContextChannel();
            }
            catch (Exception)
            {
                // Do nothing, proceed.
            }
            
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
                    catch (ServiceManagementClientException  ex)
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

        protected virtual void WriteErrorDetails(ServiceManagementClientException exception)
        {
            if (CommandRuntime != null)
            {
                WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        protected void ExecuteClientActionInOCS<TResult>(object input, string operationDescription, Func<string, TResult> action, Func<string, Operation> waitOperation, Func<Operation, TResult, object> contextFactory) where TResult : class
        {
            IContextChannel contextChannel = null;

            try
            {
                contextChannel = Channel.ToContextChannel();
            }
            catch (Exception)
            {
                // Do nothing, proceed.
            }

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
                    catch (ServiceManagementClientException ex)
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

        protected override Operation GetOperationStatus(string subscriptionId, string operationId)
        {
            var channel = (IServiceManagement)Channel;
            return channel.GetOperationStatusTask(subscriptionId, operationId).Result;
        }

        protected override Operation WaitForOperation(string opdesc)
        {
            return WaitForOperation(opdesc, false);
        }

        protected override Operation WaitForOperation(string opdesc, bool silent)
        {
            string operationId = RetrieveOperationId();
            Operation operation = null;

            if (!string.IsNullOrEmpty(operationId))
            {
                try
                {
                    SubscriptionData currentSubscription = this.CurrentSubscription;

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
                catch (ServiceManagementClientException ex)
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
    }
}
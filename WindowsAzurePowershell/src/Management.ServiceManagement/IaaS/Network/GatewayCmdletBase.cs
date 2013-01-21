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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.ServiceModel;
    using Samples.WindowsAzure.ServiceManagement;
    using Service;
    using Service.Gateway;
    using Cmdlets.Common;

    public class GatewayCmdletBase : CloudBaseCmdlet<IGatewayServiceManagement>
    {
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

        protected override IGatewayServiceManagement CreateChannel()
        {
            if (ServiceBinding == null)
            {
                ServiceBinding = ConfigurationConstants.WebHttpBinding();
            }

            if (string.IsNullOrEmpty(CurrentSubscription.ServiceEndpoint))
            {
                ServiceEndpoint = ConfigurationConstants.ServiceManagementEndpoint;
            }
            else
            {
                ServiceEndpoint = CurrentSubscription.ServiceEndpoint;
            }

            return GatewayManagementHelper.CreateGatewayManagementChannel(ServiceBinding, new Uri(ServiceEndpoint), CurrentSubscription.Certificate);
        }

        protected override void WriteErrorDetails(CommunicationException exception)
        {
            ServiceManagementError error;
            GatewayManagementHelper.TryGetExceptionDetails(exception, out error);

            if (error == null)
            {
                WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
            }
            else
            {
                string errorDetails = string.Format(
                    CultureInfo.InvariantCulture,
                    "HTTP Status Code: {0} - HTTP Error Message: {1}",
                    error.Code,
                    error.Message);

                WriteError(new ErrorRecord(new CommunicationException(errorDetails), string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
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

namespace Microsoft.WindowsAzure.Management.Websites.Cmdlets.Common
{
    using System;
    using System.Management.Automation;
    using Management.Cmdlets.Common;
    using Properties;
    using Samples.WindowsAzure.ServiceManagement;
    using Services;

    public abstract class WebsitesCmdletBase : CloudBaseCmdlet<IWebsitesServiceManagement>
    {
        protected override Operation WaitForOperation(string opdesc)
        {
            string operationId = RetrieveOperationId();
            Operation operation = new Operation();
            operation.OperationTrackingId = operationId;
            operation.Status = "Success";
            return operation;
        }

        internal abstract bool ExecuteCommand();

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                if (ExecuteCommand())
                {
                    SafeWriteObjectWithTimestamp(Resources.CompleteMessage);
                }
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}

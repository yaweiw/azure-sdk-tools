// ----------------------------------------------------------------------------------
//
// Copyright 2012 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ---------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Storage.Model;
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    public class BaseCmdlet : CloudBaseCmdlet<IServiceManagement>
    {
        //FIXME this operationContext are from SDK
        protected OperationContext operationContext = null;
        private int restCallCount = 0;

        internal void InitOperationContext()
        {
            operationContext = new OperationContext();
            operationContext.StartTime = DateTime.Now;
            operationContext.SendingRequest += (s, e) =>
            {
                restCallCount++;
                string message = String.Format("Start {0}th remote call: {1} {2}",
                    restCallCount, e.Request.Method, e.Request.RequestUri.ToString());
                SafeWriteVerboseLog(message);
            };
            //FIXME can not work with ctrl + c
            operationContext.ResponseReceived += (s, e) =>
            {
                string message = String.Format("Finish remote call with status code {0} and service request id {1}",
                    e.Response.StatusCode, e.RequestInformation.ServiceRequestID);
                SafeWriteVerboseLog(message);
            };
            operationContext.ClientRequestID = GetClientRequestID();
            SafeWriteVerboseLog("Init Operation Context with operation id " + operationContext.ClientRequestID);
        }

        internal string GetClientRequestID()
        {
            string prefix = "Azure-Storage-PowerShell-";
            string uniqueId = System.Guid.NewGuid().ToString();
            return prefix + uniqueId;
        }

        //FIXME tips should not in pipeline and can not be sorted.
        internal void SafeWriteTips(string message)
        {
            //WriteOutputObject(message);
        }

        internal void SafeWriteVerboseLog(string msg)
        {
            string time = DateTime.Now.ToString();
            string log = String.Format("{0} {1}", time, msg);
            SafeWriteVerbose(log);
        }

        internal double GetRunningMilliseconds()
        {
            if (operationContext == null)
            {
                return 0;
            }
            TimeSpan span = DateTime.Now - operationContext.StartTime;
            return span.TotalMilliseconds;
        }

        protected override void BeginProcessing()
        {
            InitOperationContext();
            string message = String.Format("using ParameterSet {0}", ParameterSetName);
            if (string.IsNullOrEmpty(ParameterSetName))
            {
                message = "without ParameterSet";
            }
            SafeWriteVerboseLog(this.GetType().Name + " begin processing " + message);
            base.BeginProcessing();
        }

        protected virtual void WriteErrorDetails(StorageException exception)
        {
            ErrorCategory errorCategory = ErrorCategory.CloseError;
            exception = StorageExceptionUtil.RepackStorageException(exception);
            SafeWriteError(new ErrorRecord(exception, exception.GetType().Name, errorCategory, null));
        }

        protected override void SafeWriteError(Exception e)
        {
            Debug.Assert(e != null, "ex cannot be null or empty.");

            ErrorCategory errorCategory = ErrorCategory.CloseError; //default error category
            if (e is ArgumentException)
            {
                errorCategory = ErrorCategory.InvalidArgument;
            }
            else if (e is ResourceNotFoundException)
            {
                errorCategory = ErrorCategory.ObjectNotFound;
            }
            else if (e is ResourceAlreadyExistException)
            {
                errorCategory = ErrorCategory.ResourceExists;
            }
            else if (e is StorageException)
            {
                WriteErrorDetails((StorageException)e);
                return;
            }
            SafeWriteError(new ErrorRecord(e, e.GetType().Name, errorCategory, null));
        }

        internal virtual void ExecuteCommand()
        {
            return;
        }

        protected override void ProcessRecord()
        {
            try
            {

                base.ProcessRecord();

                this.ExecuteCommand();
            }
            catch (Exception e)
            {
                SafeWriteError(e);
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            double timespan = GetRunningMilliseconds();
            string message = string.Format("{0} end processing, Use {1} remote calls. Elapsed time {2:0.00} ms. Operation id: {3}",
                this.GetType().Name, restCallCount, timespan, operationContext.ClientRequestID);
            SafeWriteVerboseLog(message);
        }

        //FIXME can not be called
        protected override void StopProcessing()
        {
            double timespan = GetRunningMilliseconds();
            string message = string.Format("{0} stop processing, Use {1} remote calls. Elapsed time {2:0.00} ms. Operation id: {3}",
                this.GetType().Name, restCallCount, timespan, operationContext.ClientRequestID);
            SafeWriteVerboseLog(message);
            base.StopProcessing();
        }
    }
}

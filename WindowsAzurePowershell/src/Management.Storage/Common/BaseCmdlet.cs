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

    /// <summary>
    /// base cmdlet for cmdlet in storage package
    /// </summary>
    public class BaseCmdlet : CloudBaseCmdlet<IServiceManagement>
    {
        /// <summary>
        /// cmdlet operation context.
        /// </summary>
        protected OperationContext OperationContext { get; private set; }

        /// <summary>
        /// remote call counter
        /// </summary>
        private int remoteCallCounter = 0;

        /// <summary>
        /// forbidden to write output to console in order to get a quick response for users interaction such as ctrl c
        /// </summary>
        private bool forbiddenWriteOutput = false;

        /// <summary>
        /// init storage client operation context
        /// </summary>
        internal void InitOperationContext()
        {
            OperationContext = new OperationContext();
            OperationContext.Init();

            OperationContext.SendingRequest += (s, e) =>
            {
                remoteCallCounter++;
                string message = String.Format(Resources.StartRemoteCall,
                    remoteCallCounter, e.Request.Method, e.Request.RequestUri.ToString());
                SafeWriteVerboseLog(message);
            };
            
            OperationContext.ResponseReceived += (s, e) =>
            {
                string message = String.Format(Resources.FinishRemoteCall,
                    e.Response.StatusCode, e.RequestInformation.ServiceRequestID);
                SafeWriteVerboseLog(message);
            };

            SafeWriteVerboseLog(String.Format(Resources.InitOperationContextLog, OperationContext.ClientRequestID));
        }

        /// <summary>
        /// write log in verbose mode
        /// </summary>
        /// <param name="msg">verbose log</param>
        internal void SafeWriteVerboseLog(string msg)
        {
            string time = DateTime.Now.ToString();
            string log = String.Format(Resources.VerboseLogFormat, time, msg);

            if (!forbiddenWriteOutput)
            {
                SafeWriteVerbose(log);
            }
        }

        /// <summary>
        /// cmdlet begin process
        /// </summary>
        protected override void BeginProcessing()
        {
            InitOperationContext();

            if (string.IsNullOrEmpty(ParameterSetName))
            {
                SafeWriteVerboseLog(String.Format(Resources.BeginProcessingWithoutParameterSetLog, this.GetType().Name));
            }
            else
            {
                SafeWriteVerboseLog(String.Format(Resources.BeginProcessingWithParameterSetLog, this.GetType().Name, ParameterSetName));
            }

            base.BeginProcessing();
        }

        /// <summary>
        /// write error detials for storageexception
        /// </summary>
        /// <param name="exception">StorageException from storage client</param>
        protected virtual void WriteErrorDetails(StorageException exception)
        {
            ErrorCategory errorCategory = ErrorCategory.CloseError;
            exception = exception.RepackStorageException();
            SafeWriteError(new ErrorRecord(exception, exception.GetType().Name, errorCategory, null));
        }

        /// <summary>
        /// safe write error
        /// </summary>
        /// <param name="e">an exception object</param>
        protected void SafeWriteError(Exception e)
        {
            Debug.Assert(e != null, Resources.ExceptionCannotEmpty);

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

        /// <summary>
        /// execute command
        /// </summary>
        internal virtual void ExecuteCommand()
        {
            return;
        }

        /// <summary>
        /// process record
        /// </summary>
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

        /// <summary>
        /// end processing
        /// </summary>
        protected override void EndProcessing()
        {
            base.EndProcessing();
            double timespan = OperationContext.GetRunningMilliseconds();
            string message = string.Format(Resources.EndProcessingLog,
                this.GetType().Name, remoteCallCounter, timespan, OperationContext.ClientRequestID);
            SafeWriteVerboseLog(message);
        }

        /// <summary>
        /// stop processing
        /// </summary>
        protected override void StopProcessing()
        {
            forbiddenWriteOutput = true;
            base.StopProcessing();
        }
    }
}

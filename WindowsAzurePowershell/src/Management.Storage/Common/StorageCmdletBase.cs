﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Util;
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    /// <summary>
    /// base cmdlet for cmdlet in storage package
    /// The cmdlet with cloud should extend StorageCloudCmdlBase.
    /// The cmdlet without cloud could extend StorageCmdletBase.
    /// </summary>
    public class StorageCmdletBase<T> : CloudBaseCmdlet<T>
        where T : class
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
                WriteVerboseLog(message);
            };
            
            OperationContext.ResponseReceived += (s, e) =>
            {
                string message = String.Format(Resources.FinishRemoteCall,
                    e.Response.StatusCode, e.RequestInformation.ServiceRequestID);
                WriteVerboseLog(message);
            };

            WriteVerboseLog(String.Format(Resources.InitOperationContextLog, OperationContext.ClientRequestID));
        }

        /// <summary>
        /// write log in verbose mode
        /// </summary>
        /// <param name="msg">verbose log</param>
        internal void WriteVerboseLog(string msg)
        {
            if (!forbiddenWriteOutput)
            {
                WriteVerboseWithTimestamp(msg);
            }
        }

        /// <summary>
        /// init channel with or without subscription in storage cmdlet
        /// </summary>
        /// <param name="force">force to call the base.InitChannelCurrentSubscription</param>
        protected override void InitChannelCurrentSubscription(bool force)
        {
            if (force)
            {
                WriteVerboseLog(Resources.InitChannelFromSubscription);
                //don't force to create the channel
                base.InitChannelCurrentSubscription(false);
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
                WriteVerboseLog(String.Format(Resources.BeginProcessingWithoutParameterSetLog, this.GetType().Name));
            }
            else
            {
                WriteVerboseLog(String.Format(Resources.BeginProcessingWithParameterSetLog, this.GetType().Name, ParameterSetName));
            }

            base.BeginProcessing();
        }

        /// <summary>
        /// write error detials for storageexception
        /// </summary>
        /// <param name="exception">StorageException from storage client</param>
        protected void WriteErrorDetails(StorageException exception)
        {
            exception = exception.RepackStorageException();
            WriteExceptionError(exception);
        }

        /// <summary>
        /// write error with category and identifier
        /// </summary>
        /// <param name="e">an exception object</param>
        protected override void WriteExceptionError(Exception e)
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

            WriteError(new ErrorRecord(e, e.GetType().Name, errorCategory, null));
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
            WriteVerboseLog(message);
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

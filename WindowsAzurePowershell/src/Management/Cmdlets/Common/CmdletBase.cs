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
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using Model;
    using Samples.WindowsAzure.ServiceManagement;
    using Service;
    using Utilities;
    using Constants = Samples.WindowsAzure.ServiceManagement.Constants;

    public abstract class CmdletBase<T> : PSCmdlet, IDynamicParameters
        where T : class
    {
        private IMessageWriter writer;
        private bool hasOutput = false;

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

        public IMessageWriter Writer { get { return writer; } set { writer = value; } }

        protected T Channel
        {
            get;
            set;
        }

        protected bool SkipChannelInit
        {
            get;
            set;
        }

        protected CmdletBase()
        {
        }

        protected CmdletBase(IMessageWriter writer)
            : this()
        {
            this.writer = writer;
        }

        protected string GetServiceRootPath() { return PathUtility.FindServiceRootDirectory(CurrentPath()); }

        protected string CurrentPath()
        {
            // SessionState is only available within Powershell so default to
            // the CurrentDirectory when being run from tests.
            return (SessionState != null) ?
                SessionState.Path.CurrentLocation.Path :
                Environment.CurrentDirectory;
        }

        protected static string RetrieveOperationId()
        {
            var operationId = string.Empty;

            if ((WebOperationContext.Current != null) && (WebOperationContext.Current.IncomingResponse != null))
            {
                operationId = WebOperationContext.Current.IncomingResponse.Headers[Constants.OperationTrackingIdHeader];
            }

            return operationId;
        }

        protected bool IsVerbose()
        {
            bool verbose = MyInvocation.BoundParameters.ContainsKey("Verbose") && ((SwitchParameter)MyInvocation.BoundParameters["Verbose"]).ToBool();
            return verbose;
        }

        protected virtual void WriteErrorDetails(CommunicationException exception)
        {
            ServiceManagementError error;

            string operationId;
            SMErrorHelper.TryGetExceptionDetails(exception, out error, out operationId);
            if (error == null)
            {
                WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
            }
            else
            {
                string errorDetails = string.Format(
                    CultureInfo.InvariantCulture,
                    "HTTP Status Code: {0} - HTTP Error Message: {1}\nOperation ID: {2}",
                    error.Code,
                    error.Message,
                    operationId);

                WriteError(new ErrorRecord(new CommunicationException(errorDetails), string.Empty, ErrorCategory.CloseError, null));
            }
        }

       public virtual object GetDynamicParameters()
        {
            return null;
        }
        protected virtual Operation GetOperationStatus(string subscriptionId, string operationId)
        {
            var channel = (IServiceManagement)Channel;
            return channel.GetOperationStatus(subscriptionId, operationId);
        }

        private void SafeWriteObjectInternal(object sendToPipeline)
        {
            if (CommandRuntime != null)
            {
                WriteObject(sendToPipeline);
            }
            else
            {
                Trace.WriteLine(sendToPipeline);
            }
        }

        private void WriteLineIfFirstOutput()
        {
            if (!hasOutput)
            {
                hasOutput = true;
                SafeWriteObjectInternal(Environment.NewLine);
            }
        }

        protected void SafeWriteObject(string message, params object[] args)
        {
            object sendToPipeline = message;
            WriteLineIfFirstOutput();
            if (args.Length > 0)
            {
                sendToPipeline = string.Format(message, args);
            }
            SafeWriteObjectInternal(sendToPipeline);

            if (writer != null)
            {
                writer.Write(sendToPipeline.ToString());
            }
        }

        protected void WriteOutputObject(object sendToPipeline)
        {
            SafeWriteObjectInternal(sendToPipeline);

            if (writer != null)
            {
                writer.WriteObject(sendToPipeline);
            }
        }

        protected void SafeWriteObjectWithTimestamp(string message, params object[] args)
        {
            SafeWriteObject(string.Format("{0:T} - {1}", DateTime.Now, string.Format(message, args)));
        }

        /// <summary>
        /// Wrap the base Cmdlet's SafeWriteProgress call so that it will not
        /// throw a NotSupportedException when called without a CommandRuntime
        /// (i.e., when not called from within Powershell).
        /// </summary>
        /// <param name="progress">The progress to write.</param>
        protected void SafeWriteProgress(ProgressRecord progress)
        {
            WriteLineIfFirstOutput();

            if (CommandRuntime != null)
            {
                WriteProgress(progress);
            }
            else
            {
                Trace.WriteLine(string.Format("{0}% Complete", progress.PercentComplete));
            }
        }

        /// <summary>
        /// Wrap the base Cmdlet's WriteError call so that it will not throw
        /// a NotSupportedException when called without a CommandRuntime (i.e.,
        /// when not called from within Powershell).
        /// </summary>
        /// <param name="errorRecord">The error to write.</param>
        protected void SafeWriteError(ErrorRecord errorRecord)
        {
            Debug.Assert(errorRecord != null, "errorRecord cannot be null.");

            // If the exception is an Azure Service Management error, pull the
            // Azure message out to the front instead of the generic response.
            errorRecord = AzureServiceManagementException.WrapExistingError(errorRecord);

            if (CommandRuntime != null)
            {
                WriteError(errorRecord);
            }
            else
            {
                Trace.WriteLine(errorRecord);
            }

            if (writer != null)
            {
                writer.WriteError(errorRecord);
            }
        }
        /// <summary>
        /// Write an error message for a given exception.
        /// </summary>
        /// <param name="ex">The exception resulting from the error.</param>
        protected void SafeWriteError(Exception ex)
        {
            Debug.Assert(ex != null, "ex cannot be null or empty.");
            SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
        }

    }
}

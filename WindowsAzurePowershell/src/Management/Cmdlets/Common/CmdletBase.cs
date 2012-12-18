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
    using System.Management.Automation;
    using System.ServiceModel.Web;
    using Model;
    using Utilities;
    using Constants = Samples.WindowsAzure.ServiceManagement.Constants;

    public abstract class CmdletBase<T> : PSCmdlet, IDynamicParameters
        where T : class
    {
        private IMessageWriter writer;

        private bool hasOutput = false;

        public IMessageWriter Writer { get { return writer; } set { writer = value; } }

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

        public virtual object GetDynamicParameters()
        {
            return null;
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

        /// <summary>
        /// Wrap the base Cmdlet's WriteVerbose call so that it will not throw
        /// a NotSupportedException when called without a CommandRuntime (i.e.,
        /// when not called from within Powershell) and uses a writer object if
        /// it's not set to null.
        /// </summary>
        /// <param name="errorRecord">The message to write.</param>
        protected void SafeWriteVerbose(string message)
        {
            Debug.Assert(message != null, "message cannot be null.");

            if (CommandRuntime != null)
            {
                WriteVerbose(message);
            }
            else
            {
                Trace.WriteLine(message);
            }

            if (writer != null)
            {
                writer.WriteVerbose(message);
            }
        }

        protected PSObject ConstructPSObject(string typeName, params object[] args)
        {
            Debug.Assert(args.Length % 2 == 0, "The parameter args length must be even number");
            Debug.Assert(!string.IsNullOrEmpty(typeName), "typeName can't be null or empty");

            PSObject outputObject = new PSObject();
            outputObject.TypeNames.Add(typeName);

            for (int i = 0; i <= args.Length / 2; i += 2)
            {
                outputObject.Properties.Add(new PSNoteProperty(args[i].ToString(), args[i + 1]));
            }

            return outputObject;
        }

        protected void SafeWriteOutputPSObject(string typeName, params object[] args)
        {
            PSObject customObject = this.ConstructPSObject(typeName, args);
            WriteOutputObject(customObject);
        }

        public virtual void ExecuteCmdlet()
        {
            // Do nothing.
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ExecuteCmdlet();
            }
            catch (Exception ex)
            {
                SafeWriteError(ex);
            }
        }
    }
}

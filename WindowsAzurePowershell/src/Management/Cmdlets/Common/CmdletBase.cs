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
    using Utilities;

    public abstract class CmdletBase<T> : PSCmdlet, IDynamicParameters
        where T : class
    {
        protected string GetServiceRootPath() { return PathUtility.FindServiceRootDirectory(CurrentPath()); }

        protected string CurrentPath()
        {
            // SessionState is only available within Powershell so default to
            // the CurrentDirectory when being run from tests.
            return (SessionState != null) ?
                SessionState.Path.CurrentLocation.Path :
                Environment.CurrentDirectory;
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

        protected void WriteVerboseWithTimestamp(string message, params object[] args)
        {
            WriteVerbose(string.Format("{0:T} - {1}", DateTime.Now, string.Format(message, args)));
        }

        /// <summary>
        /// Write an error message for a given exception.
        /// </summary>
        /// <param name="ex">The exception resulting from the error.</param>
        protected void WriteExceptionError(Exception ex)
        {
            Debug.Assert(ex != null, "ex cannot be null or empty.");
            WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
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
            WriteObject(customObject);
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
                WriteExceptionError(ex);
            }
        }
    }
}

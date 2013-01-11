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
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Properties;

    public abstract class CmdletBase : PSCmdlet, IDynamicParameters
    {
        public string GetServiceRootPath()
        {
            // Get the service path
            var servicePath = FindServiceRootDirectory(CurrentPath());

            // Was the service path found?
            if (servicePath == null)
            {
                throw new InvalidOperationException(Resources.CannotFindServiceRoot);
            }

            return servicePath;
        }

        public string FindServiceRootDirectory(string path)
        {
            // Is the csdef file present in the folder
            bool found = Directory.GetFiles(path, Resources.ServiceDefinitionFileName).Length == 1;

            if (found)
            {
                return path;
            }

            // Find the last slash
            int slash = path.LastIndexOf('\\');
            if (slash > 0)
            {
                // Slash found trim off the last path
                path = path.Substring(0, slash);

                // Recurse
                return FindServiceRootDirectory(path);
            }

            // Couldn't locate the service root, exit
            return null;
        }

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
        protected virtual void WriteExceptionError(Exception ex)
        {
            Debug.Assert(ex != null, "ex cannot be null or empty.");
            WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
        }

        protected PSObject ConstructPSObject(string typeName, params object[] args)
        {
            Debug.Assert(args.Length % 2 == 0, "The parameter args length must be even number");

            PSObject outputObject = new PSObject();

            if (!string.IsNullOrEmpty(typeName))
            {
                outputObject.TypeNames.Add(typeName);
            }

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

        /// <summary>
        /// cmdlet begin process
        /// </summary>
        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(ParameterSetName))
            {
                WriteVerboseWithTimestamp(String.Format(Resources.BeginProcessingWithoutParameterSetLog, this.GetType().Name));
            }
            else
            {
                WriteVerboseWithTimestamp(String.Format(Resources.BeginProcessingWithParameterSetLog, this.GetType().Name, ParameterSetName));
            }

            base.BeginProcessing();
        }

        /// <summary>
        /// end processing
        /// </summary>
        protected override void EndProcessing()
        {
            string message = string.Format(Resources.EndProcessingLog, this.GetType().Name);
            WriteVerboseWithTimestamp(message);

            base.EndProcessing();
        }
    }
}

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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Text;

    public class MockCommandRuntime : ICommandRuntime
    {
        public List<ErrorRecord> ErrorRecords = new List<ErrorRecord>();
        public List<object> WrittenObjects = new List<object>();
        public StringBuilder WarningOutput = new StringBuilder();

        public override string ToString()
        {
            return "MockCommand";
        }

        public PSTransactionContext CurrentPSTransaction
        {
            get { throw new NotImplementedException(); }
        }

        public PSHost Host
        {
            get { throw new NotImplementedException(); }
        }

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            throw new NotImplementedException();
        }

        public bool ShouldContinue(string query, string caption)
        {
            throw new NotImplementedException();
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            throw new NotImplementedException();
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            return true;
        }

        public bool ShouldProcess(string target, string action)
        {
            throw new NotImplementedException();
        }

        public bool ShouldProcess(string target)
        {
            throw new NotImplementedException();
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            throw new NotImplementedException();
        }

        public bool TransactionAvailable()
        {
            throw new NotImplementedException();
        }

        public void WriteCommandDetail(string text)
        {
            throw new NotImplementedException();
        }

        public void WriteDebug(string text)
        {
            throw new NotImplementedException();
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            ErrorRecords.Add(errorRecord);
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            WrittenObjects.Add(sendToPipeline);
        }

        public void WriteObject(object sendToPipeline)
        {
            WrittenObjects.Add(sendToPipeline);
        }

        public void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
            // Do nothing
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
            throw new NotImplementedException();
        }

        public void WriteVerbose(string text)
        {
            throw new NotImplementedException();
        }

        public void WriteWarning(string text)
        {
            WarningOutput.AppendLine(text);
        }
    }
}
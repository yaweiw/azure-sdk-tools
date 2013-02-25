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

namespace Microsoft.WindowsAzure.Management.ScenarioTest.Common.CustomPowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;

    class CustomerPSHostUserInterface : PSHostUserInterface
    {
        public List<string> WarningStream { get; private set; }

        public List<string> VerboseStream { get; private set; }

        public List<string> DebugStream { get; private set; }

        public List<ProgressRecord> ProgressStream { get; private set; }

        public int PromptChoice { get; set; }

        public CustomerPSHostUserInterface()
        {
            WarningStream = new List<string>();

            VerboseStream = new List<string>();

            DebugStream = new List<string>();

            ProgressStream = new List<ProgressRecord>();

            PromptChoice = int.MaxValue;
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            if (PromptChoice != int.MaxValue)
            {
                return PromptChoice;
            }
            else
            {
                return defaultChoice;
            }
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName, System.Management.Automation.PSCredentialTypes allowedCredentialTypes, System.Management.Automation.PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override System.Management.Automation.PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return null; }
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            throw new NotImplementedException();
        }

        public override void Write(string value)
        {
            throw new NotImplementedException();
        }

        public override void WriteDebugLine(string message)
        {
            DebugStream.Add(message);
        }

        public override void WriteErrorLine(string value)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string value)
        {
            throw new NotImplementedException();
        }

        public override void WriteProgress(long sourceId, System.Management.Automation.ProgressRecord record)
        {
            ProgressStream.Add(record);
        }

        public override void WriteVerboseLine(string message)
        {
            VerboseStream.Add(message);
        }

        public override void WriteWarningLine(string message)
        {
            WarningStream.Add(message);
        }
    }
}
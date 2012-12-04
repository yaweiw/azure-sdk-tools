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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Utilities
{
    using System.Collections.Generic;
    using Management.Utilities;
using System.Management.Automation;

    public class FakeWriter : IMessageWriter
    {
        private List<string> messages;

        private List<object> outputChannel;

        private List<ErrorRecord> errorChannel;

        private List<string> verboseChannel;

        public FakeWriter()
        {
            messages = new List<string>();
            outputChannel = new List<object>();
            errorChannel = new List<ErrorRecord>();
            verboseChannel = new List<string>();
        }

        public List<string> Messages { get { return messages; }}

        public List<object> OutputChannel { get { return outputChannel; } }

        public List<ErrorRecord> ErrorChannel { get { return errorChannel; } }

        public List<string> VerboseChannel { get { return verboseChannel; } }

        public void Write(string message)
        {
            Messages.Add(message);
        }

        public void WriteObject(object obj)
        {
            outputChannel.Add(obj);
        }

        public void WriteError(ErrorRecord error)
        {
            errorChannel.Add(error);
        }

        public void WriteVerbose(string message)
        {
            verboseChannel.Add(message);
        }
    }
}

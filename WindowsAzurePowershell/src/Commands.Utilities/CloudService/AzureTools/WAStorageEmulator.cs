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

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudService.AzureTools
{
    using System;
    using System.IO;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    public class WAStorageEmulator
    {
        private string _emulatorPath = string.Empty;
        private bool _storageEmulatorInstalled = false;

        public WAStorageEmulator(string emulatorDirectory)
        {
            if (!string.IsNullOrEmpty(emulatorDirectory))
            {
                _storageEmulatorInstalled = true;
                _emulatorPath = Path.Combine(emulatorDirectory, Resources.StorageEmulatorExe);
            }
        }

        public string Error { get; private set; }

        internal ProcessHelper CommandRunner { get; set; } 

        public void Start()
        {
            if (_storageEmulatorInstalled)
            {
                ProcessHelper runner = GetCommandRunner();
                runner.StartAndWaitForProcess(_emulatorPath, Resources.StartStorageEmulatorCommandArgument);
                Error = CommandRunner.StandardError;
            }
            else
            {
                Error = Resources.WarningWhenStorageEmulatorIsMissing;
            }
        }

        public void Stop()
        {
            if (_storageEmulatorInstalled)
            {
                ProcessHelper runner = GetCommandRunner();
                runner.StartAndWaitForProcess(_emulatorPath, Resources.StopStorageEmulatorCommandArgument);
                Error = CommandRunner.StandardError;
            }
            else
            {
                Error = string.Empty;
            }
        }

        private ProcessHelper GetCommandRunner()
        {
            if (CommandRunner == null)
            {
                CommandRunner = new ProcessHelper();
            }
            return CommandRunner;
        }
    }
}

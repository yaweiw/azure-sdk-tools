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

namespace Microsoft.WindowsAzure.Commands.Subscription
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Utilities.Common;

    [Cmdlet(VerbsData.Import, "AzurePublishSettingsNew")]
    public class ImportAzurePublishSettingsNewCommand : CmdletWithSubscriptionBase
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true,
            HelpMessage = "Path to the publish settings file.")]
        public string PublishSettingsFile { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            if (IsDirectory())
            {
                ImportDirectory();
            }
            else
            {
                ImportFile();
            }
        }

        private bool IsDirectory()
        {
            if (Directory.Exists(PublishSettingsFile))
            {
                return true;
            }

            if (string.IsNullOrEmpty(PublishSettingsFile))
            {
                return true;
            }
            return false;
        }

        private void ImportDirectory()
        {
            throw new NotImplementedException("Getting there...");
        }

        private void ImportFile()
        {
            var fullFile = ResolveFileName(PublishSettingsFile);
            GuardFileExists(fullFile);
            Profile.ImportPublishSettings(PublishSettingsFile);
        }

        private void GuardFileExists(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception();
            }
        }

        private string ResolveFileName(string filename)
        {
            return this.TryResolvePath(filename);
        }
    }
}
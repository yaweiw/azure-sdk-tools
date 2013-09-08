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
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Commands.Utilities.Common;

    /// <summary>
    /// Sets a Windows Azure environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureEnvironment"), OutputType(typeof(WindowsAzureEnvironment))]
    public class SetAzureEnvironmentCommand : CmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string PublishSettingsFileUrl { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string ServiceEndpoint { get; set; }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string ManagementPortalUrl { get; set; }

        [Parameter(Position = 4, Mandatory = false, HelpMessage = "The storage endpoint")]
        public string StorageEndpoint { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            WriteObject(GlobalSettingsManager.Instance.ChangeEnvironmentStorageEndpoint(
                Name,
                PublishSettingsFileUrl,
                ServiceEndpoint,
                ManagementPortalUrl,
                StorageEndpoint));
        }
    }
}
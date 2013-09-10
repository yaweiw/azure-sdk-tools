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

namespace Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet
{
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Storage.Model.Contract;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Text;

    [Cmdlet(VerbsCommon.New, StorageNouns.ContainerSas), OutputType(typeof(String))]
    public class NewAzureStorageContainerSasCommand : StorageCloudBlobCmdletBase
    {
        [Alias("N", "Container")]
        [Parameter(Position = 0, Mandatory = true,
            HelpMessage = "Container Name",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(HelpMessage = "Policy Identifier")]
        public string Policy { get; set; }

        [Parameter(HelpMessage = "Permissions for a container. Permissions can be any not-empty subset of \"rwdl\".")]
        public string Permission { get; set; }

        [Parameter(HelpMessage = "Start Time")]
        public DateTime? StartTime { get; set; }

        [Parameter(HelpMessage = "Expiry Time")]
        public DateTime? ExpiryTime { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Display full uri with sas token")]
        public SwitchParameter FullUri { get; set; }

        /// <summary>
        /// Initializes a new instance of the NewAzureStorageContainerSasCommand class.
        /// </summary>
        public NewAzureStorageContainerSasCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureStorageContainerSasCommand class.
        /// </summary>
        /// <param name="channel">IStorageBlobManagement channel</param>
        public NewAzureStorageContainerSasCommand(IStorageBlobManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            if (String.IsNullOrEmpty(Name)) return;
            CloudBlobContainer container = Channel.GetContainerReference(Name);
            SharedAccessBlobPolicy accessPolicy = new SharedAccessBlobPolicy();
            SetupAccessPolicy(accessPolicy);
            SasTokenHelper.ValidateContainerAccessPolicy(Channel, container.Name, accessPolicy, Policy);
            string sasToken = container.GetSharedAccessSignature(accessPolicy, Policy);

            if (FullUri)
            {
                string fullUri = container.Uri.ToString() + sasToken;
                WriteObject(fullUri);
            }
            else
            {
                WriteObject(sasToken);
            }
        }

        /// <summary>
        /// Update the access policy
        /// </summary>
        /// <param name="policy">Access policy object</param>
        private void SetupAccessPolicy(SharedAccessBlobPolicy policy)
        {
            SasTokenHelper.SetupAccessPolicyLifeTime(policy.SharedAccessStartTime,
                policy.SharedAccessExpiryTime, StartTime, ExpiryTime);
            SetupAccessPolicyPermission(policy, Permission);
        }

        /// <summary>
        /// Set up access policy permission
        /// </summary>
        /// <param name="policy">SharedAccessBlobPolicy object</param>
        /// <param name="permission">Permisson</param>
        private void SetupAccessPolicyPermission(SharedAccessBlobPolicy policy, string permission)
        {
            if (string.IsNullOrEmpty(permission)) return;
            policy.Permissions = SharedAccessBlobPermissions.None;
            foreach (char op in permission)
            {
                switch(op)
                {
                    case 'r':
                        policy.Permissions |= SharedAccessBlobPermissions.Read;
                        break;
                    case 'w':
                        policy.Permissions |= SharedAccessBlobPermissions.Write;
                        break;
                    case 'd':
                        policy.Permissions |= SharedAccessBlobPermissions.Delete;
                        break;
                    case 'l':
                        policy.Permissions |= SharedAccessBlobPermissions.List;
                        break;
                    default:
                        throw new ArgumentException(string.Format(Resources.InvalidAccessPermission, op));
                }
            }
        }
    }
}

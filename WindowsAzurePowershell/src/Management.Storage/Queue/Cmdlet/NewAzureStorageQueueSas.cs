namespace Microsoft.WindowsAzure.Management.Storage.Queue.Cmdlet
{
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Storage.Model.Contract;
    using Microsoft.WindowsAzure.Storage.Queue;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.Text;

    [Cmdlet(VerbsCommon.New, StorageNouns.QueueSas), OutputType(typeof(String))]
    public class NewAzureStorageQueueSasCommand : StorageQueueBaseCmdlet
    {
        [Alias("N", "Queue")]
        [Parameter(Position = 0, Mandatory = true,
            HelpMessage = "Table Name",
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
        public NewAzureStorageQueueSasCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureStorageContainerSasCommand class.
        /// </summary>
        /// <param name="channel">IStorageBlobManagement channel</param>
        public NewAzureStorageQueueSasCommand(IStorageQueueManagement channel)
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
            CloudQueue queue = Channel.GetQueueReference(Name);
            SharedAccessQueuePolicy policy = new SharedAccessQueuePolicy();
            SetupAccessPolicy(policy);
            SasTokenHelper.ValidateQueueAccessPolicy(Channel, queue.Name, policy, Policy);
            string sasToken = queue.GetSharedAccessSignature(policy, Policy);

            if (FullUri)
            {
                string fullUri = queue.Uri.ToString() + sasToken;
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
        private void SetupAccessPolicy(SharedAccessQueuePolicy policy)
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
        private void SetupAccessPolicyPermission(SharedAccessQueuePolicy policy, string permission)
        {
            if (string.IsNullOrEmpty(permission)) return;
            policy.Permissions = SharedAccessQueuePermissions.None;
            foreach (char op in permission)
            {
                switch(op)
                {
                    case 'r':
                        policy.Permissions |= SharedAccessQueuePermissions.Read;
                        break;
                    case 'a':
                        policy.Permissions |= SharedAccessQueuePermissions.Add;
                        break;
                    case 'u':
                        policy.Permissions |= SharedAccessQueuePermissions.Update;
                        break;
                    case 'p':
                        policy.Permissions |= SharedAccessQueuePermissions.ProcessMessages;
                        break;
                    default:
                        throw new ArgumentException(string.Format(Resources.InvalidAccessPermission, op));
                }
            }
        }
    }
}

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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Storage;
    using Storage.Auth;
    using Storage.Blob;
    using Model;

    [Cmdlet(
        VerbsCommon.Set,
        VirtualMachineCustomScriptExtensionNoun,
        DefaultParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName),
    OutputType(
        typeof(IPersistentVM))]
    public class SetAzureVMCustomScriptExtensionCommand : VirtualMachineCustomScriptExtensionCmdletBase
    {
        protected const string SetCustomScriptExtensionByContainerBlobsParamSetName = "SetCustomScriptExtensionByContainerAndFileNames";
        protected const string SetCustomScriptExtensionByUrisParamSetName = "SetCustomScriptExtensionByUriLinks";
        protected const string DisableCustomScriptExtensionParamSetName = "DisableCustomScriptExtension";

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = false,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Reference Name.")]
        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByUrisParamSetName,
            Mandatory = false,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Reference Name.")]
        [Parameter(
            ParameterSetName = DisableCustomScriptExtensionParamSetName,
            Mandatory = false,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Reference Name.")]
        [ValidateNotNullOrEmpty]
        public override string ReferenceName { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = false,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Version.")]
        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByUrisParamSetName,
            Mandatory = false,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Version.")]
        [Parameter(
            ParameterSetName = DisableCustomScriptExtensionParamSetName,
            Mandatory = false,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Version.")]
        [ValidateNotNullOrEmpty]
        public override string Version { get; set; }

        [Parameter(
            ParameterSetName = DisableCustomScriptExtensionParamSetName,
            Mandatory = false,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Disable VM Custom Script Extension")]
        public override SwitchParameter Disable { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Name of the Container.")]
        [ValidateNotNullOrEmpty]
        public override string ContainerName { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = true,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Blob Files in the Container.")]
        [ValidateNotNullOrEmpty]
        public override string[] FileName { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = false,
            Position = 4,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Storage Account Name.")]
        [ValidateNotNullOrEmpty]
        public override string StorageAccountName { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = false,
            Position = 5,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Storage Account Key.")]
        [ValidateNotNullOrEmpty]
        public override string StorageAccountKey { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByUrisParamSetName,
            Mandatory = false,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The File URIs.")]
        [ValidateNotNullOrEmpty]
        public override Uri[] FileUri { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = false,
            Position = 6,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Command to Execute.")]
        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByUrisParamSetName,
            Mandatory = true,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Command to Execute.")]
        public override string Run { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = false,
            Position = 7,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Argument String for the Command.")]
        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByUrisParamSetName,
            Mandatory = false,
            Position = 4,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Argument String for the Command.")]
        [ValidateNotNullOrEmpty]
        public override string Argument { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ExecuteCommand();
        }

        internal void ExecuteCommand()
        {
            ValidateParameters();
            RemovePredicateExtensions();
            AddResourceExtension();
            WriteObject(VM);
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();

            if (string.Equals(this.ParameterSetName, SetCustomScriptExtensionByContainerBlobsParamSetName))
            {
                this.StorageAccountName = this.StorageAccountName ?? GetStorageName();
                this.StorageAccountKey = this.StorageAccountKey ?? GetStorageKey(this.StorageAccountName);

                if (this.FileName != null && this.FileName.Any())
                {
                    this.FileUri = (from blobName in this.FileName
                                    select GetSasUrl(this.ContainerName, blobName)).ToArray();

                    this.Run = string.IsNullOrEmpty(this.Run) ? this.FileName[0] : this.Run;
                }
            }

            this.ReferenceName = this.ReferenceName ?? LegacyReferenceName;
            this.PublicConfiguration = GetPublicConfiguration();
            this.PrivateConfiguration = GetPrivateConfiguration();
        }

        protected string GetStorageName()
        {
            return CurrentSubscription.CurrentStorageAccountName;
        }

        protected string GetStorageKey(string storageName)
        {
            string storageKey = string.Empty;

            if (!string.IsNullOrEmpty(storageName))
            {
                var storageAccount = this.StorageClient.StorageAccounts.Get(storageName);
                if (storageAccount != null)
                {
                    var keys = this.StorageClient.StorageAccounts.GetKeys(storageName);
                    if (keys != null)
                    {
                        storageKey = !string.IsNullOrEmpty(keys.PrimaryKey) ? keys.PrimaryKey : keys.SecondaryKey;
                    }
                }
            }

            return storageKey;
        }

        protected Uri GetSasUrl(string containerName, string blobName)
        {
            var cred = new StorageCredentials(this.StorageAccountName, this.StorageAccountKey);
            var storageAccount = new CloudStorageAccount(cred, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            var blobPermissions = new BlobContainerPermissions();
            var policyKey = Guid.NewGuid().ToString();
            blobPermissions.SharedAccessPolicies.Add(policyKey, new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24.0),
                Permissions = SharedAccessBlobPermissions.Read
            });

            container.SetPermissions(blobPermissions);

            var sasToken = container.GetSharedAccessSignature(new SharedAccessBlobPolicy(), policyKey);
            var blobUri = string.Format("{0}/{1}{2}", container.Uri, blobName, sasToken);

            return new Uri(blobUri);
        }
    }
}

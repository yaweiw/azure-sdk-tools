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
            HelpMessage = "The Storage Endpoint Suffix.")]
        [ValidateNotNullOrEmpty]
        public override string StorageEndpointSuffix { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = false,
            Position = 6,
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
        public override string[] FileUri { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = false,
            Position = 7,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Run File to Execute in PowerShell on the VM.")]
        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByUrisParamSetName,
            Mandatory = true,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Run File to Execute in PowerShell on the VM.")]
        [ValidateNotNullOrEmpty]
        [Alias("Run")]
        public override string RunFile { get; set; }

        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByContainerBlobsParamSetName,
            Mandatory = false,
            Position = 8,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Argument String for the Run File.")]
        [Parameter(
            ParameterSetName = SetCustomScriptExtensionByUrisParamSetName,
            Mandatory = false,
            Position = 4,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Argument String for the Run File.")]
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
                var sName = this.StorageAccountName ?? GetStorageName();
                var sKey = this.StorageAccountKey ?? GetStorageKey(this.StorageAccountName);

                if (this.FileName != null && this.FileName.Any())
                {
                    this.FileUri = (from blobName in this.FileName
                                    select GetSasUrlStr(sName, sKey, this.ContainerName, blobName)).ToArray();

                    this.RunFile = string.IsNullOrEmpty(this.RunFile) ? this.FileName[0] : this.RunFile;
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

        protected string GetSasUrlStr(string storageName, string storageKey, string containerName, string blobName)
        {
            var cred = new StorageCredentials(storageName, storageKey);
            var storageAccount = string.IsNullOrEmpty(this.StorageEndpointSuffix)
                               ? new CloudStorageAccount(cred, true)
                               : new CloudStorageAccount(cred, this.StorageEndpointSuffix, true);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var cloudBlob = container.GetBlockBlobReference(blobName);
            var sasToken = cloudBlob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy()
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24.0),
                    Permissions = SharedAccessBlobPermissions.Read
                });

            // Try not to use a Uri object in order to keep the following 
            // special characters in the SAS signature section:
            //     '+'   ->   '%2B'
            //     '/'   ->   '%2F'
            //     '='   ->   '%3D'
            return cloudBlob.Uri + sasToken;
        }
    }
}

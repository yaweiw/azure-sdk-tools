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
    using Management.Storage;
    using Newtonsoft.Json;

    public class VirtualMachineCustomScriptExtensionCmdletBase : VirtualMachineExtensionCmdletBase
    {
        protected const string VirtualMachineCustomScriptExtensionNoun = "AzureVMCustomScriptExtension";

        protected const string ExtensionDefaultPublisher = "Microsoft.WindowsAzure.Compute";
        protected const string ExtensionDefaultName = "CustomScriptHandler";
        protected const string LegacyReferenceName = "MyCustomScriptExtension";

        public virtual string ContainerName { get; set; }
        public virtual string[] File { get; set; }
        public virtual Uri[] Uri{ get; set; }
        public virtual string StorageAccountName { get; set; }
        public virtual string StorageAccountKey { get; set; }
        public virtual string Command { get; set; }
        public virtual string[] Argument { get; set; }

        public VirtualMachineCustomScriptExtensionCmdletBase()
        {
            base.publisherName = ExtensionDefaultPublisher;
            base.extensionName = ExtensionDefaultName;
        }

        protected Uri GetSasUrl(string storageAccount, string container, string blobFile)
        {
            return null;
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

        protected string GetPublicConfiguration()
        {
            string commandToExecute = this.Argument == null || this.Argument.Length <= 0 ? this.Command
                 : string.Concat(this.Command, " ", string.Join(" ", this.Argument.AsEnumerable()));

            return JsonConvert.SerializeObject(new PublicSettings
            {
                fileUris = this.Uri,
                commandToExecute = commandToExecute ?? string.Empty
            });
        }

        protected string GetPrivateConfiguration()
        {
            return JsonConvert.SerializeObject(new PrivateSettings
            {
                storageAccountName = this.StorageAccountName ?? string.Empty,
                storageAccountKey = !string.IsNullOrEmpty(this.StorageAccountKey) ? this.StorageAccountKey : GetStorageKey(this.StorageAccountName)
            });
        }
    }
}

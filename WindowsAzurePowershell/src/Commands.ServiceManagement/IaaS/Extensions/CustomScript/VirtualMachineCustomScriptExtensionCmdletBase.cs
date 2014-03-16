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
    using Management.Storage;

    public class VirtualMachineCustomScriptExtensionCmdletBase : VirtualMachineExtensionCmdletBase
    {
        protected const string VirtualMachineCustomScriptExtensionNoun = "AzureVMCustomScriptExtension";

        protected const string ExtensionDefaultPublisher = "Microsoft.Compute";
        protected const string ExtensionDefaultName = "ScriptHandler";
        protected const string LegacyReferenceName = "MyCustomScriptExtension";

        public virtual Uri[] FileUris{ get; set; }
        public virtual string StorageAccountName { get; set; }
        public virtual string CommandToExecute { get; set; }

        public VirtualMachineCustomScriptExtensionCmdletBase()
        {
            base.publisherName = ExtensionDefaultPublisher;
            base.extensionName = ExtensionDefaultName;
        }

        protected Uri GetSasUrl(string storageAccount, string container, string file)
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
            const string publicConfigFormat = @"""publicSettings"":
                                              {
                                                  ""fileUris"": [{0}],
                                                  ""commandToExecute"":""{1}""
                                              },";

            string urlString = FileUris == null || FileUris.Length == 0 ? string.Empty
                                                                        : string.Concat(FileUris, ',');

            return string.Format(publicConfigFormat, urlString, this.CommandToExecute);
        }

        protected string GetPrivateConfiguration()
        {
            const string privateConfigFormat = @"""protectedSettings"":
                                               {
                                                   ""storageAccountName"": {0},
                                                   ""storageAccountKey"":""{1}""
                                               },";

            return string.Format(privateConfigFormat, this.StorageAccountName, GetStorageKey(this.StorageAccountName));
        }
    }
}

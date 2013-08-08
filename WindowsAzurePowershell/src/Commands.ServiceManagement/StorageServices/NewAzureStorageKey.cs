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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.StorageServices
{
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.ServiceManagement.Model;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Regenerates storage keys with the key-type parameter specifying 
    /// which key to regenerate. Should have the storage account resource specified.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureStorageKey"), OutputType(typeof(StorageServiceKeyOperationContext))]
    public class NewAzureStorageKeyCommand : ServiceManagementBaseCmdlet
    {
        public NewAzureStorageKeyCommand()
        {
        }

        public NewAzureStorageKeyCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Key to regenerate. Primary | Secondary")]
        [ValidateSet("Primary", "Secondary", IgnoreCase = true)]
        public string KeyType
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        [Alias("ServiceName")]
        public string StorageAccountName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            var regenerateKeys = new RegenerateKeys
            {
                KeyType = this.KeyType
            };

            ExecuteClientActionInOCS(
                regenerateKeys, this.CommandRuntime.ToString(),
                s => this.Channel.RegenerateStorageServiceKeys(s, this.StorageAccountName, regenerateKeys), 
                (operation, storageService) => new StorageServiceKeyOperationContext
                {
                    StorageAccountName = this.StorageAccountName,
                    Primary = storageService.StorageServiceKeys.Primary,
                    Secondary = storageService.StorageServiceKeys.Secondary,
                    OperationDescription = this.CommandRuntime.ToString(),
                    OperationId = operation.OperationTrackingId,
                    OperationStatus = operation.Status
                });
        }
    }
}

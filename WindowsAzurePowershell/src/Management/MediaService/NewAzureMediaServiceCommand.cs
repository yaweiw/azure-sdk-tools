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

using System.Management.Automation;
using Microsoft.WindowsAzure.Management.Utilities.MediaService;
using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services.MediaServicesEntities;

namespace Microsoft.WindowsAzure.Management.MediaService
{
    [Cmdlet(VerbsCommon.New, "AzureMediaServicesAccount"), OutputType(typeof(AccountCreationResult))]
    public class NewAzureMediaServiceCommand : AzureMediaServicesHttpClientCommandBase
    {
        public IMediaServicesClient MediaServicesClient { get; set; }


        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The media service account name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The media service region.")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Azure blobstorage endpoint uri.")]
        [ValidateNotNullOrEmpty]
        public string BlobStorageEndpointUri { get; set; }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Azure storage account name")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName { get; set; }

        [Parameter(Position = 4, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Azure storage account key")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountKey { get; set; }


        public override void ExecuteCmdlet()
        {
            MediaServicesClient = MediaServicesClient ?? new MediaServicesClient(CurrentSubscription, WriteDebug);

            AccountCreationResult result = null;
            var request = new AccountCreationRequest {
                AccountName = Name,
                BlobStorageEndpointUri = BlobStorageEndpointUri,
                Region = Location,
                StorageAccountKey = StorageAccountKey,
                StorageAccountName = StorageAccountName
            };
            CatchAggregatedExceptionFlattenAndRethrow(() => { result = MediaServicesClient.CreateNewAzureMediaServiceAsync(request).Result; });
            WriteObject(result, false);
        }
    }
}
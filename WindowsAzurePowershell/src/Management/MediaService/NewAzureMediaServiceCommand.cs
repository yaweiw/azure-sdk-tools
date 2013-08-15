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

using System;
using System.Management.Automation;
using System.Net;
using Microsoft.WindowsAzure.Management.Utilities.Common;
using Microsoft.WindowsAzure.Management.Utilities.MediaService;
using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services.MediaServicesEntities;
using Microsoft.WindowsAzure.ServiceManagement;

namespace Microsoft.WindowsAzure.Management.MediaService
{
    [Cmdlet(VerbsCommon.New, "AzureMediaServicesAccount"), OutputType(typeof(AccountCreationResult))]
    public class NewAzureMediaServiceCommand : AzureMediaServicesHttpClientCommandBase
    {
        public IMediaServicesClient MediaServicesClient { get; set; }


        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The media service account name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The media service location.")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The URLs that are used to perform a retrieval of a public blob")]
        [ValidateNotNullOrEmpty]
        public Uri StorageEndPoint { get; set; }

        [Parameter(Position = 3, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Storage account name")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName { get; set; }

        [Parameter(Position = 4, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Storage account key")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountKey { get; set; }




        public override void ExecuteCmdlet()
        {
            MediaServicesClient = MediaServicesClient ?? new MediaServicesClient(CurrentSubscription, WriteDebug);
            
            if (String.IsNullOrEmpty(StorageAccountKey))
            {
                StorageService storage = ((MediaServicesClient)MediaServicesClient).GetStorageServiceKeys(StorageAccountName).Result;
                StorageAccountKey = storage.StorageServiceKeys.Primary;
            }

            if (StorageEndPoint == null)
            {
                try
                {
                    StorageService storage = null;
                     CatchAggregatedExceptionFlattenAndRethrow(() => { storage = ((MediaServicesClient) MediaServicesClient).GetStorageServiceProperties(StorageAccountName).Result; });

                    if (storage.StorageServiceProperties != null && storage.StorageServiceProperties.Endpoints.Count > 0)
                    {
                        StorageEndPoint = new Uri(storage.StorageServiceProperties.Endpoints[0]);
                    }
                }
                catch (ServiceManagementClientException ex)
                {
                    if (ex.HttpStatus == HttpStatusCode.NotFound)
                    {
                        StorageEndPoint = GlobalSettingsManager.Instance.DefaultEnvironment.GetStorageBlobEndpoint(StorageAccountName);
                    }
                }

            }

            AccountCreationResult result = null;
            var request = new AccountCreationRequest {
                AccountName = Name,
                BlobStorageEndpointUri = StorageEndPoint.ToString(),
                Region = Location,
                StorageAccountKey = StorageAccountKey,
                StorageAccountName = StorageAccountName
            };
            CatchAggregatedExceptionFlattenAndRethrow(() => { result = MediaServicesClient.CreateNewAzureMediaServiceAsync(request).Result; });
            WriteObject(result, false);
        }
    }
}
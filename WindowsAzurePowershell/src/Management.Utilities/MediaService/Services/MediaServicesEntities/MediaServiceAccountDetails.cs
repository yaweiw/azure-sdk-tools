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

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.Management.Utilities.MediaService.Services.MediaServicesEntities
{
    [JsonObject(Title = "AccountDetails")]
    [DataContract(Namespace = MediaServicesUriElements.AccountDetailsNamespace, Name = "AccountDetails")]
    public class MediaServiceAccountDetails
    {
        [DataMember]
        internal string AccountKey { get; set; }

        [DataMember]
        internal AccountKeys AccountKeys { get; set; }

        [DataMember]
        public string AccountName { get; set; }

        [DataMember]
        public string AccountRegion { get; set; }

        [DataMember]
        public string StorageAccountName { get; set; }

        public string PrimaryAccountKey
        {
            get { return AccountKeys.Primary; }
        }

        public string SecondaryAccountKey
        {
            get { return AccountKeys.Secondary; }
        }
    }
}
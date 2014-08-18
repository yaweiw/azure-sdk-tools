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

using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Models;
using System;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.Utilities.Profile;

namespace Microsoft.WindowsAzure.Commands.Profile
{


    /// <summary>
    /// Sets an azure subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureSubscription", DefaultParameterSetName = "CommonSettings"), OutputType(typeof(AzureSubscription))]
    public class AddAzureSubscriptionCommand : SubscriptionCmdletBase
    {
        public AddAzureSubscriptionCommand()
            : base(true)
        {
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription.", ParameterSetName = "CommonSettings")]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription.", ParameterSetName = "ResetCurrentStorageAccount")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Account subscription ID.", ParameterSetName = "CommonSettings")]
        public string SubscriptionId { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Account certificate.", ParameterSetName = "CommonSettings")]
        public X509Certificate2 Certificate { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Service endpoint.", ParameterSetName = "CommonSettings")]
        public string ServiceEndpoint { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Cloud service endpoint.", ParameterSetName = "CommonSettings")]
        public string ResourceManagerEndpoint { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Current storage account name.", ParameterSetName = "CommonSettings")]
        [ValidateNotNullOrEmpty]
        public string CurrentStorageAccountName { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        public override void ExecuteCmdlet()
        {
            var subscription = new AzureSubscription
            {
                Name = SubscriptionName,
                Id = new Guid(SubscriptionId)
            };

            if (CurrentStorageAccountName != null)
            {
                subscription.Properties[AzureSubscription.Property.CloudStorageAccount] = CurrentStorageAccountName;
            }
            if (Certificate != null)
            {
                ProfileClient.ImportCertificate(Certificate);
                subscription.Properties[AzureSubscription.Property.Thumbprint] = Certificate.Thumbprint;
            }

            WriteObject(ProfileClient.AddAzureSubscription(subscription));
        }
    }
}
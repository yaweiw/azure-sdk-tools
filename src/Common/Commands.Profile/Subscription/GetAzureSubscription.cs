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

namespace Microsoft.WindowsAzure.Commands.Profile
{
    using Management;
    using Microsoft.WindowsAzure.Commands.Common.Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using Utilities.Profile;

    /// <summary>
    /// Implementation of the get-azuresubscription cmdlet that works against
    /// the WindowsAzureProfile layer.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSubscription", DefaultParameterSetName = "ByName")]
    [OutputType(typeof(WindowsAzureSubscription))]
    public class GetAzureSubscriptionCommand : SubscriptionCmdletBase
    {
        public GetAzureSubscriptionCommand() : base(false)
        {
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the subscription", ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        public string SubscriptionName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Retrieves the default subscription", ParameterSetName = "Default")]
        public SwitchParameter Default { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Retrieves the current subscription", ParameterSetName = "Current")]
        public SwitchParameter Current { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Retrieves extended details about subscription such as quota and usage")]
        public SwitchParameter ExtendedDetails { get; set; }

        public override void ExecuteCmdlet()
        {
            switch (ParameterSetName)
            {
                case "ByName":
                    GetByName();
                    break;
                case "Default":
                    GetDefault();
                    break;
                case "Current":
                    GetCurrent();
                    break;
            }
        }

        public void GetByName()
        {
            IEnumerable<AzureSubscription> subscriptions = AzureSession.Profile
                .Subscriptions.Values.Union(LoadSubscriptionsFromServer());
            if (!string.IsNullOrEmpty(SubscriptionName))
            {
                subscriptions = subscriptions.Where(s => s.Name == SubscriptionName);
            }
            WriteSubscriptions(subscriptions);
        }

        public void GetDefault()
        {
            var defaultSubscription = AzureSession.Profile.DefaultSubscription;

            if (defaultSubscription == null)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException(Resources.InvalidDefaultSubscription), 
                    string.Empty,
                    ErrorCategory.InvalidData, null));
            }
            else
            {
                WriteSubscriptions(defaultSubscription);
            }
        }

        public void GetCurrent()
        {
            //
            // Explicitly ignore the SubscriptionDataFile property here,
            // since current is strictly in-memory and we want the real
            // current subscription.
            //
            var currentProfile = BaseProfile;
            if (currentProfile.CurrentSubscription == null)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException(Resources.InvalidSelectedSubscription),
                    string.Empty,
                    ErrorCategory.InvalidData, null));
            }
            else
            {
                WriteSubscriptions(AzureSession.CurrentSubscription);
            }
        }

        private void WriteSubscriptions(params AzureSubscription[] subscriptions)
        {
            WriteSubscriptions((IEnumerable<AzureSubscription>) subscriptions);
        }

        private void WriteSubscriptions(IEnumerable<AzureSubscription> subscriptions)
        {
            IEnumerable<AzureSubscription> subscriptionOutput;

            if (ExtendedDetails.IsPresent)
            {
                subscriptionOutput = subscriptions.Select(s => s.ToExtendedData(AzureSession.ClientFactory, AzureSession.Profile));
            }
            else
            {
                subscriptionOutput = subscriptions;
            }

            foreach (AzureSubscription subscription in subscriptionOutput)
            {
                WriteObject(subscription);
            }
        }
    }

    static class SubscriptionConversions
    {
        internal static SubscriptionDataExtended ToExtendedData(this AzureSubscription subscription, IClientFactory clientFactory, AzureProfile azureProfile)
        {
            using (var client = clientFactory.CreateClient<ManagementClient>())
            {
                var response = client.Subscriptions.Get();
                var environment = azureProfile.Environments[subscription.Environment];

                SubscriptionDataExtended result = new SubscriptionDataExtended
                {
                    AccountAdminLiveEmailId = response.AccountAdminLiveEmailId,
                    ActiveDirectoryUserId = subscription.GetProperty(AzureSubscription.Property.UserAccount),
                    CurrentCoreCount = response.CurrentCoreCount,
                    CurrentHostedServices = response.CurrentHostedServices,
                    CurrentDnsServers = 0, // TODO: Add to spec
                    CurrentLocalNetworkSites = 0, // TODO: Add to spec
                    MaxCoreCount = response.MaximumCoreCount,
                    MaxDnsServers = response.MaximumDnsServers,
                    MaxHostedServices = response.MaximumHostedServices,
                    MaxVirtualNetworkSites = response.MaximumVirtualNetworkSites,
                    MaxStorageAccounts = response.MaximumStorageAccounts,
                    ServiceAdminLiveEmailId = response.ServiceAdminLiveEmailId,
                    SubscriptionRealName = response.SubscriptionName,
                    SubscriptionStatus = response.SubscriptionStatus.ToString(),
                    Name = subscription.Name,
                    Id = subscription.Id,
                    ServiceEndpoint = environment.GetEndpoint(AzureEnvironment.Endpoint.ServiceEndpoint),
                    ResourceManagerEndpoint = environment.GetEndpoint(AzureEnvironment.Endpoint.ResourceManagerEndpoint),
                    IsDefault = subscription.GetProperty(AzureSubscription.Property.Default) != null,
                    Certificate = WindowsAzureCertificate.FromThumbprint(subscription.GetProperty(AzureSubscription.Property.Thumbprint)),
                    CurrentStorageAccountName = subscription.GetProperty(AzureSubscription.Property.CloudStorageAccount)
                };
                
                return result;
            }
        }
    }
}

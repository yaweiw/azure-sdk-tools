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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Microsoft.Azure.Subscriptions;
using Microsoft.Azure.Subscriptions.Models;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Convenience client for azure subscriptions.
    /// </summary>
    public class AzureSubscriptionsClient
    {
        private AzurePowerShell azurePowerShell;

        public AzureSubscriptionsClient(AzurePowerShell azurePowerShell)
        {
            this.azurePowerShell = azurePowerShell;
        }

        public IEnumerable<AzureSubscription> LoadSubscriptionsFromPublishSettingsFile(string filePath)
        {
            var currentEnvironment = azurePowerShell.CurrentEnvironment;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new ArgumentException("File path is not valid.", "filePath");
            }
            return PublishSettingsImporter.ImportAzureSubscription(File.OpenRead(filePath), currentEnvironment.Name);
        }

        public IEnumerable<AzureSubscription> LoadSubscriptionsFromServer()
        {
            var currentMode = PowerShellUtilities.GetCurrentMode();
            var currentSubscription = azurePowerShell.CurrentSubscription;
            var currentEnvironment = azurePowerShell.CurrentEnvironment;
            if (currentSubscription == null)
            {
                string userId = null;
                return AzurePowerShell.AuthenticationFactory.Authenticate(currentEnvironment, currentMode, true, out userId);
            }
            else
            {
                // Get all AD accounts and iterate
                var userIds = azurePowerShell.Profile.Subscriptions.Values
                    .Select(s => s.Properties[AzureSubscription.Property.UserAccount]).Distinct();

                List<AzureSubscription> subscriptions = new List<AzureSubscription>();
                foreach (var userId in userIds)
                {
                    subscriptions = subscriptions.Union(AzurePowerShell.AuthenticationFactory
                        .Authenticate(currentEnvironment, currentMode, true, userId)).ToList();
                }

                if (subscriptions.Any())
                {
                    return subscriptions;
                }
                else
                {
                    return new AzureSubscription[0];
                }
            }
        }

        private IEnumerable<AzureSubscription> GetServerSubscriptions(AzureEnvironment environment)
        {
            IAccessToken commonTenantToken = azurePowerShell.AuthenticationFactory.GetToken();

            TenantListResult tenants;
            using (var subscriptionClient = azurePowerShell.ClientFactory.CreateClient<Azure.Subscriptions.SubscriptionClient>(
                new TokenCloudCredentials(commonTenantToken.AccessToken),
                new Uri(environment.GetEndpoint(AzureEnvironment.Endpoint.ResourceManagerEndpoint))))
            {
                tenants = subscriptionClient.Tenants.List();
            }

            // Go over each tenant and get all subscriptions for tenant
            foreach (var tenant in tenants.TenantIds)
            {
                // Generate tenant specific token to query list of subscriptions
                IAccessToken tenantToken = azurePowerShell.AuthenticationFactory.GetToken(GetAdalConfiguration(environment, tenant.TenantId), commonTenantToken.UserId,
                    password);

                using (var subscriptionClient = azurePowerShell.ClientFactory.CreateClient<Azure.Subscriptions.SubscriptionClient>(
                        new TokenCloudCredentials(tenantToken.AccessToken),
                        new Uri(environment.GetEndpoint(AzureEnvironment.Endpoint.ResourceManagerEndpoint))))
                {
                    var subscriptionListResult = subscriptionClient.Subscriptions.List();
                    foreach (var subscription in subscriptionListResult.Subscriptions)
                    {
                        AzureSubscription psSubscription = new AzureSubscription
                        {
                            Id = new Guid(subscription.SubscriptionId),
                            Name = subscription.DisplayName,
                            Environment = environment.Name
                        };
                        if (commonTenantToken.LoginType == LoginType.LiveId)
                        {
                            subscriptionTokenCache[psSubscription.Id] = tenantToken;
                        }
                        else
                        {
                            subscriptionTokenCache[psSubscription.Id] = commonTenantToken;
                        }

                        yield return psSubscription;
                    }
                }
            }
        }

        public void SaveSubscriptions(IEnumerable<AzureSubscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                azurePowerShell.Profile.Subscriptions[subscription.Id] = subscription;
            }
        }
    }
}
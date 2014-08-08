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
using Microsoft.Azure.Subscriptions;
using Microsoft.Azure.Subscriptions.Models;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using Microsoft.WindowsAzure.Commands.Common.Interfaces;

namespace Microsoft.WindowsAzure.Commands.Common
{
    /// <summary>
    /// Convenience client for azure profile and subscriptions.
    /// </summary>
    public class ProfileClient
    {
        public static Func<string, IDataStore> DataStore { get; set; }

        public AzureProfile Profile { get; private set; }

        private static void UpgradeProfile()
        {
            string oldProfilePath = Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.OldProfileFile);
            AzureProfile profile = new AzureProfile(DataStore(oldProfilePath));

            // Save the profile to the disk
            profile.Save();

            // Rename WindowsAzureProfile.xml to AzureProfile.json
            File.Move(oldProfilePath, Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.ProfileFile));
        }

        static ProfileClient()
        {
            DataStore = p => new DiskDataStore(p);
            if (File.Exists(Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.OldProfileFile)))
            {
                UpgradeProfile();
            }
        }

        public ProfileClient()
            : this(Path.Combine(AzurePowerShell.ProfileDirectory, AzurePowerShell.ProfileFile))
        {

        }

        public ProfileClient(string profilePath)
        {
            Profile = new AzureProfile(DataStore(profilePath));
        }

        public IEnumerable<AzureSubscription> LoadSubscriptionsFromPublishSettingsFile(string filePath)
        {
            var currentEnvironment = AzureSession.CurrentEnvironment;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new ArgumentException("File path is not valid.", "filePath");
            }
            return PublishSettingsImporter.ImportAzureSubscription(File.OpenRead(filePath), currentEnvironment.Name);
        }

        public IEnumerable<AzureSubscription> LoadSubscriptionsFromServer()
        {
            var currentMode = PowerShellUtilities.GetCurrentMode();
            var currentSubscription = AzureSession.CurrentSubscription;
            var currentEnvironment = AzureSession.CurrentEnvironment;
            if (currentSubscription == null)
            {
                UserCredentials credentials = new UserCredentials {NoPrompt = true};
                return LoadSubscriptionsFromServer(currentEnvironment, currentMode, ref credentials);
            }
            else
            {
                // Get all AD accounts and iterate
                var userIds = Profile.Subscriptions.Values
                    .Select(s => s.Properties[AzureSubscription.Property.UserAccount]).Distinct();

                List<AzureSubscription> subscriptions = new List<AzureSubscription>();
                foreach (var userId in userIds)
                {
                    UserCredentials credentials = new UserCredentials { NoPrompt = true, UserName = userId };
                    subscriptions = subscriptions
                        .Union(LoadSubscriptionsFromServer(currentEnvironment, currentMode, ref credentials)).ToList();
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

        private IList<AzureSubscription> LoadSubscriptionsFromServer(AzureEnvironment environment, AzureModule currentMode,
            ref UserCredentials credentials)
        {
            List<AzureSubscription> result;
            if (currentMode == AzureModule.AzureResourceManager)
            {
                result = GetResourceManagerSubscriptions(environment, ref credentials)
                    .Union(GetServiceManagementSubscriptions(environment, ref credentials))
                    .ToList();
            }
            else
            {
                result = GetServiceManagementSubscriptions(environment, ref credentials)
                    .ToList();
            }

            // Set user ID
            foreach (var subscription in result)
            {
                subscription.Properties[AzureSubscription.Property.UserAccount] = credentials.UserName;
            }

            return result;
        }

        private IEnumerable<AzureSubscription> GetResourceManagerSubscriptions(AzureEnvironment environment, ref UserCredentials credentials)
        {
            IAccessToken commonTenantToken = AzureSession.AuthenticationFactory.Authenticate(environment, ref credentials);

            List<AzureSubscription> result = new List<AzureSubscription>();
            TenantListResult tenants;
            using (var subscriptionClient = AzureSession.ClientFactory.CreateClient<Azure.Subscriptions.SubscriptionClient>(
                new TokenCloudCredentials(commonTenantToken.AccessToken),
                environment.GetEndpoint(AzureEnvironment.Endpoint.ResourceManagerEndpoint)))
            {
                tenants = subscriptionClient.Tenants.List();
            }

            // Go over each tenant and get all subscriptions for tenant
            foreach (var tenant in tenants.TenantIds)
            {
                // Generate tenant specific token to query list of subscriptions
                IAccessToken tenantToken = AzureSession.AuthenticationFactory.Authenticate(environment, tenant.TenantId, ref credentials);

                using (var subscriptionClient = AzureSession.ClientFactory.CreateClient<Azure.Subscriptions.SubscriptionClient>(
                        new TokenCloudCredentials(tenantToken.AccessToken),
                        environment.GetEndpoint(AzureEnvironment.Endpoint.ResourceManagerEndpoint)))
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
                            AzureSession.SubscriptionTokenCache[psSubscription.Id] = tenantToken;
                        }
                        else
                        {
                            AzureSession.SubscriptionTokenCache[psSubscription.Id] = commonTenantToken;
                        }

                        result.Add(psSubscription);
                    }
                }
            }

            return result;
        }

        private IEnumerable<AzureSubscription> GetServiceManagementSubscriptions(AzureEnvironment environment, ref UserCredentials credentials)
        {
            IAccessToken commonTenantToken = AzureSession.AuthenticationFactory.Authenticate(environment, ref credentials);

            List<AzureSubscription> result = new List<AzureSubscription>();
            using (var subscriptionClient = AzureSession.ClientFactory.CreateClient<WindowsAzure.Subscriptions.SubscriptionClient>(
                        new TokenCloudCredentials(commonTenantToken.AccessToken),
                        environment.GetEndpoint(AzureEnvironment.Endpoint.ServiceEndpoint)))
            {
                var subscriptionListResult = subscriptionClient.Subscriptions.List();
                foreach (var subscription in subscriptionListResult.Subscriptions)
                {
                    AzureSubscription psSubscription = new AzureSubscription
                    {
                        Id = new Guid(subscription.SubscriptionId),
                        Name = subscription.SubscriptionName,
                        Environment = environment.Name
                    };
                    if (commonTenantToken.LoginType == LoginType.LiveId)
                    {
                        AzureSession.SubscriptionTokenCache[psSubscription.Id] = 
                            AzureSession.AuthenticationFactory.Authenticate(environment, 
                            subscription.ActiveDirectoryTenantId, ref credentials);
                    }
                    else
                    {
                        AzureSession.SubscriptionTokenCache[psSubscription.Id] = commonTenantToken;
                    }

                    result.Add(psSubscription);
                }
            }

            return result;
        }

        public void SaveSubscriptions(IEnumerable<AzureSubscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                Profile.Subscriptions[subscription.Id] = subscription;
            }

            Profile.Save();
        }
    }
}
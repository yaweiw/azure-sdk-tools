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
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Subscriptions;
using Microsoft.Azure.Subscriptions.Models;
using Microsoft.WindowsAzure.Commands.Common.Model;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

namespace Microsoft.WindowsAzure.Commands.Common.Model
{
    public class AuthenticationFactory : IAuthenticationFactory
    {
        private const string CommonAdTenant = "Common";

        private IDictionary<Guid, IAccessToken> subscriptionTokenCache = new Dictionary<Guid, IAccessToken>();

        public AuthenticationFactory()
        {
            TokenProvider = new AdalTokenProvider();
        }

        public ITokenProvider TokenProvider { get; set; }

        public IEnumerable<Guid> Authenticate(AzureEnvironment environment, out string userId)
        {
            string newUserId = null;
            var subscriptions = AuthenticateAndGetSubscriptions(environment, ref newUserId, null, false);
            userId = newUserId;
            return subscriptions;
        }

        public IEnumerable<Guid> Authenticate(AzureEnvironment environment, string userId)
        {
            return AuthenticateAndGetSubscriptions(environment, ref userId, null, false);
        }

        public IEnumerable<Guid> Authenticate(AzureEnvironment environment, string userId, SecureString password)
        {
            return AuthenticateAndGetSubscriptions(environment, ref userId, password, false);
        }

        public IEnumerable<Guid> RefreshUserToken(AzureEnvironment environment, string userId)
        {
            return AuthenticateAndGetSubscriptions(environment, ref userId, null, true);
        }

        private IList<Guid> AuthenticateAndGetSubscriptions(AzureEnvironment environment, ref string userId, SecureString password, bool noPrompt)
        {
            Func<AdalConfiguration, string, SecureString, IAccessToken> getTokenFunction = TokenProvider.GetNewToken;
            List<Guid> result = new List<Guid>();

            if (noPrompt)
            {
                getTokenFunction = TokenProvider.GetCachedToken;
            }

            // Get common token and list all tenants
            var commonTenantToken = getTokenFunction(GetAdalConfiguration(environment, CommonAdTenant), userId, password);
            userId = commonTenantToken.UserId;
            TenantListResult tenants;
            using (var subscriptionClient = AzurePowerShell.ClientFactory.CreateClient<SubscriptionClient>(
                    new TokenCloudCredentials(commonTenantToken.AccessToken),
                    environment.GetEndpoint(AzureEnvironment.Endpoint.ResourceManagerEndpoint)))
            {
                tenants = subscriptionClient.Tenants.List();
            }

            // Go over each tenant and get all subscriptions for tenant
            foreach (var tenant in tenants.TenantIds)
            {
                // Generate tenant specific token to query list of subscriptions
                var tenantToken = getTokenFunction(GetAdalConfiguration(environment, tenant.TenantId), commonTenantToken.UserId, password);

                using (var subscriptionClient = AzurePowerShell.ClientFactory.CreateClient<SubscriptionClient>(
                    new TokenCloudCredentials(tenantToken.AccessToken), 
                    environment.GetEndpoint(AzureEnvironment.Endpoint.ResourceManagerEndpoint)))
                {
                    var subscriptionListResult = subscriptionClient.Subscriptions.List();
                    foreach (var subscription in subscriptionListResult.Subscriptions)
                    {
                        var subscriptionId = new Guid(subscription.Id);
                        if (commonTenantToken.LoginType == LoginType.LiveId)
                        {
                            subscriptionTokenCache[subscriptionId] = tenantToken;
                        }
                        else
                        {
                            subscriptionTokenCache[subscriptionId] = commonTenantToken;
                        }

                        result.Add(subscriptionId);
                    }
                }
            }

            return result;
        }

        public SubscriptionCloudCredentials GetSubscriptionCloudCredentials(Guid subscriptionId)
        {
            var subscription = AzurePowerShell.Profile.GetSubscription(subscriptionId);
            if (subscription == null)
            {
                throw new ArgumentException("Specified subscription has not been loaded.");
            }

            var environment = AzurePowerShell.Profile.GetEnvironment(subscription.Environment);
            var userId = GetUserId(subscriptionId, environment);
            var certificate = GetCertificate(subscriptionId, environment);

            if (subscriptionTokenCache.ContainsKey(subscriptionId))
            {
                return new AccessTokenCredential(subscriptionId.ToString(), subscriptionTokenCache[subscriptionId]);
            }
            else if (userId != null)
            {
                AuthenticateAndGetSubscriptions(environment, ref userId, null, true);
                if (!subscriptionTokenCache.ContainsKey(subscriptionId))
                {
                    throw new ArgumentException(Resources.InvalidSubscriptionState);
                }
                return new AccessTokenCredential(subscriptionId.ToString(), subscriptionTokenCache[subscriptionId]);
            }
            else if (certificate != null)
            {
                return new CertificateCloudCredentials(subscriptionId.ToString(), certificate);
            }
            else
            {
                throw new ArgumentException(Resources.InvalidSubscriptionState);
            }
        }

        private string GetUserId(Guid subscriptionId, AzureEnvironment environment)
        {
            foreach (var userAccount in environment.UserAccountSubscriptionsMap.Keys)
            {
                if (environment.UserAccountSubscriptionsMap[userAccount].Any(id => id == subscriptionId))
                {
                    return userAccount;
                }
            }
            return null;
        }

        private X509Certificate2 GetCertificate(Guid subscriptionId, AzureEnvironment environment)
        {
            foreach (var thumbprint in environment.ThumbprintSubscriptionsMap.Keys)
            {
                if (environment.ThumbprintSubscriptionsMap[thumbprint].Any(id => id == subscriptionId))
                {
                    return WindowsAzureCertificate.FromThumbprint(thumbprint);
                }
            }
            return null;
        }

        private AdalConfiguration GetAdalConfiguration(AzureEnvironment environment, string tenantId)
        {
            var adEndpoint = environment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectoryEndpoint];
            var adResourceId = environment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId];

            return new AdalConfiguration
            {
                AdEndpoint = adEndpoint,
                ResourceClientUri = adResourceId,
                AdDomain = tenantId
            };
        }
    }
}

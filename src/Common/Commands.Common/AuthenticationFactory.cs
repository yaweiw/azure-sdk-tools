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
using Microsoft.WindowsAzure.Commands.Common.Model;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

namespace Microsoft.WindowsAzure.Commands.Common
{
    public class AuthenticationFactory
    {
        private static readonly Lazy<AuthenticationFactory> instance =
            new Lazy<AuthenticationFactory>(() => new AuthenticationFactory());

        private const string CommonAdTenant = "Common";

        private IDictionary<Guid, IAccessToken> subscriptionTokenCache = new Dictionary<Guid, IAccessToken>();

        public static AuthenticationFactory Instance
        {
            get
            {
                return instance.Value;
            }
        }

        private AuthenticationFactory()
        {
            TokenProvider = new AdalTokenProvider();
        }

        public ITokenProvider TokenProvider { get; set; }

        public void Login()
        {
            Login(null, null, false);
        }

        public void Login(string userId)
        {
            Login(userId, null, false);
        }

        public void Login(string userId, SecureString password)
        {
            Login(userId, password, false);
        }

        public void RefreshUserToken(string userId)
        {
            Login(userId, null, true);
        }

        private void Login(string userId, SecureString password, bool noPrompt)
        {
            Func<AdalConfiguration, string, SecureString, IAccessToken> getTokenFunction = TokenProvider.GetNewToken;

            if (noPrompt)
            {
                getTokenFunction = TokenProvider.GetCachedToken;
            }

            var environment = AzureProfile.Instance.Environments[AzureProfile.Instance.CurrentEnvironment];
            var commonTenantToken = getTokenFunction(GetAdalConfiguration(environment, CommonAdTenant), userId, password);
            if (commonTenantToken.LoginType == LoginType.LiveId)
            {
                // TODO: Implement LiveId logging
            }
            else
            {
                return commonTenantToken;
            }
        }

        public SubscriptionCloudCredentials GetCredentials(Guid subscriptionId)
        {
            if (AzureProfile.Instance.Subscriptions.ContainsKey(subscriptionId))
            {
                throw new ArgumentException("Specified subscription has not been loaded.");
            }

            var subscription = AzureProfile.Instance.Subscriptions[subscriptionId];
            var environment = AzureProfile.Instance.Environments[subscription.Environment];
            var userId = GetUserId(subscriptionId, environment);
            var certificate = GetCertificate(subscriptionId, environment);

            if (subscriptionTokenCache.ContainsKey(subscriptionId))
            {
                return new AccessTokenCredential(subscriptionId.ToString(), subscriptionTokenCache[subscriptionId]);
            }
            else if (userId != null)
            {
                var commonTenantToken = TokenProvider.GetCachedToken(GetAdalConfiguration(environment, CommonAdTenant), userId, null);
                if (commonTenantToken.LoginType == LoginType.LiveId)
                {
                    // TODO: Implement LiveId logging
                    throw new NotImplementedException();
                }
                else
                {
                    subscriptionTokenCache[subscriptionId] = commonTenantToken;
                    return new AccessTokenCredential(subscriptionId.ToString(), commonTenantToken);
                }
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
            foreach (var userAccount in environment.UserAccounts.Keys)
            {
                if (environment.UserAccounts[userAccount].Any(id => id == subscriptionId))
                {
                    return userAccount;
                }
            }
            return null;
        }

        private X509Certificate2 GetCertificate(Guid subscriptionId, AzureEnvironment environment)
        {
            foreach (var thumbprint in environment.CertificateThumbprints.Keys)
            {
                if (environment.CertificateThumbprints[thumbprint].Any(id => id == subscriptionId))
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

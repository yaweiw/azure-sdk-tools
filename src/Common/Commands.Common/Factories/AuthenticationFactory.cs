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
using System.Security;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;

namespace Microsoft.WindowsAzure.Commands.Common.Factories
{
    public class AuthenticationFactory : IAuthenticationFactory
    {
        private const string CommonAdTenant = "Common";

        private readonly AzureProfile profile;

        public AuthenticationFactory(AzureProfile azureProfile)
        {
            TokenProvider = new AdalTokenProvider();
            profile = azureProfile;
        }

        public ITokenProvider TokenProvider { get; set; }

        public IAccessToken Authenticate(AzureEnvironment environment, ref UserCredentials credentials)
        {
            return Authenticate(environment, CommonAdTenant, ref credentials);
        }

        public IAccessToken Authenticate(AzureEnvironment environment, string tenant, ref UserCredentials credentials)
        {
            Func<AdalConfiguration, string, SecureString, IAccessToken> getTokenFunction = TokenProvider.GetNewToken;

            if (credentials.NoPrompt)
            {
                getTokenFunction = TokenProvider.GetCachedToken;
            }

            var token = getTokenFunction(GetAdalConfiguration(environment, CommonAdTenant), credentials.UserName, credentials.Password);
            credentials.UserName = token.UserId;
            return token;
        }

        public SubscriptionCloudCredentials GetSubscriptionCloudCredentials(Guid subscriptionId)
        {
            var subscription = profile.Subscriptions[subscriptionId];
            if (subscription == null)
            {
                throw new ArgumentException("Specified subscription has not been loaded.");
            }

            var environment = profile.Environments[subscription.Environment];
            var userId = subscription.GetProperty(AzureSubscription.Property.UserAccount);
            var certificate = WindowsAzureCertificate.FromThumbprint(subscription.GetProperty(AzureSubscription.Property.Thumbprint));

            if (AzureSession.SubscriptionTokenCache.ContainsKey(subscriptionId))
            {
                return new AccessTokenCredential(subscriptionId.ToString(), AzureSession.SubscriptionTokenCache[subscriptionId]);
            }
            else if (userId != null)
            {
                if (!AzureSession.SubscriptionTokenCache.ContainsKey(subscriptionId))
                {
                    throw new ArgumentException(Resources.InvalidSubscriptionState);
                }
                return new AccessTokenCredential(subscriptionId.ToString(), AzureSession.SubscriptionTokenCache[subscriptionId]);
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

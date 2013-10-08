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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// A token provider that uses ADAL to retrieve
    /// tokens from Azure Active Directory
    /// </summary>
    public class AdalTokenProvider : ITokenProvider
    {
        // TODO: Add storage for token cache
        private readonly IWin32Window parentWindow;

        public AdalTokenProvider()
            : this(new ConsoleParentWindow())
        {
        }

        public AdalTokenProvider(IWin32Window parentWindow)
        {
            this.parentWindow = parentWindow;
        }

        public IAccessToken GetToken(WindowsAzureSubscription subscription, string userId)
        {
            var config = new AdalConfiguration(subscription);
            return new AdalAccessToken(AcquireToken(config, userId), this, config);
        }

        public IAccessToken GetNewToken(WindowsAzureEnvironment environment)
        {
            var config = new AdalConfiguration(environment);
            return new AdalAccessToken(AcquireToken(config), this, config);
        }

        private void Renew(AdalAccessToken token)
        {
            token.AuthResult = AcquireToken(token.Configuration, token.UserId);
        }

        private AuthenticationContext CreateContext(AdalConfiguration config)
        {
            return new AuthenticationContext(config.AdEndpoint + config.AdDomain, config.ValidateAuthority)
            {
                OwnerWindow = parentWindow
            };
        }

        // We have to run this in a separate thread to guarantee that it's STA. This method
        // handles the threading details.
        private AuthenticationResult AcquireToken(AdalConfiguration config, string userId = null)
        {
            AuthenticationResult result = null;
            Exception ex = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var context = CreateContext(config);
                    if (string.IsNullOrEmpty(userId))
                    {
                        result = context.AcquireToken(config.ResourceClientUri, config.ClientId,
                            config.ClientRedirectUri, PromptBehavior.Always, AdalConfiguration.EnableEbdMagicCookie);
                    }
                    else
                    {
                        result = context.AcquireToken(config.ResourceClientUri, config.ClientId,
                            config.ClientRedirectUri, userId, AdalConfiguration.EnableEbdMagicCookie);
                    }
                }
                catch (Exception threadEx)
                {
                    ex = threadEx;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "AcquireTokenThread";
            thread.Start();
            thread.Join();
            if (ex != null)
            {
                throw new Exception(string.Format("Could not acquire access token: {0}", ex.Message), ex);
            }

            return result;
        }

        /// <summary>
        /// Implementation of <see cref="IAccessToken"/> using data from ADAL
        /// </summary>
        private class AdalAccessToken : IAccessToken
        {
            internal AdalConfiguration Configuration;
            internal AuthenticationResult AuthResult;
            private readonly AdalTokenProvider tokenProvider;

            public AdalAccessToken(AuthenticationResult authResult, AdalTokenProvider tokenProvider, AdalConfiguration configuration)
            {
                AuthResult = authResult;
                this.tokenProvider = tokenProvider;
                Configuration = configuration;
            }

            public void AuthorizeRequest(Action<string, string> authTokenSetter)
            {
                tokenProvider.Renew(this);
                authTokenSetter(AuthResult.AccessTokenType, AuthResult.AccessToken);
            }

            public string AccessToken { get { return AuthResult.AccessToken; } }
            public string UserId { get { return AuthResult.UserInfo.UserId; } }

            public LoginType LoginType
            {
                get
                {
                    if (AuthResult.UserInfo.IdentityProvider != null)
                    {
                        return LoginType.LiveId;
                    }
                    return LoginType.OrgId;
                }
            }
        }
    }
}
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
        private IWin32Window parentWindow;

        public AdalTokenProvider()
        {
            parentWindow = new ConsoleParentWindow();
        }

        public IAccessToken GetToken(WindowsAzureEnvironment environment, string userId, LoginType loginType)
        {
            throw new System.NotImplementedException();
        }

        public IAccessToken GetNewToken(WindowsAzureEnvironment environment, LoginType loginType)
        {
            var config = new AdalConfiguration(environment);
            var context = new AuthenticationContext(config.AdEndpoint + config., config.ValidateAuthority)
            {
                OwnerWindow = parentWindow
            };

            Func<AuthenticationResult> acquireFunc = () => context.AcquireToken(config.ResourceClientUri, config.ClientId,
                config.ClientRedirectUri, PromptBehavior.Always);
            return new AdalAccessToken(AcquireToken(acquireFunc), this);
        }

        private void Renew(AdalAccessToken token)
        {
            // TODO: Update the token. Need to update in place to preserve identity.
            token.AuthResult = null;
        }

        private AuthenticationResult AcquireToken(Func<AuthenticationResult> acquireFunc)
        {
            AuthenticationResult result = null;
            Exception ex = null;

            var thread = new Thread(() =>
            {
                try
                {
                    result = acquireFunc();
                }
                catch (Exception threadEx)
                {
                    ex = threadEx;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null)
            {
                throw new Exception("Could not acquire token", ex);
            }

            return result;
        }

        /// <summary>
        /// Implementation of <see cref="IAccessToken"/> using data from ADAL
        /// </summary>
        private class AdalAccessToken : IAccessToken
        {
            internal AuthenticationResult AuthResult;
            private readonly AdalTokenProvider tokenProvider;

            public AdalAccessToken(AuthenticationResult authResult, AdalTokenProvider tokenProvider)
            {
                this.AuthResult = authResult;
                this.tokenProvider = tokenProvider;
            }

            public void AuthorizeRequest(Action<string, string> authTokenSetter)
            {
                tokenProvider.Renew(this);
                authTokenSetter(AuthResult.AccessTokenType, AuthResult.AccessToken);
            }

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
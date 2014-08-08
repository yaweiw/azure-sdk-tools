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

using System.Security;

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.Common
{
    using Commands.Utilities.Common;
    using Commands.Utilities.Common.Authentication;

    public class FakeAccessTokenProvider : ITokenProvider
    {
        private readonly IAccessToken accessToken;

        public FakeAccessTokenProvider(string token)
            : this(token, "user@live.com")
        { }

        public FakeAccessTokenProvider(string token, string userId)
        {
            this.accessToken = new FakeAccessToken()
            {
                AccessToken = token,
                UserId = userId
            };
        }

        public IAccessToken GetCachedToken(WindowsAzureSubscription subscription, string userId)
        {
            return this.accessToken;
        }

        public IAccessToken GetNewToken(WindowsAzureEnvironment environment)
        {
            return this.accessToken;
        }

        public IAccessToken GetNewToken(WindowsAzureSubscription subscription, string userId)
        {
            return this.accessToken;
        }

        public IAccessToken GetNewToken(WindowsAzureEnvironment environment, string userId, SecureString password)
        {
            return this.accessToken;
        }

        public IAccessToken GetNewToken(AdalConfiguration config, string userId, SecureString password)
        {
            return this.accessToken;
        }

        public IAccessToken GetCachedToken(AdalConfiguration config, string userId, SecureString password)
        {
            return this.accessToken;
        }
    }
}
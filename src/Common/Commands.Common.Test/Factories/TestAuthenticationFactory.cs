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
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Test.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Xunit;

namespace Microsoft.WindowsAzure.Commands.Common.Test.Factories
{
    public class TestAuthenticationFactory
    {
        private MockCmdlt cmdlt = new MockCmdlt();
        public TestAuthenticationFactory()
        {
            HttpRestCallLogger.CurrentCmdlet = cmdlt;
        }

        [Fact]
        public void GetCloudCredentialThrowsExceptionForInvalidSubscription()
        {
            AzurePowerShell.Profile = new AzureProfile(new MockFileStore());
            AzurePowerShell.Profile.Subscriptions.Add(new AzureSubscription
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Environment = "Test"
            });
            AzurePowerShell.Profile.Environments.Add(new AzureEnvironment
            {
                Name = "Test"
            });
            Assert.Throws<ArgumentException>(() => AzurePowerShell.AuthenticationFactory.GetSubscriptionCloudCredentials(Guid.NewGuid()));
        }

        [Fact]
        public void AuthenticateReturnsSubscriptions()
        {
            AzurePowerShell.Profile = new AzureProfile(new MockFileStore());
            string userName = "";
            var ids = AzurePowerShell.AuthenticationFactory.Authenticate(AzurePowerShell.Profile.CurrentEnvironment, out userName);
            Assert.NotNull(userName);
            Assert.NotEmpty(ids);
        }
    }
}

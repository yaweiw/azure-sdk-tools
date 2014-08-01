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
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Commands.Common.Model;
using Xunit;

namespace Microsoft.WindowsAzure.Commands.Common.Test.Factories
{
    public class TestAuthenticationFactory
    {
        [Fact]
        public void GetCloudCredentialThrowsExceptionForInvalidSubscription()
        {
            AzurePowerShell.Profile = new AzureProfile(new InMemoryFileStore());
            AzurePowerShell.Profile.Subscriptions = new List<AzureSubscription>();
            AzurePowerShell.Profile.Subscriptions.Add(new AzureSubscription
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Environment = "Test"
            });
            AzurePowerShell.AuthenticationFactory.GetSubscriptionCloudCredentials(Guid.NewGuid());
        }
    }
}

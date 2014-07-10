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


using Microsoft.WindowsAzure.Testing;
using Xunit;

namespace Microsoft.Azure.Commands.Resources.Test.ScenarioTests
{
    public class ResourceTests : ResourcesTestsBase
    {
        [Fact]
        public void TestCreatesNewSimpleResource()
        {
            using (UndoContext context = UndoContext.Current)
            {
                context.Start();

                var resourceManagementClient = GetResourceManagementClient();
                var subscriptionsClient = GetSubscriptionClient();
                SetupManagementClients(resourceManagementClient, subscriptionsClient);

                RunPowerShellTest("Test-CreatesNewSimpleResource");
            }
        }

        [Fact]
        public void TestCreatesNewComplexResource()
        {
            using (UndoContext context = UndoContext.Current)
            {
                context.Start();

                var resourceManagementClient = GetResourceManagementClient();
                var subscriptionsClient = GetSubscriptionClient();
                SetupManagementClients(resourceManagementClient, subscriptionsClient);

                RunPowerShellTest("Test-CreatesNewComplexResource");
            }
        }

        [Fact]
        public void TestGetResourcesViaPiping()
        {
            using (UndoContext context = UndoContext.Current)
            {
                context.Start();

                var resourceManagementClient = GetResourceManagementClient();
                var subscriptionsClient = GetSubscriptionClient();
                SetupManagementClients(resourceManagementClient, subscriptionsClient);

                RunPowerShellTest("Test-GetResourcesViaPiping");
            }
        }

        [Fact]
        public void TestGetResourcesFromEmptyGroup()
        {
            RunPowerShellTest("Test-GetResourcesFromEmptyGroup");
        }

        [Fact]
        public void TestGetResourcesFromNonExisingGroup()
        {
            RunPowerShellTest("Test-GetResourcesFromNonExisingGroup");
        }

        [Fact]
        public void TestGetResourcesForNonExisingType()
        {
            RunPowerShellTest("Test-GetResourcesForNonExisingType");
        }

        [Fact]
        public void TestGetResourceForNonExisingResource()
        {
            RunPowerShellTest("Test-GetResourceForNonExisingResource");
        }

        [Fact]
        public void TestGetResourcesViaPipingFromAnotherResource()
        {
            RunPowerShellTest("Test-GetResourcesViaPipingFromAnotherResource");
        }
    }
}

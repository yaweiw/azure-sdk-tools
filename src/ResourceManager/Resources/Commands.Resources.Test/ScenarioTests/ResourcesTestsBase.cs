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
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Subscriptions;
using Microsoft.Azure.Utilities.HttpRecorder;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Testing;

namespace Microsoft.Azure.Commands.Resources.Test.ScenarioTests
{
    public abstract class ResourcesTestsBase : IDisposable
    {
        private EnvironmentSetupHelper helper;

        protected ResourcesTestsBase()
        {
            helper = new EnvironmentSetupHelper();
        }

        protected void SetupManagementClients(params object[] initializedManagementClients)
        {
            helper.SetupManagementClients(initializedManagementClients);
        }

        protected void RunPowerShellTest(params string[] scripts)
        {
            helper.SetupEnvironment(AzureModule.AzureResourceManager);
            helper.SetupModules(AzureModule.AzureResourceManager, "ScenarioTests\\Common.ps1",
                "ScenarioTests\\" + this.GetType().Name + ".ps1");

            helper.RunPowerShellTest(scripts);
        }

        /// <summary>
        /// Default constructor for management clients, using the TestSupport Infrastructure
        /// </summary>
        /// <returns>A resource management client, created from the current context (environment variables)</returns>
        protected ResourceManagementClient GetResourceManagementClient()
        {
            return TestBase.GetServiceClient<ResourceManagementClient>(new CSMTestEnvironmentFactory());
        }

        /// <summary>
        /// Default constructor for management clients, using the TestSupport Infrastructure
        /// </summary>
        /// <returns>A subscription client, created from the current context (environment variables)</returns>
        protected SubscriptionClient GetSubscriptionClient()
        {
            return TestBase.GetServiceClient<SubscriptionClient>(new CSMTestEnvironmentFactory());
        }

        public void Dispose()
        {
            helper.Dispose();
        }
    }
}

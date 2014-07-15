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
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Management;
using Microsoft.WindowsAzure.Management.Compute;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.WebSites;
using Microsoft.WindowsAzure.Testing;

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.WebsitesTests
{
    public abstract class WebsitesTestsBase : IDisposable
    {
        private EnvironmentSetupHelper helper;

        protected WebsitesTestsBase()
        {
            helper = new EnvironmentSetupHelper();
        }

        protected void SetupManagementClients()
        {
            var websitesClient = GetWebsitesClient();
            var managementClient = GetManagementClient();
            var storageClient = GetStorageManagementClient();
            var computeClient = GetComputeManagementClient();

            helper.SetupManagementClients(websitesClient, managementClient, storageClient, computeClient);
        }

        protected void RunPowerShellTest(params string[] scripts)
        {
            using (UndoContext context = UndoContext.Current)
            {
                context.Start(TestUtilities.GetCallingClass(2), TestUtilities.GetCurrentMethodName(2));

                SetupManagementClients();

                helper.SetupEnvironment(AzureModule.AzureServiceManagement);
                helper.SetupModules(AzureModule.AzureServiceManagement, 
                    "Resources\\Websites\\Common.ps1",
                    "Resources\\Websites\\" + this.GetType().Name + ".ps1");

                helper.RunPowerShellTest(scripts);
            }
        }

        protected WebSiteManagementClient GetWebsitesClient()
        {
            return TestBase.GetServiceClient<WebSiteManagementClient>(new RDFETestEnvironmentFactory());
        }

        protected StorageManagementClient GetStorageManagementClient()
        {
            return TestBase.GetServiceClient<StorageManagementClient>(new RDFETestEnvironmentFactory());
        }

        protected ComputeManagementClient GetComputeManagementClient()
        {
            return TestBase.GetServiceClient<ComputeManagementClient>(new RDFETestEnvironmentFactory());
        }

        protected ManagementClient GetManagementClient()
        {
            return TestBase.GetServiceClient<ManagementClient>(new RDFETestEnvironmentFactory());
        }

        public void Dispose()
        {
            helper.Dispose();
        }
    }
}

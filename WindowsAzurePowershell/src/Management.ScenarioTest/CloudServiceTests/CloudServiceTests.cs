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

namespace Microsoft.WindowsAzure.Management.ScenarioTest.CloudServiceTests
{
    using System.Management.Automation;
    using Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;

    [TestClass]
    public class CloudServiceTests : WindowsAzurePowerShellTest
    {
        public CloudServiceTests()
            : base("CloudService\\Common.ps1",
                   "CloudService\\CloudServiceTests.ps1")
        {

        }

        #region Publish-AzureServiceProject Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestPublishAzureCacheService()
        {
            RunPowerShellTest("Test-PublishCacheService");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestUpdateAzureCacheService()
        {
            RunPowerShellTest("Test-UpdateCacheService");
        }

        #endregion

 #region Remove-AzureService Scenario Tests

 [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestRemoveAzureServiceWithInvalidCredentials()
        {
            RunPowerShellTest("Test-WithInvalidCredentials { Remove-AzureService $(Get-CloudServiceName) -Force }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestRemoveAzureServiceWithNonExistingService()
        {
            RunPowerShellTest("Test-RemoveAzureServiceWithNonExistingService");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestRemoveAzureServiceWithProductionDeployment()
        {
            RunPowerShellTest("Test-RemoveAzureServiceWithProductionDeployment");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestRemoveAzureServiceWhatIf()
        {
            RunPowerShellTest("Test-RemoveAzureServiceWhatIf");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestRemoveAzureServiceWhatIfWithInvalidName()
        {
            RunPowerShellTest("Test-RemoveAzureServiceWhatIfWithInvalidName");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.CloudService)]
        public void TestRemoveAzureServicePipedFromGetAzureService()
        {
            RunPowerShellTest("Test-RemoveAzureServicePipedFromGetAzureService");
        }

        #endregion
    }
}

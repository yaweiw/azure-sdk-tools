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

namespace Microsoft.WindowsAzure.Management.ScenarioTest.ServiceManagemenet
{
    using System.Management.Automation;
    using Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;

    [TestClass]
    public class VMProvisionScenarioTests : WindowsAzurePowerShellTest
    {
        public VMProvisionScenarioTests()
            : base("CloudService\\Common.ps1",
                   "ServiceManagemenet\\Common.ps1",
                   "ServiceManagemenet\\VMProvisionTests.ps1"
            )
        {

        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceManagemenet)]
        [TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,Get-AzureVM,Get-AzureLocation,Import-AzurePublishSettingsFile,Get-AzureSubscription,Set-AzureSubscription)")]
        public void NewkWindowsAzureQuicVM()
        {
            powershell.Invoke();

            ServiceManagementCmdletTestHelper vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();
            
            string imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "MSFT", "testvmimage" }, false);
            string locationName = vmPowershellCmdlets.GetAzureLocationName(new[] { Resource.Location }, false);

            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            string newAzureQuickVMSvcName = Utilities.GetUniqueShortName("PSTestService");

            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, newAzureQuickVMSvcName, imageName, "p@ssw0rd", locationName);

            // Verify
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(newAzureQuickVMName, vmRoleCtxt.Name, true);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);

            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));
        }

        //[TestMethod]
        //[TestCategory(Category.All)]
        //[TestCategory(Category.ServiceManagemenet)]
        //[TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,Get-AzureVM,Get-AzureLocation,Import-AzurePublishSettingsFile,Get-AzureSubscription,Set-AzureSubscription)")]
        //public void TestCreateWindowsAzureQuickVM()
        //{
        //    // Setup
        //    ServiceManagementCmdletTestHelper vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();
        //    powershell.SetVariable("imageName", vmPowershellCmdlets.GetAzureVMImageName(new[] { "MSFT", "testvmimage" }, false));
        //    powershell.SetVariable("newAzureQuickVMName", Utilities.GetUniqueShortName("PSTestVM"));
        //    powershell.SetVariable("newAzureQuickVMSvcName", Utilities.GetUniqueShortName("PSTestService"));
            
        //    RunPowerShellTest("Set-CurrentStorageAccount", "Test-CreateWindowsAzureQuickVM");
        //}
    }
}

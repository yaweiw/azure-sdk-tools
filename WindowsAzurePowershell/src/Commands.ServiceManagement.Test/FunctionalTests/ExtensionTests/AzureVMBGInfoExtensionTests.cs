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



namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ExtensionTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [TestClass]
    public class AzureVMBGInfoExtensionTests:ServiceManagementTest
    {
        private string serviceName;
        private string vmName;
        private const string referenceNamePrefix = "Reference";
        private string version1 = "1.0";
        private string referenceName;
        private const string extesnionName = "BGInfo";
        private const string DisabledState = "Disable";
        private const string EnableState = "Enable";

        [ClassInitialize]
        public static void Intialize(TestContext context)
        {
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
        }

        [TestInitialize]
        public void TestIntialize()
        {
            pass = false;
            serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
            vmName = Utilities.GetUniqueShortName(vmNamePrefix);
            testStartTime = DateTime.Now;
            referenceName = Utilities.GetUniqueShortName(referenceNamePrefix);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            CleanupService(serviceName);
        }


        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureVMBGInfoExtension)")]
        public void GetAzureVMBGInfoExtensionTest()
        {
            try
            {
                //Deploy a new IaaS VM with Extension using Set-AzureVMExtension
                Console.WriteLine("Deploying a new vm with BGIinfo extension.");
                var vm = CreateIaaSVMObject(vmName);
                vm = vmPowershellCmdlets.SetAzureVMBGInfoExtension(vm, version1, referenceName, false);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("Deployed a vm {0}with BGIinfo extension.", vmName);

                
                var extesnion = GetBGInfo(vmName, serviceName);
                VerifyExtension(extesnion);

                //Disable the extension
                Console.WriteLine("Disable BGIinfo extension and update VM.");
                vm = vmPowershellCmdlets.SetAzureVMBGInfoExtension(vm, version1, referenceName, true);
                vmPowershellCmdlets.UpdateAzureVM(vmName, serviceName, vm);
                Console.WriteLine("BGIinfo extension disabled");

                extesnion = GetBGInfo(vmName, serviceName);
                VerifyExtension(extesnion,true);

                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
                vmPowershellCmdlets.RemoveAzureVMBGInfoExtension(vmRoleContext.VM);

                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureVMBGInfoExtension)")]
        public void UpdateVMWithExtensionTest()
        {
            try
            {
                Console.WriteLine("Deploying a new vm {0}",vmName);
                var vm = CreateIaaSVMObject(vmName);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("Deployed vm {0}", vmName);

                Console.WriteLine("Set BGInfo extension and update vm {0}." , vmName);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
                vm = vmPowershellCmdlets.SetAzureVMBGInfoExtension(vmRoleContext.VM, version1, referenceName, false);
                vmPowershellCmdlets.UpdateAzureVM(vmName, serviceName, vm);
                Console.WriteLine("BGInfo extension set and updated vm {0}.", vmName);

                var extesnion = GetBGInfo(vmName, serviceName);
                VerifyExtension(extesnion);
                
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureVMBGInfoExtension)")]
        public void AddRoleWithExtensionTest()
        {
            try
            {
                //Deploy a new IaaS VM with Extension using Add-AzureVMExtension
                Console.WriteLine("Deploying a new vm {0}", vmName);
                var vm1 = CreateIaaSVMObject(vmName);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm1 }, locationName);

                //Add a role with extension config
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("Deploying a new vm {0} with BGIinfo extension", vmName2);
                var vm2 = CreateIaaSVMObject(vmName2);
                vm2 = vmPowershellCmdlets.SetAzureVMBGInfoExtension(vm2, version1, referenceName, false);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm2 });

                var extesnion = GetBGInfo(vmName2, serviceName);
                VerifyExtension(extesnion);

                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }


        [TestMethod(), TestCategory("Sequential"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureVMBGInfoExtension)")]
        public void UpdateRoleWithExtensionTest()
        {
            try
            {
                //Deploy a new IaaS VM with Extension using Add-AzureVMExtension
                Console.WriteLine("Deploying a new vm {0}", vmName);
                var vm1 = CreateIaaSVMObject(vmName);

                //Add a role with extension config
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("Deploying a new vm {0}", vmName2);
                var vm2 = CreateIaaSVMObject(vmName2);

                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm1,vm2 }, locationName);

                Console.WriteLine("Set BGInfo extension and update vm {0}.", vmName2);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName2, serviceName);
                vm2 = vmPowershellCmdlets.SetAzureVMBGInfoExtension(vm2, version1, referenceName, false);
                vmPowershellCmdlets.UpdateAzureVM(vmName2, serviceName, vm2);
                Console.WriteLine("BGInfo extension set and updated vm {0}.", vmName2);

                var extesnion = GetBGInfo(vmName2, serviceName);
                VerifyExtension(extesnion);

                vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName2,serviceName);
                vmPowershellCmdlets.RemoveAzureVMBGInfoExtension(vmRoleContext.VM);

                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        private PersistentVM CreateIaaSVMObject(string vmName)
        {
            //Create an IaaS VM with a static CA.
            var azureVMConfigInfo = new AzureVMConfigInfo(vmName, InstanceSize.Small.ToString(), imageName);
            var azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password);
            var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
            return vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);
        }

        private VirtualMachineBGInfoExtensionContext GetBGInfo(string vmName, string serviceName)
        {
            Console.WriteLine("Get BGIinfo extension info.");
            var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
            var extesnion = vmPowershellCmdlets.GetAzureVMBGInfoExtension(vmRoleContext.VM);
            Utilities.PrintCompleteContext(extesnion);
            Console.WriteLine("Fetched BGIinfo extension info successfully.");
            return extesnion;
        }

        private void VerifyExtension(VirtualMachineBGInfoExtensionContext extension,bool disable=false)
        {
            Console.WriteLine("Verifying BGIinfo extension info.");
            Assert.AreEqual(version1, extension.Version);
            Assert.AreEqual(referenceName, extension.ReferenceName);
            if (disable)
            {
                Assert.AreEqual(DisabledState, extension.State);
            }
            else
            {
                Assert.AreEqual(EnableState, extension.State);
            }
            Console.WriteLine("BGIinfo extension verified successfully.");
        }

        
    }
}

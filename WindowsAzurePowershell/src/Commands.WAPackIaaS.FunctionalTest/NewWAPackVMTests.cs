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

namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.FunctionalTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Linq;

    [TestClass]
    public class NewWAPackVMTests : CmdletTestBase
    {
        public const string cmdletName = "New-WAPackVM";

        public List<PSObject> CreatedVirtualMachines;

        [TestInitialize]
        public void Initialize()
        {
            CreatedVirtualMachines = new List<PSObject>();
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateVMFromTemplateWithVNet()
        {
            string vmNameToCreate = "TestWindowsVM_VMFromTemplateWithVNet";

            var ps = this.PowerShell;
            ps.AddCommand("Get-WAPackVMTemplate");
            ps.AddParameter("Name", WAPackConfigurationFactory.Win7_64TemplateName);
            var template = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(template.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVnet");
            ps.AddParameter("Name", WAPackConfigurationFactory.AvenzVnetName);
            var vNet = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(vNet.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand(cmdletName);
            ps.AddParameter("Name", vmNameToCreate);
            ps.AddParameter("Template", template[0]);
            ps.AddParameter("VNet", vNet[0]);
            ps.AddParameter("VMCredential", WAPackConfigurationFactory.WindowsVMCredential);
            ps.AddParameter("Windows");
            var actualCreatedVM = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(1, actualCreatedVM.Count);

            var createdVMName = actualCreatedVM[0].Properties["Name"].Value;
            Assert.AreEqual(vmNameToCreate, createdVMName);

            this.CreatedVirtualMachines.AddRange(actualCreatedVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateVMFromTemplateWithoutVNet()
        {
            string vmNameToCreate = "TestWindowsVM_VMFromTemplateWithoutVNet";

            var ps = this.PowerShell;
            ps.AddCommand("Get-WAPackVMTemplate");
            ps.AddParameter("Name", WAPackConfigurationFactory.Win7_64TemplateName);
            var template = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(template.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand(cmdletName);
            ps.AddParameter("Name", vmNameToCreate);
            ps.AddParameter("Template", template[0]);
            ps.AddParameter("VMCredential", WAPackConfigurationFactory.WindowsVMCredential);
            ps.AddParameter("Windows");
            var actualCreatedVM = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(1, actualCreatedVM.Count);
            var createdVMName = actualCreatedVM[0].Properties["Name"].Value;

            Assert.AreEqual(vmNameToCreate, createdVMName);
            this.CreatedVirtualMachines.AddRange(actualCreatedVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateVMFromVHDWithVNet()
        {
            string vmNameToCreate = "TestWindowsVM_VMFromVHDWithVNet";
            var ps = this.PowerShell;

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVnet");
            ps.AddParameter("Name", WAPackConfigurationFactory.AvenzVnetName);
            var vNet = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(vNet.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMOSDisk");
            ps.AddParameter("Name", WAPackConfigurationFactory.Ws2k8R2OSDiskName);
            var osDisk = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(osDisk.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMSizeProfile");
            ps.AddParameter("Name", WAPackConfigurationFactory.VMSizeProfileName);
            var vmSizeProfile = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(vmSizeProfile.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand(cmdletName);
            ps.AddParameter("Name", vmNameToCreate);
            ps.AddParameter("OSDisk", osDisk[0]);
            ps.AddParameter("VNet", vNet[0]);
            ps.AddParameter("VMSizeProfile", vmSizeProfile[0]);
            var actualCreatedVM = ps.InvokeAndAssertForNoErrors();

            Assert.AreEqual(1, actualCreatedVM.Count);
            var createdVMName = actualCreatedVM[0].Properties["Name"].Value;

            Assert.AreEqual(vmNameToCreate, createdVMName);

            this.CreatedVirtualMachines.AddRange(actualCreatedVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateVMFromVHDWithoutVNet()
        {
            string vmNameToCreate = "TestWindowsVM_VMFromVHDWithoutVNet";
            var ps = this.PowerShell;

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMOSDisk");
            ps.AddParameter("Name", WAPackConfigurationFactory.Ws2k8R2OSDiskName);
            var osDisk = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(osDisk.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMSizeProfile");
            ps.AddParameter("Name", WAPackConfigurationFactory.VMSizeProfileName);
            var vmSizeProfile = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(vmSizeProfile.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand(cmdletName);
            ps.AddParameter("Name", vmNameToCreate);
            ps.AddParameter("OSDisk", osDisk[0]);
            ps.AddParameter("VMSizeProfile", vmSizeProfile[0]);
            var actualCreatedVM = ps.InvokeAndAssertForNoErrors();

            Assert.AreEqual(1, actualCreatedVM.Count);
            var createdVMName = actualCreatedVM[0].Properties["Name"].Value;

            Assert.AreEqual(vmNameToCreate, createdVMName);

            this.CreatedVirtualMachines.AddRange(actualCreatedVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateUbuntuVMFromTemplate()
        {
            string vmNameToCreate = "TestUbuntuVM_FromTemplate";

            var ps = this.PowerShell;
            ps.AddCommand("Get-WAPackVMTemplate");
            ps.AddParameter("Name", WAPackConfigurationFactory.LinuxUbuntu_64TemplateName);
            var template = this.PowerShell.InvokeAndAssertForNoErrors();
            Assert.AreEqual(template.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand(cmdletName);
            ps.AddParameter("Name", vmNameToCreate);
            ps.AddParameter("Template", template[0]);
            ps.AddParameter("VMCredential", WAPackConfigurationFactory.LinuxVMCredential);
            ps.AddParameter("Linux");
            var actualCreatedVM = ps.InvokeAndAssertForNoErrors();

            Assert.AreEqual(1, actualCreatedVM.Count);
            var createdVMName = actualCreatedVM[0].Properties["Name"].Value;

            Assert.AreEqual(vmNameToCreate, createdVMName);

            this.CreatedVirtualMachines.AddRange(actualCreatedVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateUbuntuVMFromVHDWithVNet()
        {
            string vmNameToCreate = "TestUbuntuVM_FromVHD";

            var ps = this.PowerShell;
            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMOSDisk");
            ps.AddParameter("Name", WAPackConfigurationFactory.LinuxOSDiskName);
            var osDisk = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(osDisk.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMSizeProfile");
            ps.AddParameter("Name", WAPackConfigurationFactory.VMSizeProfileName);
            var vmSizeProfile = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(vmSizeProfile.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVnet");
            ps.AddParameter("Name", WAPackConfigurationFactory.AvenzVnetName);
            var vNet = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(vNet.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand(cmdletName);
            ps.AddParameter("Name", vmNameToCreate);
            ps.AddParameter("OSDisk", osDisk[0]);
            ps.AddParameter("VMSizeProfile", vmSizeProfile[0]);
            ps.AddParameter("VNet", vNet[0]);
            var actualCreatedVM = ps.InvokeAndAssertForNoErrors();

            Assert.AreEqual(1, actualCreatedVM.Count);
            var createdVMName = actualCreatedVM[0].Properties["Name"].Value;

            Assert.AreEqual(vmNameToCreate, createdVMName);

            this.CreatedVirtualMachines.AddRange(actualCreatedVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateUbuntuVMFromVHDWithoutVnet()
        {
            string vmNameToCreate = "TestUbuntuVM_FromVHDWithoutVnet";

            var ps = this.PowerShell;
            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMOSDisk");
            ps.AddParameter("Name", WAPackConfigurationFactory.LinuxOSDiskName);
            var osDisk = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(osDisk.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMSizeProfile");
            ps.AddParameter("Name", WAPackConfigurationFactory.VMSizeProfileName);
            var vmSizeProfile = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(vmSizeProfile.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand(cmdletName);
            ps.AddParameter("Name", vmNameToCreate);
            ps.AddParameter("OSDisk", osDisk[0]);
            ps.AddParameter("VMSizeProfile", vmSizeProfile[0]);
            var actualCreatedVM = ps.InvokeAndAssertForNoErrors();

            Assert.AreEqual(1, actualCreatedVM.Count);
            var createdVMName = actualCreatedVM[0].Properties["Name"].Value;

            Assert.AreEqual(vmNameToCreate, createdVMName);

            this.CreatedVirtualMachines.AddRange(actualCreatedVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateVMQuickCreate()
        {
            string vmNameToCreate = "TestWindowsVM_QuickCreate";

            var ps = this.PowerShell;
            ps.AddCommand("Get-WAPackVMTemplate");
            ps.AddParameter("Name", WAPackConfigurationFactory.Win7_64TemplateName);
            var template = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(template.Count, 1);

            ps.Commands.Clear();
            ps.AddCommand("New-WAPackQuickVM");
            ps.AddParameter("Name", vmNameToCreate);
            ps.AddParameter("Template", template[0]);
            ps.AddParameter("VMCredential", WAPackConfigurationFactory.WindowsVMCredential);
            var actualCreatedVM = ps.InvokeAndAssertForNoErrors();

            Assert.AreEqual(1, actualCreatedVM.Count);
            var createdVMName = actualCreatedVM[0].Properties["Name"].Value;
            Assert.AreEqual(vmNameToCreate, createdVMName);

            this.CreatedVirtualMachines.AddRange(actualCreatedVM);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (!this.CreatedVirtualMachines.Any())
                return;

            var ps = this.PowerShell;

            foreach (var vm in this.CreatedVirtualMachines)
            {
                ps.Commands.Clear();
                ps.AddCommand("Remove-WAPackVM");
                ps.Commands.AddParameter("VM", vm);
                ps.Commands.AddParameter("Force");
                ps.InvokeAndAssertForErrors();
            }
        }
    }
}

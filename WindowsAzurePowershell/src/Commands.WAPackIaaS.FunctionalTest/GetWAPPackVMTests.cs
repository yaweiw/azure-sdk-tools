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
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Management.Automation;
    
    [TestClass]
    public class GetWAPPackVMTests : CmdletTestBase
    {
        public const string cmdletName = "Get-WAPackVM";

        public string VMNameToCreate = "TestVirtualMachineForGetTests";

        public List<PSObject> CreatedVirtualMachines;

        [TestInitialize]
        public void TestInitialize()
        {
            CreatedVirtualMachines = new List<PSObject>();
            this.CreateVirtualMachine(VMNameToCreate);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void GetVmWithNoParam()
        {
            var allVms = this.InvokeCmdlet(cmdletName, null);
            Assert.IsTrue(allVms.Count > 0);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void GetVmFromName()
        {
            string expectedVmName = VMNameToCreate;
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", expectedVmName}
            };
            var vmFromName = this.InvokeCmdlet(cmdletName, inputParams);

            var actualVmName = vmFromName.First().Properties["Name"].Value;
            Assert.AreEqual(expectedVmName, actualVmName);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void GetWAPackVmFromId()
        {
            string expectedVmName = VMNameToCreate;
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", expectedVmName}
            };
            var vmFromName = this.InvokeCmdlet(cmdletName, inputParams);

            var expectedVmId = vmFromName.First().Properties["Id"].Value;

            inputParams = new Dictionary<string, object>()
            {
                {"Id", expectedVmId}
            };
            var vmFromId = this.InvokeCmdlet(cmdletName, inputParams);

            Assert.AreEqual(1, vmFromId.Count);
            var actualvmFromId = vmFromId.First().Properties["Id"].Value;
            Assert.AreEqual(expectedVmId, actualvmFromId);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void GetWAPackVmByNameDoesNotExist()
        {
            string expectedVmName = "WAPackVmDoesNotExist";
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", expectedVmName}
            };
            var vmFromName = this.InvokeCmdlet(cmdletName, inputParams);

            Assert.AreEqual(0, vmFromName.Count);
        }

        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        public void GetWAPackVmByIdDoesNotExist()
        {
            var expectedVmId = Guid.NewGuid().ToString();
            var expectedError = string.Format(Resources.ResourceNotFound, expectedVmId);
            var inputParams = new Dictionary<string, object>()
            {
                {"Id", expectedVmId}
            };
            var vmFromName = this.InvokeCmdlet(cmdletName, inputParams,expectedError);
            Assert.AreEqual(0, vmFromName.Count);
        }

        public void CreateVirtualMachine(string vmNameToCreate)
        {
            var ps = this.PowerShell;
            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVnet");
            ps.AddParameter("Name", WAPackConfigurationFactory.AvenzVnetName);
            var vNet = this.PowerShell.Invoke();
            Assert.AreEqual(vNet.Count, 1, string.Format("Actual Vnet found - {0}, Expected Vnet - {1}", vNet.Count, 1));

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMOSDisk");
            ps.AddParameter("Name", WAPackConfigurationFactory.BlankOSDiskName);
            var osDisk = this.PowerShell.Invoke();
            Assert.AreEqual(1, osDisk.Count, string.Format("Actual OSDisks found - {0}, Expected OSDisks - {1}", osDisk.Count, 1));

            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMSizeProfile");
            ps.AddParameter("Name", WAPackConfigurationFactory.VMSizeProfileName);
            var vmSizeProfile = this.PowerShell.Invoke();
            Assert.AreEqual(1, vmSizeProfile.Count, string.Format("Actual VMSizeProfiles found - {0}, Expected VMSizeProfiles - {1}", vmSizeProfile.Count, 1));

            ps.Commands.Clear();
            ps.AddCommand("New-WAPackVM");
            ps.AddParameter("Name", vmNameToCreate);
            ps.AddParameter("OSDisk", osDisk[0]);
            ps.AddParameter("VNet", vNet[0]);
            ps.AddParameter("VMSizeProfile", vmSizeProfile[0]);
            var actualCreatedVM = ps.Invoke();

            Assert.AreEqual(1, actualCreatedVM.Count, string.Format("Actual VirtualMachines found - {0}, Expected VirtualMachines - {1}", actualCreatedVM.Count, 1));
            var createdVMName = actualCreatedVM[0].Properties["Name"].Value;

            ps.Commands.Clear();

            Assert.AreEqual(vmNameToCreate, createdVMName, string.Format("Actual VirtualMachines name - {0}, Expected VirtualMachines name- {1}", createdVMName, vmNameToCreate));
            CreatedVirtualMachines.AddRange(actualCreatedVM);
        }

        [TestCleanup]
        public void Cleanup()
        {
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
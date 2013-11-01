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
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using System;

    [TestClass]
    public class SetWAPackVMTests : CmdletTestVirtualMachineStatusBase
    {
        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void SetSizeProfile()
        {
            var ps = this.PowerShell;

            //Get a size profile
            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMSizeProfile");
            ps.AddParameter("Name", WAPackConfigurationFactory.VMSizeProfileName);
            var sizeProfileList = this.PowerShell.InvokeAndAssertForNoErrors();
            Assert.AreEqual(1, sizeProfileList.Count);

            //Get the VM, verify that is stopped before we change the profile
            SetVirtualMachineState(VirtualMachine, "Stop");
            var vm = VirtualMachine.BaseObject as VirtualMachine;
            Assert.IsNotNull(vm);
            Assert.AreEqual("Stopped", vm.StatusString);

            //See what the size of our VM is currently
            var currentRAM = vm.Memory;
            var currentCPU = vm.CPUCount;

            var newProfile = sizeProfileList[0].BaseObject as HardwareProfile;
            Assert.IsNotNull(newProfile);

            //Modify the profile to make sure it's different from what the VM currently has
            newProfile.Memory = 1024;
            if (currentRAM >= 1024)
                newProfile.Memory = 512;

            //Set the size profile
            ps.Commands.Clear();
            ps.Commands.AddCommand("Set-WAPackVM");
            ps.AddParameter("VM", vm);
            ps.AddParameter("VMSizeProfile", newProfile);
            var updatedVMList = ps.InvokeAndAssertForNoErrors();
            Assert.AreEqual(1, updatedVMList.Count);

            var updatedVM = updatedVMList[0].BaseObject as VirtualMachine;
            Assert.IsNotNull(updatedVM);

            //Only way to know if size profile has truly updated is if there is a different amount of CPU/RAM than before
            Assert.AreNotEqual(updatedVM.Memory, currentRAM);
        }

        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        public void ShouldFailSetOnNonexistantVM()
        {
            var ps = this.PowerShell;

            //Get a size profile
            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVMSizeProfile");
            ps.AddParameter("Name", WAPackConfigurationFactory.VMSizeProfileName);
            var sizeProfileList = this.PowerShell.InvokeAndAssertForNoErrors();
            Assert.AreEqual(1, sizeProfileList.Count);

            //Get our VM object, then change its ID to something that doesn't exist
            var vm = VirtualMachine.BaseObject as VirtualMachine;
            Assert.IsNotNull(vm);
            vm.ID = Guid.NewGuid();


            //Try to set the size profile
            ps.Commands.Clear();
            ps.Commands.AddCommand("Set-WAPackVM");
            ps.AddParameter("VM", vm);
            ps.AddParameter("VMSizeProfile", sizeProfileList[0]);

            var expectedError = string.Format(Resources.ResourceNotFound, vm.ID);
            var updatedVMList = ps.InvokeAndAssertForErrors(expectedError);
        }
    }
}

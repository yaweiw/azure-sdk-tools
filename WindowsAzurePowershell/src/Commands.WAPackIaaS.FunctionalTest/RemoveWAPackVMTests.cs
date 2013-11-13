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
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Management.Automation;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;

    [TestClass]
    public class RemoveWAPackVMTests : CmdletTestVirtualMachineStatusBase
    {
        public string cmdletName = "Remove-WAPackVM";

        protected override string OsDiskName
        {
            get { return WAPackConfigurationFactory.Ws2k8R2OSDiskName; }
        }

        protected override string VmSizeProfileName
        {
            get { return WAPackConfigurationFactory.VMSizeProfileName; }
        }


        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void RemoveExistingVM()
        {
            var expectedVMIdToDelete = this.VirtualMachine.Properties["Id"].Value.ToString();

            var inputParams = new Dictionary<string, object>()
            {
                {"VM", this.VirtualMachine},
                {"Force", null},
                {"PassThru", null}
            };
            
            var isDeleted = this.InvokeCmdlet(cmdletName, inputParams);
            
            Assert.AreEqual(1, isDeleted.Count);
            Assert.AreEqual(true, isDeleted.First());

            inputParams = new Dictionary<string, object>()
            {
                {"Id", expectedVMIdToDelete}
            };

            var deletedVM = this.InvokeCmdlet("Get-WAPackVM", inputParams, string.Format(Resources.ResourceNotFound, expectedVMIdToDelete));

            Assert.AreEqual(0, deletedVM.Count);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void RemoveExistingVMWithPipelining()
        {
            var expectedVMIdToDelete = this.VirtualMachine.Properties["Id"].Value.ToString();

            var ps = this.PowerShell;
            ps.Commands.Clear();
            ps.AddCommand("Get-WAPackVM");
            ps.Commands.AddParameter("Id", expectedVMIdToDelete);
            ps.AddCommand("Remove-WAPackVM");
            ps.Commands.AddParameter("Force");

            ps.InvokeAndAssertForNoErrors();

            var inputParams = new Dictionary<string, object>()
            {
                {"Id", expectedVMIdToDelete}
            };
            var deletedVM = this.InvokeCmdlet("Get-WAPackVM", inputParams, string.Format(Resources.ResourceNotFound, expectedVMIdToDelete));

            Assert.AreEqual(0, deletedVM.Count);
        }


        [TestMethod]
        [TestCategory("WAPackIaaS")]
        [TestCategory("Negative")]
        public void RemoveVMDoesNotExist()
        {
            this.VirtualMachine.Properties["Id"].Value = Guid.NewGuid();
            var inputParams = new Dictionary<string, object>()
            {
                {"VM", this.VirtualMachine},
                {"Force", null}
            };

            var doesNotExistErrorMessage = "The remote server returned an error: (400) Bad Request.";
            this.InvokeCmdlet(cmdletName, inputParams, doesNotExistErrorMessage);
        }
    }
}

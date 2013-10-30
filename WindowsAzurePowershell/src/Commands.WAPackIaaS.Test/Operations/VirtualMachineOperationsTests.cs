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

namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Test.Operations
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Exceptions;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Operations;
    using Microsoft.WindowsAzure.Commands.WAPackIaaS.Test.Mocks;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.WebClient;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    [TestClass]
    public class VirtualMachineOperationsTests
    {
        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        public void ShouldReturnEmptyOnNoResult()
        {
            var vmOperations = new VirtualMachineOperations(new WebClientFactory(
                new Subscription(),
                MockRequestChannel.Create()));

            Assert.IsFalse(vmOperations.Read().Any());
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void ShouldReturnOneVM()
        {
            var vmOperations = new VirtualMachineOperations(new WebClientFactory(
                new Subscription(),
                MockRequestChannel.Create()
                    .AddReturnObject(new VirtualMachine { Name = "vm1", ID = Guid.NewGuid() })));

            Assert.AreEqual(1, vmOperations.Read().Count);
        }

        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        [ExpectedException(typeof(WAPackOperationException))]
        public void ShouldThrowGetByIdNoResult()
        {
            var vmOperations = new VirtualMachineOperations(new WebClientFactory(
                new Subscription(),
                MockRequestChannel.Create()));

            vmOperations.Read(Guid.NewGuid());
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void ShouldReturnOneVMGetById()
        {
            var expectedVmId = Guid.NewGuid();
            var vmOperations = new VirtualMachineOperations(new WebClientFactory(
                new Subscription(),
                MockRequestChannel.Create()
                    .AddReturnObject(new VirtualMachine { Name = "vm1", ID = expectedVmId })));

            var vm = vmOperations.Read(expectedVmId);
            Assert.AreEqual(expectedVmId, vm.ID);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void ShouldReturnMultipleVMsGetByName()
        {
            const string expectedVmName = "myVM";
            var expectedVmIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var vmOperations = new VirtualMachineOperations(new WebClientFactory(
                new Subscription(),
                new MockRequestChannel()
                    .AddReturnObject(new List<object>
                        {new VirtualMachine { Name = expectedVmName, ID = expectedVmIds[1] },
                        new VirtualMachine { Name = expectedVmName, ID = expectedVmIds[0] }})));

            var vmList = vmOperations.Read(expectedVmName);
            Assert.AreEqual(expectedVmIds.Length, vmList.Count);
            Assert.IsTrue(vmList.All(vm => vm.Name == expectedVmName));
            CollectionAssert.AreEquivalent(expectedVmIds, vmList.Select(v => v.ID).ToArray());
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateVMFromVHD()
        {
            var mockChannel = new MockRequestChannel();
            
            var testCloud = new Cloud { ID = Guid.NewGuid(), StampId = Guid.NewGuid() };
            mockChannel.AddReturnObject(testCloud);

            var vmToCreate = new VirtualMachine { VirtualHardDiskId = Guid.NewGuid(), Name = "Test" };
            var vmToReturn = new VirtualMachine
                {
                    ID = Guid.NewGuid(),
                    Name = vmToCreate.Name,
                    CloudId = testCloud.ID,
                    StampId = testCloud.StampId
                };
            
            mockChannel.AddReturnObject(vmToReturn, new WebHeaderCollection { "x-ms-request-id:" + Guid.NewGuid() });

            var vmOps = new VirtualMachineOperations(new WebClientFactory(new Subscription(), mockChannel));
            
            Guid? jobOut;
            var resultVM = vmOps.Create(vmToCreate, out jobOut);

            //Check the results that client returns
            Assert.IsNotNull(resultVM);
            Assert.IsInstanceOfType(resultVM, typeof (VirtualMachine));
            Assert.AreEqual(resultVM.ID, vmToReturn.ID);
            Assert.AreEqual(resultVM.Name, vmToReturn.Name);
            Assert.AreEqual(resultVM.CloudId, vmToReturn.CloudId);
            Assert.AreEqual(resultVM.StampId, vmToReturn.StampId);

            //Check the requests that the client made
            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(requestList.Count, 2);
            Assert.AreEqual(requestList[1].Item1.Method, HttpMethod.Post.ToString());
            Assert.IsTrue(requestList[1].Item1.RequestUri.ToString().TrimEnd(new[]{'/'}).EndsWith("/VirtualMachines"));

            var sentVM = mockChannel.DeserializeClientPayload<VirtualMachine>(requestList[1].Item2);
            Assert.IsNotNull(sentVM);
            Assert.IsTrue(sentVM.Count == 1);
            Assert.AreEqual(sentVM[0].CloudId, testCloud.ID);
            Assert.AreEqual(sentVM[0].StampId, testCloud.StampId);
            Assert.AreEqual(sentVM[0].Name, vmToCreate.Name);
            Assert.AreEqual(sentVM[0].VirtualHardDiskId, vmToCreate.VirtualHardDiskId);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CreateVMFromTemplate()
        {
            var mockChannel = new MockRequestChannel();

            var testCloud = new Cloud { ID = Guid.NewGuid(), StampId = Guid.NewGuid() };
            mockChannel.AddReturnObject(testCloud);

            var vmToCreate = new VirtualMachine { VMTemplateId = Guid.NewGuid(), Name = "Test" };
            var vmToReturn = new VirtualMachine
            {
                ID = Guid.NewGuid(),
                Name = vmToCreate.Name,
                CloudId = testCloud.ID,
                StampId = testCloud.StampId
            };
            mockChannel.AddReturnObject(vmToReturn, new WebHeaderCollection { "x-ms-request-id:" + Guid.NewGuid() });

            var vmOps = new VirtualMachineOperations(new WebClientFactory(new Subscription(), mockChannel));

            Guid? jobOut;
            var resultVM = vmOps.Create(vmToCreate, out jobOut);

            //Check the results that client returns
            Assert.IsNotNull(resultVM);
            Assert.IsInstanceOfType(resultVM, typeof(VirtualMachine));
            Assert.AreEqual(resultVM.ID, vmToReturn.ID);
            Assert.AreEqual(resultVM.Name, vmToReturn.Name);
            Assert.AreEqual(resultVM.CloudId, vmToReturn.CloudId);
            Assert.AreEqual(resultVM.StampId, vmToReturn.StampId);

            //Check the requests that the client made
            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(requestList.Count, 2);
            Assert.AreEqual(requestList[1].Item1.Method, HttpMethod.Post.ToString());
            Assert.IsTrue(requestList[1].Item1.RequestUri.ToString().TrimEnd(new[] { '/' }).EndsWith("/VirtualMachines"));

            var sentVM = mockChannel.DeserializeClientPayload<VirtualMachine>(requestList[1].Item2);
            Assert.IsNotNull(sentVM);
            Assert.IsTrue(sentVM.Count == 1);
            Assert.AreEqual(sentVM[0].CloudId, testCloud.ID);
            Assert.AreEqual(sentVM[0].StampId, testCloud.StampId);
            Assert.AreEqual(sentVM[0].Name, vmToCreate.Name);
            Assert.AreEqual(sentVM[0].VMTemplateId, vmToCreate.VMTemplateId);
        }
        
        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        [ExpectedException(typeof(WAPackOperationException))]
        public void VmCreateShouldThrowIfNoVhdAndNoTemplateSupplied()
        {
            var channel = new MockRequestChannel();
            var testCloud = new Cloud { ID = Guid.NewGuid(), StampId = Guid.NewGuid() };
            channel.AddReturnObject(testCloud);

            var sub = new Subscription();
            var vmOps = new VirtualMachineOperations(new WebClientFactory(sub, channel));

            var vmToCreate = new VirtualMachine {Name = "Test"};

            Guid? jobOut;
            vmOps.Create(vmToCreate, out jobOut);
        }

        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        [ExpectedException(typeof(WAPackOperationException))]
        public void VmCreateShouldThrowWhenNoObjectReturned()
        {
            var mockChannel = new MockRequestChannel();

            var testCloud = new Cloud { ID = Guid.NewGuid(), StampId = Guid.NewGuid() };
            mockChannel.AddReturnObject(testCloud);

            var vmOps = new VirtualMachineOperations(new WebClientFactory(new Subscription(), mockChannel));

            var vmToCreate = new VirtualMachine { VirtualHardDiskId = Guid.NewGuid(), Name = "Test" };

            Guid? jobOut;
            vmOps.Create(vmToCreate, out jobOut);
        }

        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        [ExpectedException(typeof(WAPackOperationException))]
        public void VmUpdateShouldThrowWhenNoObjectReturned()
        {
            var mockChannel = new MockRequestChannel();

            var vmOps = new VirtualMachineOperations(new WebClientFactory(new Subscription(), mockChannel));

            var vmToUpdate = new VirtualMachine { VirtualHardDiskId = Guid.NewGuid(), Name = "Test" };

            Guid? jobOut;
            vmOps.Update(vmToUpdate, out jobOut);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void DeleteVM()
        {
            var sub = new Subscription();
            var channel = new MockRequestChannel();

            //Response to client getting /Clouds (client needs stampId, gets it from clouds)
            var testCloud = new Cloud { ID = Guid.NewGuid(), StampId = Guid.NewGuid() };
            channel.AddReturnObject(testCloud);

            //Response to the DELETE
            channel.AddReturnObject(null, new WebHeaderCollection {"x-ms-request-id:" + Guid.NewGuid()});

            var vmOps = new VirtualMachineOperations(new WebClientFactory(sub, channel));

            Guid toDelete = Guid.NewGuid();
            Guid? jobOut;

            vmOps.Delete(toDelete, out jobOut);

            //Check the requests the client generated
            Assert.AreEqual(channel.ClientRequests.Count, 2);
            Assert.AreEqual(channel.ClientRequests[1].Item1.Method, HttpMethod.Delete.ToString());
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void StartVM()
        {
            var mockChannel = new MockRequestChannel();

            VirtualMachineOperations vmOperations;
            var testVM = InitVirtualMachineOperation(mockChannel, out vmOperations);

            Guid? jobOut;
            vmOperations.Start(testVM.ID, out jobOut);

            CheckVirtualMachineOperationResult("Start", mockChannel, testVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void StopVM()
        {
            var mockChannel = new MockRequestChannel();

            VirtualMachineOperations vmOperations;
            var testVM = InitVirtualMachineOperation(mockChannel, out vmOperations);

            Guid? jobOut;
            vmOperations.Stop(testVM.ID, out jobOut);

            CheckVirtualMachineOperationResult("Stop", mockChannel, testVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void RestartVM()
        {
            var mockChannel = new MockRequestChannel();

            VirtualMachineOperations vmOperations;
            var testVM = InitVirtualMachineOperation(mockChannel, out vmOperations);

            Guid? jobOut;
            vmOperations.Restart(testVM.ID, out jobOut);

            CheckVirtualMachineOperationResult("Reset", mockChannel, testVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void ShutdownVM()
        {
            var mockChannel = new MockRequestChannel();

            VirtualMachineOperations vmOperations;
            var testVM = InitVirtualMachineOperation(mockChannel, out vmOperations);

            Guid? jobOut;
            vmOperations.Shutdown(testVM.ID, out jobOut);

            CheckVirtualMachineOperationResult("Shutdown", mockChannel, testVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void SuspendVM()
        {
            var mockChannel = new MockRequestChannel();

            VirtualMachineOperations vmOperations;
            var testVM = InitVirtualMachineOperation(mockChannel, out vmOperations);

            Guid? jobOut;
            vmOperations.Suspend(testVM.ID, out jobOut);

            CheckVirtualMachineOperationResult("Suspend", mockChannel, testVM);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void ResumeVM()
        {
            var mockChannel = new MockRequestChannel();

            VirtualMachineOperations vmOperations;
            var testVM = InitVirtualMachineOperation(mockChannel, out vmOperations);

            Guid? jobOut;
            vmOperations.Resume(testVM.ID, out jobOut);

            CheckVirtualMachineOperationResult("Resume", mockChannel, testVM);
        }

        private static VirtualMachine InitVirtualMachineOperation(MockRequestChannel mockChannel, out VirtualMachineOperations vmOperations)
        {
            //Cloud for return value of first request (client gets cloud to get stampId)
            var testCloud = new Cloud {ID = Guid.NewGuid(), StampId = Guid.NewGuid()};
            mockChannel.AddReturnObject(testCloud);

            //VM for return value of second request (client updates VM with operation)
            var testVM = new VirtualMachine {ID = Guid.NewGuid(), StampId = testCloud.StampId};
            mockChannel.AddReturnObject(testVM, new WebHeaderCollection {"x-ms-request-id:" + Guid.NewGuid()});

            var factory = new WebClientFactory(new Subscription(), mockChannel);
            vmOperations = new VirtualMachineOperations(factory);

            return testVM;
        }

        private static void CheckVirtualMachineOperationResult(string operation, MockRequestChannel mockChannel, VirtualMachine testVM)
        {
            var requests = mockChannel.ClientRequests;
            Assert.AreEqual(requests.Count, 2);
            Assert.AreEqual(requests[1].Item1.Method, HttpMethod.Put.ToString());

            var clientSentVM = mockChannel.DeserializeClientPayload<VirtualMachine>(requests[1].Item2);
            Assert.IsNotNull(clientSentVM);
            Assert.IsTrue(clientSentVM.Count == 1);
            Assert.AreEqual(testVM.ID, clientSentVM[0].ID);
            Assert.AreEqual(testVM.StampId, clientSentVM[0].StampId);
            Assert.AreEqual(clientSentVM[0].Operation, operation);
        }
    }
}

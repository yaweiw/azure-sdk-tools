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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.UnitTests.Cmdlets.IaaS.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands.Test.Utilities.Common;
    using Management.Compute;
    using Management.Compute.Models;
    using ServiceManagement.IaaS.Extensions;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VirtualMachineExtensionImageFactoryTests : TestBase
    {
        private const string nonExistingPublisherName = "nonExistingPublisherName";
        private const string nonExistingExtensionName = "nonExistingExtensionName";
        private const string testPublisherName = "testPublisherName";
        private const string testExtensionName = "testExtensionName";

        private class MockVirtualMachineExtensionOperations : IVirtualMachineExtensionOperations
        {
            public Task<VirtualMachineExtensionListResponse> ListAsync(CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }

            public Task<VirtualMachineExtensionListResponse> ListVersionsAsync(string publisherName, string extensionName, CancellationToken cancellationToken)
            {
                return Task.Factory.StartNew(() => new VirtualMachineExtensionListResponse
                {
                    ResourceExtensions = new List<VirtualMachineExtensionListResponse.ResourceExtension>(
                        Enumerable.Repeat(new VirtualMachineExtensionListResponse.ResourceExtension
                        {
                            Publisher = testPublisherName,
                            Name = testExtensionName
                        }, 1))
                });
            }
        }

        private class MockComputeManagementClient : IComputeManagementClient
        {
            private MockVirtualMachineExtensionOperations virtualMachineExtensions = new MockVirtualMachineExtensionOperations();

            public System.Uri BaseUri
            {
                get { throw new System.NotImplementedException(); }
            }

            public SubscriptionCloudCredentials Credentials
            {
                get { throw new System.NotImplementedException(); }
            }

            public IDeploymentOperations Deployments
            {
                get { throw new System.NotImplementedException(); }
            }

            public Task<OperationStatusResponse> GetOperationStatusAsync(string requestId, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }

            public IHostedServiceOperations HostedServices
            {
                get { throw new System.NotImplementedException(); }
            }

            public IOperatingSystemOperations OperatingSystems
            {
                get { throw new System.NotImplementedException(); }
            }

            public IServiceCertificateOperations ServiceCertificates
            {
                get { throw new System.NotImplementedException(); }
            }

            public IVirtualMachineDiskOperations VirtualMachineDisks
            {
                get { throw new System.NotImplementedException(); }
            }

            IVirtualMachineExtensionOperations IComputeManagementClient.VirtualMachineExtensions
            {
                get
                {
                    return virtualMachineExtensions;
                }
            }

            public IVirtualMachineOSImageOperations VirtualMachineOSImages
            {
                get { throw new System.NotImplementedException(); }
            }

            public IVirtualMachineVMImageOperations VirtualMachineVMImages
            {
                get { throw new System.NotImplementedException(); }
            }

            public IVirtualMachineOperations VirtualMachines
            {
                get { throw new System.NotImplementedException(); }
            }

            public void Dispose()
            {
                throw new System.NotImplementedException();
            }
        }

        [TestInitialize]
        public void SetupTest()
        {
        }

        [TestCleanup]
        public void CleanupTest()
        {
        }

        [TestMethod]
        public void TestNonExistingExtensionImageList()
        {
            var factory = new VirtualMachineExtensionImageFactory(null);

            var list = factory.MakeList(
                nonExistingPublisherName,
                nonExistingExtensionName,
                true);

            Assert.IsTrue(list.Count() == 0);
        }

        [TestMethod]
        public void TestMakeListWithoutClient()
        {
            var factory = new VirtualMachineExtensionImageFactory(null);

            var list = factory.MakeList(
                testPublisherName,
                testExtensionName,
                false);

            Assert.IsTrue(list.Count() == 1);

            var item = list[0];

            Assert.AreEqual(
                item.Publisher,
                testPublisherName,
                true,
                string.Empty);

            Assert.AreEqual(
                item.Name,
                testExtensionName,
                true,
                string.Empty);

            Assert.IsTrue(!string.IsNullOrEmpty(item.ReferenceName));

            Assert.IsTrue(item.ResourceExtensionParameterValues == null
                      || !item.ResourceExtensionParameterValues.Any());

            Assert.IsTrue(string.IsNullOrEmpty(item.State));

            Assert.IsTrue(string.IsNullOrEmpty(item.Version));
        }

        [TestMethod]
        public void TestMakeListWithClient()
        {
            var client = new MockComputeManagementClient();
            var factory = new VirtualMachineExtensionImageFactory(client);

            var list = factory.MakeList(
                testPublisherName,
                testExtensionName);

            Assert.IsTrue(list.Count() == 1);

            var item = list[0];

            Assert.AreEqual(
                item.Publisher,
                testPublisherName,
                true,
                string.Empty);

            Assert.AreEqual(
                item.Name,
                testExtensionName,
                true,
                string.Empty);

            Assert.IsTrue(!string.IsNullOrEmpty(item.ReferenceName));

            Assert.IsTrue(item.ResourceExtensionParameterValues == null
                      || !item.ResourceExtensionParameterValues.Any());

            Assert.IsTrue(string.IsNullOrEmpty(item.State));

            Assert.IsTrue(string.IsNullOrEmpty(item.Version));
        }
    }
}
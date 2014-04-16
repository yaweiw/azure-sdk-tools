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
    using Moq;
    using ServiceManagement.IaaS.Extensions;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VirtualMachineExtensionImageFactoryTests : TestBase
    {
        private const string nonExistingPublisherName = "nonExistingPublisherName";
        private const string nonExistingExtensionName = "nonExistingExtensionName";
        private const string testPublisherName = "testPublisherName";
        private const string testExtensionName = "testExtensionName";

        private Mock<IComputeManagementClient> client;

        [TestInitialize]
        public void SetupTest()
        {
            var source = new TaskCompletionSource<VirtualMachineExtensionListResponse>();
            source.SetResult(
                new VirtualMachineExtensionListResponse
                {
                    ResourceExtensions = new List<VirtualMachineExtensionListResponse.ResourceExtension>(
                        Enumerable.Repeat(new VirtualMachineExtensionListResponse.ResourceExtension
                        {
                            Publisher = testPublisherName,
                            Name = testExtensionName
                        }, 1))
                });

            var operations = new Mock<IVirtualMachineExtensionOperations>();
            operations.Setup(f => f.ListVersionsAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None))
                      .Returns(source.Task);

            client = new Mock<IComputeManagementClient>();
            client.Setup(f => f.VirtualMachineExtensions)
                  .Returns(operations.Object);
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
            var factory = new VirtualMachineExtensionImageFactory(client.Object);

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
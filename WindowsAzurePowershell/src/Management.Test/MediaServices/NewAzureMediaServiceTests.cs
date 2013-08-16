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

using Microsoft.WindowsAzure.Management.MediaService;
using Microsoft.WindowsAzure.Management.Utilities.MediaService;
using Microsoft.WindowsAzure.Management.Utilities.MediaService.Services.MediaServicesEntities;
using Moq;

namespace Microsoft.WindowsAzure.Management.Test.MediaServices
{
    using System.Linq;
    using Utilities.Common;
    using Utilities.Websites;
    using Management.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;
    using System;
    using Microsoft.WindowsAzure.ServiceManagement;

    [TestClass]
    public class NewAzureMediaServiceTests : TestBase
    {
        [TestMethod]
        public void NewAzureMediaServiceTest()
        {
            var clientMock = new Mock<IMediaServicesClient>();

            string expectedName = "testacc";

            var fakeResult = new AccountCreationResult
            {
                Name = expectedName,
            };

            var command = new NewAzureMediaServiceCommand()
            {
                CommandRuntime = new MockCommandRuntime(),
                Name = expectedName,
                MediaServicesClient = clientMock.Object,
                StorageAccountKey = "testKey",
                StorageAccountName = "storageaccount",
                Location = "wus"
            };

            clientMock.Setup(f => f.GetStorageServiceProperties(command.StorageAccountName)).Returns(Task.Factory.StartNew(() =>
            {
                var properties = new StorageServiceProperties { Endpoints = new EndpointList() };
                properties.Endpoints.Add("http://endpoint");

                return new StorageService
                {
                    StorageServiceProperties = properties
                };
            }));

            clientMock.Setup(f => f.CreateNewAzureMediaServiceAsync(It.IsAny<AccountCreationRequest>())).Returns(Task.Factory.StartNew(() =>
            {
                return fakeResult;
            }));

            // Test
            command.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            var result = (AccountCreationResult)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(expectedName, result.Name);
        }
    }
}
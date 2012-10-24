// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.Cmdlet
{
    using System.ServiceModel;
    using CloudService.Cmdlet;
    using CloudService.Model;
    using Extensions;
    using Management.Test.Stubs;
    using Services;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;

    [TestClass]
    public class RemoveAzureServiceTests : TestBase
    {
        private const string serviceName = "AzureService";

        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void RemoveAzureServiceProcessTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            bool serviceDeleted = false;
            bool deploymentDeleted = false;
            channel.GetDeploymentBySlotThunk = ar =>
            {
                if (deploymentDeleted) throw new EndpointNotFoundException();
                return new Deployment(serviceName, ArgumentConstants.Slots[Slot.Production], DeploymentStatus.Suspended);
            };
            channel.DeleteHostedServiceThunk = ar => serviceDeleted = true;
            channel.DeleteDeploymentBySlotThunk = ar =>
            {
                deploymentDeleted = true;
            };

            using (FileSystemHelper files = new FileSystemHelper(this))
            {

                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                var removeAzureServiceCommand = new RemoveAzureServiceCommand(channel);
                removeAzureServiceCommand.ShareChannel = true;
                removeAzureServiceCommand.RemoveAzureServiceProcess(service.Paths.RootPath, string.Empty, serviceName);
                Assert.IsTrue(deploymentDeleted);
                Assert.IsTrue(serviceDeleted);
            }
        }
    }
}
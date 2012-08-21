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
    using CloudService.Cmdlet;
    using CloudService.Model;
    using Management.Test.Stubs;
    using Services;
    using TestData;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StartAzureServiceTests : TestBase
    {
        private const string serviceName = "AzureService";
        string slot = ArgumentConstants.Slots[Slot.Production];

        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void SetDeploymentStatusProcessTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            string newStatus = DeploymentStatus.Running;
            string currentStatus = DeploymentStatus.Suspended;
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar =>
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment(serviceName, slot, newStatus);
            };
            channel.GetDeploymentBySlotThunk = ar => new Deployment(serviceName, slot, currentStatus);

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                var startAzureService = new StartAzureService(channel) { ShareChannel = true };
                startAzureService.SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionName[0], serviceName);

                Assert.IsTrue(statusUpdated);
            }
        }
    }
}

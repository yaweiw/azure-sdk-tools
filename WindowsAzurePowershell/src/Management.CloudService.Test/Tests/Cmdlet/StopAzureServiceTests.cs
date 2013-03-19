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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.Cmdlet
{
    using CloudService.Cmdlet;
    using Microsoft.WindowsAzure.Management.Utilities.CloudService;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Test.Utilities.CloudServiceProject;
    using ServiceManagement;
    using Microsoft.WindowsAzure.Management.Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StopAzureServiceTests : TestBase
    {
        private const string serviceName = "AzureService";

        string slot = ArgumentConstants.Slots[SlotType.Production];

        private MockCommandRuntime mockCommandRuntime;

        private SimpleServiceManagement channel;

        private StopAzureServiceCommand stopServiceCmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            mockCommandRuntime = new MockCommandRuntime();
            channel = new SimpleServiceManagement();

            stopServiceCmdlet = new StopAzureServiceCommand(channel) { ShareChannel = true };
            stopServiceCmdlet.CommandRuntime = mockCommandRuntime;
        }

        [TestMethod]
        public void SetDeploymentStatusProcessTest()
        {
            string newStatus = DeploymentStatus.Suspended;
            string currentStatus = DeploymentStatus.Running;
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar =>
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment{Name = serviceName, DeploymentSlot = slot, Status = newStatus};
            };
            channel.GetDeploymentBySlotThunk = ar => new Deployment{ Name = serviceName, DeploymentSlot = slot, Status = currentStatus};
            channel.IsDNSAvailableThunk = ida => new AvailabilityResponse { Result = false };

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                stopServiceCmdlet.SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionName[0], serviceName);

                Assert.IsTrue(statusUpdated);
            }
        }

        [TestMethod]
        public void SetDeploymentStatusProcessWithNotExisitingServiceFail()
        {
            string newStatus = DeploymentStatus.Suspended;
            string currentStatus = DeploymentStatus.Running;
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar =>
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment{Name = serviceName, DeploymentSlot = slot, Status = newStatus};
            };
            channel.GetDeploymentBySlotThunk = ar => new Deployment{Name = serviceName, DeploymentSlot = slot, Status = currentStatus};
            channel.IsDNSAvailableThunk = ida => new AvailabilityResponse { Result = true };

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                stopServiceCmdlet.SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionName[0], serviceName);

                Assert.IsFalse(statusUpdated);
            }
        }
    }
}

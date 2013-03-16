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
    using System.Net;
    using CloudService.Cmdlet;
    using Microsoft.WindowsAzure.Management.Utilities.CloudServiceProject;
    using Microsoft.WindowsAzure.Management.Utilities.Common.Extensions;
    using Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.CloudService.Test.TestData;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using ServiceManagement;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RemoveAzureServiceTests : TestBase
    {
        private const string serviceName = "AzureService";

        private MockCommandRuntime mockCommandRuntime;

        private SimpleServiceManagement channel;

        private RemoveAzureServiceCommand removeServiceCmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            mockCommandRuntime = new MockCommandRuntime();
            channel = new SimpleServiceManagement();

            removeServiceCmdlet = new RemoveAzureServiceCommand(channel) { ShareChannel = true };
            removeServiceCmdlet.CommandRuntime = mockCommandRuntime;
        }

        [TestMethod]
        public void RemoveAzureServiceProcessTest()
        {
            bool serviceDeleted = false;
            bool deploymentDeleted = false;
            channel.GetDeploymentBySlotThunk = ar =>
            {
                if (deploymentDeleted) throw new ServiceManagementClientException(HttpStatusCode.NotFound, new ServiceManagementError(), string.Empty);
                return new Deployment{Name = serviceName, DeploymentSlot = ArgumentConstants.Slots[SlotType.Production], Status = DeploymentStatus.Suspended};
            };
            channel.DeleteHostedServiceThunk = ar => serviceDeleted = true;
            channel.DeleteDeploymentBySlotThunk = ar =>
            {
                deploymentDeleted = true;
            };
            channel.IsDNSAvailableThunk = ida => new AvailabilityResponse { Result = false };

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                removeServiceCmdlet.PassThru = true;
                removeServiceCmdlet.RemoveAzureServiceProcess(service.Paths.RootPath, string.Empty, serviceName);
                Assert.IsTrue(deploymentDeleted);
                Assert.IsTrue(serviceDeleted);
                Assert.IsTrue((bool)mockCommandRuntime.OutputPipeline[0]);
            }
        }

        [TestMethod]
        public void RemoveAzureServiceProcessWithoutPassThruTest()
        {
            bool serviceDeleted = false;
            bool deploymentDeleted = false;
            channel.GetDeploymentBySlotThunk = ar =>
            {
                if (deploymentDeleted) throw new ServiceManagementClientException(HttpStatusCode.NotFound, new ServiceManagementError(), string.Empty);
                return new Deployment{Name = serviceName, DeploymentSlot = ArgumentConstants.Slots[SlotType.Production], Status = DeploymentStatus.Suspended};
            };
            channel.DeleteHostedServiceThunk = ar => serviceDeleted = true;
            channel.DeleteDeploymentBySlotThunk = ar =>
            {
                deploymentDeleted = true;
            };
            channel.IsDNSAvailableThunk = ida => new AvailabilityResponse { Result = false };

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                removeServiceCmdlet.RemoveAzureServiceProcess(service.Paths.RootPath, string.Empty, serviceName);
                Assert.IsTrue(deploymentDeleted);
                Assert.IsTrue(serviceDeleted);
                Assert.AreEqual<int>(0, mockCommandRuntime.OutputPipeline.Count);
            }
        }

        [TestMethod]
        public void RemoveAzureServiceProcessWithNotExistingServiceFail()
        {
            bool serviceDeleted = false;
            bool deploymentDeleted = false;
            channel.GetDeploymentBySlotThunk = ar =>
            {
                if (deploymentDeleted) throw new ServiceManagementClientException(HttpStatusCode.NotFound, new ServiceManagementError(), string.Empty);
                return new Deployment{Name = serviceName, DeploymentSlot = ArgumentConstants.Slots[SlotType.Production], Status = DeploymentStatus.Suspended};
            };
            channel.DeleteHostedServiceThunk = ar => serviceDeleted = true;
            channel.DeleteDeploymentBySlotThunk = ar =>
            {
                deploymentDeleted = true;
            };
            channel.IsDNSAvailableThunk = ida => new AvailabilityResponse { Result = true };

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                removeServiceCmdlet.PassThru = true;
                removeServiceCmdlet.RemoveAzureServiceProcess(service.Paths.RootPath, string.Empty, serviceName);
                Assert.IsFalse(deploymentDeleted);
                Assert.IsFalse(serviceDeleted);
                Assert.IsTrue(mockCommandRuntime.OutputPipeline.Count.Equals(0));
            }
        }
    }
}
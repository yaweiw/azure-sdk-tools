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

namespace Microsoft.WindowsAzure.Commands.Test.CloudService.Utilities
{
    using Commands.Utilities.CloudService;
    using Commands.Utilities.Common;
    using Moq;
    using ServiceManagement;
    using Storage.Blob;
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using Test.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class CloudServiceClientTests : TestBase
	{
		private SubscriptionData subscription;

		private Mock<IServiceManagement> serviceManagementChannelMock;

		private Mock<CloudBlobUtility> cloudBlobUtilityMock;

		private ICloudServiceClient client;

		private string serviceName = "cloudService";

		private string storageName = "storagename";

		private HostedService cloudService;

		private StorageService storageService;

		private Deployment deployment;

		private void ExecuteInTempCurrentDirectory(string path, Action action)
		{
			string currentDirectory = Environment.CurrentDirectory;

			try
			{
				Environment.CurrentDirectory = path;
				action();
			}
			catch
			{
				Environment.CurrentDirectory = currentDirectory;
				throw;
			}
		}

		[TestInitialize]
		public void TestSetup()
		{
			GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
			CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
			
			storageService = new StorageService()
			{
				ServiceName = storageName,
				StorageServiceKeys = new StorageServiceKeys()
				{
					Primary = "MNao3bm7t7B/x+g2/ssh9HnG0mEh1QV5EHpcna8CetYn+TSRoA8/SBoH6B3Ufwtnz3jZLSw9GEUuCTr3VooBWq==",
					Secondary = "secondaryKey"
				},
				StorageServiceProperties = new StorageServiceProperties()
				{
					Endpoints = new EndpointList()
					{
						"http://awesome.blob.core.windows.net/",
						"http://awesome.queue.core.windows.net/",
						"http://awesome.table.core.windows.net/"
					}
				}
			};

			deployment = new Deployment()
			{
				DeploymentSlot = DeploymentSlotType.Production,
				Name = "mydeployment",
				PrivateID = "privateId",
				Status = DeploymentStatus.Starting,
				RoleInstanceList = new RoleInstanceList()
				{
					new RoleInstance()
					{
						InstanceStatus = RoleInstanceStatus.ReadyRole,
						RoleName = "Role1",
						InstanceName = "Instance_Role1"
					}
				}
			};

			cloudService = new HostedService()
			{
				ServiceName = serviceName,
				Deployments = new DeploymentList()
			};
			subscription = new SubscriptionData()
			{
				Certificate = It.IsAny<X509Certificate2>(),
				IsDefault = true,
				ServiceEndpoint = "https://www.azure.com",
				SubscriptionId = Guid.NewGuid().ToString(),
				SubscriptionName = Data.Subscription1
			};

			serviceManagementChannelMock = new Mock<IServiceManagement>();
			serviceManagementChannelMock.Setup(f => f.EndGetHostedServiceWithDetails(It.IsAny<IAsyncResult>()))
				.Returns(cloudService);
			serviceManagementChannelMock.Setup(f => f.EndGetStorageService((It.IsAny<IAsyncResult>())))
				.Returns(storageService);
			serviceManagementChannelMock.Setup(f => f.EndGetStorageKeys(It.IsAny<IAsyncResult>()))
				.Returns(storageService);
			serviceManagementChannelMock.Setup(f => f.EndGetDeploymentBySlot(It.IsAny<IAsyncResult>()))
				.Returns(deployment);

			cloudBlobUtilityMock = new Mock<CloudBlobUtility>();
			cloudBlobUtilityMock.Setup(f => f.UploadPackageToBlob(
				serviceManagementChannelMock.Object,
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<BlobRequestOptions>())).Returns(new Uri("http://www.packageurl.azure.com"));

			client = new CloudServiceClient(subscription)
			{ 
				ServiceManagementChannel = serviceManagementChannelMock.Object,
				CloudBlobUtility = cloudBlobUtilityMock.Object
			};
		}

		[TestMethod]
		public void TestStartCloudService()
		{
			// Setup
            cloudService.Deployments.Add(deployment);
			UpdateDeploymentStatusInput actual = null;
			serviceManagementChannelMock.Setup(f => f.BeginUpdateDeploymentStatusBySlot(
				subscription.SubscriptionId,
				serviceName,
				DeploymentSlotType.Production,
				It.IsAny<UpdateDeploymentStatusInput>(), null, null))
				.Callback((
					string s,
					string name,
					string slot,
					UpdateDeploymentStatusInput input,
					AsyncCallback callback,
					object state) => actual = input);

			serviceManagementChannelMock.Setup(f => f.EndUpdateDeploymentStatusBySlot(It.IsAny<IAsyncResult>()));

			// Test
			client.StartCloudService(serviceName);

			// Assert
			Assert.AreEqual<string>(DeploymentStatus.Running, actual.Status);
			serviceManagementChannelMock.Verify(
				f => f.EndUpdateDeploymentStatusBySlot(It.IsAny<IAsyncResult>()),
				Times.Once());
		}

		[TestMethod]
		public void TestStopCloudService()
		{
			// Setup
            cloudService.Deployments.Add(deployment);
			UpdateDeploymentStatusInput actual = null;
			serviceManagementChannelMock.Setup(f => f.BeginUpdateDeploymentStatusBySlot(
				subscription.SubscriptionId,
				serviceName,
				DeploymentSlotType.Production,
				It.IsAny<UpdateDeploymentStatusInput>(), null, null))
				.Callback((
					string s,
					string name,
					string slot,
					UpdateDeploymentStatusInput input,
					AsyncCallback callback,
					object state) => actual = input);

			serviceManagementChannelMock.Setup(f => f.EndUpdateDeploymentStatusBySlot(It.IsAny<IAsyncResult>()));

			// Test
			client.StopCloudService(serviceName);

			// Assert
			Assert.AreEqual<string>(DeploymentStatus.Suspended, actual.Status);
			serviceManagementChannelMock.Verify(
				f => f.EndUpdateDeploymentStatusBySlot(It.IsAny<IAsyncResult>()),
				Times.Once());
		}

		[TestMethod]
		public void TestRemoveCloudService()
		{
			// Setup
			cloudService.Deployments.Add(deployment);

			// Test
			client.RemoveCloudService(serviceName);

			// Assert
			serviceManagementChannelMock.Verify(f => f.BeginDeleteDeploymentBySlot(
				subscription.SubscriptionId,
				serviceName,
				DeploymentSlotType.Production,
				null,
				null), Times.Once());

			serviceManagementChannelMock.Verify(f => f.BeginDeleteHostedService(
				subscription.SubscriptionId,
				serviceName,
				null,
				null), Times.Once());
		}

		[TestMethod]
		public void TestRemoveCloudServiceWithStaging()
		{
			// Setup
			deployment.DeploymentSlot = DeploymentSlotType.Staging;
			cloudService.Deployments.Add(deployment);

			// Test
			client.RemoveCloudService(serviceName);

			// Assert
			serviceManagementChannelMock.Verify(f => f.BeginDeleteDeploymentBySlot(
				subscription.SubscriptionId,
				serviceName,
				DeploymentSlotType.Staging,
				null,
				null), Times.Once());

			serviceManagementChannelMock.Verify(f => f.BeginDeleteHostedService(
				subscription.SubscriptionId,
				serviceName,
				null,
				null), Times.Once());
		}

		[TestMethod]
		public void TestRemoveCloudServiceWithoutDeployments()
		{
			// Setup
			cloudService.Deployments.Clear();
			serviceManagementChannelMock.Setup(f => f.BeginDeleteDeploymentBySlot(
				subscription.SubscriptionId,
				serviceName,
				DeploymentSlotType.Production,
				null,
				null));
			serviceManagementChannelMock.Setup(f => f.EndDeleteDeploymentBySlot(It.IsAny<IAsyncResult>()));
			serviceManagementChannelMock.Setup(f => f.BeginDeleteHostedService(
				subscription.SubscriptionId,
				serviceName,
				null,
				null));
			serviceManagementChannelMock.Setup(f => f.EndDeleteHostedService(It.IsAny<IAsyncResult>()));

			// Test
			client.RemoveCloudService(serviceName);

			// Assert
			serviceManagementChannelMock.Verify(f => f.BeginDeleteDeploymentBySlot(
				subscription.SubscriptionId,
				serviceName,
				DeploymentSlotType.Production,
				null,
				null), Times.Never());

			serviceManagementChannelMock.Verify(f => f.BeginDeleteHostedService(
				subscription.SubscriptionId,
				serviceName,
				null,
				null), Times.Once());
		}

		[TestMethod]
		public void TestPublishNewCloudService()
		{
			using (FileSystemHelper files = new FileSystemHelper(this) { EnableMonitoring = true })
			{
				// Setup
				string rootPath = files.CreateNewService(serviceName);
				files.CreateAzureSdkDirectoryAndImportPublishSettings();
				CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath, null);
				cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);


				ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

				serviceManagementChannelMock.Verify(f => f.BeginCreateOrUpdateDeployment(
					subscription.SubscriptionId,
					serviceName,
					DeploymentSlotType.Production,
					It.IsAny<CreateDeploymentInput>(),
					null,
					null), Times.Once());
			}
		}

		[TestMethod]
		public void TestUpgradeCloudService()
		{
			using (FileSystemHelper files = new FileSystemHelper(this) { EnableMonitoring = true })
			{
				// Setup
				string rootPath = files.CreateNewService(serviceName);
				files.CreateAzureSdkDirectoryAndImportPublishSettings();
				CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath, null);
				cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);
				cloudService.Deployments.Add(deployment);

				ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

				serviceManagementChannelMock.Verify(f => f.BeginUpgradeDeploymentBySlot(
					subscription.SubscriptionId,
					serviceName,
					DeploymentSlotType.Production,
					It.IsAny<UpgradeDeploymentInput>(),
					null,
					null), Times.Once());
			}
		}

		[TestMethod]
		public void TestCreateStorageServiceWithPublish()
		{
			using (FileSystemHelper files = new FileSystemHelper(this) { EnableMonitoring = true })
			{
				// Setup
				string rootPath = files.CreateNewService(serviceName);
				files.CreateAzureSdkDirectoryAndImportPublishSettings();
				CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath, null);
				cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);
				cloudService.Deployments.Add(deployment);
				serviceManagementChannelMock.Setup(f => f.EndGetStorageService(It.IsAny<IAsyncResult>()))
					.Callback(() => serviceManagementChannelMock.Setup(f => f.EndGetStorageService(
						It.IsAny<IAsyncResult>()))
						.Returns(storageService))
					.Throws(new EndpointNotFoundException());

				ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

				serviceManagementChannelMock.Verify(f => f.BeginCreateStorageService(
					subscription.SubscriptionId,
					It.IsAny<CreateStorageServiceInput>(),
					null,
					null), Times.Once());
			}
		}

		[TestMethod]
		public void TestPublishWithCurrentStorageAccount()
		{
			using (FileSystemHelper files = new FileSystemHelper(this) { EnableMonitoring = true })
			{
				// Setup
				string rootPath = files.CreateNewService(serviceName);
				files.CreateAzureSdkDirectoryAndImportPublishSettings();
				CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath, null);
				cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);
				subscription.CurrentStorageAccount = storageName;

				ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

				cloudBlobUtilityMock.Verify(f => f.UploadPackageToBlob(
					serviceManagementChannelMock.Object,
					subscription.CurrentStorageAccount,
					subscription.SubscriptionId,
					It.IsAny<string>(),
					It.IsAny<BlobRequestOptions>()), Times.Once());
			}
		}

		[TestMethod]
		public void TestPublishWithDefaultLocation()
		{
			using (FileSystemHelper files = new FileSystemHelper(this) { EnableMonitoring = true })
			{
				// Setup
				string rootPath = files.CreateNewService(serviceName);
				files.CreateAzureSdkDirectoryAndImportPublishSettings();
				CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath, null);
				cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);
				serviceManagementChannelMock.Setup(f => f.EndListLocations(It.IsAny<IAsyncResult>()))
					.Returns(new LocationList() { new Location() { Name = "East US" } });

				ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService());

				serviceManagementChannelMock.Verify(f => f.BeginListLocations(
					subscription.SubscriptionId,
					null,
					null), Times.Once());
			}
		}
	}
}

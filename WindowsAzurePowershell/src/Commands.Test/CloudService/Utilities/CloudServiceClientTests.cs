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
    using System.Net;
    using System.Net.Http;
    using Commands.Utilities.CloudService;
    using Commands.Utilities.Common;
    using Management.Compute.Models;
    using Management.Models;
    using Management.Storage;
    using Management.Storage.Models;
    using Moq;
    using ServiceManagement;
    using Storage.Blob;
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using Test.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;
    using DeploymentStatus = ServiceManagement.DeploymentStatus;
    using OperationStatus = Management.Compute.Models.OperationStatus;
    using RoleInstance = ServiceManagement.RoleInstance;
    using RoleInstanceStatus = ServiceManagement.RoleInstanceStatus;
    using StorageServiceProperties = ServiceManagement.StorageServiceProperties;

    [TestClass]
    public class CloudServiceClientTests : TestBase
    {
        private SubscriptionData subscription;

        private ClientMocks clientMocks;

        private Mock<CloudBlobUtility> cloudBlobUtilityMock;

        private ICloudServiceClient client;

        private string serviceName = "cloudService";

        private string storageName = "storagename";

        private HostedService cloudService;

        private StorageService storageService;

        private StorageServiceGetResponse storageServiceGetResponse;
        private StorageAccountGetKeysResponse storageAccountGetKeysResponse;

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

            storageServiceGetResponse = new StorageServiceGetResponse
            {
                ServiceName = storageName,
                Properties = new Management.Storage.Models.StorageServiceProperties
                {
                    Endpoints =
                    {
                        new Uri("http://awesome.blob.core.windows.net/"),
				        new Uri("http://awesome.queue.core.windows.net/"),
				        new Uri("http://awesome.table.core.windows.net/")
                    },
                }
            };

            storageAccountGetKeysResponse = new StorageAccountGetKeysResponse()
            {
                PrimaryKey = "MNao3bm7t7B/x+g2/ssh9HnG0mEh1QV5EHpcna8CetYn+TSRoA8/SBoH6B3Ufwtnz3jZLSw9GEUuCTr3VooBWq==",
                SecondaryKey = "secondaryKey"
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
                SubscriptionName = Data.Subscription1,
            };

            cloudBlobUtilityMock = new Mock<CloudBlobUtility>();
            cloudBlobUtilityMock.Setup(f => f.UploadPackageToBlob(
                It.IsAny<StorageManagementClient>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<BlobRequestOptions>())).Returns(new Uri("http://www.packageurl.azure.com"));

            clientMocks = new ClientMocks(subscription.SubscriptionId);

            clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult(storageServiceGetResponse));
            clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetKeysAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult(storageAccountGetKeysResponse));
            clientMocks.ComputeManagementClientMock.Setup(c => c.ServiceCertificates.ListAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult<ServiceCertificateListResponse>(null));

            client = new CloudServiceClient(subscription,
                clientMocks.ManagementClientMock.Object,
                clientMocks.StorageManagementClientMock.Object,
                clientMocks.ComputeManagementClientMock.Object
                )
            {
                CloudBlobUtility = cloudBlobUtilityMock.Object,
                StatusRetriever = clientMocks.StatusRetriverMock.Object
            };
        }

        [TestMethod]
        public void TestStartCloudService()
        {
            DeploymentUpdateStatusParameters actualUpdateParameters = null;

            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                    {
                        ServiceName = s,
                        StatusCode = HttpStatusCode.OK,
                        Deployments =
                        {
                            new HostedServiceGetDetailedResponse.Deployment()
                            {
                                DeploymentSlot = DeploymentSlot.Production,
                                Name = "mydeployment",
                                Roles =
                                {
                                    new Management.Compute.Models.Role()
                                    {
                                        RoleName = "Role1",
                                    }
                                }
                            }
                        }
                    }));

            clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult(storageServiceGetResponse));
            clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetKeysAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult(storageAccountGetKeysResponse));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.GetBySlotAsync(It.IsAny<string>(), It.IsAny<DeploymentSlot>()))
                .Returns((string name, DeploymentSlot slot) => Tasks.FromResult(new DeploymentGetResponse()
                {
                    Name = name,
                    DeploymentSlot = slot,
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.Deployments.UpdateStatusByDeploymentSlotAsync(It.IsAny<string>(), It.IsAny<DeploymentSlot>(),
                                                                It.IsAny<DeploymentUpdateStatusParameters>()))
                .Callback((string name, DeploymentSlot slot, DeploymentUpdateStatusParameters parameters) =>
                {
                    actualUpdateParameters = parameters;
                })
                .Returns(Tasks.FromResult(new ComputeOperationStatusResponse()
                {
                    Status = OperationStatus.InProgress
                }))
                .Verifiable();

            client.StartCloudService(serviceName);

            Assert.AreEqual(UpdatedDeploymentStatus.Running, actualUpdateParameters.Status);
            clientMocks.Verify();
        }

        [TestMethod]
        public void TestStopCloudService()
        {
            DeploymentUpdateStatusParameters actualUpdateParameters = null;

            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                {
                    ServiceName = s,
                    StatusCode = HttpStatusCode.OK,
                    Deployments =
                        {
                            new HostedServiceGetDetailedResponse.Deployment()
                            {
                                DeploymentSlot = DeploymentSlot.Production,
                                Name = "mydeployment",
                                Roles =
                                {
                                    new Management.Compute.Models.Role()
                                    {
                                        RoleName = "Role1",
                                    }
                                }
                            }
                        }
                }));

            clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult(storageServiceGetResponse));
            clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetKeysAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult(storageAccountGetKeysResponse));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.GetBySlotAsync(It.IsAny<string>(), It.IsAny<DeploymentSlot>()))
                .Returns((string name, DeploymentSlot slot) => Tasks.FromResult(new DeploymentGetResponse()
                {
                    Name = name,
                    DeploymentSlot = slot,
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.Deployments.UpdateStatusByDeploymentSlotAsync(It.IsAny<string>(), It.IsAny<DeploymentSlot>(),
                                                                It.IsAny<DeploymentUpdateStatusParameters>()))
                .Callback((string name, DeploymentSlot slot, DeploymentUpdateStatusParameters parameters) =>
                {
                    actualUpdateParameters = parameters;
                })
                .Returns(Tasks.FromResult(new ComputeOperationStatusResponse()
                {
                    Status = OperationStatus.InProgress
                }))
                .Verifiable();

            client.StopCloudService(serviceName);

            Assert.AreEqual(UpdatedDeploymentStatus.Suspended, actualUpdateParameters.Status);
            clientMocks.Verify();

        }

        [TestMethod]
        public void TestRemoveCloudService()
        {
            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                {
                    ServiceName = s,
                    StatusCode = HttpStatusCode.OK,
                    Deployments =
                                    {
                                        new HostedServiceGetDetailedResponse.Deployment()
                                        {
                                            DeploymentSlot = DeploymentSlot.Production,
                                            Name = "mydeployment",
                                            Roles =
                                            {
                                                new Management.Compute.Models.Role()
                                                {
                                                    RoleName = "Role1",
                                                }
                                            }
                                        }
                                    }
                }));


            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.BeginDeletingBySlotAsync(It.IsAny<string>(), It.IsAny<DeploymentSlot>()))
                .Returns((string s, DeploymentSlot slot) => Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "req0",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.HostedServices.DeleteAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "request000",
                    StatusCode = HttpStatusCode.OK
                }));

            // Test
            client.RemoveCloudService(serviceName);

            // Assert
            clientMocks.ComputeManagementClientMock.Verify(
                c => c.Deployments.BeginDeletingBySlotAsync(serviceName, DeploymentSlot.Production), Times.Once);

            clientMocks.ComputeManagementClientMock.Verify(
                c => c.HostedServices.DeleteAsync(serviceName), Times.Once);
        }

        [TestMethod]
        public void TestRemoveCloudServiceWithStaging()
        {
            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                {
                    ServiceName = s,
                    StatusCode = HttpStatusCode.OK,
                    Deployments =
                                    {
                                        new HostedServiceGetDetailedResponse.Deployment()
                                        {
                                            DeploymentSlot = DeploymentSlot.Staging,
                                            Name = "mydeployment",
                                            Roles =
                                            {
                                                new Management.Compute.Models.Role()
                                                {
                                                    RoleName = "Role1",
                                                }
                                            }
                                        }
                                    }
                }));


            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.BeginDeletingBySlotAsync(It.IsAny<string>(), It.IsAny<DeploymentSlot>()))
                .Returns((string s, DeploymentSlot slot) => Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "req0",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.HostedServices.DeleteAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "request000",
                    StatusCode = HttpStatusCode.OK
                }));

            // Test
            client.RemoveCloudService(serviceName);

            // Assert
            clientMocks.ComputeManagementClientMock.Verify(
                c => c.Deployments.BeginDeletingBySlotAsync(serviceName, DeploymentSlot.Staging), Times.Once);

            clientMocks.ComputeManagementClientMock.Verify(
                c => c.HostedServices.DeleteAsync(serviceName), Times.Once);

        }

        [TestMethod]
        public void TestRemoveCloudServiceWithoutDeployments()
        {
            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                {
                    ServiceName = s,
                    StatusCode = HttpStatusCode.OK,
                }));


            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.BeginDeletingBySlotAsync(It.IsAny<string>(), DeploymentSlot.Production))
                .Returns((string s, DeploymentSlot slot) => Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "req0",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.HostedServices.DeleteAsync(It.IsAny<string>()))
                .Returns(Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "request000",
                    StatusCode = HttpStatusCode.OK
                }));

            // Test
            client.RemoveCloudService(serviceName);

            // Assert
            clientMocks.ComputeManagementClientMock.Verify(
                c => c.Deployments.BeginDeletingBySlotAsync(serviceName, DeploymentSlot.Production), Times.Never);

            clientMocks.ComputeManagementClientMock.Verify(
                c => c.HostedServices.DeleteAsync(serviceName), Times.Once);

        }

        [TestMethod]
        public void TestPublishNewCloudService()
        {
            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                {
                    ServiceName = s,
                    StatusCode = HttpStatusCode.OK,
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.Deployments.CreateAsync(It.IsAny<string>(), DeploymentSlot.Production,
                                                 It.IsAny<DeploymentCreateParameters>()))
                .Returns(Tasks.FromResult(new ComputeOperationStatusResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>()))
                .Returns(Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.GetBySlotAsync(It.IsAny<string>(), DeploymentSlot.Production))
                .Returns(Tasks.FromResult(new DeploymentGetResponse()
                {
                    Configuration = "config",
                    DeploymentSlot = DeploymentSlot.Production,
                    Status = Management.Compute.Models.DeploymentStatus.Starting,
                    PersistentVMDowntime = new PersistentVMDowntime()
                    {
                        EndTime = DateTime.Now,
                        StartTime = DateTime.Now,
                        Status = "",
                    },
                    LastModifiedTime = DateTime.Now.ToString()
                }));

            using (FileSystemHelper files = new FileSystemHelper(this) { EnableMonitoring = true })
            {
                // Setup
                string rootPath = files.CreateNewService(serviceName);
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath, null);
                cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);


                ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

                clientMocks.ComputeManagementClientMock.Verify(c => c.Deployments.CreateAsync(
                    serviceName, DeploymentSlot.Production, It.IsAny<DeploymentCreateParameters>()), Times.Once);
            }

        }

        [TestMethod]
        public void TestUpgradeCloudService()
        {
            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                {
                    ServiceName = s,
                    StatusCode = HttpStatusCode.OK,
                    Deployments =
                                    {
                                        new HostedServiceGetDetailedResponse.Deployment()
                                        {
                                            DeploymentSlot = DeploymentSlot.Production,
                                            Name = "mydeployment",
                                            Roles =
                                            {
                                                new Management.Compute.Models.Role()
                                                {
                                                    RoleName = "Role1",
                                                }
                                            }
                                        }
                                    }
                }));


            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.Deployments.CreateAsync(It.IsAny<string>(), DeploymentSlot.Production,
                                                 It.IsAny<DeploymentCreateParameters>()))
                .Returns(Tasks.FromResult(new ComputeOperationStatusResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>()))
                .Returns(Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.GetBySlotAsync(It.IsAny<string>(), DeploymentSlot.Production))
                .Returns(Tasks.FromResult(new DeploymentGetResponse()
                {
                    Configuration = "config",
                    DeploymentSlot = DeploymentSlot.Production,
                    Status = Management.Compute.Models.DeploymentStatus.Starting,
                    PersistentVMDowntime = new PersistentVMDowntime()
                    {
                        EndTime = DateTime.Now,
                        StartTime = DateTime.Now,
                        Status = "",
                    },
                    LastModifiedTime = DateTime.Now.ToString()
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.Deployments.UpgradeBySlotAsync(It.IsAny<string>(), DeploymentSlot.Production,
                                                 It.IsAny<DeploymentUpgradeParameters>()))
                .Returns(Tasks.FromResult(CreateComputeOperationResponse("req002")));

            using (FileSystemHelper files = new FileSystemHelper(this) { EnableMonitoring = true })
            {
                // Setup
                string rootPath = files.CreateNewService(serviceName);
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath, null);
                cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                cloudService.Deployments.Add(deployment);

                ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

                clientMocks.ComputeManagementClientMock.Verify(c => c.Deployments.UpgradeBySlotAsync(serviceName, DeploymentSlot.Production, It.IsAny<DeploymentUpgradeParameters>()), Times.Once);
            }

        }

        [TestMethod]
        public void TestCreateStorageServiceWithPublish()
        {
            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                {
                    ServiceName = s,
                    StatusCode = HttpStatusCode.OK,
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.Deployments.CreateAsync(It.IsAny<string>(), DeploymentSlot.Production,
                                                 It.IsAny<DeploymentCreateParameters>()))
                .Returns(Tasks.FromResult(new ComputeOperationStatusResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>()))
                .Returns(Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.GetBySlotAsync(It.IsAny<string>(), DeploymentSlot.Production))
                .Returns(Tasks.FromResult(new DeploymentGetResponse()
                {
                    Configuration = "config",
                    DeploymentSlot = DeploymentSlot.Production,
                    Status = Management.Compute.Models.DeploymentStatus.Starting,
                    PersistentVMDowntime = new PersistentVMDowntime()
                    {
                        EndTime = DateTime.Now,
                        StartTime = DateTime.Now,
                        Status = "",
                    },
                    LastModifiedTime = DateTime.Now.ToString()
                }));

            clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetAsync(It.IsAny<string>()))
                .Callback(
                    () => clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetAsync(It.IsAny<string>()))
                        .Returns(() => Tasks.FromResult(storageServiceGetResponse)))
                .Returns(() =>
                {
                    var ex = new CloudException("failed", null, new HttpResponseMessage(HttpStatusCode.NotFound), "context");
                    return Tasks.FromException<StorageServiceGetResponse>(ex);
                })
                ;

            clientMocks.StorageManagementClientMock.Setup(
                c => c.StorageAccounts.BeginCreatingAsync(It.IsAny<StorageAccountCreateParameters>()))
                .Returns(Tasks.FromResult(CreateOperationResponse("request003")));

            using (FileSystemHelper files = new FileSystemHelper(this) { EnableMonitoring = true })
            {
                // Setup
                string rootPath = files.CreateNewService(serviceName);
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath, null);
                cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);
                cloudService.Deployments.Add(deployment);

                ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService(location: "West US"));

                clientMocks.StorageManagementClientMock.Verify(c => c.StorageAccounts.BeginCreatingAsync(It.IsAny<StorageAccountCreateParameters>()), Times.Once);
            }            
        }

        [TestMethod]
        public void TestPublishWithCurrentStorageAccount()
        {
            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                {
                    ServiceName = s,
                    StatusCode = HttpStatusCode.OK,
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.Deployments.CreateAsync(It.IsAny<string>(), DeploymentSlot.Production,
                                                 It.IsAny<DeploymentCreateParameters>()))
                .Returns(Tasks.FromResult(new ComputeOperationStatusResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>()))
                .Returns(Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.GetBySlotAsync(It.IsAny<string>(), DeploymentSlot.Production))
                .Returns(Tasks.FromResult(new DeploymentGetResponse()
                {
                    Configuration = "config",
                    DeploymentSlot = DeploymentSlot.Production,
                    Status = Management.Compute.Models.DeploymentStatus.Starting,
                    PersistentVMDowntime = new PersistentVMDowntime()
                    {
                        EndTime = DateTime.Now,
                        StartTime = DateTime.Now,
                        Status = "",
                    },
                    LastModifiedTime = DateTime.Now.ToString()
                }));

            clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetAsync(It.IsAny<string>()))
                .Returns(() => Tasks.FromResult(storageServiceGetResponse));

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
                    clientMocks.StorageManagementClientMock.Object,
                    subscription.CurrentStorageAccount,
                    It.IsAny<string>(),
                    It.IsAny<BlobRequestOptions>()), Times.Once());
            }           
        }

        [TestMethod]
        public void TestPublishWithDefaultLocation()
        {
            clientMocks.ComputeManagementClientMock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => Tasks.FromResult(new HostedServiceGetDetailedResponse()
                {
                    ServiceName = s,
                    StatusCode = HttpStatusCode.OK,
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.Deployments.CreateAsync(It.IsAny<string>(), DeploymentSlot.Production,
                                                 It.IsAny<DeploymentCreateParameters>()))
                .Returns(Tasks.FromResult(new ComputeOperationStatusResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c =>
                c.HostedServices.CreateAsync(It.IsAny<HostedServiceCreateParameters>()))
                .Returns(Tasks.FromResult(new OperationResponse()
                {
                    RequestId = "request001",
                    StatusCode = HttpStatusCode.OK
                }));

            clientMocks.ComputeManagementClientMock.Setup(
                c => c.Deployments.GetBySlotAsync(It.IsAny<string>(), DeploymentSlot.Production))
                .Returns(Tasks.FromResult(new DeploymentGetResponse()
                {
                    Configuration = "config",
                    DeploymentSlot = DeploymentSlot.Production,
                    Status = Management.Compute.Models.DeploymentStatus.Starting,
                    PersistentVMDowntime = new PersistentVMDowntime()
                    {
                        EndTime = DateTime.Now,
                        StartTime = DateTime.Now,
                        Status = "",
                    },
                    LastModifiedTime = DateTime.Now.ToString()
                }));

            clientMocks.StorageManagementClientMock.Setup(c => c.StorageAccounts.GetAsync(It.IsAny<string>()))
                .Returns(() => Tasks.FromResult(storageServiceGetResponse));

            clientMocks.ManagementClientMock.Setup(c => c.Locations.ListAsync())
                .Returns(Tasks.FromResult(new LocationsListResponse()
                {
                    Locations =
                    {
                        new LocationsListResponse.Location() {DisplayName = "East US", Name = "EastUS"}
                    }
                }));

            using (FileSystemHelper files = new FileSystemHelper(this) { EnableMonitoring = true })
            {
                // Setup
                string rootPath = files.CreateNewService(serviceName);
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                CloudServiceProject cloudServiceProject = new CloudServiceProject(rootPath, null);
                cloudServiceProject.AddWebRole(Data.NodeWebRoleScaffoldingPath);

                ExecuteInTempCurrentDirectory(rootPath, () => client.PublishCloudService());

                clientMocks.ManagementClientMock.Verify(c => c.Locations.ListAsync(), Times.Once);
            }            
        }

        private OperationResponse CreateOperationResponse(string requestId,
                                                          HttpStatusCode status = HttpStatusCode.OK)
        {
            return new OperationResponse()
            {
                RequestId = requestId,
                StatusCode = status
            };
        }

        private ComputeOperationStatusResponse CreateComputeOperationResponse(string requestId, OperationStatus status = OperationStatus.Succeeded)
        {
            return new ComputeOperationStatusResponse()
            {
                Error = null,
                HttpStatusCode = HttpStatusCode.OK,
                Id = "id",
                RequestId = requestId,
                Status = status,
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}

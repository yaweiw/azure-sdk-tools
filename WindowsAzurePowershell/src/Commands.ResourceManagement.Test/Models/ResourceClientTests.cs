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

using System.Collections;
using System.Net;
using System.Runtime.Serialization.Formatters;
using Microsoft.Azure.Commands.ResourceManagement.Models;
using Microsoft.Azure.Gallery;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManagement.Test.Models
{
    public class ResourceClientTests : TestBase
    {
        private Mock<IResourceManagementClient> resourceManagementClientMock;

        private Mock<IStorageClientWrapper> storageClientWrapperMock;

        private Mock<IDeploymentOperations> deploymentsMock;

        private Mock<IResourceGroupOperations> resourceGroupMock;

        private Mock<IResourceOperations> resourceOperationsMock;

        private Mock<IGalleryClient> galleryClientMock;

        private Mock<IDeploymentOperationOperations> deploymentOperationsMock;

        private Mock<Action<string>> progressLoggerMock;

        private ResourcesClient resourcesClient;

        private string resourceGroupName = "myResourceGroup";

        private string resourceGroupLocation = "West US";

        private string deploymentName = "fooDeployment";

        private string templateFile = @"Resources\sampleTemplateFile.json";

        private string parameterFile = @"Resources\sampleParameterFile.json";

        private string storageAccountName = "myStorageAccount";

        private string requestId = "1234567890";

        private string resourceName = "myResource";

        private ResourceIdentity resourceIdentity;

        private Dictionary<string, object> properties;
        
        private string serializedProperties;

        private void SetupListForResourceGroupAsync(string name, List<Resource> result)
        {
            resourceOperationsMock.Setup(f => f.ListAsync(
                It.IsAny<ResourceListParameters>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceListResult
                    {
                        Resources = result
                    }));
        }
        
        public ResourceClientTests()
        {
            resourceManagementClientMock = new Mock<IResourceManagementClient>();
            deploymentsMock = new Mock<IDeploymentOperations>();
            resourceGroupMock = new Mock<IResourceGroupOperations>();
            resourceOperationsMock = new Mock<IResourceOperations>();
            galleryClientMock = new Mock<IGalleryClient>();
            deploymentOperationsMock = new Mock<IDeploymentOperationOperations>();
            progressLoggerMock = new Mock<Action<string>>();
            resourceManagementClientMock.Setup(f => f.Deployments).Returns(deploymentsMock.Object);
            resourceManagementClientMock.Setup(f => f.ResourceGroups).Returns(resourceGroupMock.Object);
            resourceManagementClientMock.Setup(f => f.Resources).Returns(resourceOperationsMock.Object);
            resourceManagementClientMock.Setup(f => f.DeploymentOperations).Returns(deploymentOperationsMock.Object);
            storageClientWrapperMock = new Mock<IStorageClientWrapper>();
            resourcesClient = new ResourcesClient(
                resourceManagementClientMock.Object,
                storageClientWrapperMock.Object,
                galleryClientMock.Object)
                {
                    ProgressLogger = progressLoggerMock.Object
                };

            resourceIdentity = new ResourceIdentity
            {
                ParentResourcePath = "sites/siteA",
                ResourceName = "myResource",
                ResourceProviderNamespace = "Microsoft.Web",
                ResourceType = "sites"
            };
            properties = new Dictionary<string, object>
                {
                    {"name", "site1"},
                    {"siteMode", "Standard"},
                    {"computeMode", "Dedicated"}
                };
            serializedProperties = JsonConvert.SerializeObject(properties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });
        }

        [Fact]
        public void ThrowsExceptionForExistingResourceGroup()
        {
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters() { Name = resourceGroupName };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.Name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = true
                }));

            Assert.Throws<ArgumentException>(() => resourcesClient.CreatePSResourceGroup(parameters));
        }

        [Fact]
        public void CreatesBasicResourceGroup()
        {
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                Name = resourceGroupName,
                Location = resourceGroupLocation
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.Name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.Name,
                It.IsAny< BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.Name, Location = parameters.Location }
                    }));
            SetupListForResourceGroupAsync(parameters.Name, new List<Resource>());

            PSResourceGroup result = resourcesClient.CreatePSResourceGroup(parameters);

            Assert.Equal(parameters.Name, result.ResourceGroupName);
            Assert.Equal(parameters.Location, result.Location);
            Assert.Empty(result.Resources);
        }

        [Fact]
        public void CreatesNewPSResourceWithExistingResourceThrowsException()
        {
            CreatePSResourceParameters parameters = new CreatePSResourceParameters()
            {
                Location = "West US",
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                PropertyObject = new Hashtable(properties),
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceProviderNamespace + "/" + resourceIdentity.ResourceType,
            };

            resourceOperationsMock.Setup(f => f.GetAsync(resourceGroupName, resourceIdentity, It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => new ResourceGetResult
                    {
                        Resource = new Resource
                            {
                                Location = "West US",
                                Properties = serializedProperties,
                                ProvisioningState = ProvisioningState.Running
                            }
                    }));

            Assert.Throws<ArgumentException>(() => resourcesClient.CreatePSResource(parameters));
        }

        [Fact]
        public void CreatesNewPSResourceWithIncorrectTypeThrowsException()
        {
            CreatePSResourceParameters parameters = new CreatePSResourceParameters()
            {
                Location = "West US",
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                PropertyObject = new Hashtable(properties),
                ResourceGroupName = resourceGroupName,
                ResourceType = "abc",
            };

            Assert.Throws<ArgumentException>(() => resourcesClient.CreatePSResource(parameters));
        }

        [Fact]
        public void CreatesNewPSResourceWithAllParameters()
        {
            CreatePSResourceParameters parameters = new CreatePSResourceParameters()
            {
                Location = "West US",
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                PropertyObject = new Hashtable(properties),
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceProviderNamespace + "/" + resourceIdentity.ResourceType,
            };

            int counter = 0;
            resourceOperationsMock.Setup(f => f.GetAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                    {
                        counter++;
                        if (counter == 1)
                        {
                            throw new CloudException("Resource does not exist.");
                        }
                        else
                        {
                            return Task.Factory.StartNew(() => new ResourceGetResult
                            {
                                StatusCode = HttpStatusCode.OK,
                                Resource = new Resource
                                {
                                    Name = parameters.Name,
                                    Location = parameters.Location,
                                    Properties = serializedProperties,
                                    ProvisioningState = ProvisioningState.Running,
                                    ResourceGroup = parameters.ResourceGroupName
                                }
                            });
                        }
                    }
                );

            resourceGroupMock.Setup(f => f.CheckExistenceAsync(resourceGroupName, It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = true
                }));

            resourceOperationsMock.Setup(f => f.CreateOrUpdateAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<ResourceCreateOrUpdateParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => new ResourceCreateOrUpdateResult
                {
                    RequestId = "123",
                    StatusCode = HttpStatusCode.OK,
                    Resource = new BasicResource
                    {
                        Location = "West US",
                        Properties = serializedProperties,
                        ProvisioningState = ProvisioningState.Running
                    }
                }));

            PSResource result = resourcesClient.CreatePSResource(parameters);

            Assert.NotNull(result);
        }

        [Fact]
        public void GetPSResourceWithAllParametersReturnsOneItem()
        {
            GetPSResourceParameters parameters = new GetPSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceProviderNamespace + "/" + resourceIdentity.ResourceType,
            };

            resourceOperationsMock.Setup(f => f.GetAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.Factory.StartNew(() => new ResourceGetResult
                    {
                        StatusCode = HttpStatusCode.OK,
                        Resource = new Resource
                            {
                                Name = parameters.Name,
                                Properties = serializedProperties,
                                ProvisioningState = ProvisioningState.Running,
                                ResourceGroup = parameters.ResourceGroupName,
                                Location = "West US"
                            }
                    }));

            
            List<PSResource> result = resourcesClient.FilterResource(parameters);

            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }

        [Fact]
        public void GetPSResourceWithSomeParametersReturnsList()
        {
            GetPSResourceParameters parameters = new GetPSResourceParameters()
            {
                ResourceGroupName = resourceGroupName,
            };

            resourceOperationsMock.Setup(f => f.ListAsync(It.IsAny<ResourceListParameters>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.Factory.StartNew(() => new ResourceListResult
                {
                    StatusCode = HttpStatusCode.OK,
                    Resources = new List<Resource>(new []
                        {
                            new Resource
                            {
                                Name = "foo",
                                Properties = serializedProperties,
                                ProvisioningState = ProvisioningState.Running,
                                ResourceGroup = parameters.ResourceGroupName,
                                Location = "West US"
                            },
                            new Resource
                            {
                                Name = "bar",
                                Properties = serializedProperties,
                                ProvisioningState = ProvisioningState.Running,
                                ResourceGroup = parameters.ResourceGroupName,
                                Location = "West US"
                            }
                        })
                    
                }));


            List<PSResource> result = resourcesClient.FilterResource(parameters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetPSResourceWithIncorrectTypeThrowsException()
        {
            GetPSResourceParameters parameters = new GetPSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                ResourceGroupName = resourceGroupName,
                ResourceType = "abc",
            };

            Assert.Throws<ArgumentException>(() => resourcesClient.FilterResource(parameters));
        }

        [Fact]
        public void FailsResourceGroupWithInvalidDeployment()
        {
            Uri templateUri = new Uri("http://templateuri.microsoft.com");
            BasicDeployment deploymentFromGet = new BasicDeployment();
            BasicDeployment deploymentFromValidate = new BasicDeployment();
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                Name = resourceGroupName,
                Location = resourceGroupLocation,
                DeploymentName = deploymentName,
                TemplateFile = templateFile,
                ParameterFile = parameterFile,
                StorageAccountName = storageAccountName
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.Name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.Name,
                It.IsAny<BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.Name, Location = parameters.Location }
                    }));
            resourceGroupMock.Setup(f => f.GetAsync(resourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = new ResourceGroup() { Location = resourceGroupLocation }
                }));
            storageClientWrapperMock.Setup(f => f.UploadFileToBlob(It.IsAny<BlobUploadParameters>())).Returns(templateUri);
            deploymentsMock.Setup(f => f.CreateAsync(resourceGroupName, deploymentName, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsCreateResult
                {
                    RequestId = requestId
                }))
                .Callback((string name, string dName, BasicDeployment bDeploy, CancellationToken token) => { deploymentFromGet = bDeploy; });
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Name = deploymentName,
                    Properties = new DeploymentProperties()
                    {
                        Mode = DeploymentMode.Incremental,
                        ProvisioningState = ProvisioningState.Succeeded
                    },
                    ResourceGroup = resourceGroupName
                }));
            deploymentsMock.Setup(f => f.ValidateAsync(resourceGroupName, DeploymentValidationMode.Full, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentValidateResponse
                {
                    Errors = new List<ResourceManagementError>()
                    {
                        new ResourceManagementError()
                        {
                            Code = "404",
                            Message = "Awesome error message",
                            Target = "Bad deployment"
                        }
                    }
                }))
                .Callback((string rg, DeploymentValidationMode m, BasicDeployment d, CancellationToken c) => { deploymentFromValidate = d; });
            SetupListForResourceGroupAsync(parameters.Name, new List<Resource>() { new Resource() { Name = "website"} });
            deploymentOperationsMock.Setup(f => f.ListAsync(resourceGroupName, deploymentName, null, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsListResult
                {
                    Operations = new List<DeploymentOperation>()
                    {
                        new DeploymentOperation()
                        {
                            DeploymentName = deploymentName,
                            OperationId = Guid.NewGuid().ToString(),
                            ResourceGroup = resourceGroupName,
                            Properties = new DeploymentOperationProperties()
                            {
                                ProvisioningState = ProvisioningState.Succeeded,
                                TargetResource = new TargetResource()
                                {
                                    ResourceGroup = resourceGroupName,
                                    ResourceName = resourceName,
                                    ResourceType = "Microsoft.Website"
                                }
                            }
                        }
                    }
                }));

            Assert.Throws<ArgumentException>(() => resourcesClient.CreatePSResourceGroup(parameters));
        }

        [Fact]
        public void CreatesResourceGroupWithDeployment()
        {
            Uri templateUri = new Uri("http://templateuri.microsoft.com");
            BasicDeployment deploymentFromGet = new BasicDeployment();
            BasicDeployment deploymentFromValidate = new BasicDeployment();
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                Name = resourceGroupName,
                Location = resourceGroupLocation,
                DeploymentName = deploymentName,
                TemplateFile = templateFile,
                ParameterFile = parameterFile,
                StorageAccountName = storageAccountName
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.Name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.Name,
                It.IsAny<BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.Name, Location = parameters.Location }
                    }));
            resourceGroupMock.Setup(f => f.GetAsync(resourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = new ResourceGroup() { Location = resourceGroupLocation }
                }));
            storageClientWrapperMock.Setup(f => f.UploadFileToBlob(It.IsAny<BlobUploadParameters>())).Returns(templateUri);
            deploymentsMock.Setup(f => f.CreateAsync(resourceGroupName, deploymentName, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsCreateResult
                {
                    RequestId = requestId
                }))
                .Callback((string name, string dName, BasicDeployment bDeploy, CancellationToken token) => { deploymentFromGet = bDeploy; });
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Name = deploymentName,
                    Properties = new DeploymentProperties()
                    {
                        Mode = DeploymentMode.Incremental,
                        ProvisioningState = ProvisioningState.Succeeded
                    },
                    ResourceGroup = resourceGroupName
                }));
            deploymentsMock.Setup(f => f.ValidateAsync(resourceGroupName, DeploymentValidationMode.Full, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentValidateResponse
                {
                    Errors = new List<ResourceManagementError>()
                }))
                .Callback((string rg, DeploymentValidationMode m, BasicDeployment d, CancellationToken c) => { deploymentFromValidate = d; });
            SetupListForResourceGroupAsync(parameters.Name, new List<Resource>() { new Resource() { Name = "website" } });
            deploymentOperationsMock.Setup(f => f.ListAsync(resourceGroupName, deploymentName, null, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsListResult
                {
                    Operations = new List<DeploymentOperation>()
                    {
                        new DeploymentOperation()
                        {
                            DeploymentName = deploymentName,
                            OperationId = Guid.NewGuid().ToString(),
                            ResourceGroup = resourceGroupName,
                            Properties = new DeploymentOperationProperties()
                            {
                                ProvisioningState = ProvisioningState.Succeeded,
                                TargetResource = new TargetResource()
                                {
                                    ResourceGroup = resourceGroupName,
                                    ResourceName = resourceName,
                                    ResourceType = "Microsoft.Website"
                                }
                            }
                        }
                    }
                }));

            PSResourceGroup result = resourcesClient.CreatePSResourceGroup(parameters);

            deploymentsMock.Verify((f => f.CreateAsync(resourceGroupName, deploymentName, deploymentFromGet, new CancellationToken())), Times.Once());
            Assert.Equal(parameters.Name, result.ResourceGroupName);
            Assert.Equal(parameters.Location, result.Location);
            Assert.Equal(1, result.Resources.Count);

            Assert.Equal(DeploymentMode.Incremental, deploymentFromGet.Mode);
            Assert.Equal(templateUri, deploymentFromGet.TemplateLink.Uri);
            Assert.Equal(File.ReadAllText(parameters.ParameterFile), deploymentFromGet.Parameters);

            Assert.Equal(DeploymentMode.Incremental, deploymentFromValidate.Mode);
            Assert.Equal(templateUri, deploymentFromValidate.TemplateLink.Uri);
            Assert.Equal(File.ReadAllText(parameters.ParameterFile), deploymentFromValidate.Parameters);

            progressLoggerMock.Verify(
                f => f(string.Format("Resource {0} '{1}' provisioning status in location '{2}' is {3}",
                        "Microsoft.Website",
                        resourceName,
                        resourceGroupLocation,
                        ProvisioningState.Succeeded)),
                Times.Once());
        }

        [Fact]
        public void CreatesResourceGroupWithDeploymentFromParameterObject()
        {
            Uri templateUri = new Uri("http://templateuri.microsoft.com");
            BasicDeployment deploymentFromGet = new BasicDeployment();
            BasicDeployment deploymentFromValidate = new BasicDeployment();
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                Name = resourceGroupName,
                Location = resourceGroupLocation,
                DeploymentName = deploymentName,
                TemplateFile = templateFile,
                ParameterObject = new Hashtable()
                {
                    { "string", "myvalue" },
                    { "securestring", "myvalue" },
                    { "int", 12 },
                    { "bool", true },
                },
                StorageAccountName = storageAccountName
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.Name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.Name,
                It.IsAny<BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.Name, Location = parameters.Location }
                    }));
            resourceGroupMock.Setup(f => f.GetAsync(resourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = new ResourceGroup() { Location = resourceGroupLocation }
                }));
            storageClientWrapperMock.Setup(f => f.UploadFileToBlob(It.IsAny<BlobUploadParameters>())).Returns(templateUri);
            deploymentsMock.Setup(f => f.CreateAsync(resourceGroupName, deploymentName, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsCreateResult
                {
                    RequestId = requestId
                }))
                .Callback((string name, string dName, BasicDeployment bDeploy, CancellationToken token) => { deploymentFromGet = bDeploy; });
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Name = deploymentName,
                    Properties = new DeploymentProperties()
                    {
                        Mode = DeploymentMode.Incremental,
                        ProvisioningState = ProvisioningState.Succeeded
                    },
                    ResourceGroup = resourceGroupName
                }));
            deploymentsMock.Setup(f => f.ValidateAsync(resourceGroupName, DeploymentValidationMode.Full, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentValidateResponse
                {
                    Errors = new List<ResourceManagementError>()
                }))
                .Callback((string rg, DeploymentValidationMode m, BasicDeployment d, CancellationToken c) => { deploymentFromValidate = d; });
            SetupListForResourceGroupAsync(parameters.Name, new List<Resource>() { new Resource() { Name = "website" } });
            deploymentOperationsMock.Setup(f => f.ListAsync(resourceGroupName, deploymentName, null, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsListResult
                {
                    Operations = new List<DeploymentOperation>()
                    {
                        new DeploymentOperation()
                        {
                            DeploymentName = deploymentName,
                            OperationId = Guid.NewGuid().ToString(),
                            ResourceGroup = resourceGroupName,
                            Properties = new DeploymentOperationProperties()
                            {
                                ProvisioningState = ProvisioningState.Succeeded,
                                TargetResource = new TargetResource()
                                {
                                    ResourceGroup = resourceGroupName,
                                    ResourceName = resourceName,
                                    ResourceType = "Microsoft.Website"
                                }
                            }
                        }
                    }
                }));

            PSResourceGroup result = resourcesClient.CreatePSResourceGroup(parameters);

            deploymentsMock.Verify((f => f.CreateAsync(resourceGroupName, deploymentName, deploymentFromGet, new CancellationToken())), Times.Once());
            Assert.Equal(parameters.Name, result.ResourceGroupName);
            Assert.Equal(parameters.Location, result.Location);
            Assert.Equal(1, result.Resources.Count);

            Assert.Equal(DeploymentMode.Incremental, deploymentFromGet.Mode);
            Assert.Equal(templateUri, deploymentFromGet.TemplateLink.Uri);
            Assert.Equal(File.ReadAllText(parameterFile), deploymentFromGet.Parameters);

            Assert.Equal(DeploymentMode.Incremental, deploymentFromValidate.Mode);
            Assert.Equal(templateUri, deploymentFromValidate.TemplateLink.Uri);
            Assert.Equal(File.ReadAllText(parameterFile), deploymentFromValidate.Parameters);

            progressLoggerMock.Verify(
                f => f(string.Format("Resource {0} '{1}' provisioning status in location '{2}' is {3}",
                        "Microsoft.Website",
                        resourceName,
                        resourceGroupLocation,
                        ProvisioningState.Succeeded)),
                Times.Once());
        }

        [Fact]
        public void ShowsFailureErrorWhenResourceGroupWithDeploymentFails()
        {
            Uri templateUri = new Uri("http://templateuri.microsoft.com");
            BasicDeployment deploymentFromGet = new BasicDeployment();
            BasicDeployment deploymentFromValidate = new BasicDeployment();
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                Name = resourceGroupName,
                Location = resourceGroupLocation,
                DeploymentName = deploymentName,
                TemplateFile = templateFile,
                ParameterFile = parameterFile,
                StorageAccountName = storageAccountName
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.Name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.Name,
                It.IsAny<BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.Name, Location = parameters.Location }
                    }));
            resourceGroupMock.Setup(f => f.GetAsync(resourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = new ResourceGroup() { Location = resourceGroupLocation }
                }));
            storageClientWrapperMock.Setup(f => f.UploadFileToBlob(It.IsAny<BlobUploadParameters>())).Returns(templateUri);
            deploymentsMock.Setup(f => f.CreateAsync(resourceGroupName, deploymentName, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsCreateResult
                {
                    RequestId = requestId
                }))
                .Callback((string name, string dName, BasicDeployment bDeploy, CancellationToken token) => { deploymentFromGet = bDeploy; });
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Name = deploymentName,
                    Properties = new DeploymentProperties()
                    {
                        Mode = DeploymentMode.Incremental,
                        ProvisioningState = ProvisioningState.Succeeded
                    },
                    ResourceGroup = resourceGroupName
                }));
            deploymentsMock.Setup(f => f.ValidateAsync(resourceGroupName, DeploymentValidationMode.Full, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentValidateResponse
                {
                    Errors = new List<ResourceManagementError>()
                }))
                .Callback((string rg, DeploymentValidationMode m, BasicDeployment d, CancellationToken c) => { deploymentFromValidate = d; });
            SetupListForResourceGroupAsync(parameters.Name, new List<Resource>() { new Resource() { Name = "website" } });
            deploymentOperationsMock.Setup(f => f.ListAsync(resourceGroupName, deploymentName, null, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsListResult
                {
                    Operations = new List<DeploymentOperation>()
                    {
                        new DeploymentOperation()
                        {
                            DeploymentName = deploymentName,
                            OperationId = Guid.NewGuid().ToString(),
                            ResourceGroup = resourceGroupName,
                            Properties = new DeploymentOperationProperties()
                            {
                                ProvisioningState = ProvisioningState.Failed,
                                StatusMessage = "A really bad error occured",
                                TargetResource = new TargetResource()
                                {
                                    ResourceGroup = resourceGroupName,
                                    ResourceName = resourceName,
                                    ResourceType = "Microsoft.Website"
                                }
                            }
                        }
                    }
                }));

            PSResourceGroup result = resourcesClient.CreatePSResourceGroup(parameters);

            deploymentsMock.Verify((f => f.CreateAsync(resourceGroupName, deploymentName, deploymentFromGet, new CancellationToken())), Times.Once());
            Assert.Equal(parameters.Name, result.ResourceGroupName);
            Assert.Equal(parameters.Location, result.Location);
            Assert.Equal(1, result.Resources.Count);

            Assert.Equal(DeploymentMode.Incremental, deploymentFromGet.Mode);
            Assert.Equal(templateUri, deploymentFromGet.TemplateLink.Uri);
            Assert.Equal(File.ReadAllText(parameters.ParameterFile), deploymentFromGet.Parameters);

            Assert.Equal(DeploymentMode.Incremental, deploymentFromValidate.Mode);
            Assert.Equal(templateUri, deploymentFromValidate.TemplateLink.Uri);
            Assert.Equal(File.ReadAllText(parameters.ParameterFile), deploymentFromValidate.Parameters);

            progressLoggerMock.Verify(
                f => f(string.Format("Resource {0} '{1}' in location '{2}' failed with message {3}",
                        "Microsoft.Website",
                        resourceName,
                        resourceGroupLocation,
                        "A really bad error occured")),
                Times.Once());
        }

        [Fact]
        public void GetsOneResource()
        {
            FilterResourcesOptions options = new FilterResourcesOptions() { ResourceGroup = resourceGroupName, Name = resourceName };
            Resource expected = new Resource() { Id = "resourceId", Location = resourceGroupLocation, Name = resourceName };
            ResourceIdentity actualParameters = new ResourceIdentity();
            string actualResourceGroup = null;
            resourceOperationsMock.Setup(f => f.GetAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGetResult
                {
                    Resource = expected
                }))
                .Callback((string rg, ResourceIdentity p, CancellationToken ct) => { actualParameters = p; actualResourceGroup = rg; });
            
            List<Resource> result = resourcesClient.FilterResources(options);

            Assert.Equal(1, result.Count);
            Assert.Equal(options.Name, result.First().Name);
            Assert.Equal(expected.Id, result.First().Id);
            Assert.Equal(expected.Location, result.First().Location);
            Assert.Equal(expected.Name, actualParameters.ResourceName);
            Assert.Equal(resourceGroupName, actualResourceGroup);
        }

        [Fact]
        public void GetsAllResourcesUsingResourceType()
        {
            FilterResourcesOptions options = new FilterResourcesOptions() { ResourceGroup = resourceGroupName, ResourceType = "websites" };
            Resource resource1 = new Resource() { Id = "resourceId", Location = resourceGroupLocation, Name = resourceName };
            Resource resource2 = new Resource() { Id = "resourceId2", Location = resourceGroupLocation, Name = resourceName + "2", };
            ResourceListParameters actualParameters = new ResourceListParameters();
            resourceOperationsMock.Setup(f => f.ListAsync(It.IsAny<ResourceListParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceListResult
                {
                    Resources = new List<Resource>() { resource1, resource2 }
                }))
                .Callback((ResourceListParameters p, CancellationToken ct) => { actualParameters = p; });

            List<Resource> result = resourcesClient.FilterResources(options);

            Assert.Equal(2, result.Count);
            Assert.Equal(options.ResourceType, actualParameters.ResourceType);
        }

        [Fact]
        public void GetsAllResourceGroupResources()
        {
            FilterResourcesOptions options = new FilterResourcesOptions() { ResourceGroup = resourceGroupName};
            Resource resource1 = new Resource() { Id = "resourceId", Location = resourceGroupLocation, Name = resourceName };
            Resource resource2 = new Resource() { Id = "resourceId2", Location = resourceGroupLocation, Name = resourceName + "2" };
            ResourceListParameters actualParameters = new ResourceListParameters();
            resourceOperationsMock.Setup(f => f.ListAsync(It.IsAny<ResourceListParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceListResult
                {
                    Resources = new List<Resource>() { resource1, resource2 }
                }))
                .Callback((ResourceListParameters p, CancellationToken ct) => { actualParameters = p; });

            List<Resource> result = resourcesClient.FilterResources(options);

            Assert.Equal(2, result.Count);
            Assert.True(string.IsNullOrEmpty(actualParameters.ResourceType));
        }

        [Fact]
        public void GetsSpecificResourceGroup()
        {
            string name = resourceGroupName;
            Resource resource1 = new Resource() { Id = "resourceId", Location = resourceGroupLocation, Name = resourceName };
            Resource resource2 = new Resource() { Id = "resourceId2", Location = resourceGroupLocation, Name = resourceName + "2" };
            ResourceGroup resourceGroup = new ResourceGroup() { Name = name, Location = resourceGroupLocation };
            resourceGroupMock.Setup(f => f.GetAsync(name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = resourceGroup
                }));
            SetupListForResourceGroupAsync(name, new List<Resource>() { resource1, resource2 });

            List<PSResourceGroup> actual = resourcesClient.FilterResourceGroups(name);

            Assert.Equal(1, actual.Count);
            Assert.Equal(name, actual[0].ResourceGroupName);
            Assert.Equal(resourceGroupLocation, actual[0].Location);
            Assert.Equal(2, actual[0].Resources.Count);
            Assert.True(!string.IsNullOrEmpty(actual[0].ResourcesTable));
        }

        [Fact]
        public void GetsAllResourceGroups()
        {
            ResourceGroup resourceGroup1 = new ResourceGroup() { Name = resourceGroupName + 1, Location = resourceGroupLocation };
            ResourceGroup resourceGroup2 = new ResourceGroup() { Name = resourceGroupName + 2, Location = resourceGroupLocation };
            ResourceGroup resourceGroup3 = new ResourceGroup() { Name = resourceGroupName + 3, Location = resourceGroupLocation };
            ResourceGroup resourceGroup4 = new ResourceGroup() { Name = resourceGroupName + 4, Location = resourceGroupLocation };
            resourceGroupMock.Setup(f => f.ListAsync(null, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupListResult
                {
                    ResourceGroups = new List<ResourceGroup>() { resourceGroup1, resourceGroup2, resourceGroup3, resourceGroup4 }
                }));
            SetupListForResourceGroupAsync(resourceGroup1.Name, new List<Resource>() { new Resource() { Name = "resource" } });
            SetupListForResourceGroupAsync(resourceGroup2.Name, new List<Resource>() { new Resource() { Name = "resource" } });
            SetupListForResourceGroupAsync(resourceGroup3.Name, new List<Resource>() { new Resource() { Name = "resource" } });
            SetupListForResourceGroupAsync(resourceGroup4.Name, new List<Resource>() { new Resource() { Name = "resource" } });

            List<PSResourceGroup> actual = resourcesClient.FilterResourceGroups(null);

            Assert.Equal(4, actual.Count);
            Assert.Equal(resourceGroup1.Name, actual[0].ResourceGroupName);
            Assert.Equal(resourceGroup2.Name, actual[1].ResourceGroupName);
            Assert.Equal(resourceGroup3.Name, actual[2].ResourceGroupName);
            Assert.Equal(resourceGroup4.Name, actual[3].ResourceGroupName);
        }

        [Fact]
        public void DeletesResourcesGroup()
        {
            resourcesClient.DeleteResourceGroup(resourceGroupName);

            resourceGroupMock.Verify(f => f.DeleteAsync(resourceGroupName, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void ConstructsDynamicParameter()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "DPSet1" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "Mode1, Mode2, Mode3",
                DefaultValue = "Mode1",
                MaxLength = "5",
                MinLength = "1",
                Type = "string"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = resourcesClient.ConstructDynamicParameter(parameters, parameterSetNames, parameter);

            Assert.Equal("ComputeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(string), dynamicParameter.ParameterType);
            Assert.Equal(3, dynamicParameter.Attributes.Count);
            
            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateSetAttribute validateSetAttribute = (ValidateSetAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(3, validateSetAttribute.ValidValues.Count);
            Assert.True(validateSetAttribute.IgnoreCase);
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[0]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[1]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[2]));
            Assert.False(validateSetAttribute.ValidValues[0].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[1].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[2].Contains(' '));
            
            ValidateLengthAttribute validateLengthAttribute = (ValidateLengthAttribute)dynamicParameter.Attributes[2];
            Assert.Equal(int.Parse(value.MinLength), validateLengthAttribute.MinLength);
            Assert.Equal(int.Parse(value.MaxLength), validateLengthAttribute.MaxLength);
        }

        [Fact]
        public void ResolvesDuplicatedDynamicParameterName()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "DPSet1" };
            string key = "Name";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "Mode1, Mode2, Mode3",
                MaxLength = "5",
                MinLength = "1",
                Type = "bool"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = resourcesClient.ConstructDynamicParameter(parameters, parameterSetNames, parameter);

            Assert.Equal(key + "FromTemplate", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(bool), dynamicParameter.ParameterType);
            Assert.Equal(3, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.True(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateSetAttribute validateSetAttribute = (ValidateSetAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(3, validateSetAttribute.ValidValues.Count);
            Assert.True(validateSetAttribute.IgnoreCase);
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[0]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[1]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[2]));
            Assert.False(validateSetAttribute.ValidValues[0].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[1].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[2].Contains(' '));

            ValidateLengthAttribute validateLengthAttribute = (ValidateLengthAttribute)dynamicParameter.Attributes[2];
            Assert.Equal(int.Parse(value.MinLength), validateLengthAttribute.MinLength);
            Assert.Equal(int.Parse(value.MaxLength), validateLengthAttribute.MaxLength);
        }

        [Fact]
        public void ConstructsDynamicParameterWithRangeValidation()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "DPSet1" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "1-10",
                DefaultValue = "Mode1",
                Type = "securestring"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = resourcesClient.ConstructDynamicParameter(parameters, parameterSetNames, parameter);

            Assert.Equal("ComputeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(SecureString), dynamicParameter.ParameterType);
            Assert.Equal(2, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateRangeAttribute validateRangeAttribute = (ValidateRangeAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(1, validateRangeAttribute.MinRange);
            Assert.Equal(10, validateRangeAttribute.MaxRange);
        }

        [Fact]
        public void ConstructsDynamicParameterNoValidation()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "DPSet1" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "",
                DefaultValue = "Mode1",
                Type = "securestring"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = resourcesClient.ConstructDynamicParameter(parameters, parameterSetNames, parameter);

            Assert.Equal("ComputeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(SecureString), dynamicParameter.ParameterType);
            Assert.Equal(1, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);
        }

        [Fact]
        public void ConstructsDynamicParameterWithMinRangeValidation()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "DPSet1" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "5-",
                DefaultValue = "Mode1",
                Type = "securestring"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = resourcesClient.ConstructDynamicParameter(parameters, parameterSetNames, parameter);

            Assert.Equal("ComputeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(SecureString), dynamicParameter.ParameterType);
            Assert.Equal(2, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateRangeAttribute validateRangeAttribute = (ValidateRangeAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(5, validateRangeAttribute.MinRange);
            Assert.Equal(int.MaxValue, validateRangeAttribute.MaxRange);
        }

        [Fact]
        public void ConstructsDynamicParameterWithMaxRangeValidation()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "DPSet1" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "-200",
                DefaultValue = "Mode1",
                Type = "securestring"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = resourcesClient.ConstructDynamicParameter(parameters, parameterSetNames, parameter);

            Assert.Equal("ComputeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(SecureString), dynamicParameter.ParameterType);
            Assert.Equal(2, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateRangeAttribute validateRangeAttribute = (ValidateRangeAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(0, validateRangeAttribute.MinRange);
            Assert.Equal(200, validateRangeAttribute.MaxRange);
        }

        [Fact]
        public void FiltersOneResourceGroupDeployment()
        {
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Name = deploymentName,
                    Properties = new DeploymentProperties()
                    {
                        Mode = DeploymentMode.Incremental,
                        TemplateLink = new TemplateLink()
                        {
                            Uri = new Uri("http://microsoft.com")
                        }
                    },
                    ResourceGroup = resourceGroupName
                }));

            List<PSResourceGroupDeployment> result = resourcesClient.FilterResourceGroupDeployments(resourceGroupName, deploymentName, null);

            Assert.Equal(deploymentName, result[0].DeploymentName);
            Assert.Equal(resourceGroupName, result[0].ResourceGroupName);
            Assert.Equal(DeploymentMode.Incremental, result[0].Mode);
            Assert.Equal(new Uri("http://microsoft.com").ToString(), result[0].TemplateLink.Uri.ToString());
        }

        [Fact]
        public void FiltersResourceGroupDeployments()
        {
            DeploymentListParameters actualParameters = new DeploymentListParameters();
            deploymentsMock.Setup(f => f.ListAsync(
                It.IsAny<DeploymentListParameters>(),
                new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentListResult
                {
                    Deployments = new List<Deployment>()
                    {
                        new Deployment()
                        {
                            DeploymentName = deploymentName + 1,
                            Properties = new DeploymentProperties()
                            {
                                Mode = DeploymentMode.Incremental,
                                TemplateLink = new TemplateLink()
                                {
                                    Uri = new Uri("http://microsoft1.com")
                                }
                            },
                            ResourceGroup = resourceGroupName
                        }
                    },
                    NextLink = "nextLink"
                }))
                .Callback((DeploymentListParameters p, CancellationToken t) => { actualParameters = p; });

            deploymentsMock.Setup(f => f.ListNextAsync(
                "nextLink",
                new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentListResult
                {
                    Deployments = new List<Deployment>()
                    {
                        new Deployment()
                        {
                            DeploymentName = deploymentName + 2,
                            Properties = new DeploymentProperties()
                            {
                                Mode = DeploymentMode.Incremental,
                                TemplateLink = new TemplateLink()
                                {
                                    Uri = new Uri("http://microsoft2.com")
                                }
                            },
                            ResourceGroup = resourceGroupName
                        }
                    }
                }));

            List<PSResourceGroupDeployment> result = resourcesClient.FilterResourceGroupDeployments(resourceGroupName, null, null);

            Assert.Equal(2, result.Count);
            Assert.Equal(deploymentName + 1, result[0].DeploymentName);
            Assert.Equal(resourceGroupName, result[0].ResourceGroupName);
            Assert.Equal(DeploymentMode.Incremental, result[0].Mode);
            Assert.Equal(new Uri("http://microsoft1.com").ToString(), result[0].TemplateLink.Uri.ToString());

            Assert.Equal(deploymentName + 2, result[1].DeploymentName);
            Assert.Equal(resourceGroupName, result[1].ResourceGroupName);
            Assert.Equal(DeploymentMode.Incremental, result[1].Mode);
            Assert.Equal(new Uri("http://microsoft2.com").ToString(), result[1].TemplateLink.Uri.ToString());

            Assert.Equal(resourceGroupName, actualParameters.ResourceGroupName);
        }

        [Fact]
        public void GetsDynamicParametersForTemplateFile()
        {
            RuntimeDefinedParameterDictionary result = resourcesClient.GetTemplateParameters(
                templateFile,
                new string[] { },
                "TestPS");

            Assert.Equal(4, result.Count);

            Assert.Equal("String", result["String"].Name);
            Assert.Equal(typeof(string), result["String"].ParameterType);

            Assert.Equal("Int", result["Int"].Name);
            Assert.Equal(typeof(int), result["Int"].ParameterType);
            
            Assert.Equal("Securestring", result["Securestring"].Name);
            Assert.Equal(typeof(SecureString), result["Securestring"].ParameterType);

            Assert.Equal("Bool", result["Bool"].Name);
            Assert.Equal(typeof(bool), result["Bool"].ParameterType);
        }
    }
}

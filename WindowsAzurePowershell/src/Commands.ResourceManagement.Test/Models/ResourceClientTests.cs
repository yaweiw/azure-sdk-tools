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

using Microsoft.Azure.Commands.ResourceManagement.Models;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManagement.Test.Models
{
    public class ResourceClientTests : TestBase
    {
        private Mock<IResourceManagementClient> resourceManagementClientMock;

        private Mock<IStorageClientWrapper> storageClientWrapperMock;

        private Mock<IDeploymentOperations> deploymentOperationsMock;

        private Mock<IResourceGroupOperations> resourceGroupOperationsMock;

        private Mock<IResourceOperations> resourceOperationsMock;

        private ResourcesClient resourcesClient;

        private string resourceGroupName = "myResourceGroup";

        private string resourceGroupLocation = "West US";

        private string deploymentName = "fooDeployment";

        private string templateFile = @"Resources\sampleTemplateFile.json";

        private string parameterFile = @"Resources\sampleParameterFile.json";

        private string storageAccountName = "myStorageAccount";

        private string requestId = "1234567890";

        private string resourceName = "myResource";
        
        public ResourceClientTests()
        {
            resourceManagementClientMock = new Mock<IResourceManagementClient>();
            deploymentOperationsMock = new Mock<IDeploymentOperations>();
            resourceGroupOperationsMock = new Mock<IResourceGroupOperations>();
            resourceOperationsMock = new Mock<IResourceOperations>();
            resourceManagementClientMock.Setup(f => f.Deployments).Returns(deploymentOperationsMock.Object);
            resourceManagementClientMock.Setup(f => f.ResourceGroups).Returns(resourceGroupOperationsMock.Object);
            resourceManagementClientMock.Setup(f => f.Resources).Returns(resourceOperationsMock.Object);
            storageClientWrapperMock = new Mock<IStorageClientWrapper>();
            resourcesClient = new ResourcesClient(
                resourceManagementClientMock.Object,
                storageClientWrapperMock.Object, null);
        }

        [Fact]
        public void ThrowsExceptionForExistingResourceGroup()
        {
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters() { Name = resourceGroupName };
            resourceGroupOperationsMock.Setup(f => f.ExistsAsync(parameters.Name, new CancellationToken()))
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
            resourceGroupOperationsMock.Setup(f => f.ExistsAsync(parameters.Name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupOperationsMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.Name,
                It.IsAny< BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.Name, Location = parameters.Location }
                    }));
            resourceOperationsMock.Setup(f => f.ListForResourceGroupAsync(
                parameters.Name,
                It.IsAny<ResourceListParameters>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceListResult
                    {
                        Resources = new List<Resource>()
                    }));

            PSResourceGroup result = resourcesClient.CreatePSResourceGroup(parameters);

            Assert.Equal(parameters.Name, result.Name);
            Assert.Equal(parameters.Location, result.Location);
            Assert.Empty(result.Resources);
        }

        [Fact]
        public void CreatesResourceGroupWithDeployment()
        {
            Uri templateUri = new Uri("http://templateuri.microsoft.com");
            BasicDeployment deploymentInfo = new BasicDeployment();
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                Name = resourceGroupName,
                Location = resourceGroupLocation,
                DeploymentName = deploymentName,
                TemplateFile = templateFile,
                ParameterFile = parameterFile,
                StorageAccountName = storageAccountName
            };
            resourceGroupOperationsMock.Setup(f => f.ExistsAsync(parameters.Name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupOperationsMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.Name,
                It.IsAny<BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.Name, Location = parameters.Location }
                    }));
            storageClientWrapperMock.Setup(f => f.UploadFileToBlob(It.IsAny<BlobUploadParameters>())).Returns(templateUri);
            deploymentOperationsMock.Setup(f => f.CreateAsync(resourceGroupName, deploymentName, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsCreateResult
                {
                    RequestId = requestId
                }))
                .Callback((string name, string dName, BasicDeployment bDeploy, CancellationToken token) => { deploymentInfo = bDeploy; });
            resourceOperationsMock.Setup(f => f.ListForResourceGroupAsync(
                parameters.Name,
                It.IsAny<ResourceListParameters>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceListResult
                    {
                        Resources = new List<Resource>() { new Resource() { Name = "website", ResourceGroup = parameters.Name} }
                    }));

            PSResourceGroup result = resourcesClient.CreatePSResourceGroup(parameters);

            deploymentOperationsMock.Verify((f => f.CreateAsync(resourceGroupName, deploymentName, deploymentInfo, new CancellationToken())), Times.Once());
            Assert.Equal(parameters.Name, result.Name);
            Assert.Equal(parameters.Location, result.Location);
            Assert.Equal(DeploymentMode.Incremental, deploymentInfo.Mode);
            Assert.Equal(templateUri, deploymentInfo.TemplateLink.Uri);
            Assert.Equal(File.ReadAllText(parameters.ParameterFile), deploymentInfo.Parameters);
            Assert.Equal(1, result.Resources.Count);
        }

        [Fact]
        public void GetsOneResource()
        {
            FilterResourcesOptions options = new FilterResourcesOptions() { ResourceGroup = resourceGroupName, Name = resourceName };
            Resource expected = new Resource() { Id = "resourceId", Location = resourceGroupLocation, Name = resourceName, ResourceGroup = resourceGroupName };
            ResourceParameters actualParameters = new ResourceParameters();
            resourceOperationsMock.Setup(f => f.GetAsync(It.IsAny<ResourceParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGetResult
                {
                    Resource = expected
                }))
                .Callback((ResourceParameters p, CancellationToken ct) => { actualParameters = p; });
            
            List<Resource> result = resourcesClient.FilterResources(options);

            Assert.Equal(1, result.Count);
            Assert.Equal(options.Name, result.First().Name);
            Assert.Equal(options.ResourceGroup, result.First().ResourceGroup);
            Assert.Equal(expected.Id, result.First().Id);
            Assert.Equal(expected.Location, result.First().Location);
            Assert.Equal(expected.Name, actualParameters.ResourceName);
            Assert.Equal(expected.ResourceGroup, actualParameters.ResourceGroupName);
        }

        [Fact]
        public void GetsAllResourcesUsingResourceType()
        {
            FilterResourcesOptions options = new FilterResourcesOptions() { ResourceGroup = resourceGroupName, ResourceType = "websites" };
            Resource resource1 = new Resource() { Id = "resourceId", Location = resourceGroupLocation, Name = resourceName, ResourceGroup = resourceGroupName };
            Resource resource2 = new Resource() { Id = "resourceId2", Location = resourceGroupLocation, Name = resourceName + "2", ResourceGroup = resourceGroupName };
            ResourceListParameters actualParameters = new ResourceListParameters();
            resourceOperationsMock.Setup(f => f.ListForResourceGroupAsync(options.ResourceGroup, It.IsAny<ResourceListParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceListResult
                {
                    Resources = new List<Resource>() { resource1, resource2 }
                }))
                .Callback((string rgm, ResourceListParameters p, CancellationToken ct) => { actualParameters = p; });

            List<Resource> result = resourcesClient.FilterResources(options);

            Assert.Equal(2, result.Count);
            Assert.Equal(options.ResourceType, actualParameters.ResourceType);
        }

        [Fact]
        public void GetsAllResourceGroupResources()
        {
            FilterResourcesOptions options = new FilterResourcesOptions() { ResourceGroup = resourceGroupName};
            Resource resource1 = new Resource() { Id = "resourceId", Location = resourceGroupLocation, Name = resourceName, ResourceGroup = resourceGroupName };
            Resource resource2 = new Resource() { Id = "resourceId2", Location = resourceGroupLocation, Name = resourceName + "2", ResourceGroup = resourceGroupName };
            ResourceListParameters actualParameters = new ResourceListParameters();
            resourceOperationsMock.Setup(f => f.ListForResourceGroupAsync(options.ResourceGroup, It.IsAny<ResourceListParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceListResult
                {
                    Resources = new List<Resource>() { resource1, resource2 }
                }))
                .Callback((string rgm, ResourceListParameters p, CancellationToken ct) => { actualParameters = p; });

            List<Resource> result = resourcesClient.FilterResources(options);

            Assert.Equal(2, result.Count);
            Assert.True(string.IsNullOrEmpty(actualParameters.ResourceType));
        }
    }
}

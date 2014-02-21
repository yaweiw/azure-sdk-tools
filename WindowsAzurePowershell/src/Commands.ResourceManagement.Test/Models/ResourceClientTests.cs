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
using Microsoft.Azure.Gallery;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security;
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

        private Mock<IGalleryClient> galleryClientMock;

        private ResourcesClient resourcesClient;

        private string resourceGroupName = "myResourceGroup";

        private string resourceGroupLocation = "West US";

        private string deploymentName = "fooDeployment";

        private string templateFile = @"Resources\sampleTemplateFile.json";

        private string parameterFile = @"Resources\sampleParameterFile.json";

        private string storageAccountName = "myStorageAccount";

        private string requestId = "1234567890";

        private string resourceName = "myResource";

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
            deploymentOperationsMock = new Mock<IDeploymentOperations>();
            resourceGroupOperationsMock = new Mock<IResourceGroupOperations>();
            resourceOperationsMock = new Mock<IResourceOperations>();
            galleryClientMock = new Mock<IGalleryClient>();
            resourceManagementClientMock.Setup(f => f.Deployments).Returns(deploymentOperationsMock.Object);
            resourceManagementClientMock.Setup(f => f.ResourceGroups).Returns(resourceGroupOperationsMock.Object);
            resourceManagementClientMock.Setup(f => f.Resources).Returns(resourceOperationsMock.Object);
            storageClientWrapperMock = new Mock<IStorageClientWrapper>();
            resourcesClient = new ResourcesClient(
                resourceManagementClientMock.Object,
                storageClientWrapperMock.Object,
                galleryClientMock.Object);
        }

        [Fact]
        public void ThrowsExceptionForExistingResourceGroup()
        {
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters() { Name = resourceGroupName };
            resourceGroupOperationsMock.Setup(f => f.CheckExistenceAsync(parameters.Name, new CancellationToken()))
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
            resourceGroupOperationsMock.Setup(f => f.CheckExistenceAsync(parameters.Name, new CancellationToken()))
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
            SetupListForResourceGroupAsync(parameters.Name, new List<Resource>());

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
            resourceGroupOperationsMock.Setup(f => f.CheckExistenceAsync(parameters.Name, new CancellationToken()))
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
            SetupListForResourceGroupAsync(parameters.Name, new List<Resource>() { new Resource() { Name = "website"} });

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
                .Callback((string rgm, ResourceListParameters p, CancellationToken ct) => { actualParameters = p; });

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
                .Callback((string rgm, ResourceListParameters p, CancellationToken ct) => { actualParameters = p; });

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
            resourceGroupOperationsMock.Setup(f => f.GetAsync(name, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = resourceGroup
                }));
            SetupListForResourceGroupAsync(name, new List<Resource>() { resource1, resource2 });

            List<PSResourceGroup> actual = resourcesClient.FilterResourceGroups(name);

            Assert.Equal(1, actual.Count);
            Assert.Equal(name, actual[0].Name);
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
            resourceGroupOperationsMock.Setup(f => f.ListAsync(null, new CancellationToken()))
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
            Assert.Equal(resourceGroup1.Name, actual[0].Name);
            Assert.Equal(resourceGroup2.Name, actual[1].Name);
            Assert.Equal(resourceGroup3.Name, actual[2].Name);
            Assert.Equal(resourceGroup4.Name, actual[3].Name);
        }

        [Fact]
        public void DeletesResourcesGroup()
        {
            resourcesClient.DeleteResourceGroup(resourceGroupName);

            resourceGroupOperationsMock.Verify(f => f.DeleteAsync(resourceGroupName, It.IsAny<CancellationToken>()), Times.Once());
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
    }
}

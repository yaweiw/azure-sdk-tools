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
using Microsoft.Azure.Management.Resources.Models;
using Moq;
using System.Collections.Generic;
using System.Management.Automation;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManagement.Test
{
    public class NewAzureResourceGroupCommandTests
    {
        private NewAzureResourceGroupCommand cmdlet;

        private Mock<ResourcesClient> resourcesClientMock;

        private Mock<ICommandRuntime> commandRuntimeMock;

        private string resourceGroupName = "myResourceGroup";

        private string resourceGroupLocation = "West US";

        private string deploymentName = "fooDeployment";

        private string templateFile = @"Resources\sampleTemplateFile.json";

        private string storageAccountName = "myStorageAccount";

        public NewAzureResourceGroupCommandTests()
        {
            resourcesClientMock = new Mock<ResourcesClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new NewAzureResourceGroupCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                ResourceClient = resourcesClientMock.Object
            };
        }

        [Fact]
        public void CreatesNewPSResourceGroupWithUserTemplate()
        {
            CreatePSResourceGroupParameters expectedParameters = new CreatePSResourceGroupParameters()
            {
                ResourceGroupName = resourceGroupName,
                Location = resourceGroupLocation,
                TemplateFile = templateFile,
                Name = deploymentName,
                StorageAccountName = storageAccountName,
                TemplateHash = "hash",
                TemplateHashAlgorithm = "sha1",
                TemplateVersion = "1.0"
            };
            CreatePSResourceGroupParameters actualParameters = new CreatePSResourceGroupParameters();
            PSResourceGroup expected = new PSResourceGroup()
            {
                Location = expectedParameters.Location,
                ResourceGroupName = expectedParameters.ResourceGroupName,
                Resources = new List<Resource>() { new Resource() { Name = "resource1"} }
            };
            resourcesClientMock.Setup(f => f.CreatePSResourceGroup(It.IsAny<CreatePSResourceGroupParameters>()))
                .Returns(expected)
                .Callback((CreatePSResourceGroupParameters p) => { actualParameters = p; });

            cmdlet.Name = expectedParameters.ResourceGroupName;
            cmdlet.Location = expectedParameters.Location;
            cmdlet.TemplateFile = expectedParameters.TemplateFile;
            cmdlet.DeploymentName = expectedParameters.Name;
            cmdlet.StorageAccountName = expectedParameters.StorageAccountName;
            cmdlet.TemplateHash = expectedParameters.TemplateHash;
            cmdlet.TemplateHashAlgorithm = expectedParameters.TemplateHashAlgorithm;
            cmdlet.TemplateVersion = expectedParameters.TemplateVersion;

            cmdlet.ExecuteCmdlet();

            Assert.Equal(expectedParameters.ResourceGroupName, actualParameters.ResourceGroupName);
            Assert.Equal(expectedParameters.Location, actualParameters.Location);
            Assert.Equal(expectedParameters.Name, actualParameters.Name);
            Assert.Equal(expectedParameters.GalleryTemplateName, actualParameters.GalleryTemplateName);
            Assert.Equal(expectedParameters.TemplateFile, actualParameters.TemplateFile);
            Assert.NotNull(actualParameters.TemplateParameterObject);
            Assert.Equal(expectedParameters.TemplateVersion, actualParameters.TemplateVersion);
            Assert.Equal(expectedParameters.TemplateHash, actualParameters.TemplateHash);
            Assert.Equal(expectedParameters.TemplateHashAlgorithm, actualParameters.TemplateHashAlgorithm);
            Assert.Equal(expectedParameters.StorageAccountName, actualParameters.StorageAccountName);

            commandRuntimeMock.Verify(f => f.WriteObject(expected), Times.Once());
        }

        [Fact]
        public void CreatesNewPSResourceGroupWithGalleryTemplate()
        {
            CreatePSResourceGroupParameters expectedParameters = new CreatePSResourceGroupParameters()
            {
                ResourceGroupName = resourceGroupName,
                Location = resourceGroupLocation,
                GalleryTemplateName = "sqlServer",
                Name = deploymentName,
                StorageAccountName = storageAccountName,
                TemplateHash = "hash",
                TemplateHashAlgorithm = "sha1",
                TemplateVersion = "1.0"
            };
            CreatePSResourceGroupParameters actualParameters = new CreatePSResourceGroupParameters();
            PSResourceGroup expected = new PSResourceGroup()
            {
                Location = expectedParameters.Location,
                ResourceGroupName = expectedParameters.ResourceGroupName,
                Resources = new List<Resource>() { new Resource() { Name = "resource1" } }
            };
            resourcesClientMock.Setup(f => f.CreatePSResourceGroup(It.IsAny<CreatePSResourceGroupParameters>()))
                .Returns(expected)
                .Callback((CreatePSResourceGroupParameters p) => { actualParameters = p; });

            cmdlet.Name = expectedParameters.ResourceGroupName;
            cmdlet.Location = expectedParameters.Location;
            cmdlet.GalleryTemplateName = expectedParameters.GalleryTemplateName;
            cmdlet.DeploymentName = expectedParameters.Name;
            cmdlet.StorageAccountName = expectedParameters.StorageAccountName;
            cmdlet.TemplateHash = expectedParameters.TemplateHash;
            cmdlet.TemplateHashAlgorithm = expectedParameters.TemplateHashAlgorithm;
            cmdlet.TemplateVersion = expectedParameters.TemplateVersion;

            cmdlet.ExecuteCmdlet();

            Assert.Equal(expectedParameters.ResourceGroupName, actualParameters.ResourceGroupName);
            Assert.Equal(expectedParameters.Location, actualParameters.Location);
            Assert.Equal(expectedParameters.Name, actualParameters.Name);
            Assert.Equal(expectedParameters.GalleryTemplateName, actualParameters.GalleryTemplateName);
            Assert.Equal(expectedParameters.TemplateFile, actualParameters.TemplateFile);
            Assert.NotNull(actualParameters.TemplateParameterObject);
            Assert.Equal(expectedParameters.TemplateVersion, actualParameters.TemplateVersion);
            Assert.Equal(expectedParameters.TemplateHash, actualParameters.TemplateHash);
            Assert.Equal(expectedParameters.TemplateHashAlgorithm, actualParameters.TemplateHashAlgorithm);
            Assert.Equal(expectedParameters.StorageAccountName, actualParameters.StorageAccountName);

            commandRuntimeMock.Verify(f => f.WriteObject(expected), Times.Once());
        }
    }
}

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
using Microsoft.Azure.Commands.ResourceManagement.ResourceGroupDeployments;
using Microsoft.Azure.Management.Resources.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManagement.Test.Resources
{
    public class NewAzureResourceGroupDeploymentCommandTests
    {
        private NewAzureResourceGroupDeploymentCommand cmdlet;

        private Mock<ResourcesClient> resourcesClientMock;

        private Mock<ICommandRuntime> commandRuntimeMock;

        private string resourceGroupName = "myResourceGroup";

        private string deploymentName = "fooDeployment";

        private string templateFile = @"Resources\sampleTemplateFile.json";

        private string parameterFile = @"Resources\sampleParameterFile.json";

        private string storageAccountName = "myStorageAccount";

        public NewAzureResourceGroupDeploymentCommandTests()
        {
            resourcesClientMock = new Mock<ResourcesClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new NewAzureResourceGroupDeploymentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                ResourceClient = resourcesClientMock.Object
            };
        }

        [Fact]
        public void CreatesNewPSResourceGroupDeploymentWithUserTemplate()
        {
            CreatePSResourceGroupDeploymentParameters expectedParameters = new CreatePSResourceGroupDeploymentParameters()
            {
                ParameterFile = parameterFile,
                TemplateFile = templateFile,
                DeploymentName = deploymentName,
                StorageAccountName = storageAccountName,
                TemplateHash = "hash",
                TemplateHashAlgorithm = "Sha256",
                TemplateVersion = "1.0"
            };
            CreatePSResourceGroupDeploymentParameters actualParameters = new CreatePSResourceGroupDeploymentParameters();
            PSResourceGroupDeployment expected = new PSResourceGroupDeployment()
            {
                Mode = DeploymentMode.Incremental,
                DeploymentName = deploymentName,
                Outputs = new Dictionary<string, DeploymentVariable>()
                {
                    { "Variable1", new DeploymentVariable() { Value = "true", Type = "bool" } },
                    { "Variable2", new DeploymentVariable() { Value = "10", Type = "int" } },
                    { "Variable3", new DeploymentVariable() { Value = "hello world", Type = "string" } }
                },
                Parameters = new Dictionary<string, DeploymentVariable>()
                {
                    { "Parameter1", new DeploymentVariable() { Value = "true", Type = "bool" } },
                    { "Parameter2", new DeploymentVariable() { Value = "10", Type = "int" } },
                    { "Parameter3", new DeploymentVariable() { Value = "hello world", Type = "string" } }
                },
                ProvisioningState = ProvisioningState.Succeeded,
                ResourceGroupName = resourceGroupName,
                TemplateLink = new TemplateLink()
                {
                    ContentHash = new ContentHash()
                    {
                        Algorithm = ContentHashAlgorithm.Sha256,
                        Value = "hash"
                    },
                    ContentVersion = "1.0",
                    Uri = new Uri("http://mytemplate.com")
                },
                Timestamp = new DateTime(2014, 2, 13)
            };
            resourcesClientMock.Setup(f => f.CreatePSResourceGroupDeployment(resourceGroupName,
                It.IsAny<CreatePSResourceGroupDeploymentParameters>()))
                .Returns(expected)
                .Callback((string rg, CreatePSResourceGroupDeploymentParameters p) => { actualParameters = p; });

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.Name = expectedParameters.DeploymentName;
            cmdlet.ParameterFile = expectedParameters.ParameterFile;
            cmdlet.TemplateFile = expectedParameters.TemplateFile;
            cmdlet.StorageAccountName = expectedParameters.StorageAccountName;
            cmdlet.TemplateHash = expectedParameters.TemplateHash;
            cmdlet.TemplateHashAlgorithm = expectedParameters.TemplateHashAlgorithm;
            cmdlet.TemplateVersion = expectedParameters.TemplateVersion;

            cmdlet.ExecuteCmdlet();

            Assert.Equal(expectedParameters.DeploymentName, actualParameters.DeploymentName);
            Assert.Equal(expectedParameters.GalleryTemplateName, actualParameters.GalleryTemplateName);
            Assert.Equal(expectedParameters.TemplateFile, actualParameters.TemplateFile);
            Assert.Equal(expectedParameters.ParameterObject, actualParameters.ParameterObject);
            Assert.Equal(expectedParameters.ParameterFile, actualParameters.ParameterFile);
            Assert.Equal(expectedParameters.TemplateVersion, actualParameters.TemplateVersion);
            Assert.Equal(expectedParameters.TemplateHash, actualParameters.TemplateHash);
            Assert.Equal(expectedParameters.TemplateHashAlgorithm, actualParameters.TemplateHashAlgorithm);
            Assert.Equal(expectedParameters.StorageAccountName, actualParameters.StorageAccountName);

            commandRuntimeMock.Verify(f => f.WriteObject(expected), Times.Once());
        }

        [Fact]
        public void CreatesNewPSResourceGroupDeploymentWithGalleryTemplate()
        {
            CreatePSResourceGroupDeploymentParameters expectedParameters = new CreatePSResourceGroupDeploymentParameters()
            {
                ParameterFile = parameterFile,
                GalleryTemplateName = "sqlServer",
                DeploymentName = deploymentName,
                StorageAccountName = storageAccountName,
                TemplateHash = "hash",
                TemplateHashAlgorithm = "Sha256",
                TemplateVersion = "1.0"
            };
            CreatePSResourceGroupDeploymentParameters actualParameters = new CreatePSResourceGroupDeploymentParameters();
            PSResourceGroupDeployment expected = new PSResourceGroupDeployment()
            {
                Mode = DeploymentMode.Incremental,
                DeploymentName = deploymentName,
                Outputs = new Dictionary<string, DeploymentVariable>()
                {
                    { "Variable1", new DeploymentVariable() { Value = "true", Type = "bool" } },
                    { "Variable2", new DeploymentVariable() { Value = "10", Type = "int" } },
                    { "Variable3", new DeploymentVariable() { Value = "hello world", Type = "string" } }
                },
                Parameters = new Dictionary<string, DeploymentVariable>()
                {
                    { "Parameter1", new DeploymentVariable() { Value = "true", Type = "bool" } },
                    { "Parameter2", new DeploymentVariable() { Value = "10", Type = "int" } },
                    { "Parameter3", new DeploymentVariable() { Value = "hello world", Type = "string" } }
                },
                ProvisioningState = ProvisioningState.Succeeded,
                ResourceGroupName = resourceGroupName,
                TemplateLink = new TemplateLink()
                {
                    ContentHash = new ContentHash()
                    {
                        Algorithm = ContentHashAlgorithm.Sha256,
                        Value = "hash"
                    },
                    ContentVersion = "1.0",
                    Uri = new Uri("http://mytemplate.com")
                },
                Timestamp = new DateTime(2014, 2, 13)
            };
            resourcesClientMock.Setup(f => f.CreatePSResourceGroupDeployment(resourceGroupName,
                It.IsAny<CreatePSResourceGroupDeploymentParameters>()))
                .Returns(expected)
                .Callback((string rg, CreatePSResourceGroupDeploymentParameters p) => { actualParameters = p; });

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.Name = expectedParameters.DeploymentName;
            cmdlet.ParameterFile = expectedParameters.ParameterFile;
            cmdlet.GalleryTemplateName = expectedParameters.GalleryTemplateName;
            cmdlet.StorageAccountName = expectedParameters.StorageAccountName;
            cmdlet.TemplateHash = expectedParameters.TemplateHash;
            cmdlet.TemplateHashAlgorithm = expectedParameters.TemplateHashAlgorithm;
            cmdlet.TemplateVersion = expectedParameters.TemplateVersion;

            cmdlet.ExecuteCmdlet();

            Assert.Equal(expectedParameters.DeploymentName, actualParameters.DeploymentName);
            Assert.Equal(expectedParameters.GalleryTemplateName, actualParameters.GalleryTemplateName);
            Assert.Equal(expectedParameters.TemplateFile, actualParameters.TemplateFile);
            Assert.Equal(expectedParameters.ParameterObject, actualParameters.ParameterObject);
            Assert.Equal(expectedParameters.ParameterFile, actualParameters.ParameterFile);
            Assert.Equal(expectedParameters.TemplateVersion, actualParameters.TemplateVersion);
            Assert.Equal(expectedParameters.TemplateHash, actualParameters.TemplateHash);
            Assert.Equal(expectedParameters.TemplateHashAlgorithm, actualParameters.TemplateHashAlgorithm);
            Assert.Equal(expectedParameters.StorageAccountName, actualParameters.StorageAccountName);

            commandRuntimeMock.Verify(f => f.WriteObject(expected), Times.Once());
        }
    }
}

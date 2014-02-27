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

using System.Net;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Commands.ResourceManagement.Models;
using Microsoft.Azure.Gallery;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using Newtonsoft.Json;
using Xunit;
using System.Collections;

namespace Microsoft.Azure.Commands.ResourceManagement.Test
{
    public class NewAzureResourceCommandTests
    {
        private NewAzureResourceCommand cmdlet;

        private ResourcesClient resourcesClientMock;
        private Mock<IResourceOperations> resourceOperationsMock;
        private Mock<ICommandRuntime> commandRuntimeMock;
        private Mock<IResourceManagementClient> resourceManagementClientMock;
        private Mock<IStorageClientWrapper> storageClientWrapperMock;
        private Mock<IGalleryClient> galleryClientMock;
        private Mock<Action<string>> progressLoggerMock;

        private string resourceGroupName = "myResourceGroup";
        private ResourceIdentity resourceIdentity;
        private Dictionary<string, object> properties;
        private string serializedProperties;

        public NewAzureResourceCommandTests()
        {
            resourcesClientMock = new ResourcesClient(
                resourceManagementClientMock.Object,
                storageClientWrapperMock.Object,
                galleryClientMock.Object)
            {
                ProgressLogger = progressLoggerMock.Object
            };
            commandRuntimeMock = new Mock<ICommandRuntime>();
            resourceOperationsMock = new Mock<IResourceOperations>();
            storageClientWrapperMock = new Mock<IStorageClientWrapper>();
            galleryClientMock = new Mock<IGalleryClient>();
            cmdlet = new NewAzureResourceCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                ResourceClient = resourcesClientMock
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
        public void CreatesNewPSResourceWithAllParameters()
        {
            CreatePSResourceParameters inputParameters = new CreatePSResourceParameters()
            {
                Location ="West US",
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                PropertyObject = new Hashtable(properties),
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceType,
            };

            PSResource output = new PSResource()
            {
                Location = "West US",
                Name = resourceIdentity.ResourceName,
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceType
            };

            resourceOperationsMock.Setup(f => f.CreateOrUpdateAsync(resourceGroupName, resourceIdentity, It.IsAny<ResourceCreateOrUpdateParameters>(), new CancellationToken()))
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

            cmdlet.Name = inputParameters.Name;
            cmdlet.ResourceGroupName = inputParameters.ResourceGroupName;
            cmdlet.Location = inputParameters.Location;
            cmdlet.ResourceType = inputParameters.ResourceType;
            cmdlet.ParentResourceName = inputParameters.ParentResourceName;
            cmdlet.PropertyObject = inputParameters.PropertyObject;

            cmdlet.ExecuteCmdlet();

            commandRuntimeMock.Verify(f => f.WriteObject(output), Times.Once());
        }
    }
}

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
using Microsoft.Azure.Gallery.Models;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Microsoft.WindowsAzure.Common.OData;
using Microsoft.WindowsAzure.Management.Monitoring.Events;
using Microsoft.WindowsAzure.Management.Monitoring.Events.Models;
using Microsoft.WindowsAzure.Management.Monitoring.Models;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

        private Mock<IEventsClient> eventsClientMock;

        private Mock<IDeploymentOperationOperations> deploymentOperationsMock;

        private Mock<IEventDataOperations> eventDataOperationsMock;

        private Mock<IProviderOperations> providersMock;

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

        private List<EventData> sampleEvents;

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
            eventsClientMock = new Mock<IEventsClient>();
            deploymentOperationsMock = new Mock<IDeploymentOperationOperations>();
            eventDataOperationsMock = new Mock<IEventDataOperations>();
            providersMock = new Mock<IProviderOperations>();
            providersMock.Setup(f => f.ListAsync(null, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ProviderListResult
                {
                    Providers = new List<Provider>()
                }));
            progressLoggerMock = new Mock<Action<string>>();
            resourceManagementClientMock.Setup(f => f.Deployments).Returns(deploymentsMock.Object);
            resourceManagementClientMock.Setup(f => f.ResourceGroups).Returns(resourceGroupMock.Object);
            resourceManagementClientMock.Setup(f => f.Resources).Returns(resourceOperationsMock.Object);
            resourceManagementClientMock.Setup(f => f.DeploymentOperations).Returns(deploymentOperationsMock.Object);
            resourceManagementClientMock.Setup(f => f.Providers).Returns(providersMock.Object);
            eventsClientMock.Setup(f => f.EventData).Returns(eventDataOperationsMock.Object);
            storageClientWrapperMock = new Mock<IStorageClientWrapper>();
            resourcesClient = new ResourcesClient(
                resourceManagementClientMock.Object,
                storageClientWrapperMock.Object,
                galleryClientMock.Object,
                eventsClientMock.Object)
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
                    {"computeMode", "Dedicated"},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key1", "value1"},
                            {"key2", "value2"}
                        }}
                };
            serializedProperties = JsonConvert.SerializeObject(properties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            sampleEvents = new List<EventData>();
            sampleEvents.Add(new EventData
                {
                    EventDataId = "ac7d2ab5-698a-4c33-9c19-0a93d3d7f527",
                    EventName = new LocalizableString {LocalizedValue = "Start request"},
                    EventSource = new LocalizableString {LocalizedValue = "Microsoft Resources"},
                    EventChannels = EventChannels.Operation,
                    Level = EventLevel.Informational,
                    EventTimestamp = DateTime.Now,
                    OperationId = "c0f2e85f-efb0-47d0-bf90-f983ec8be91d",
                    OperationName =
                        new LocalizableString
                            {
                                LocalizedValue = "Microsoft.Resources/subscriptions/resourcegroups/deployments/write"
                            },
                    Status = new LocalizableString {LocalizedValue = "Succeeded"},
                    SubStatus = new LocalizableString {LocalizedValue = "Created"},
                    ResourceGroupName = "foo",
                    ResourceProviderName = new LocalizableString {LocalizedValue = "Microsoft Resources"},
                    ResourceUri =
                        "/subscriptions/ffce8037-a374-48bf-901d-dac4e3ea8c09/resourcegroups/foo/deployments/testdeploy",
                    HttpRequest = new HttpRequestInfo
                        {
                            Uri =
                                "http://path/subscriptions/ffce8037-a374-48bf-901d-dac4e3ea8c09/resourcegroups/foo/deployments/testdeploy",
                            Method = "PUT",
                            ClientRequestId = "1234",
                            ClientIpAddress = "123.123.123.123"
                        },
                    Authorization = new SenderAuthorization
                        {
                            Action = "PUT",
                            Condition = "",
                            Role = "Sender",
                            Scope = "None"
                        },
                    Claims = new Dictionary<string, string>
                        {
                            {"aud", "https://management.core.windows.net/"},
                            {"iss", "https://sts.windows.net/123456/"},
                            {"iat", "h123445"}
                        },
                    Properties = new Dictionary<string,string>()
                });
            sampleEvents.Add(new EventData
            {
                EventDataId = "ac7d2ab5-698a-4c33-9c19-0sdfsdf34r54",
                EventName = new LocalizableString { LocalizedValue = "End request" },
                EventSource = new LocalizableString { LocalizedValue = "Microsoft Resources" },
                EventChannels = EventChannels.Operation,
                Level = EventLevel.Informational,
                EventTimestamp = DateTime.Now,
                OperationId = "c0f2e85f-efb0-47d0-bf90-f983ec8be91d",
                OperationName =
                    new LocalizableString
                    {
                        LocalizedValue = "Microsoft.Resources/subscriptions/resourcegroups/deployments/write"
                    },
                Status = new LocalizableString { LocalizedValue = "Succeeded" },
                SubStatus = new LocalizableString { LocalizedValue = "Created" },
                ResourceGroupName = "foo",
                ResourceProviderName = new LocalizableString { LocalizedValue = "Microsoft Resources" },
                ResourceUri =
                    "/subscriptions/ffce8037-a374-48bf-901d-dac4e3ea8c09/resourcegroups/foo/deployments/testdeploy",
                HttpRequest = new HttpRequestInfo
                {
                    Uri =
                        "http://path/subscriptions/ffce8037-a374-48bf-901d-dac4e3ea8c09/resourcegroups/foo/deployments/testdeploy",
                    Method = "PUT",
                    ClientRequestId = "1234",
                    ClientIpAddress = "123.123.123.123"
                },
                Authorization = new SenderAuthorization
                {
                    Action = "PUT",
                    Condition = "",
                    Role = "Sender",
                    Scope = "None"
                },
                Claims = new Dictionary<string, string>
                        {
                            {"aud", "https://management.core.windows.net/"},
                            {"iss", "https://sts.windows.net/123456/"},
                            {"iat", "h123445"}
                        },
                Properties = new Dictionary<string, string>()
            });
        }

        [Fact]
        public void NewResourceGroupThrowsExceptionForExistingResourceGroup()
        {
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters() { ResourceGroupName = resourceGroupName };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.ResourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = true
                }));

            Assert.Throws<ArgumentException>(() => resourcesClient.CreatePSResourceGroup(parameters));
        }

        [Fact]
        public void NewResourceGroupWithoutDeploymentSucceeds()
        {
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                ResourceGroupName = resourceGroupName,
                Location = resourceGroupLocation
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.ResourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.ResourceGroupName,
                It.IsAny< BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.ResourceGroupName, Location = parameters.Location }
                    }));
            SetupListForResourceGroupAsync(parameters.ResourceGroupName, new List<Resource>());

            PSResourceGroup result = resourcesClient.CreatePSResourceGroup(parameters);

            Assert.Equal(parameters.ResourceGroupName, result.ResourceGroupName);
            Assert.Equal(parameters.Location, result.Location);
            Assert.Empty(result.Resources);
        }

        [Fact]
        public void NewResourceWithExistingResourceThrowsException()
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

            resourceOperationsMock.Setup(f => f.CheckExistenceAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => new ResourceExistsResult
                {
                    Exists = true
                }));

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

            Assert.Throws<ArgumentException>(() => resourcesClient.CreateResource(parameters));
        }

        [Fact]
        public void NewResourceWithIncorrectTypeThrowsException()
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

            Assert.Throws<ArgumentException>(() => resourcesClient.CreateResource(parameters));
        }

        [Fact]
        public void NewResourceWithAllParametersSucceeds()
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

            resourceOperationsMock.Setup(f => f.GetAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.Factory.StartNew(() => new ResourceGetResult
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
                    }));

            resourceGroupMock.Setup(f => f.CheckExistenceAsync(resourceGroupName, It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = true
                }));

            resourceOperationsMock.Setup(f => f.CheckExistenceAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => new ResourceExistsResult
                {
                    Exists = false
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

            PSResource result = resourcesClient.CreateResource(parameters);

            Assert.NotNull(result);
        }

        [Fact]
        public void SetResourceWithoutExistingResourceThrowsException()
        {
            UpdatePSResourceParameters parameters = new UpdatePSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                PropertyObject = new Hashtable(properties),
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceProviderNamespace + "/" + resourceIdentity.ResourceType,
            };

            resourceOperationsMock.Setup(f => f.GetAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(() => { throw new CloudException("Resource does not exist."); });

            Assert.Throws<ArgumentException>(() => resourcesClient.UpdatePSResource(parameters));
        }

        [Fact]
        public void SetResourceWithIncorrectTypeThrowsException()
        {
            UpdatePSResourceParameters parameters = new UpdatePSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                PropertyObject = new Hashtable(properties),
                ResourceGroupName = resourceGroupName,
                ResourceType = "abc",
            };

            Assert.Throws<ArgumentException>(() => resourcesClient.UpdatePSResource(parameters));
        }

        [Fact]
        public void SetResourceWithAllParameters()
        {
            UpdatePSResourceParameters parameters = new UpdatePSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                PropertyObject = new Hashtable(properties),
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
                                Location = "West US",
                                Properties = serializedProperties,
                                ProvisioningState = ProvisioningState.Running,
                                ResourceGroup = parameters.ResourceGroupName
                            }
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

            PSResource result = resourcesClient.UpdatePSResource(parameters);

            Assert.NotNull(result);
        }

        [Fact]
        public void SetResourceWithUpdatePatchesResource()
        {
            var originalProperties = new Dictionary<string, object>
                {
                    {"name", "site1"},
                    {"siteMode", "Standard"},
                    {"computeMode", "Dedicated"},
                    {"list", new [] {1,2,3}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key1", "value1"},
                            {"key2", "value2"}
                        }}};

            var originalPropertiesSerialized = JsonConvert.SerializeObject(originalProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var patchProperties = new Dictionary<string, object>
                {
                    {"siteMode", "Dedicated"},
                    {"newMode", "NewValue"},
                    {"list", new [] {4,5,6}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key3", "value3"}
                        }}};
            
            UpdatePSResourceParameters parameters = new UpdatePSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                PropertyObject = new Hashtable(patchProperties),
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceProviderNamespace + "/" + resourceIdentity.ResourceType,
            };

            ResourceCreateOrUpdateParameters actual = new ResourceCreateOrUpdateParameters();

            resourceOperationsMock.Setup(f => f.GetAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.Factory.StartNew(() => new ResourceGetResult
                {
                    StatusCode = HttpStatusCode.OK,
                    Resource = new Resource
                    {
                        Name = parameters.Name,
                        Location = "West US",
                        Properties = originalPropertiesSerialized,
                        ProvisioningState = ProvisioningState.Running,
                        ResourceGroup = parameters.ResourceGroupName
                    }
                }));

            resourceOperationsMock.Setup(f => f.CreateOrUpdateAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<ResourceCreateOrUpdateParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => new ResourceCreateOrUpdateResult
                {
                    RequestId = "123",
                    StatusCode = HttpStatusCode.OK,
                    Resource = new BasicResource
                    {
                        Location = "West US",
                        Properties = originalPropertiesSerialized,
                        ProvisioningState = ProvisioningState.Running
                    }
                }))
                .Callback((string groupName, ResourceIdentity id, ResourceCreateOrUpdateParameters p, CancellationToken token) => actual = p);

            resourcesClient.UpdatePSResource(parameters);

            JToken actualJson = JToken.Parse(actual.Resource.Properties);

            Assert.Equal("site1", actualJson["name"].ToObject<string>());
            Assert.Equal("Dedicated", actualJson["siteMode"].ToObject<string>());
            Assert.Equal("Dedicated", actualJson["computeMode"].ToObject<string>());
            Assert.Equal("NewValue", actualJson["newMode"].ToObject<string>());
            Assert.Equal("[4,5,6]", actualJson["list"].ToString(Formatting.None));
            Assert.Equal("value1", actualJson["misc"]["key1"].ToObject<string>());
            Assert.Equal("value2", actualJson["misc"]["key2"].ToObject<string>());
            Assert.Equal("value3", actualJson["misc"]["key3"].ToObject<string>());
        }

        [Fact]
        public void SetResourceWithReplaceRewritesResource()
        {
            var originalProperties = new Dictionary<string, object>
                {
                    {"name", "site1"},
                    {"siteMode", "Standard"},
                    {"computeMode", "Dedicated"},
                    {"list", new [] {1,2,3}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key1", "value1"},
                            {"key2", "value2"}
                        }}};

            var originalPropertiesSerialized = JsonConvert.SerializeObject(originalProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var patchProperties = new Dictionary<string, object>
                {
                    {"siteMode", "Dedicated"},
                    {"newMode", "NewValue"},
                    {"list", new [] {4,5,6}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key3", "value3"}
                        }}};

            UpdatePSResourceParameters parameters = new UpdatePSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                PropertyObject = new Hashtable(patchProperties),
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceProviderNamespace + "/" + resourceIdentity.ResourceType,
                Mode = SetResourceMode.Replace
            };

            ResourceCreateOrUpdateParameters actual = new ResourceCreateOrUpdateParameters();

            resourceOperationsMock.Setup(f => f.GetAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.Factory.StartNew(() => new ResourceGetResult
                {
                    StatusCode = HttpStatusCode.OK,
                    Resource = new Resource
                    {
                        Name = parameters.Name,
                        Location = "West US",
                        Properties = originalPropertiesSerialized,
                        ProvisioningState = ProvisioningState.Running,
                        ResourceGroup = parameters.ResourceGroupName
                    }
                }));

            resourceOperationsMock.Setup(f => f.CreateOrUpdateAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<ResourceCreateOrUpdateParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => new ResourceCreateOrUpdateResult
                {
                    RequestId = "123",
                    StatusCode = HttpStatusCode.OK,
                    Resource = new BasicResource
                    {
                        Location = "West US",
                        Properties = originalPropertiesSerialized,
                        ProvisioningState = ProvisioningState.Running
                    }
                }))
                .Callback((string groupName, ResourceIdentity id, ResourceCreateOrUpdateParameters p, CancellationToken token) => actual = p);

            resourcesClient.UpdatePSResource(parameters);

            JToken actualJson = JToken.Parse(actual.Resource.Properties);

            Assert.Null(actualJson["name"]);
            Assert.Equal("Dedicated", actualJson["siteMode"].ToObject<string>());
            Assert.Null(actualJson["computeMode"]);
            Assert.Equal("NewValue", actualJson["newMode"].ToObject<string>());
            Assert.Equal("[4,5,6]", actualJson["list"].ToString(Formatting.None));
            Assert.Null(actualJson["misc"]["key1"]);
            Assert.Null(actualJson["misc"]["key2"]);
            Assert.Equal("value3", actualJson["misc"]["key3"].ToObject<string>());
        }

        [Fact]
        public void RemoveResourceWithoutExistingResourceThrowsException()
        {
            BasePSResourceParameters parameters = new BasePSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceProviderNamespace + "/" + resourceIdentity.ResourceType,
            };

            resourceOperationsMock.Setup(f => f.CheckExistenceAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.Factory.StartNew(() => new ResourceExistsResult
                            {
                                Exists = false
                            }
                    ));

            Assert.Throws<ArgumentException>(() => resourcesClient.DeleteResource(parameters));
        }


        [Fact]
        public void RemoveResourceWithIncorrectTypeThrowsException()
        {
            BasePSResourceParameters parameters = new BasePSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                ResourceGroupName = resourceGroupName,
                ResourceType = "abc",
            };

            Assert.Throws<ArgumentException>(() => resourcesClient.DeleteResource(parameters));
        }

        [Fact]
        public void RemoveResourceWithAllParametersSucceeds()
        {
            BasePSResourceParameters parameters = new BasePSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                ResourceGroupName = resourceGroupName,
                ResourceType = resourceIdentity.ResourceProviderNamespace + "/" + resourceIdentity.ResourceType,
            };

            resourceOperationsMock.Setup(f => f.CheckExistenceAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.Factory.StartNew(() => new ResourceExistsResult
                {
                    Exists = true
                }
            ));

            resourceOperationsMock.Setup(f => f.DeleteAsync(resourceGroupName, It.IsAny<ResourceIdentity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.Factory.StartNew(() => new OperationResponse
                {
                    RequestId = "123",
                    StatusCode = HttpStatusCode.OK
                }));

            resourcesClient.DeleteResource(parameters);
        }

        [Fact]
        public void GetResourceWithAllParametersReturnsOneItem()
        {
            BasePSResourceParameters parameters = new BasePSResourceParameters()
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
                                Location = "West US",
                            }
                    }));

            
            List<PSResource> result = resourcesClient.FilterPSResources(parameters);

            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal(4, result[0].ParameterObject.Count);
            Assert.Equal(2, ((Dictionary<string, object>)result[0].ParameterObject["misc"]).Count);
        }

        [Fact]
        public void GetResourceWithSomeParametersReturnsList()
        {
            BasePSResourceParameters parameters = new BasePSResourceParameters()
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
                                Properties = null,
                                ProvisioningState = ProvisioningState.Running,
                                ResourceGroup = parameters.ResourceGroupName,
                                Location = "West US"
                            },
                            new Resource
                            {
                                Name = "bar",
                                Properties = null,
                                ProvisioningState = ProvisioningState.Running,
                                ResourceGroup = parameters.ResourceGroupName,
                                Location = "West US"
                            }
                        })
                    
                }));


            List<PSResource> result = resourcesClient.FilterPSResources(parameters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.False(result.Any(r => r.ParameterObject != null));
        }

        [Fact]
        public void GetResourceWithIncorrectTypeThrowsException()
        {
            BasePSResourceParameters parameters = new BasePSResourceParameters()
            {
                Name = resourceIdentity.ResourceName,
                ParentResourceName = resourceIdentity.ParentResourcePath,
                ResourceGroupName = resourceGroupName,
                ResourceType = "abc",
            };

            Assert.Throws<ArgumentException>(() => resourcesClient.FilterPSResources(parameters));
        }

        [Fact]
        public void NewResourceGroupFailsWithInvalidDeployment()
        {
            Uri templateUri = new Uri("http://templateuri.microsoft.com");
            BasicDeployment deploymentFromGet = new BasicDeployment();
            BasicDeployment deploymentFromValidate = new BasicDeployment();
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                ResourceGroupName = resourceGroupName,
                Location = resourceGroupLocation,
                Name = deploymentName,
                TemplateFile = templateFile,
                ParameterFile = parameterFile,
                StorageAccountName = storageAccountName
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.ResourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.ResourceGroupName,
                It.IsAny<BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.ResourceGroupName, Location = parameters.Location }
                    }));
            resourceGroupMock.Setup(f => f.GetAsync(resourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = new ResourceGroup() { Location = resourceGroupLocation }
                }));
            storageClientWrapperMock.Setup(f => f.UploadFileToBlob(It.IsAny<BlobUploadParameters>())).Returns(templateUri);
            deploymentsMock.Setup(f => f.CreateOrUpdateAsync(resourceGroupName, deploymentName, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsCreateResult
                {
                    RequestId = requestId
                }))
                .Callback((string name, string dName, BasicDeployment bDeploy, CancellationToken token) => { deploymentFromGet = bDeploy; });
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Deployment = new Deployment
                        {
                            DeploymentName = deploymentName,
                            Properties = new DeploymentProperties()
                            {
                                Mode = DeploymentMode.Incremental,
                                ProvisioningState = ProvisioningState.Succeeded
                            },
                            ResourceGroup = resourceGroupName
                        }
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
            SetupListForResourceGroupAsync(parameters.ResourceGroupName, new List<Resource>() { new Resource() { Name = "website"} });
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
        public void NewResourceGroupWithDeploymentSucceeds()
        {
            Uri templateUri = new Uri("http://templateuri.microsoft.com");
            BasicDeployment deploymentFromGet = new BasicDeployment();
            BasicDeployment deploymentFromValidate = new BasicDeployment();
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                ResourceGroupName = resourceGroupName,
                Location = resourceGroupLocation,
                Name = deploymentName,
                TemplateFile = templateFile,
                ParameterFile = parameterFile,
                StorageAccountName = storageAccountName
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.ResourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.ResourceGroupName,
                It.IsAny<BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.ResourceGroupName, Location = parameters.Location }
                    }));
            resourceGroupMock.Setup(f => f.GetAsync(resourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = new ResourceGroup() { Location = resourceGroupLocation }
                }));
            storageClientWrapperMock.Setup(f => f.UploadFileToBlob(It.IsAny<BlobUploadParameters>())).Returns(templateUri);
            deploymentsMock.Setup(f => f.CreateOrUpdateAsync(resourceGroupName, deploymentName, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsCreateResult
                {
                    RequestId = requestId
                }))
                .Callback((string name, string dName, BasicDeployment bDeploy, CancellationToken token) => { deploymentFromGet = bDeploy; });
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Deployment = new Deployment
                        {
                            DeploymentName = deploymentName,
                            Properties = new DeploymentProperties()
                            {
                                Mode = DeploymentMode.Incremental,
                                TrackingId = "123",
                                ProvisioningState = ProvisioningState.Succeeded
                            },
                            ResourceGroup = resourceGroupName
                        }
                }));
            deploymentsMock.Setup(f => f.ValidateAsync(resourceGroupName, DeploymentValidationMode.Full, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentValidateResponse
                {
                    Errors = new List<ResourceManagementError>()
                }))
                .Callback((string rg, DeploymentValidationMode m, BasicDeployment d, CancellationToken c) => { deploymentFromValidate = d; });
            SetupListForResourceGroupAsync(parameters.ResourceGroupName, new List<Resource>() { new Resource() { Name = "website" } });
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

            deploymentsMock.Verify((f => f.CreateOrUpdateAsync(resourceGroupName, deploymentName, deploymentFromGet, new CancellationToken())), Times.Once());
            Assert.Equal(parameters.ResourceGroupName, result.ResourceGroupName);
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
        public void NewResourceGroupWithDeploymentFailsWithoutStorageName()
        {
            WindowsAzureProfile.Instance.CurrentSubscription.CurrentStorageAccountName = null;

            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                ResourceGroupName = resourceGroupName,
                Location = resourceGroupLocation,
                Name = deploymentName,
                TemplateFile = templateFile,
                ParameterFile = parameterFile,
                StorageAccountName = null
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.ResourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            Assert.Throws<ArgumentException>(() => resourcesClient.CreatePSResourceGroup(parameters));
            deploymentsMock.Verify((f => f.CreateOrUpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BasicDeployment>(), new CancellationToken())), Times.Never());
            resourceGroupMock.Verify((f => f.CreateOrUpdateAsync(It.IsAny<string>(), It.IsAny<BasicResourceGroup>(), new CancellationToken())), Times.Never());
        }

        [Fact]
        public void NewResourceGroupWithDeploymentFailsWithExistingGroup()
        {
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                ResourceGroupName = resourceGroupName,
                Location = resourceGroupLocation,
                Name = deploymentName,
                TemplateFile = templateFile,
                ParameterFile = parameterFile,
                StorageAccountName = storageAccountName
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.ResourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = true
                }));

            Assert.Throws<ArgumentException>(()=>resourcesClient.CreatePSResourceGroup(parameters));
        }

        [Fact]
        public void CreatesResourceGroupWithDeploymentFromParameterObject()
        {
            Uri templateUri = new Uri("http://templateuri.microsoft.com");
            BasicDeployment deploymentFromGet = new BasicDeployment();
            BasicDeployment deploymentFromValidate = new BasicDeployment();
            CreatePSResourceGroupParameters parameters = new CreatePSResourceGroupParameters()
            {
                ResourceGroupName = resourceGroupName,
                Location = resourceGroupLocation,
                Name = deploymentName,
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
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.ResourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.ResourceGroupName,
                It.IsAny<BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.ResourceGroupName, Location = parameters.Location }
                    }));
            resourceGroupMock.Setup(f => f.GetAsync(resourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = new ResourceGroup() { Location = resourceGroupLocation }
                }));
            storageClientWrapperMock.Setup(f => f.UploadFileToBlob(It.IsAny<BlobUploadParameters>())).Returns(templateUri);
            deploymentsMock.Setup(f => f.CreateOrUpdateAsync(resourceGroupName, deploymentName, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsCreateResult
                {
                    RequestId = requestId
                }))
                .Callback((string name, string dName, BasicDeployment bDeploy, CancellationToken token) => { deploymentFromGet = bDeploy; });
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Deployment = new Deployment
                        {
                            DeploymentName = deploymentName,
                            Properties = new DeploymentProperties()
                            {
                                Mode = DeploymentMode.Incremental,
                                ProvisioningState = ProvisioningState.Succeeded
                            },
                            ResourceGroup = resourceGroupName
                        }
                }));
            deploymentsMock.Setup(f => f.ValidateAsync(resourceGroupName, DeploymentValidationMode.Full, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentValidateResponse
                {
                    Errors = new List<ResourceManagementError>()
                }))
                .Callback((string rg, DeploymentValidationMode m, BasicDeployment d, CancellationToken c) => { deploymentFromValidate = d; });
            SetupListForResourceGroupAsync(parameters.ResourceGroupName, new List<Resource>() { new Resource() { Name = "website" } });
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

            deploymentsMock.Verify((f => f.CreateOrUpdateAsync(resourceGroupName, deploymentName, deploymentFromGet, new CancellationToken())), Times.Once());
            Assert.Equal(parameters.ResourceGroupName, result.ResourceGroupName);
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
                ResourceGroupName = resourceGroupName,
                Location = resourceGroupLocation,
                Name = deploymentName,
                TemplateFile = templateFile,
                ParameterFile = parameterFile,
                StorageAccountName = storageAccountName
            };
            resourceGroupMock.Setup(f => f.CheckExistenceAsync(parameters.ResourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupExistsResult
                {
                    Exists = false
                }));

            resourceGroupMock.Setup(f => f.CreateOrUpdateAsync(
                parameters.ResourceGroupName,
                It.IsAny<BasicResourceGroup>(),
                new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ResourceGroupCreateOrUpdateResult
                    {
                        ResourceGroup = new ResourceGroup() { Name = parameters.ResourceGroupName, Location = parameters.Location }
                    }));
            resourceGroupMock.Setup(f => f.GetAsync(resourceGroupName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                {
                    ResourceGroup = new ResourceGroup() { Location = resourceGroupLocation }
                }));
            storageClientWrapperMock.Setup(f => f.UploadFileToBlob(It.IsAny<BlobUploadParameters>())).Returns(templateUri);
            deploymentsMock.Setup(f => f.CreateOrUpdateAsync(resourceGroupName, deploymentName, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentOperationsCreateResult
                {
                    RequestId = requestId
                }))
                .Callback((string name, string dName, BasicDeployment bDeploy, CancellationToken token) => { deploymentFromGet = bDeploy; });
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Deployment = new Deployment
                        {
                            DeploymentName = deploymentName,
                            Properties = new DeploymentProperties()
                            {
                                Mode = DeploymentMode.Incremental,
                                ProvisioningState = ProvisioningState.Succeeded
                            },
                            ResourceGroup = resourceGroupName
                        }
                }));
            deploymentsMock.Setup(f => f.ValidateAsync(resourceGroupName, DeploymentValidationMode.Full, It.IsAny<BasicDeployment>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentValidateResponse
                {
                    Errors = new List<ResourceManagementError>()
                }))
                .Callback((string rg, DeploymentValidationMode m, BasicDeployment d, CancellationToken c) => { deploymentFromValidate = d; });
            SetupListForResourceGroupAsync(parameters.ResourceGroupName, new List<Resource>() { new Resource() { Name = "website" } });
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

            deploymentsMock.Verify((f => f.CreateOrUpdateAsync(resourceGroupName, deploymentName, deploymentFromGet, new CancellationToken())), Times.Once());
            Assert.Equal(parameters.ResourceGroupName, result.ResourceGroupName);
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
        public void GetAzureResourceGroupLogWithAllCallsListEventsForResourceGroup()
        {
            eventDataOperationsMock.Setup(f => f.ListEventsForResourceGroupAsync(It.IsAny<ListEventsForResourceGroupParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new EventDataListResponse
                    {
                        EventDataCollection = new EventDataCollection
                            {
                                Value = sampleEvents
                            }
                    }));

            IEnumerable<PSDeploymentEventData> results = resourcesClient.GetResourceGroupLogs(new GetPSResourceGroupLogParameters
                {
                    Name = "foo",
                    All = true
                });

            Assert.Equal(2, results.Count());
            eventDataOperationsMock.Verify(f => f.ListEventsForResourceGroupAsync(It.IsAny<ListEventsForResourceGroupParameters>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void GetAzureResourceGroupLogWithDeploymentCallsListEventsForCorrelationId()
        {
            deploymentsMock.Setup(
                f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                           .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                               {
                                   Deployment = new Deployment()
                                        {
                                            DeploymentName = deploymentName + 1,
                                            Properties = new DeploymentProperties()
                                                {
                                                    Mode = DeploymentMode.Incremental,
                                                    TrackingId = "123",
                                                    TemplateLink = new TemplateLink()
                                                        {
                                                            Uri = new Uri("http://microsoft1.com")
                                                        }
                                                },
                                            ResourceGroup = resourceGroupName
                                        }
                               }));

            eventDataOperationsMock.Setup(f => f.ListEventsForCorrelationIdAsync(It.IsAny<ListEventsForCorrelationIdParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new EventDataListResponse
                {
                    EventDataCollection = new EventDataCollection
                    {
                        Value = sampleEvents
                    }
                }));

            IEnumerable<PSDeploymentEventData> results = resourcesClient.GetResourceGroupLogs(new GetPSResourceGroupLogParameters
            {
                Name = resourceGroupName,
                DeploymentName = deploymentName
            });

            Assert.Equal(2, results.Count());
            deploymentsMock.Verify(f => f.GetAsync(resourceGroupName, deploymentName, It.IsAny<CancellationToken>()), Times.Once());
            eventDataOperationsMock.Verify(f => f.ListEventsForCorrelationIdAsync(It.IsAny<ListEventsForCorrelationIdParameters>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void GetAzureResourceGroupLogWithLastDeploymentCallsListEventsForCorrelationId()
        {
            deploymentsMock.Setup(
                f => f.ListAsync(resourceGroupName, It.IsAny<DeploymentListParameters>(), new CancellationToken()))
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
                                                           TrackingId = "123",
                                                           TemplateLink = new TemplateLink()
                                                               {
                                                                   Uri = new Uri("http://microsoft1.com")
                                                               }
                                                       },
                                                   ResourceGroup = resourceGroupName
                                               }
                                       }
                           }));

            eventDataOperationsMock.Setup(f => f.ListEventsForCorrelationIdAsync(It.IsAny<ListEventsForCorrelationIdParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new EventDataListResponse
                {
                    EventDataCollection = new EventDataCollection
                    {
                        Value = sampleEvents
                    }
                }));

            IEnumerable<PSDeploymentEventData> results = resourcesClient.GetResourceGroupLogs(new GetPSResourceGroupLogParameters
            {
                Name = resourceGroupName,
                LastDeployment = true
            });

            Assert.Equal(2, results.Count());
            deploymentsMock.Verify(f => f.ListAsync(resourceGroupName, It.IsAny<DeploymentListParameters>(), It.IsAny<CancellationToken>()), Times.Once());
            eventDataOperationsMock.Verify(f => f.ListEventsForCorrelationIdAsync(It.IsAny<ListEventsForCorrelationIdParameters>(), It.IsAny<CancellationToken>()), Times.Once());
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
            FilterResourceGroupDeploymentOptions options = new FilterResourceGroupDeploymentOptions()
            {
                DeploymentName = deploymentName,
                ResourceGroupName = resourceGroupName
            };
            deploymentsMock.Setup(f => f.GetAsync(resourceGroupName, deploymentName, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new DeploymentGetResult
                {
                    Deployment = new Deployment
                        {
                            DeploymentName = deploymentName,
                            Properties = new DeploymentProperties()
                            {
                                Mode = DeploymentMode.Incremental,
                                TrackingId = "123",
                                TemplateLink = new TemplateLink()
                                {
                                    Uri = new Uri("http://microsoft.com")
                                }
                            },
                            ResourceGroup = resourceGroupName
                        }
                }));

            List<PSResourceGroupDeployment> result = resourcesClient.FilterResourceGroupDeployments(options);

            Assert.Equal(deploymentName, result[0].DeploymentName);
            Assert.Equal(resourceGroupName, result[0].ResourceGroupName);
            Assert.Equal(DeploymentMode.Incremental, result[0].Mode);
            Assert.Equal("123", result[0].TrackingId);
            Assert.Equal(new Uri("http://microsoft.com").ToString(), result[0].TemplateLink.Uri.ToString());
        }

        [Fact]
        public void FiltersResourceGroupDeployments()
        {
            FilterResourceGroupDeploymentOptions options = new FilterResourceGroupDeploymentOptions()
            {
                ResourceGroupName = resourceGroupName
            };
            DeploymentListParameters actualParameters = new DeploymentListParameters();
            deploymentsMock.Setup(f => f.ListAsync(
                resourceGroupName,
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
                                TrackingId = "123",
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
                .Callback((string rgn, DeploymentListParameters p, CancellationToken t) => { actualParameters = p; });

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
                                TrackingId = "456",
                                TemplateLink = new TemplateLink()
                                {
                                    Uri = new Uri("http://microsoft2.com")
                                }
                            },
                            ResourceGroup = resourceGroupName
                        }
                    }
                }));

            List<PSResourceGroupDeployment> result = resourcesClient.FilterResourceGroupDeployments(options);

            Assert.Equal(2, result.Count);
            Assert.Equal(deploymentName + 1, result[0].DeploymentName);
            Assert.Equal("123", result[0].TrackingId);
            Assert.Equal(resourceGroupName, result[0].ResourceGroupName);
            Assert.Equal(DeploymentMode.Incremental, result[0].Mode);
            Assert.Equal(new Uri("http://microsoft1.com").ToString(), result[0].TemplateLink.Uri.ToString());

            Assert.Equal(deploymentName + 2, result[1].DeploymentName);
            Assert.Equal(resourceGroupName, result[1].ResourceGroupName);
            Assert.Equal("456", result[1].TrackingId);
            Assert.Equal(DeploymentMode.Incremental, result[1].Mode);
            Assert.Equal(new Uri("http://microsoft2.com").ToString(), result[1].TemplateLink.Uri.ToString());
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

        [Fact]
        public void CancelsActiveDeployment()
        {
            DeploymentListParameters actualParameters = new DeploymentListParameters();
            deploymentsMock.Setup(f => f.ListAsync(
                resourceGroupName,
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
                                },
                                ProvisioningState = ProvisioningState.Succeeded
                            },
                            ResourceGroup = resourceGroupName
                        },
                        new Deployment()
                        {
                            DeploymentName = deploymentName + 2,
                            Properties = new DeploymentProperties()
                            {
                                Mode = DeploymentMode.Incremental,
                                TemplateLink = new TemplateLink()
                                {
                                    Uri = new Uri("http://microsoft1.com")
                                },
                                ProvisioningState = ProvisioningState.Failed
                            },
                            ResourceGroup = resourceGroupName
                        },
                        new Deployment()
                        {
                            DeploymentName = deploymentName + 3,
                            Properties = new DeploymentProperties()
                            {
                                Mode = DeploymentMode.Incremental,
                                TemplateLink = new TemplateLink()
                                {
                                    Uri = new Uri("http://microsoft1.com")
                                },
                                ProvisioningState = ProvisioningState.Running
                            },
                            ResourceGroup = resourceGroupName
                        }
                    }
                }))
                .Callback((string rgn, DeploymentListParameters p, CancellationToken t) => { actualParameters = p; });

            resourcesClient.CancelDeployment(resourceGroupName, null);

            deploymentsMock.Verify(f => f.CancelAsync(resourceGroupName, deploymentName + 3, new CancellationToken()), Times.Once());
        }

        [Fact]
        public void FiltersGalleryTemplates()
        {
            string filterString = FilterString.Generate<ItemListFilter>(f => f.Publisher == "Microsoft");
            ItemListParameters actual = new ItemListParameters();
            galleryClientMock.Setup(f => f.Items.ListAsync(It.IsAny<ItemListParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ItemListResult
                {
                    Items = new List<GalleryItem>()
                    {
                        new GalleryItem()
                        {
                            Name = "Template1",
                            Publisher = "Microsoft"
                        },
                        new GalleryItem()
                        {
                            Name = "Template2",
                            Publisher = "Microsoft"
                        }
                    }
                }))
                .Callback((ItemListParameters p, CancellationToken c) => actual = p);

            FilterGalleryTemplatesOptions options = new FilterGalleryTemplatesOptions()
            {
                Publisher = "Microsoft"
            };

            List<GalleryItem> result = resourcesClient.FilterGalleryTemplates(options);

            Assert.Equal(2, result.Count);
            Assert.True(result.All(g => g.Publisher == "Microsoft"));
            Assert.Equal(filterString, actual.Filter);
        }

        [Fact]
        public void FiltersGalleryTemplatesUsingComplexQuery()
        {
            string filterString = "Publisher eq 'Microsoft' and CategoryIds/any(c: c eq 'awesome') and ItemName eq 'hello world'";
            ItemListParameters actual = new ItemListParameters();
            galleryClientMock.Setup(f => f.Items.ListAsync(It.IsAny<ItemListParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ItemListResult
                {
                    Items = new List<GalleryItem>()
                    {
                        new GalleryItem()
                        {
                            Name = "Template1",
                            Publisher = "Microsoft"
                        },
                        new GalleryItem()
                        {
                            Name = "Template2",
                            Publisher = "Microsoft"
                        }
                    }
                }))
                .Callback((ItemListParameters p, CancellationToken c) => actual = p);

            FilterGalleryTemplatesOptions options = new FilterGalleryTemplatesOptions()
            {
                Publisher = "Microsoft",
                Category = "awesome",
                Name = "hello world"
            };

            List<GalleryItem> result = resourcesClient.FilterGalleryTemplates(options);

            Assert.Equal(2, result.Count);
            Assert.Equal(filterString, actual.Filter);
        }

        [Fact]
        public void DownloadsGalleryTemplateFile()
        {
            string galleryTemplateFileName = "myFile";
            string expectedFilePath = Path.Combine(Path.GetTempPath(), galleryTemplateFileName + ".json");
            try
            {
                galleryClientMock.Setup(f => f.Items.GetAsync(galleryTemplateFileName, new CancellationToken()))
                                 .Returns(Task.Factory.StartNew(() => new ItemGetParameters()
                                     {
                                         Item = new GalleryItem()
                                             {
                                                 Name = galleryTemplateFileName,
                                                 Publisher = "Microsoft",
                                                 DefinitionTemplates = new DefinitionTemplates()
                                                     {
                                                         DeploymentTemplateFileUrls = new Dictionary<string, string>()
                                                             {
                                                                 {"DefaultUri", "fakeurl"}
                                                             }
                                                     }
                                             }
                                     }));

                resourcesClient.DownloadGalleryTemplateFile(
                    galleryTemplateFileName,
                    expectedFilePath);

                Assert.Equal(string.Empty, File.ReadAllText(expectedFilePath));
            }
            finally
            {
                File.Delete(expectedFilePath);
            }
        }

        [Fact]
        public void DownloadsGalleryTemplateFileFromDirectoryName()
        {
            string galleryTemplateFileName = "myFile";
            string expectedFilePath = Path.Combine(Path.GetTempPath(), galleryTemplateFileName + ".json");
            try
            {
                galleryClientMock.Setup(f => f.Items.GetAsync(galleryTemplateFileName, new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ItemGetParameters()
                    {
                        Item = new GalleryItem()
                        {
                            Name = galleryTemplateFileName,
                            Publisher = "Microsoft",
                            DefinitionTemplates = new DefinitionTemplates()
                            {
                                DeploymentTemplateFileUrls = new Dictionary<string, string>()
                                {
                                    { "DefaultUri", "fakeurl" }
                                }
                            }
                        }
                    }));

                resourcesClient.DownloadGalleryTemplateFile(
                    galleryTemplateFileName,
                    Path.GetTempPath());

                Assert.Equal(string.Empty, File.ReadAllText(expectedFilePath));
            }
            finally
            {
                File.Delete(expectedFilePath);
            }
        }

        [Fact]
        public void DownloadsGalleryTemplateFileFromFileName()
        {
            string galleryTemplateFileName = "myFile.adeek";
            string expectedFilePath = Path.Combine(Path.GetTempPath(), galleryTemplateFileName + ".adeek");
            try
            {
                galleryClientMock.Setup(f => f.Items.GetAsync(galleryTemplateFileName, new CancellationToken()))
                                 .Returns(Task.Factory.StartNew(() => new ItemGetParameters()
                                     {
                                         Item = new GalleryItem()
                                             {
                                                 Name = galleryTemplateFileName,
                                                 Publisher = "Microsoft",
                                                 DefinitionTemplates = new DefinitionTemplates()
                                                     {
                                                         DeploymentTemplateFileUrls = new Dictionary<string, string>()
                                                             {
                                                                 {"DefaultUri", "http://onesdkauremustinvalid-uri12"}
                                                             }
                                                     }
                                             }
                                     }));

                resourcesClient.DownloadGalleryTemplateFile(
                    galleryTemplateFileName,
                    expectedFilePath);

                Assert.Equal(string.Empty, File.ReadAllText(expectedFilePath));
            }
            finally
            {
                File.Delete(expectedFilePath);
            }
        }

        [Fact]
        public void GetsLocations()
        {
            providersMock.Setup(f => f.ListAsync(null, new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ProviderListResult()
                {
                    Providers = new List<Provider>()
                    {
                        new Provider()
                        {
                            Namespace = "Microsoft.Web",
                            RegistrationState = "Registered",
                            ResourceTypes = new List<ProviderResourceType>()
                            {
                                new ProviderResourceType()
                                {
                                    Locations = new List<string>() {"West US", "East US"},
                                    Name = "database"
                                },
                                new ProviderResourceType()
                                {
                                    Locations = new List<string>() {"West US", "South Central US"},
                                    Name = "servers"
                                }
                            }
                        },
                        new Provider()
                        {
                            Namespace = "Microsoft.HDInsight",
                            RegistrationState = "UnRegistered",
                            ResourceTypes = new List<ProviderResourceType>()
                            {
                                new ProviderResourceType()
                                {
                                    Locations = new List<string>() {"West US", "East US"},
                                    Name = "hadoop"
                                },
                                new ProviderResourceType()
                                {
                                    Locations = new List<string>() {"West US", "South Central US"},
                                    Name = "websites"
                                }
                            }
                        }
                    }
                }));
            List<PSResourceProviderType> resourceTypes = resourcesClient.GetLocations(
                ResourcesClient.ResourceGroupTypeName,
                "Microsoft.HDInsight");

            Assert.Equal(3, resourceTypes.Count);
            Assert.Equal(ResourcesClient.ResourceGroupTypeName, resourceTypes[0].Name);
            Assert.Equal(ResourcesClient.KnownLocations.Count, resourceTypes[0].Locations.Count);
            Assert.Equal("East Asia", resourceTypes[0].Locations[0]);
            Assert.Equal("Microsoft.HDInsight/hadoop", resourceTypes[1].Name);
        }
    }
}

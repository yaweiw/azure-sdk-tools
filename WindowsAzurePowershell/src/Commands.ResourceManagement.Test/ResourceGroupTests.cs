﻿// ----------------------------------------------------------------------------------
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

using Microsoft.Azure.Commands.ResourceManagement.ResourceGroups;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Commands.ResourceManagement.Test
{
    [TestClass]
    public class ResourceGroupTests : TestBase
    {
        private Mock<IResourceManagementClient> _resourceMgmtClientMock;
        private Mock<IResourceGroupOperations> _resourceGroupOperationMock;
        private MockCommandRuntime _mockCommandRuntime;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            _mockCommandRuntime = new MockCommandRuntime();
            _resourceMgmtClientMock = new Mock<IResourceManagementClient>();
            _resourceGroupOperationMock = new Mock<IResourceGroupOperations>();
            _resourceMgmtClientMock.Setup(rmc => rmc.ResourceGroups).Returns(_resourceGroupOperationMock.Object);
            _resourceGroupOperationMock.Setup(rgo => rgo.GetAsync(null, new CancellationToken()))
                                       .Returns(() => { throw new ArgumentNullException(); });
        }

        private void SetupGetFooReturnsFoo()
        {
            _resourceGroupOperationMock.Setup(rgo => rgo.GetAsync("foo", new CancellationToken()))
                                       .Returns(Task.Factory.StartNew(() => new ResourceGroupGetResult
                                       {
                                           RequestId = "abc",
                                           StatusCode = System.Net.HttpStatusCode.OK,
                                           ResourceGroup = new ResourceGroup
                                           {
                                               Name = "foo",
                                               Location = "EastUS"
                                           }
                                       }));
        }

        private void SetupGetFooReturnsNull()
        {
            _resourceGroupOperationMock.Setup(rgo => rgo.GetAsync("foo", new CancellationToken()))
                                       .Returns(Task.Factory.StartNew<ResourceGroupGetResult>(() => null));
        }

        private void SetupListAllReturnsThreeValues()
        {
            _resourceGroupOperationMock.Setup(rgo => rgo.ListAsync(null, new CancellationToken()))
                                       .Returns(Task.Factory.StartNew(() => new ResourceGroupListResult
                                       {
                                           RequestId = "abc",
                                           StatusCode = System.Net.HttpStatusCode.OK,
                                           ResourceGroups = new[]
                                                   {
                                                       new ResourceGroup
                                                           {
                                                               Name = "foo",
                                                               Location = "EastUS"
                                                           },
                                                       new ResourceGroup
                                                           {
                                                               Name = "bar",
                                                               Location = "WestUS"
                                                           }
                                                   }
                                       }));
        }

        [TestMethod]
        public void GetResourceGroupReturnsList()
        {
            using (var files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var getAzureResourceGroup = InitializeGetAzureResourceGroupCommandlet();
                SetupListAllReturnsThreeValues();

                getAzureResourceGroup.ExecuteCmdlet();
                var listOfGroups = _mockCommandRuntime.OutputPipeline[0] as List<ResourceGroup>;

                Assert.IsNotNull(listOfGroups);
                Assert.AreEqual(2, listOfGroups.Count);
                _resourceGroupOperationMock.Verify(f => f.ListAsync(null, new CancellationToken()), Times.Once());
            }
        }

        [TestMethod]
        public void GetResourceGroupReturnsOneValue()
        {
            using (var files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var getAzureResourceGroup = InitializeGetAzureResourceGroupCommandlet();
                SetupListAllReturnsThreeValues();
                SetupGetFooReturnsFoo();

                getAzureResourceGroup.Name = "foo";

                getAzureResourceGroup.ExecuteCmdlet();
                var pipelineValue = _mockCommandRuntime.OutputPipeline[0] as ResourceGroup;

                Assert.IsNotNull(pipelineValue);
                Assert.AreEqual("foo", pipelineValue.Name);
                Assert.AreEqual("EastUS", pipelineValue.Location);
                _resourceGroupOperationMock.Verify(f => f.GetAsync("foo", new CancellationToken()), Times.Once());
            }
        }

        [TestMethod]
        public void AddResourceGroupWithoutParametersReturnsError()
        {
            using (var files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                WindowsAzureProfile.Instance.CurrentSubscription = new WindowsAzureSubscription();
                var newAzureResourceGroup = InitializeNewAzureResourceGroupCommandlet();

                newAzureResourceGroup.ExecuteCmdlet();

                Assert.AreEqual(0, _mockCommandRuntime.OutputPipeline.Count);
                Assert.AreEqual(1, _mockCommandRuntime.ErrorStream.Count);
                _resourceGroupOperationMock.Verify(f => f.ListAsync(null, new CancellationToken()), Times.Once());
            }
        }

        [TestMethod]
        public void AddResourceGroupCreatesGroup()
        {
            using (var files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var newAzureResourceGroup = InitializeNewAzureResourceGroupCommandlet();
                SetupGetFooReturnsNull();

                newAzureResourceGroup.Name = "foo";
                newAzureResourceGroup.Location = "WestUS";

                newAzureResourceGroup.ExecuteCmdlet();
                var listOfGroups = _mockCommandRuntime.OutputPipeline[0] as List<ResourceGroup>;

                Assert.IsNotNull(listOfGroups);
                Assert.AreEqual(2, listOfGroups.Count);
                _resourceGroupOperationMock.Verify(f => f.ListAsync(null, new CancellationToken()), Times.Once());
            }
        }

        [TestMethod]
        public void AddResourceGroupThrowsExceptionIfGroupAlreadyExists()
        {
            using (var files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                var newAzureResourceGroup = InitializeNewAzureResourceGroupCommandlet();
                SetupGetFooReturnsFoo();

                newAzureResourceGroup.Name = "foo";
                newAzureResourceGroup.Location = "WestUS";

                newAzureResourceGroup.ExecuteCmdlet();

                Assert.AreEqual(1, _mockCommandRuntime.ErrorStream.Count);
            }
        }

        private GetAzureResourceGroup InitializeGetAzureResourceGroupCommandlet()
        {
            var getAzureResourceGroup = new GetAzureResourceGroup
            {
                CommandRuntime = _mockCommandRuntime,
                CurrentSubscription = WindowsAzureProfile.Instance.CurrentSubscription
            };
            getAzureResourceGroup.CurrentSubscription.CloudServiceEndpoint = new Uri("http://foo");
            getAzureResourceGroup.ResourceClient.ResourceManagementClient = _resourceMgmtClientMock.Object;
            return getAzureResourceGroup;
        }

        private NewAzureResourceGroup InitializeNewAzureResourceGroupCommandlet()
        {
            var newAzureResourceGroup = new NewAzureResourceGroup
            {
                CommandRuntime = _mockCommandRuntime,
                CurrentSubscription = WindowsAzureProfile.Instance.CurrentSubscription
            };
            newAzureResourceGroup.CurrentSubscription.CloudServiceEndpoint = new Uri("http://foo");
            newAzureResourceGroup.ResourceClient.ResourceManagementClient = _resourceMgmtClientMock.Object;
            return newAzureResourceGroup;
        }
    }
}

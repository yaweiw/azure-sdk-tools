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

namespace Microsoft.WindowsAzure.Management.Storage.Test.Blob
{
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Storage.Blob.ResourceModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Storage.Blob;
    using Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [TestClass]
    public class NewAzureStorageContainerTest : StorageBlobTestBase
    {
        public NewAzureStorageContainerCommand command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new NewAzureStorageContainerCommand
                {
                    BlobClient = BlobMock,
                    CommandRuntime = new MockCommandRuntime()
                };
        }

        [TestCleanup]
        public void CleanCommand()
        {
            command = null;
        }

        [TestMethod]
        public void CreateContainerWithInvalidContainerNameTest()
        {
            string name = String.Empty;
            AssertThrows<ArgumentException>(() => command.CreateAzureContainer(name),
                String.Format(Resources.InvalidContainerName, name));

            name = "a";
            AssertThrows<ArgumentException>(() => command.CreateAzureContainer(name),
                String.Format(Resources.InvalidContainerName, name));

            name = "&*(";
            AssertThrows<ArgumentException>(() => command.CreateAzureContainer(name),
                String.Format(Resources.InvalidContainerName, name));
        }

        [TestMethod]
        public void CreateContainerForAlreadyExistsContainerTest()
        {
            AddTestContainers();
            string name = "text";
            AssertThrows<ResourceAlreadyExistException>(() => command.CreateAzureContainer(name),
                String.Format(Resources.ContainerAlreadyExists, name));
        }

        [TestMethod]
        public void CreateContainerSuccessfullyTest()
        {
            string name = String.Empty;

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            name = "test";
            command.CreateAzureContainer(name);
            AzureStorageContainer container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.AreEqual("test", container.Name);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            AssertThrows<ResourceAlreadyExistException>(() => command.CreateAzureContainer(name),
                String.Format(Resources.ContainerAlreadyExists, name));
        }

        [TestMethod]
        public void ExcuteCommandNewContainerTest()
        {
            string name = "containername";
            command.Name = name;
            command.ExecuteCmdlet();
            AzureStorageContainer container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.AreEqual(name, container.Name);
        }
    }
}

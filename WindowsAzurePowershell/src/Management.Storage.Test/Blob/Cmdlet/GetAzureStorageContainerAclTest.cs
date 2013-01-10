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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Storage.Blob;
    using Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.ResourceModel;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// unit test for Get-AzureStorageContainerAcl
    /// </summary>
    [TestClass]
    public class GetAzureStorageContainerAclTest : StorageBlobTestBase
    {
        /// <summary>
        /// get azure storage container acl command
        /// </summary>
        public GetAzureStorageContainerAclCommand command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new GetAzureStorageContainerAclCommand(BlobMock)
            {
                CommandRuntime = new MockCommandRuntime()
            };
        }

        [TestCleanup]
        public void CleanCommand()
        {
            command = null;
        }

        [TestMethod]
        public void GetContainerAclWithInvalidContainerNameTest()
        {
            string name = "a";
            AssertThrows<ArgumentException>(() => command.GetContainerAcl(name), String.Format(Resources.InvalidContainerName, name));
        }

        [TestMethod]
        public void GetContainerAclForNotExistsContainerTest()
        {
            string name = "test";
            AssertThrows<ResourceNotFoundException>(() => command.GetContainerAcl(name), String.Format(Resources.ContainerNotFound, name));
        }

        [TestMethod]
        public void GetContainerAclSuccessfullyTest()
        {
            AddTestContainers();
            string name = string.Empty;

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            name = "test";
            command.GetContainerAcl(name);
            AzureStorageContainer container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(BlobContainerPublicAccessType.Off, container.PublicAccess);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            name = "publicoff";
            command.GetContainerAcl(name);
            container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(BlobContainerPublicAccessType.Off, container.PublicAccess);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            name = "publicblob";
            command.GetContainerAcl(name);
            container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(BlobContainerPublicAccessType.Blob, container.PublicAccess);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            name = "publiccontainer";
            command.GetContainerAcl(name);
            container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(BlobContainerPublicAccessType.Container, container.PublicAccess);
        }

        [TestMethod]
        public void ExecuteCommandGetContainerAclTest()
        {
            string name = "test";
            command.Name = name;
            AssertThrows<ResourceNotFoundException>(() => command.ExecuteCmdlet(), String.Format(Resources.ContainerNotFound, name));

            AddTestContainers();

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.ExecuteCmdlet();
            AzureStorageContainer container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(BlobContainerPublicAccessType.Off, container.PublicAccess);
        }
    }
}

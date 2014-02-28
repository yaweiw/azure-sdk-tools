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

namespace Microsoft.WindowsAzure.Commands.Storage.Test.Blob.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Model.ResourceModel;
    using Storage.Blob.Cmdlet;
    using Storage.Common;

    /// <summary>
    /// Unit test for get azure storage container cmdlet
    /// </summary>
    [TestClass]
    public class GetAzureStorageContainerTest : StorageBlobTestBase
    {
        /// <summary>
        /// Get azure storage container command
        /// </summary>
        private GetAzureStorageContainerCommand command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new GetAzureStorageContainerCommand(BlobMock)
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
        public void ListContainersByNameWithInvalidNameTest()
        {
            string invalidName = "a";
            AssertThrows<ArgumentException>(() => command.ListContainersByName(invalidName).ToList(),
                String.Format(Resources.InvalidContainerName, invalidName));
            invalidName = "xx%%d";
            AssertThrows<ArgumentException>(() => command.ListContainersByName(invalidName).ToList(),
                String.Format(Resources.InvalidContainerName, invalidName));
        }

        [TestMethod]
        public void ListContainersByNameWithContainerNameTest()
        {
            AddTestContainers();
            IEnumerable<Tuple<CloudBlobContainer, BlobContinuationToken>> containerList = command.ListContainersByName("text");
            Assert.AreEqual(1, containerList.Count());
            Assert.AreEqual("text", containerList.First().Item1.Name);
        }

        [TestMethod]
        public void ListContainersByNameWithNotExistingContainerTest()
        {
            string notExistingName = "abcdefg";
            AssertThrows<ResourceNotFoundException>(() => command.ListContainersByName(notExistingName).ToList(),
                String.Format(Resources.ContainerNotFound, notExistingName));
        }

        [TestMethod]
        public void ListContainerByPrefixWithInvalidPrefixTest()
        {
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            string prefix = "?";
            AssertThrows<ArgumentException>(() => command.ListContainersByPrefix(prefix).ToList(), String.Format(Resources.InvalidContainerName, prefix));
        }

        [TestMethod]
        public void PackCloudBlobContainerWithAclTest()
        {
            IEnumerable<AzureStorageContainer> containerList = command.PackCloudBlobContainerWithAcl(null);
            Assert.IsFalse(containerList.Any());

            containerList = command.PackCloudBlobContainerWithAcl(BlobMock.ContainerAndTokenList);
            Assert.IsFalse(containerList.Any());

            AddTestContainers();
            containerList = command.PackCloudBlobContainerWithAcl(BlobMock.ContainerAndTokenList);
            Assert.AreEqual(5, containerList.Count());
        }

        [TestMethod]
        public void ExecuteCommandGetContainerTest()
        {
            AddTestContainers();
            command.Name = "test";
            command.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
        }
    }
}

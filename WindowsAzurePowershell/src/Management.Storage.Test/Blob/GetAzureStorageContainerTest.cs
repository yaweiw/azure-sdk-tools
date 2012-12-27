// ----------------------------------------------------------------------------------
//
// Copyright 2012 Microsoft Corporation
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
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Storage.Test.Service;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Microsoft.WindowsAzure.Management.Storage.Model;

    /// <summary>
    /// Summary description for GetAzureContainerTest
    /// </summary>
    [TestClass]
    public class GetAzureStorageContainerTest : StorageBlobTestBase
    {
        public GetAzureStorageContainerCommand command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new GetAzureStorageContainerCommand
            {
                blobClient = blobMock,
                CommandRuntime = new MockCommandRuntime()
            };
        }

        [TestCleanup]
        public void CleanCommand()
        {
            command = null;
        }

        [TestMethod]
        public void ListContainersByNameTest()
        {
            //list all the azure container
            command.ListContainersByName();
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);

            AddTestContainers();

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.ListContainersByName();
            Assert.AreEqual(5, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.ListContainersByName("te*t");
            Assert.AreEqual(2, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.ListContainersByName("tx*t");
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            string invalidName = "a";
            AssertThrows<ArgumentException>(() => command.ListContainersByName(invalidName), 
                String.Format(Resources.InvalidContainerName, invalidName));
            invalidName = "xx%%d";
            AssertThrows<ArgumentException>(() => command.ListContainersByName(invalidName), 
                String.Format(Resources.InvalidContainerName, invalidName));
            
            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.ListContainersByName("t?st");
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);
            AzureStorageContainer container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.AreEqual("test", container.Name);

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.ListContainersByName("text");
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);
            container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.AreEqual("text", container.Name);

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            string notExistingName = "abcdefg";
            AssertThrows<ResourceNotFoundException>(() => command.ListContainersByName(notExistingName), 
                String.Format(Resources.ContainerNotFound, notExistingName));
        }

        [TestMethod]
        public void ListContainersByPrefixTest()
        {
            command.ListContainersByPrefix("");
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);

            AddTestContainers();
            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.ListContainersByPrefix("te");
            Assert.AreEqual(2, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.ListContainersByPrefix("tes");
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);
            AzureStorageContainer container = (AzureStorageContainer)((MockCommandRuntime)command.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.AreEqual("test", container.Name);

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.ListContainersByPrefix("testx");
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            string prefix = "?";
            AssertThrows<ArgumentException>(() => command.ListContainersByPrefix(prefix), String.Format(Resources.InvalidContainerName, prefix));
        }

        [TestMethod]
        public void WriteContainersWithAclTest()
        {
            command.WriteContainersWithAcl(null);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.WriteContainersWithAcl(blobMock.containerList);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);

            AddTestContainers();
            ((MockCommandRuntime)command.CommandRuntime).Clean();
            command.WriteContainersWithAcl(blobMock.containerList);
            Assert.AreEqual(5, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);
        }

        [TestMethod]
        public void ExecuteCommandGetContainerTest()
        {
            int accountCount = 0;
            AddTestContainers();
            command.Name = "test";
            command.ExecuteCommand();
            Assert.AreEqual(accountCount + 1, ((MockCommandRuntime)command.CommandRuntime).WrittenObjects.Count);
        }
    }
}

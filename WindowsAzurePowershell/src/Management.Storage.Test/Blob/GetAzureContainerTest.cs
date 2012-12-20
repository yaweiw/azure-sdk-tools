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
    public class GetAzureContainerTest : StorageTestBase
    {
        private MockBlobManagement blobMock = null;

        [TestInitialize]
        public void initMock()
        {
            blobMock = new MockBlobManagement();
        }


        private void AddTestContainers()
        {
            blobMock.containerList.Clear();
            string testUri = "http://127.0.0.1/account/test";
            string textUri = "http://127.0.0.1/account/text";
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(testUri)));
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(textUri)));
        }

        [TestMethod]
        public void ListContainersByNameTest()
        {
            //list all the azure container
            GetAzureContainerCommand getContainerCommand = new GetAzureContainerCommand
            {
                blobClient = blobMock,
                CommandRuntime = new MockCommandRuntime()
            };
            getContainerCommand.ListContainersByName();
            Assert.AreEqual(0, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);

            AddTestContainers();
            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            getContainerCommand.ListContainersByName("te*t");
            Assert.AreEqual(2, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            getContainerCommand.ListContainersByName("tx*t");
            Assert.AreEqual(0, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            string invalidName = "a";
            AssertThrows<ArgumentException>(() => getContainerCommand.ListContainersByName(invalidName), 
                String.Format(Resource.InvalidContainerName, invalidName));
            invalidName = "xx%%d";
            AssertThrows<ArgumentException>(() => getContainerCommand.ListContainersByName(invalidName), 
                String.Format(Resource.InvalidContainerName, invalidName));
            
            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            getContainerCommand.ListContainersByName("t?st");
            Assert.AreEqual(1, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);
            AzureContainer container = (AzureContainer)((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.AreEqual("test", container.Name);

            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            getContainerCommand.ListContainersByName("text");
            Assert.AreEqual(1, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);
            container = (AzureContainer)((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.AreEqual("text", container.Name);

            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            string notExistingName = "abcdefg";
            AssertThrows<ResourceNotFoundException>(() => getContainerCommand.ListContainersByName(notExistingName), 
                String.Format(Resource.ContainerNotFound, notExistingName));
        }

        [TestMethod]
        public void ListContainersByPrefixTest()
        {
            GetAzureContainerCommand getContainerCommand = new GetAzureContainerCommand
            {
                blobClient = blobMock,
                CommandRuntime = new MockCommandRuntime()
            };
            getContainerCommand.ListContainersByPrefix("");
            Assert.AreEqual(0, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);

            AddTestContainers();
            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            getContainerCommand.ListContainersByPrefix("te");
            Assert.AreEqual(2, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            getContainerCommand.ListContainersByPrefix("tes");
            Assert.AreEqual(1, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);
            AzureContainer container = (AzureContainer)((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.FirstOrDefault();
            Assert.AreEqual("test", container.Name);

            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            getContainerCommand.ListContainersByPrefix("testx");
            Assert.AreEqual(0, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            string prefix = "?";
            AssertThrows<ArgumentException>(() => getContainerCommand.ListContainersByPrefix(prefix), String.Format(Resource.InvalidContainerName, prefix));
        }

        [TestMethod]
        public void WriteContainersWithAclTest()
        {
            GetAzureContainerCommand getContainerCommand = new GetAzureContainerCommand
            {
                blobClient = blobMock,
                CommandRuntime = new MockCommandRuntime()
            };
            getContainerCommand.WriteContainersWithAcl(null);
            Assert.AreEqual(0, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);

            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            getContainerCommand.WriteContainersWithAcl(blobMock.containerList);
            Assert.AreEqual(0, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);

            AddTestContainers();
            ((MockCommandRuntime)getContainerCommand.CommandRuntime).Clean();
            getContainerCommand.WriteContainersWithAcl(blobMock.containerList);
            Assert.AreEqual(2, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);
        }

        [TestMethod]
        public void ExecuteCommandTest()
        {
            int accountCount = 1;
            GetAzureContainerCommand getContainerCommand = new GetAzureContainerCommand
            {
                blobClient = blobMock,
                CommandRuntime = new MockCommandRuntime()
            };
            AddTestContainers();
            getContainerCommand.Name = "test";
            getContainerCommand.ExecuteCommand();
            Assert.AreEqual(accountCount + 1, ((MockCommandRuntime)getContainerCommand.CommandRuntime).WrittenObjects.Count);
        }
    }
}

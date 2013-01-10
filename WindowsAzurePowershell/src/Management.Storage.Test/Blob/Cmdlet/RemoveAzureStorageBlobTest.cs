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
// ---------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Storage.Test.Blob.Cmdlet
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Storage.Blob;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [TestClass]
    public class RemoveAzureStorageBlobTest : StorageBlobTestBase
    {
        public RemoveStorageAzureBlobCommand command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new RemoveStorageAzureBlobCommand(BlobMock)
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
        public void ValidatePipelineCloudBlobContainerTest()
        {
            CloudBlobContainer container = null;
            AssertThrows<ArgumentException>(()=>command.ValidatePipelineCloudBlobContainer(container), 
                String.Format(Resources.ObjectCannotBeNull, typeof(CloudBlobContainer).Name));

            string invalidUri = "http://127.0.0.1/account/t";
            container = new CloudBlobContainer(new Uri(invalidUri));
            AssertThrows<ArgumentException>(() => command.ValidatePipelineCloudBlobContainer(container),
                String.Format(Resources.InvalidContainerName, "t"));
            string testUri = "http://127.0.0.1/account/test";
            container = new CloudBlobContainer(new Uri(testUri));
            AssertThrows<ResourceNotFoundException>(() => command.ValidatePipelineCloudBlobContainer(container),
                String.Format(Resources.ContainerNotFound, "test"));

            AddTestContainers();
            command.ValidatePipelineCloudBlobContainer(container);
            string textUri = "http://127.0.0.1/account/text";
            container = new CloudBlobContainer(new Uri(textUri));
            command.ValidatePipelineCloudBlobContainer(container);
        }

        [TestMethod]
        public void ValidatePipelineICloudBlobTest()
        {
            CloudBlockBlob blockBlob = null;
            AssertThrows<ArgumentException>(() => command.ValidatePipelineICloudBlob(blockBlob),
                String.Format(Resources.ObjectCannotBeNull, typeof(ICloudBlob).Name));
            string blobUri = "http://127.0.0.1/account/test/";
            blockBlob = new CloudBlockBlob(new Uri(blobUri));
            AssertThrows<ArgumentException>(() => command.ValidatePipelineICloudBlob(blockBlob),
                String.Format(Resources.InvalidBlobName, blockBlob.Name));
            //blobUri = "http://xxx.xxxx.com/";
            //blockBlob = new CloudBlockBlob(new Uri(blobUri));
            //AssertThrows<ArgumentException>(() => command.ValidatePipelineICloudBlob(blockBlob),
            //    String.Format(Resources.InvalidBlobWithoutContainer, blockBlob.Name));

            AddTestBlobs();
            string container1Uri = "http://127.0.0.1/account/container1/blob";
            blockBlob = new CloudBlockBlob(new Uri(container1Uri));
            AssertThrows<ResourceNotFoundException>(() => command.ValidatePipelineICloudBlob(blockBlob),
                String.Format(Resources.BlobNotFound, blockBlob.Name, blockBlob.Container.Name));
            container1Uri = "http://127.0.0.1/account/container1/blob0";
            blockBlob = new CloudBlockBlob(new Uri(container1Uri));
            command.ValidatePipelineICloudBlob(blockBlob);
        }

        [TestMethod]
        public void RemoveAzureBlobByICloudBlobTest()
        {
            CloudBlockBlob blockBlob = null;
            string blobUri = "http://127.0.0.1/account/test/blob";
            AssertThrows<ArgumentException>(() => command.RemoveAzureBlobByICloudBlob(blockBlob, false),
                String.Format(Resources.ObjectCannotBeNull, typeof(ICloudBlob).Name));
            blockBlob = new CloudBlockBlob(new Uri(blobUri));
            command.RemoveAzureBlobByICloudBlob(blockBlob, true);
            AssertThrows<ResourceNotFoundException>(() => command.RemoveAzureBlobByICloudBlob(blockBlob, false),
                String.Format(Resources.ContainerNotFound, blockBlob.Container.Name));

            AddTestContainers();
            AssertThrows<ResourceNotFoundException>(() => command.RemoveAzureBlobByICloudBlob(blockBlob, false),
                String.Format(Resources.BlobNotFound, blockBlob.Name, blockBlob.Container.Name));
            command.RemoveAzureBlobByICloudBlob(blockBlob, true);
            //the mock api cannot throw exception

            AddTestBlobs();
            blobUri = "http://127.0.0.1/account/container0/blob0";
            blockBlob = new CloudBlockBlob(new Uri(blobUri));
            command.RemoveAzureBlobByICloudBlob(blockBlob, true);
            AssertThrows<ResourceNotFoundException>(() => command.RemoveAzureBlobByICloudBlob(blockBlob, false),
                String.Format(Resources.BlobNotFound, blockBlob.Name, blockBlob.Container.Name));
            blobUri = "http://127.0.0.1/account/container1/blob0";
            blockBlob = new CloudBlockBlob(new Uri(blobUri));
            command.RemoveAzureBlobByICloudBlob(blockBlob, true);

            AddTestBlobs();
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.RemoveAzureBlobByICloudBlob(blockBlob, false);
            string result = (string)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(String.Format(Resources.RemoveBlobSuccessfully, blockBlob.Name, blockBlob.Container.Name), result);
            AssertThrows<ResourceNotFoundException>(() => command.RemoveAzureBlobByICloudBlob(blockBlob, false),
                String.Format(Resources.BlobNotFound, blockBlob.Name, blockBlob.Container.Name));
        }

        [TestMethod]
        public void RemoveAzureBlobByCloudBlobContainerTest()
        {
            CloudBlobContainer container = null;
            string blobName = string.Empty;
            AssertThrows<ArgumentException>(() => command.RemoveAzureBlobByCloudBlobContainer(container, blobName),
                String.Format(Resources.InValidBlobName, blobName));
            blobName = "a";
            AssertThrows<ArgumentException>(() => command.RemoveAzureBlobByCloudBlobContainer(container, blobName),
                String.Format(Resources.ObjectCannotBeNull, typeof(CloudBlobContainer).Name));
            string containeruri = "http://127.0.0.1/account/t";
            container = new CloudBlobContainer(new Uri(containeruri));
            AssertThrows<ArgumentException>(() => command.RemoveAzureBlobByCloudBlobContainer(container, blobName),
                String.Format(Resources.InvalidContainerName, container.Name));
            containeruri = "http://127.0.0.1/account/test";
            container = new CloudBlobContainer(new Uri(containeruri));
            AssertThrows<ResourceNotFoundException>(() => command.RemoveAzureBlobByCloudBlobContainer(container, blobName),
                String.Format(Resources.ContainerNotFound, container.Name));

            AddTestContainers();
            AssertThrows<ResourceNotFoundException>(() => command.RemoveAzureBlobByCloudBlobContainer(container, blobName),
                String.Format(Resources.BlobNotFound, blobName, container.Name));

            AddTestBlobs();
            containeruri = "http://127.0.0.1/account/container1";
            container = new CloudBlobContainer(new Uri(containeruri));
            blobName = "blob0";
            command.RemoveAzureBlobByCloudBlobContainer(container, blobName);
            string result = (string)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(String.Format(Resources.RemoveBlobSuccessfully, blobName, "container1"), result);
            AssertThrows<ResourceNotFoundException>(() => command.RemoveAzureBlobByCloudBlobContainer(container, blobName),
                String.Format(Resources.BlobNotFound, blobName, "container1"));
        }

        [TestMethod]
        public void RemoveAzureBlobByNameTest()
        { 
            string containerName = string.Empty;
            string blobName = string.Empty;
            AssertThrows<ArgumentException>(() => command.RemoveAzureBlobByName(containerName, blobName),
                String.Format(Resources.InValidBlobName, blobName));
            blobName = "abcd";
            AssertThrows<ArgumentException>(() => command.RemoveAzureBlobByName(containerName, blobName),
                String.Format(Resources.InvalidContainerName, containerName));
            
            AddTestBlobs();
            containerName = "container1";
            blobName = "blob0";
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.RemoveAzureBlobByName(containerName, blobName);
            string result = (string)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(String.Format(Resources.RemoveBlobSuccessfully, blobName, containerName), result);
            AssertThrows<ResourceNotFoundException>(() => command.RemoveAzureBlobByName(containerName, blobName),
                String.Format(Resources.BlobNotFound, blobName, containerName));
        }

        [TestMethod]
        public void ExecuteCommandRemoveBlobTest()
        { 
            AddTestBlobs();
            string containerName = "container20";
            string blobName = "blob0";
            command.Container = containerName;
            command.Blob = blobName;
            command.ExecuteCommand();
            string result = (string)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual(String.Format(Resources.RemoveBlobSuccessfully, blobName, containerName), result);
            AssertThrows<ResourceNotFoundException>(() => command.ExecuteCommand(),
                String.Format(Resources.BlobNotFound, blobName, containerName));
            
        }
    }
}

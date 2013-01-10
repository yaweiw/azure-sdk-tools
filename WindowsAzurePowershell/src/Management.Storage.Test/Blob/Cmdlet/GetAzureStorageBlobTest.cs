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
    using Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [TestClass]
    public class GetAzureStorageBlobTest : StorageBlobTestBase
    {
        public GetAzureStorageBlobCommand command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new GetAzureStorageBlobCommand(BlobMock)
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
        public void ListBlobsByNameTest()
        {
            string containerName = string.Empty;
            string blobName = string.Empty;
            AssertThrows<ArgumentException>(() => command.ListBlobsByName(containerName, blobName), 
                String.Format(Resources.InvalidContainerName, containerName));

            containerName = "test";
            AssertThrows<ArgumentException>(() => command.ListBlobsByName(containerName, blobName),
                String.Format(Resources.ContainerNotFound, containerName));

            AddTestContainers();
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "test";
            command.ListBlobsByName(containerName, blobName);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "test";
            blobName = "*";
            command.ListBlobsByName(containerName, blobName);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            AddTestBlobs();
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container0";
            blobName = "";
            command.ListBlobsByName(containerName, blobName);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            blobName = "blob0";
            AssertThrows<ResourceNotFoundException>(() => command.ListBlobsByName(containerName, blobName), 
                String.Format(Resources.BlobNotFound, blobName, containerName));

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container1";
            blobName = "blob0";
            command.ListBlobsByName(containerName, blobName);
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            AzureStorageBlob blob = (AzureStorageBlob)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual("blob0", blob.Name);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container1";
            blobName = "blob*";
            command.ListBlobsByName(containerName, blobName);
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            blob = (AzureStorageBlob)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual("blob0", blob.Name);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container20";
            blobName = "*1?";
            command.ListBlobsByName(containerName, blobName);
            Assert.AreEqual(10, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            blob = (AzureStorageBlob)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.IsTrue(blob.Name.StartsWith("blob1") && blob.Name.Length == "blob1".Length + 1);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container20";
            blobName = new String('a', 1025);
            AssertThrows<ArgumentException>(() => command.ListBlobsByName(containerName, blobName),
                String.Format(Resources.InValidBlobName, blobName));
        }

        [TestMethod]
        public void ListBlobsByPrefixTest()
        {
            string containerName = string.Empty;
            string prefix = string.Empty;
            AssertThrows<ArgumentException>(() => command.ListBlobsByPrefix(containerName, prefix),
                String.Format(Resources.InvalidContainerName, containerName));

            containerName = "test";
            AssertThrows<ArgumentException>(() => command.ListBlobsByPrefix(containerName, prefix),
                String.Format(Resources.ContainerNotFound, containerName));

            AddTestBlobs();
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container0";
            prefix = "blob";
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container1";
            prefix = "blob";
            command.ListBlobsByPrefix(containerName, prefix);
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            AzureStorageBlob blob = (AzureStorageBlob)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual("blob0", blob.Name);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container1";
            prefix = "blob0";
            command.ListBlobsByPrefix(containerName, prefix);
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            blob = (AzureStorageBlob)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual("blob0", blob.Name);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container1";
            prefix = "blob01";
            command.ListBlobsByPrefix(containerName, prefix);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            containerName = "container20";
            prefix = "blob1";
            command.ListBlobsByPrefix(containerName, prefix);
            Assert.AreEqual(11, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            blob = (AzureStorageBlob)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.IsTrue(blob.Name.StartsWith("blob1"));
        }

        [TestMethod]
        public void WriteBlobsWithContext()
        {
            List<ICloudBlob> blobList = null;
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.WriteBlobsWithContext(blobList);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            blobList = new List<ICloudBlob>();
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.WriteBlobsWithContext(blobList);
            Assert.AreEqual(0, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            AddTestBlobs();
            blobList = blobMock.containerBlobs["container20"];
            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.WriteBlobsWithContext(blobList);
            Assert.AreEqual(20, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
        }

        [TestMethod]
        public void ExecuteCommandGetAzureBlob()
        { 
            AddTestBlobs();

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.Container = "container1";
            command.Blob = "blob*";
            command.ExecuteCommand();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            AzureStorageBlob blob = (AzureStorageBlob)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual("blob0", blob.Name);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.Container = "container20";
            command.Blob = "blob12";
            command.ExecuteCommand();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            blob = (AzureStorageBlob)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.AreEqual("blob12", blob.Name);

            ((MockCommandRuntime)command.CommandRuntime).ResetPipelines();
            command.Container = "container20";
            command.Blob = "*";
            command.ExecuteCommand();
            Assert.AreEqual(20, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);

            //FIXME how to set the parametersetname to BlobPrefix;
        }
    }
}

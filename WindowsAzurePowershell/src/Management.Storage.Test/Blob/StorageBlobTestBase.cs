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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Storage.Test.Service;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class StorageBlobTestBase : StorageTestBase
    {
        public MockStorageBlobManagement blobMock = null;

        [TestInitialize]
        public void initMock()
        {
            blobMock = new MockStorageBlobManagement();
        }

        [TestCleanup]
        public void CleanMock()
        {
            blobMock = null;
        }

        private void CleanTestData()
        {
            blobMock.containerList.Clear();
            blobMock.containerPermissions.Clear();
            blobMock.containerBlobs.Clear();
        }

        public void AddTestContainers()
        {
            CleanTestData();
            string testUri = "http://127.0.0.1/account/test";
            string textUri = "http://127.0.0.1/account/text";
            string publicOffUri = "http://127.0.0.1/account/publicoff";
            string publicBlobUri = "http://127.0.0.1/account/publicblob";
            string publicContainerUri = "http://127.0.0.1/account/publiccontainer";
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(testUri)));
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(textUri)));
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(publicOffUri)));
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(publicBlobUri)));
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(publicContainerUri)));

            BlobContainerPermissions publicOff = new BlobContainerPermissions();
            publicOff.PublicAccess = BlobContainerPublicAccessType.Off;
            blobMock.containerPermissions.Add("publicoff", publicOff);
            BlobContainerPermissions publicBlob = new BlobContainerPermissions();
            publicBlob.PublicAccess = BlobContainerPublicAccessType.Blob;
            blobMock.containerPermissions.Add("publicblob", publicBlob);
            BlobContainerPermissions publicContainer = new BlobContainerPermissions();
            publicContainer.PublicAccess = BlobContainerPublicAccessType.Container;
            blobMock.containerPermissions.Add("publiccontainer", publicContainer);
        }

        public void AddTestBlobs()
        {
            CleanTestData();
            string container0Uri = "http://127.0.0.1/account/container0";
            string container1Uri = "http://127.0.0.1/account/container1";
            string container20Uri = "http://127.0.0.1/account/container20";
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(container0Uri)));
            AddContainerBlobs("container0", 0);
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(container1Uri)));
            AddContainerBlobs("container1", 1);
            blobMock.containerList.Add(new CloudBlobContainer(new Uri(container20Uri)));
            AddContainerBlobs("container20", 20);
        }

        private void AddContainerBlobs(string containerName, int count)
        {
            List<ICloudBlob> blobList = null;
            if (blobMock.containerBlobs.ContainsKey(containerName))
            {
                blobList = blobMock.containerBlobs[containerName];
                blobList.Clear();
            }
            else
            {
                blobList = new List<ICloudBlob>();
                blobMock.containerBlobs.Add(containerName, blobList);
            }
            string prefix = "blob";
            string uri = string.Empty;
            string endPoint = "http://127.0.0.1/account";
            for(int i = 0; i < count; i++)
            {
                uri = string.Format("{0}/{1}/{2}{3}", endPoint, containerName, prefix, i);
                CloudBlockBlob blob = new CloudBlockBlob(new Uri(uri));
                blobList.Add(blob);
            }
        }
    }
}
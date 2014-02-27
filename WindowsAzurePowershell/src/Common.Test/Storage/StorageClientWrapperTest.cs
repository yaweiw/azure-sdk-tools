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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ResourceManagement.Test.Resources
{
    [TestClass]
    public class StorageClientWrapperTest
    {
        //private Mock<StorageManagementClient> storageMgmtClientMock;

        //private Mock<CloudBlobClient> cloudBlobClientMock;

        public StorageClientWrapperTest()
        {
            //storageMgmtClientMock = new Mock<StorageManagementClient>();
            //cloudBlobClientMock = new Mock<CloudBlobClient>();
        }

        [TestMethod]
        public void UploadFileToPrivateBlobReturnsSasToken()
        {
            //var storageWrapper = new StorageClientWrapper(storageMgmtClientMock.Object);
            //storageWrapper.CloudBlobClientFactory = (uri, cred) => cloudBlobClientMock.Object;

            //storageMgmtClientMock.Setup(f => f.CreatePSResourceGroup(It.IsAny<CreatePSResourceGroupParameters>()))
            //    .Returns(expected)
            //    .Callback((CreatePSResourceGroupParameters p) => { actualParameters = p; });

            //var fileUri = storageWrapper.UploadFileToBlob(new BlobUploadParameters
            //    {
            //        ContainerName = "container",
            //        ContainerPublic = false,
            //        FileLocalPath = "Azure.publishsettings",
            //        OverrideIfExists = true,
            //        StorageName = "test"
            //    });
        }
    }
}

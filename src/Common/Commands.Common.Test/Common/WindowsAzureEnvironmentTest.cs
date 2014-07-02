// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.Common.Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;

    [TestClass]
    public class WindowsAzureEnvironmentTest
    {
        private const string TestEndpointSuffix = "test.endpoint.suffix";

        private const string TestAccount = "testaccount";

        private static readonly string BlobEndpointUri = string.Concat("blob.", TestEndpointSuffix, "/");

        private static readonly string FileEndpointUri = string.Concat("file.", TestEndpointSuffix, "/");

        private static readonly string TableEndpointUri = string.Concat("table.", TestEndpointSuffix, "/");

        private static readonly string QueueEndpointUri = string.Concat("queue.", TestEndpointSuffix, "/");

        private WindowsAzureEnvironment azureEnvironment = new WindowsAzureEnvironment()
        {
            StorageEndpointSuffix = TestEndpointSuffix
        };

        [TestMethod]
        public void StorageBlobEndpointFormatTest()
        {
            Assert.AreEqual(string.Concat("{0}://{1}.", BlobEndpointUri), azureEnvironment.StorageBlobEndpointFormat, "BlobEndpointFormat should match.");
        }

        [TestMethod]
        public void StorageFileEndpointFormatTest()
        {
            Assert.AreEqual(string.Concat("{0}://{1}.", FileEndpointUri), azureEnvironment.StorageFileEndpointFormat, "FileEndpointFormat should match.");
        }

        [TestMethod]
        public void StorageQueueEndpointFormatTest()
        {
            Assert.AreEqual(string.Concat("{0}://{1}.", QueueEndpointUri), azureEnvironment.StorageQueueEndpointFormat, "QueueEndpointFormat should match.");
        }

        [TestMethod]
        public void StorageTableEndpointFormatTest()
        {
            Assert.AreEqual(string.Concat("{0}://{1}.", TableEndpointUri), azureEnvironment.StorageTableEndpointFormat, "TableEndpointFormat should match.");
        }

        [TestMethod]
        public void GetStorageBlobEndpointTest()
        {
            GetEndpointTestInternal(this.azureEnvironment.GetStorageBlobEndpoint, true, BlobEndpointUri);
            GetEndpointTestInternal(this.azureEnvironment.GetStorageBlobEndpoint, false, BlobEndpointUri);
        }

        [TestMethod]
        public void GetStorageTableEndpointTest()
        {
            GetEndpointTestInternal(this.azureEnvironment.GetStorageTableEndpoint, true, TableEndpointUri);
            GetEndpointTestInternal(this.azureEnvironment.GetStorageTableEndpoint, false, TableEndpointUri);
        }

        [TestMethod]
        public void GetStorageFileEndpointTest()
        {
            GetEndpointTestInternal(this.azureEnvironment.GetStorageFileEndpoint, true, FileEndpointUri);
            GetEndpointTestInternal(this.azureEnvironment.GetStorageFileEndpoint, false, FileEndpointUri);
        }

        [TestMethod]
        public void GetStorageQueueEndpointTest()
        {
            GetEndpointTestInternal(this.azureEnvironment.GetStorageQueueEndpoint, true, QueueEndpointUri);
            GetEndpointTestInternal(this.azureEnvironment.GetStorageQueueEndpoint, false, QueueEndpointUri);
        }

        private void GetEndpointTestInternal(Func<string, bool, Uri> getEndpoint, bool useHttps, string endpointSuffix)
        {
            Uri uri = getEndpoint(TestAccount, useHttps);
            Assert.AreEqual(useHttps ? "https" : "http", uri.Scheme, "Unexpected uri scheme.");
            Assert.AreEqual(string.Concat(TestAccount, ".", endpointSuffix.TrimEnd('/')), uri.DnsSafeHost, "Unexpected dns name.");
        }
    }
}

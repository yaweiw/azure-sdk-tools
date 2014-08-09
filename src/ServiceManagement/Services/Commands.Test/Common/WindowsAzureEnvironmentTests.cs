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

namespace Microsoft.WindowsAzure.Commands.Test.Common
{
    using Commands.Utilities.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Subscriptions;
    using Moq;
    using System;
    using System.Security.Cryptography.X509Certificates;

    [TestClass]
    public class WindowsAzureEnvironmentTests
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

        [TestMethod]
        public void GetsHttpsEndpointByDefault()
        {
            // Setup
            string accountName = "myaccount";
            string expected = string.Format(
                WindowsAzureEnvironmentConstants.AzureStorageBlobEndpointFormat,
                "https",
                accountName);
            WindowsAzureEnvironment environment = WindowsAzureEnvironment.PublicEnvironments[EnvironmentName.AzureCloud];

            // Test
            Uri actual = environment.GetStorageBlobEndpoint(accountName);

            // Assert
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod]
        public void GetsHttpEndpoint()
        {
            // Setup
            string accountName = "myaccount";
            string expected = string.Format(
                WindowsAzureEnvironmentConstants.AzureStorageBlobEndpointFormat,
                "http",
                accountName);
            WindowsAzureEnvironment environment = WindowsAzureEnvironment.PublicEnvironments[EnvironmentName.AzureCloud];

            // Test
            Uri actual = environment.GetStorageBlobEndpoint(accountName, false);

            // Assert
            Assert.AreEqual(expected, actual.ToString());
        }

        [TestMethod]
        public void DefaultActiveDirectoryResourceUriIsSameWithServiceEndpoint()
        {
            WindowsAzureEnvironment environment = WindowsAzureEnvironment.PublicEnvironments[EnvironmentName.AzureCloud];
            //Assert
            Assert.AreEqual(true,
                environment.ServiceEndpoint == environment.ActiveDirectoryServiceEndpointResourceId);

            //do same test for china cloud
            WindowsAzureEnvironment chinaEnvironment = WindowsAzureEnvironment.PublicEnvironments[EnvironmentName.AzureChinaCloud];
            Assert.AreEqual(true,
                chinaEnvironment.ServiceEndpoint == chinaEnvironment.ActiveDirectoryServiceEndpointResourceId);

            //verify the resource uri are different between 2 environments
            Assert.AreNotEqual(environment.ActiveDirectoryServiceEndpointResourceId,
                chinaEnvironment.ActiveDirectoryServiceEndpointResourceId);

        }

        [TestMethod]
        public void AddUserAgentTest()
        {
            WindowsAzureSubscription subscription = new WindowsAzureSubscription
            {
                Certificate = It.IsAny<X509Certificate2>(),
                IsDefault = true,
                ServiceEndpoint = new Uri("https://www.azure.com"),
                SubscriptionId = Guid.NewGuid().ToString(),
                SubscriptionName = Data.Subscription1,
            };

            WindowsAzureEnvironment environment = WindowsAzureEnvironment.PublicEnvironments[EnvironmentName.AzureCloud];
            ClientMocks clientMocks = new ClientMocks(subscription.SubscriptionId);
            SubscriptionClient subscriptionClient = clientMocks.SubscriptionClientMock.Object;
            SubscriptionClient actual = environment.AddUserAgent(subscriptionClient);

            // verify the UserAgent is set in the subscription client
            Assert.IsTrue(actual.UserAgent.Contains(ApiConstants.UserAgentValue), "Missing proper UserAgent string.");
        }

        private void GetEndpointTestInternal(Func<string, bool, Uri> getEndpoint, bool useHttps, string endpointSuffix)
        {
            Uri uri = getEndpoint(TestAccount, useHttps);
            Assert.AreEqual(useHttps ? "https" : "http", uri.Scheme, "Unexpected uri scheme.");
            Assert.AreEqual(string.Concat(TestAccount, ".", endpointSuffix.TrimEnd('/')), uri.DnsSafeHost, "Unexpected dns name.");
        }
    }
}

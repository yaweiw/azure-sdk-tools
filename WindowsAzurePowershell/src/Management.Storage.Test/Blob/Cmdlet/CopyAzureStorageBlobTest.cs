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

namespace Microsoft.WindowsAzure.Management.Storage.Test.Blob.Cmdlet
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo;
    using Microsoft.WindowsAzure.Management.Storage.Test.Blob.CmdletInfo;
    using Microsoft.WindowsAzure.Management.Storage.Test.Properties;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    [TestClass]
    public class CopyAzureStorageBlobTest
    {
        #region Helper Classes

        /// <summary>
        /// Contains information about this test run.
        /// </summary>
        private class TestInfo
        {
            /// <summary>
            /// The URI of the source for the copy.
            /// </summary>
            public TestUri Source { get; set; }

            /// <summary>
            /// The URI of the destination for the copy.
            /// </summary>
            public TestUri Destination { get; set; }

            /// <summary>
            /// The storage account key for the destination account. Must be specified if UseStorageKey == true.
            /// </summary>
            public string DestStorageAccountKey { get; set; }

            /// <summary>
            /// Indicates whether to use the DestinationStorageAccountKey parameter.
            /// </summary>
            public bool UseStorageKey { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the copy operation should overwrite the destination blob.
            /// </summary>
            public bool Overwrite { get; set; }

            /// <summary>
            /// Gets or sets the name of the file that should be uploaded as the copy source.
            /// </summary>
            public string TestBlobFile { get; set; }

            /// <summary>
            /// Initializes a new instance of the CopyAzureStorageBlobInfo class.
            /// </summary>
            public TestInfo()
            {
                this.Overwrite = false;
            }
        }

        /// <summary>
        /// Contains information about a URI used the source or destination in a test.
        /// </summary>
        private class TestUri
        {
            /// <summary>
            /// The URI.
            /// </summary>
            public string Uri { get; private set; }

            /// <summary>
            /// The account name in this URI.
            /// </summary>
            public string AccountName { get; private set; }

            /// <summary>
            /// The container name in this URI.
            /// </summary>
            public string ContainerName { get; private set; }

            /// <summary>
            /// The blob name in this URI.
            /// </summary>
            public string BlobName { get; private set; }

            /// <summary>
            /// Initializes a new instance of the TestUri class from the given URI.
            /// </summary>
            /// <param name="uri">The URI to use to initialize this instance.</param>
            public TestUri(string uri)
            {
                this.Uri = uri;
                this.AccountName = string.Empty;
                this.ContainerName = string.Empty;
                this.BlobName = string.Empty;
            }

            /// <summary>
            /// Initializes a new instance of the TestUri class from the given account, container and blob names.
            /// </summary>
            /// <param name="accountName">The storage account name to use in this URI.</param>
            /// <param name="containerName">The container name to use in this URI.</param>
            /// <param name="blobName">The blob name to use in this URI.</param>
            public TestUri (string accountName, string containerName, string blobName)
            {
                this.Uri = string.Format(@"http://{0}.blob.core.windows.net/{1}/{2}", accountName, containerName, blobName);
                this.AccountName = accountName;
                this.ContainerName = containerName;
                this.BlobName = blobName;
            }
        }

        #endregion Helper Classes

        /// <summary>
        /// Reference to the service management cmdlet helper.
        /// </summary>
        private static ServiceManagementCmdletTestHelper smPowershellCmdlets;

        /// <summary>
        /// Reference to the storage cmdlet helper.
        /// </summary>
        private static StorageCmdletTestHelper storePowershellCmdlets;

        /// <summary>
        /// Reference to the default subscription data.
        /// </summary>
        private static SubscriptionData defaultAzureSubscription;

        /// <summary>
        /// Name of the US West account created for testing.
        /// </summary>
        private static string AccountNameWest;

        /// <summary>
        /// Name of the US East account created for testing.
        /// </summary>
        private static string AccountNameEast;

        /// <summary>
        /// The info used for a specific test case.
        /// </summary>
        private TestInfo testInfo;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            CopyAzureStorageBlobTest.smPowershellCmdlets = new ServiceManagementCmdletTestHelper();
            CopyAzureStorageBlobTest.smPowershellCmdlets.ImportAzurePublishSettingsFile();
            CopyAzureStorageBlobTest.storePowershellCmdlets = new StorageCmdletTestHelper();

            CopyAzureStorageBlobTest.defaultAzureSubscription = CopyAzureStorageBlobTest.smPowershellCmdlets.SetDefaultAzureSubscription(Resource.DefaultSubscriptionName);
            Assert.AreEqual(Resource.DefaultSubscriptionName, CopyAzureStorageBlobTest.defaultAzureSubscription.SubscriptionName);

            CopyAzureStorageBlobTest.AccountNameWest = CopyAzureStorageBlobTest.CreateStorageAccount("West US");
            CopyAzureStorageBlobTest.AccountNameEast = CopyAzureStorageBlobTest.CreateStorageAccount("East US");
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            CopyAzureStorageBlobTest.smPowershellCmdlets.RemoveAzureStorageAccount(CopyAzureStorageBlobTest.AccountNameWest);
            CopyAzureStorageBlobTest.smPowershellCmdlets.RemoveAzureStorageAccount(CopyAzureStorageBlobTest.AccountNameEast);
        }

        #region Test Cases

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "Storage"), Priority(1), Owner("dagarv")]
        [Description("Tests Copy-AzureStorageBlob in the same storage account.")]
        public void CopyAzureStorageBlobTestCopy()
        {
            string containerName = "copytest";
            this.testInfo = new TestInfo()
            {
                Source = new TestUri(
                    CopyAzureStorageBlobTest.AccountNameWest,
                    containerName,
                    Utilities.GetUniqueShortName("testblob")),
                Destination = new TestUri(
                    CopyAzureStorageBlobTest.AccountNameWest,
                    containerName,
                    Utilities.GetUniqueShortName("testblob")),
                DestStorageAccountKey = string.Empty,
                TestBlobFile = @".\fixed_50.vhd",
                UseStorageKey = false,
                Overwrite = false
            };

            CopyAzureStorageBlobTest.UploadBlob(this.testInfo.TestBlobFile, this.testInfo.Source.Uri);
            CopyAzureStorageBlobTest.MakeContainerPublic(CopyAzureStorageBlobTest.AccountNameWest, containerName);
            CopyAzureStorageBlobTest.RunCmdlet(this.testInfo);

            CopyAzureStorageBlobTest.AssertMD5Matches(this.testInfo);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "Storage"), Priority(1), Owner("dagarv")]
        [Description("Tests Copy-AzureStorageBlob between two different storage accounts.")]
        public void CopyAzureStorageBlobTestCrossCluster()
        {
            string containerName = "crossclustertest";
            this.testInfo = new TestInfo()
            {
                Source = new TestUri(
                    CopyAzureStorageBlobTest.AccountNameWest, 
                    containerName, 
                    Utilities.GetUniqueShortName("testblob")),
                Destination = new TestUri(
                    CopyAzureStorageBlobTest.AccountNameEast, 
                    containerName, 
                    Utilities.GetUniqueShortName("testblob")),
                DestStorageAccountKey = string.Empty,
                TestBlobFile = @".\fixed_50.vhd",
                UseStorageKey = false,
                Overwrite = false
            };

            CopyAzureStorageBlobTest.UploadBlob(this.testInfo.TestBlobFile, this.testInfo.Source.Uri);
            CopyAzureStorageBlobTest.MakeContainerPublic(CopyAzureStorageBlobTest.AccountNameWest, containerName);
            CopyAzureStorageBlobTest.RunCmdlet(this.testInfo);

            CopyAzureStorageBlobTest.AssertMD5Matches(this.testInfo);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "Storage"), Priority(1), Owner("dagarv")]
        [Description("Tests Copy-AzureStorageBlob overwrite behavior.")]
        public void CopyAzureStorageBlobTestOverwrite()
        {
            string containerName = "overwritetest";
            this.testInfo = new TestInfo()
            {
                Source = new TestUri(
                    CopyAzureStorageBlobTest.AccountNameWest, 
                    containerName, 
                    Utilities.GetUniqueShortName("testblob")),
                Destination = new TestUri(
                    CopyAzureStorageBlobTest.AccountNameWest, 
                    containerName, 
                    Utilities.GetUniqueShortName("testblob")),
                DestStorageAccountKey = string.Empty,
                TestBlobFile = @".\fixed_50.vhd",
                UseStorageKey = false,
                Overwrite = false
            };

            CopyAzureStorageBlobTest.UploadBlob(this.testInfo.TestBlobFile, this.testInfo.Source.Uri);
            CopyAzureStorageBlobTest.UploadBlob(@".\dynamic_50.vhd", this.testInfo.Destination.Uri);
            CopyAzureStorageBlobTest.MakeContainerPublic(CopyAzureStorageBlobTest.AccountNameWest, containerName);

            // Test that copy will fail if overwrite not specified.
            try
            {
                CopyAzureStorageBlobTest.RunCmdlet(this.testInfo);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count != 1
                    || !( ex.InnerExceptions[0].Message.Contains("Destination blob ") && ex.InnerExceptions[0].Message.Contains(" exists. Not copying.")))
                {
                    throw;
                }
            }

            CopyAzureStorageBlobTest.AssertMD5DoesNotMatch(this.testInfo);

            // Test that copy will succeed if overwrite specified.
            this.testInfo.Overwrite = true;
            CopyAzureStorageBlobTest.RunCmdlet(this.testInfo);
            CopyAzureStorageBlobTest.AssertMD5Matches(this.testInfo);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "Storage"), Priority(1), Owner("dagarv")]
        [Description("Tests Copy-AzureStorageBlob using a SAS URI for the source.")]
        public void CopyAzureStorageBlobTestSAS()
        {
            string containerName = "sastest";
            var sourceBlobName = Utilities.GetUniqueShortName("testblob");
            var uploadUri = new TestUri(CopyAzureStorageBlobTest.AccountNameWest, containerName, sourceBlobName);
            CopyAzureStorageBlobTest.UploadBlob(@".\fixed_50.vhd", uploadUri.Uri);

            this.testInfo = new TestInfo()
            {
                Source = new TestUri(
                    CopyAzureStorageBlobTest.GetSasUri(
                        CopyAzureStorageBlobTest.AccountNameWest, 
                        containerName,
                        sourceBlobName)),
                Destination = new TestUri(
                    CopyAzureStorageBlobTest.AccountNameEast, 
                    containerName, 
                    Utilities.GetUniqueShortName("testblob")),
                DestStorageAccountKey = string.Empty,
                TestBlobFile = @".\fixed_50.vhd",
                UseStorageKey = false,
                Overwrite = false
            };


            CopyAzureStorageBlobTest.RunCmdlet(this.testInfo);

            this.testInfo.Source = uploadUri;

            CopyAzureStorageBlobTest.AssertMD5Matches(this.testInfo);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "Storage"), Priority(1), Owner("dagarv")]
        [Description("Tests Copy-AzureStorageBlob with a user specified storage account key.")]
        public void CopyAzureStorageBlobTestStorageKey()
        {
            string containerName = "storagekeytest";
            this.testInfo = new TestInfo()
            {
                Source = new TestUri(
                    CopyAzureStorageBlobTest.AccountNameWest, 
                    containerName, 
                    Utilities.GetUniqueShortName("testblob")),
                Destination = new TestUri(
                    CopyAzureStorageBlobTest.AccountNameEast, 
                    containerName, 
                    Utilities.GetUniqueShortName("testblob")),
                DestStorageAccountKey = CopyAzureStorageBlobTest.GetStorageKey(CopyAzureStorageBlobTest.AccountNameEast),
                TestBlobFile = @".\fixed_50.vhd",
                UseStorageKey = true,
                Overwrite = false
            };

            CopyAzureStorageBlobTest.UploadBlob(this.testInfo.TestBlobFile, this.testInfo.Source.Uri);
            CopyAzureStorageBlobTest.MakeContainerPublic(CopyAzureStorageBlobTest.AccountNameWest, containerName);
            CopyAzureStorageBlobTest.RunCmdlet(this.testInfo);

            CopyAzureStorageBlobTest.AssertMD5Matches(this.testInfo);
        }

        #endregion Test Cases

        #region Helper Methods

        /// <summary>
        /// Runs the cmdlet.
        /// </summary>
        private static void RunCmdlet(TestInfo testInfo)
        {
            CopyAzureStorageBlobCmdletInfo cmdletInfo;
            if (testInfo.UseStorageKey)
            {
                cmdletInfo = new CopyAzureStorageBlobCmdletInfo(testInfo.Source.Uri, testInfo.Destination.Uri, testInfo.DestStorageAccountKey, testInfo.Overwrite);
            }
            else
            {
                cmdletInfo = new CopyAzureStorageBlobCmdletInfo(testInfo.Source.Uri, testInfo.Destination.Uri, testInfo.Overwrite);
            }

            CopyAzureStorageBlobTest.storePowershellCmdlets.CopyAzureStorageBlob(cmdletInfo);
        }

        /// <summary>
        /// Creates a new storage account in the given location.
        /// </summary>
        /// <param name="location">The location to create the storage account in.</param>
        /// <returns>The name of the new storage account.</returns>
        private static string CreateStorageAccount(string location)
        {
            var name = Utilities.GetUniqueShortName("cbtest").ToLowerInvariant();
            var ret = smPowershellCmdlets.NewAzureStorageAccount(name, location);
            return ret.StorageAccountName;
        }

        /// <summary>
        /// Gets a sas uri for the given blob.
        /// </summary>
        /// <param name="accountName">The blob's storage account name.</param>
        /// <param name="containerName">The blob's container.</param>
        /// <param name="blobName">The blob's name.</param>
        /// <returns></returns>
        private static string GetSasUri(string accountName, string containerName, string blobName)
        {
            var primaryKey = GetStorageKey(accountName);
            var credentials = new StorageCredentials(accountName, primaryKey);
            var account = new CloudStorageAccount(credentials, false);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            var blob = container.GetBlobReferenceFromServer(blobName);

            var sharedAccessPolicy = new SharedAccessBlobPolicy() { Permissions = SharedAccessBlobPermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1) };
            var sas = container.GetSharedAccessSignature(sharedAccessPolicy);

            return blob.Uri.AbsoluteUri + sas;
        }

        /// <summary>
        /// Gets the primary storage key for the given account in this subscription.
        /// </summary>
        /// <param name="accountName">The name of the account to get the primary storage key for.</param>
        /// <returns></returns>
        private static string GetStorageKey(string accountName)
        {
            return CopyAzureStorageBlobTest.smPowershellCmdlets.GetAzureStorageAccountKey(accountName).Primary;
        }

        /// <summary>
        /// Uploads a file to the given blob URI.
        /// </summary>
        /// <param name="fileName">The file to upload.</param>
        /// <param name="uri">The blob URI to upload the file to.</param>
        private static void UploadBlob(string fileName, string uri)
        {
            var localFile = new FileInfo(fileName);
            CopyAzureStorageBlobTest.smPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(uri, localFile.FullName));
        }

        /// <summary>
        /// Changes the specified container's permissions to public.
        /// </summary>
        /// <param name="accountName">The container's storage account name.</param>
        /// <param name="containerName">The container's name.</param>
        private static void MakeContainerPublic(string accountName, string containerName)
        {
            var primaryKey = GetStorageKey(accountName);
            var credentials = new StorageCredentials(accountName, primaryKey);
            var account = new CloudStorageAccount(credentials, false);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            var permissions = new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            container.SetPermissions(permissions);
        }

        #endregion Helper Methods

        #region Asserts

        /// <summary>
        /// Asserts that the copy operation was successful.
        /// </summary>
        private static void AssertMD5Matches(TestInfo testInfo)
        {
            var sourceCredentials = new StorageCredentials(
                    CopyAzureStorageBlobTest.AccountNameWest, 
                    CopyAzureStorageBlobTest.GetStorageKey(CopyAzureStorageBlobTest.AccountNameWest));
            var sourceAccount = new CloudStorageAccount(sourceCredentials, false);
            var sourceClient = sourceAccount.CreateCloudBlobClient();
            var sourceBlob = sourceClient.GetBlobReferenceFromServer(new Uri(testInfo.Source.Uri));

            var destCredentials = new StorageCredentials(
                    testInfo.Destination.AccountName,
                    CopyAzureStorageBlobTest.GetStorageKey(testInfo.Destination.AccountName));
            var destAccount = new CloudStorageAccount(destCredentials, false);
            var destClient = destAccount.CreateCloudBlobClient();
            var destBlob = destClient.GetBlobReferenceFromServer(new Uri(testInfo.Destination.Uri));

            Assert.AreEqual(sourceBlob.Properties.ContentMD5, destBlob.Properties.ContentMD5);
        }

        /// <summary>
        /// Asserts that the copy operation was not successful.
        /// </summary>
        private static void AssertMD5DoesNotMatch(TestInfo testInfo)
        {
            var sourceCredentials = new StorageCredentials(
                    CopyAzureStorageBlobTest.AccountNameWest,
                    CopyAzureStorageBlobTest.GetStorageKey(CopyAzureStorageBlobTest.AccountNameWest));
            var sourceAccount = new CloudStorageAccount(sourceCredentials, false);
            var sourceClient = sourceAccount.CreateCloudBlobClient();
            var sourceBlob = sourceClient.GetBlobReferenceFromServer(new Uri(testInfo.Source.Uri));

            var destCredentials = new StorageCredentials(
                    testInfo.Destination.AccountName,
                    CopyAzureStorageBlobTest.GetStorageKey(testInfo.Destination.AccountName));
            var destAccount = new CloudStorageAccount(destCredentials, false);
            var destClient = destAccount.CreateCloudBlobClient();
            var destBlob = destClient.GetBlobReferenceFromServer(new Uri(testInfo.Destination.Uri));

            Assert.AreNotEqual(sourceBlob.Properties.ContentMD5, destBlob.Properties.ContentMD5);
        }

        #endregion Asserts
    }
}

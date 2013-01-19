using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests
{
    [TestClass]
    public class CopyAzureBlobTest
    {
        private class CopyAzureBlobInfo
        {
            /// <summary>
            /// Gets or sets the source account name.
            /// </summary>
            public static string SourceAccountName { get; set; }

            /// <summary>
            /// Gets or sets the destination account name.
            /// </summary>
            public static string DestAccountName { get; set; }

            /// <summary>
            /// Indicates whether the source URI needs to be updated.
            /// </summary>
            private bool updateSourceUri;

            /// <summary>
            /// Gets or sets the name of the source container.
            /// </summary>
            public string SourceContainerName
            {
                get { return this.sourceContainerName; }
                set
                {
                    this.updateSourceUri = true;
                    this.sourceContainerName = value;
                }
            }
            private string sourceContainerName;

            /// <summary>
            /// Gets or sets the name of the source blob.
            /// </summary>
            public string SourceBlobName
            {
                get { return this.sourceBlobName; }
                set
                {
                    this.updateSourceUri = true;
                    this.sourceBlobName = value;
                }
            }
            private string sourceBlobName;

            /// <summary>
            /// Gets the complete URI to the source blob.
            /// </summary>
            public string SourceBlobUri
            {
                get
                {
                    if (this.updateSourceUri)
                    {
                        this.UpdateSourceUri();
                        this.updateSourceUri = false;
                    }

                    return this.sourceBlobUri;
                }
            }
            private string sourceBlobUri;

            /// <summary>
            /// Gets the source storage account key.
            /// </summary>
            public StorageServiceKeyOperationContext SourceStorageAccountKey
            {
                get
                {
                    if (this.updateSourceUri)
                    {
                        this.UpdateSourceUri();
                        this.updateSourceUri = false;
                    }

                    return this.sourceStorageAccountKey;
                }
            }
            private StorageServiceKeyOperationContext sourceStorageAccountKey;

            /// <summary>
            /// Indicates whether the destination URI needs to be updated.
            /// </summary>
            private bool updateDestUri;

            /// <summary>
            /// Gets or sets the name of the destination container.
            /// </summary>
            public string DestContainerName
            {
                get { return this.destContainerName; }
                set
                {
                    this.updateDestUri = true;
                    this.destContainerName = value;
                }
            }
            private string destContainerName;

            /// <summary>
            /// Gets or sets the name of the destination blob.
            /// </summary>
            public string DestBlobName
            {
                get { return this.destBlobName; }
                set
                {
                    this.updateDestUri = true;
                    this.destBlobName = value;
                }
            }
            private string destBlobName;

            /// <summary>
            /// Gets the complete URI to the destination blob.
            /// </summary>
            public string DestBlobUri
            {
                get
                {
                    if (this.updateDestUri)
                    {
                        this.UpdateDestUri();
                        this.updateDestUri = false;
                    }

                    return this.destBlobUri;
                }
            }
            private string destBlobUri;

            /// <summary>
            /// Gets the destination storage account key.
            /// </summary>
            public StorageServiceKeyOperationContext DestStorageAccountKey
            {
                get
                {
                    if (this.updateDestUri)
                    {
                        this.UpdateDestUri();
                        this.updateDestUri = false;
                    }

                    return destStorageAccountKey;
                }
            }
            private StorageServiceKeyOperationContext destStorageAccountKey;

            /// <summary>
            /// Gets or sets a value indicating whether the copy operation should overwrite the destination blob.
            /// </summary>
            public bool Overwrite { get; set; }

            /// <summary>
            /// Gets or sets the name of the file that should be uploaded as the copy source.
            /// </summary>
            public string TestBlobFile { get; set; }

            /// <summary>
            /// Initializes a new instance of the CopyAzureBlobInfo class.
            /// </summary>
            public CopyAzureBlobInfo()
            {
                this.updateSourceUri = true;
                this.updateDestUri = true;

                this.Overwrite = false;
            }

            /// <summary>
            /// Updates the source URI and keys using the given account, container and blob names.
            /// </summary>
            private void UpdateSourceUri()
            {
                this.sourceStorageAccountKey = CopyAzureBlobTest.powershellCmdlets.GetAzureStorageAccountKey(CopyAzureBlobInfo.SourceAccountName);
                this.sourceBlobUri = string.Format(@"http://{0}.blob.core.windows.net/{1}/{2}", CopyAzureBlobInfo.SourceAccountName, this.SourceContainerName, this.SourceBlobName);
            }

            /// <summary>
            /// Updates the destination URI and keys using the given account, container and blob names.
            /// </summary>
            private void UpdateDestUri()
            {
                this.destStorageAccountKey = CopyAzureBlobTest.powershellCmdlets.GetAzureStorageAccountKey(CopyAzureBlobInfo.DestAccountName);
                this.destBlobUri = string.Format(@"http://{0}.blob.core.windows.net/{1}/{2}", CopyAzureBlobInfo.DestAccountName, this.DestContainerName, this.DestBlobName);
            }
        }

        private static CmdletTestHelper powershellCmdlets;
        private static SubscriptionData defaultAzureSubscription;

        private CopyAzureBlobInfo testInfo;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            CopyAzureBlobTest.powershellCmdlets = new CmdletTestHelper();
            CopyAzureBlobTest.powershellCmdlets.ImportAzurePublishSettingsFile();
            CopyAzureBlobTest.defaultAzureSubscription = CopyAzureBlobTest.powershellCmdlets.SetDefaultAzureSubscription(Resource.DefaultSubscriptionName);
            Assert.AreEqual(Resource.DefaultSubscriptionName, CopyAzureBlobTest.defaultAzureSubscription.SubscriptionName);

            CopyAzureBlobInfo.SourceAccountName = CopyAzureBlobTest.CreateStorageAccount("West US");
            CopyAzureBlobInfo.DestAccountName = CopyAzureBlobTest.CreateStorageAccount("East US");
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            CopyAzureBlobTest.powershellCmdlets.RemoveAzureStorageAccount(CopyAzureBlobInfo.SourceAccountName);
            CopyAzureBlobTest.powershellCmdlets.RemoveAzureStorageAccount(CopyAzureBlobInfo.DestAccountName);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var sourceCredentials = new StorageCredentials(CopyAzureBlobInfo.SourceAccountName, this.testInfo.SourceStorageAccountKey.Primary);
            var sourceAccount = new CloudStorageAccount(sourceCredentials, false);
            var sourceClient = sourceAccount.CreateCloudBlobClient();
            var sourceContainer = sourceClient.GetContainerReference(this.testInfo.SourceContainerName);
            sourceContainer.DeleteIfExists();
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("dagarv"), Description("Test the cmdlet (Copy-AzureBlob)")]
        public void CopyAzureBlobTestCrossCluster()
        {
            this.testInfo = new CopyAzureBlobInfo()
            {
                SourceContainerName = "crossclustertest",
                SourceBlobName = Utilities.GetUniqueShortName("testblob"),
                DestContainerName = "crossclustertest",
                DestBlobName = Utilities.GetUniqueShortName("testblob"),
                TestBlobFile = @".\fixed_50.vhd",
                Overwrite = false
            };

            this.RunTest();

            this.AssertCopySuccess(true);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("dagarv"), Description("Test the cmdlet (Copy-AzureBlob)")]
        public void CopyAzureBlobTestOverwrite()
        {
            this.testInfo = new CopyAzureBlobInfo()
            {
                SourceContainerName = "overwritetest",
                SourceBlobName = Utilities.GetUniqueShortName("testblob"),
                DestContainerName = "overwritetest",
                DestBlobName = Utilities.GetUniqueShortName("testblob"),
                TestBlobFile = @".\fixed_50.vhd",
                Overwrite = false
            };

            CopyAzureBlobTest.UploadBlob(@".\dynamic_50.vhd", this.testInfo.DestBlobUri);

            // Test that copy will fail if overwrite not specified.
            try
            {
                this.RunTest();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count != 1
                    || !ex.InnerExceptions[0].Message.Contains("Parameter name: destBlob"))
                {
                    throw;
                }
            }

            this.AssertCopyFailed(false);

            this.testInfo = new CopyAzureBlobInfo()
            {
                SourceContainerName = "overwritetest",
                SourceBlobName = Utilities.GetUniqueShortName("testblob"),
                DestContainerName = "overwritetest",
                DestBlobName = Utilities.GetUniqueShortName("testblob"),
                TestBlobFile = @".\fixed_50.vhd",
                Overwrite = true
            };

            // Test that copy will succeed if overwrite specified.
            this.RunTest();
            this.AssertCopySuccess(true);
        }

        /// <summary>
        /// Uploads the test blob and runs the cmdlet.
        /// </summary>
        public void RunTest()
        {
            CopyAzureBlobTest.UploadBlob(this.testInfo.TestBlobFile, this.testInfo.SourceBlobUri);
            CopyAzureBlobTest.powershellCmdlets.CopyAzureBlob(this.testInfo.SourceBlobUri, this.testInfo.DestBlobUri, this.testInfo.Overwrite);
        }

        /// <summary>
        /// Creates a new storage account in the given location.
        /// </summary>
        /// <param name="location">The location to create the storage account in.</param>
        /// <returns>The name of the new storage account.</returns>
        private static string CreateStorageAccount(string location)
        {
            var name = Utilities.GetUniqueShortName("cbtest").ToLowerInvariant();
            var ret = powershellCmdlets.NewAzureStorageAccount(name, location);
            return ret.StorageAccountName;
        }

        /// <summary>
        /// Uploads a file to the given blob URI.
        /// </summary>
        /// <param name="fileName">The file to upload.</param>
        /// <param name="uri">The blob URI to upload the file to.</param>
        private static void UploadBlob(string fileName, string uri)
        {
            var localFile = new FileInfo(fileName);
            CopyAzureBlobTest.powershellCmdlets.AddAzureVhd(new IaasCmdletInfo.AddAzureVhdCmdletInfo(uri, localFile.FullName));
        }

        /// <summary>
        /// Asserts that the copy operation was successful.
        /// </summary>
        /// <param name="deleteBlob">Indicates if the dest blob should be deleted after assertion.</param>
        private void AssertCopySuccess(bool deleteBlob)
        {
            var sourceCredentials = new StorageCredentials(CopyAzureBlobInfo.SourceAccountName, this.testInfo.SourceStorageAccountKey.Primary);
            var sourceAccount = new CloudStorageAccount(sourceCredentials, false);
            var sourceClient = sourceAccount.CreateCloudBlobClient();
            var sourceBlob = sourceClient.GetBlobReferenceFromServer(new Uri(this.testInfo.SourceBlobUri));

            var destCredentials = new StorageCredentials(CopyAzureBlobInfo.DestAccountName, this.testInfo.DestStorageAccountKey.Primary);
            var destAccount = new CloudStorageAccount(destCredentials, false);
            var destClient = destAccount.CreateCloudBlobClient();
            var destBlob = destClient.GetBlobReferenceFromServer(new Uri(this.testInfo.DestBlobUri));

            Assert.AreEqual(sourceBlob.Properties.ContentMD5, destBlob.Properties.ContentMD5);

            if (deleteBlob)
            {
                destBlob.Delete();
            }
        }

        /// <summary>
        /// Asserts that the copy operation was not successful.
        /// </summary>
        /// <param name="deleteBlob">Indicates if the dest blob should be deleted after assertion.</param>
        private void AssertCopyFailed(bool deleteBlob)
        {
            var sourceCredentials = new StorageCredentials(CopyAzureBlobInfo.SourceAccountName, this.testInfo.SourceStorageAccountKey.Primary);
            var sourceAccount = new CloudStorageAccount(sourceCredentials, false);
            var sourceClient = sourceAccount.CreateCloudBlobClient();
            var sourceBlob = sourceClient.GetBlobReferenceFromServer(new Uri(this.testInfo.SourceBlobUri));

            var destCredentials = new StorageCredentials(CopyAzureBlobInfo.DestAccountName, this.testInfo.DestStorageAccountKey.Primary);
            var destAccount = new CloudStorageAccount(destCredentials, false);
            var destClient = destAccount.CreateCloudBlobClient();
            var destBlob = destClient.GetBlobReferenceFromServer(new Uri(this.testInfo.DestBlobUri));

            Assert.AreNotEqual(sourceBlob.Properties.ContentMD5, destBlob.Properties.ContentMD5);

            if (deleteBlob)
            {
                destBlob.Delete();
            }
        }
    }
}

// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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
//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo;
using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Sync.Download;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests
{
    [TestClass]
    public class AddAzureVhdTest
    {
        private CmdletTestHelper vmPowershellCmdlets;
        private SubscriptionData defaultAzureSubscription;
        private StorageServiceKeyOperationContext storageAccountKey;
        private string destination;
        private string patchDestination;
        private string destinationSasUri;
        private string patchDestinationSasUri;

        [TestInitialize]
        public void Initialize()
        {
            vmPowershellCmdlets = new CmdletTestHelper();
            vmPowershellCmdlets.ImportAzurePublishSettingsFile();
            defaultAzureSubscription = vmPowershellCmdlets.SetDefaultAzureSubscription(Resource.DefaultSubscriptionName);
            Assert.AreEqual(Resource.DefaultSubscriptionName, defaultAzureSubscription.SubscriptionName);
            storageAccountKey = vmPowershellCmdlets.GetAzureStorageAccountKey(defaultAzureSubscription.CurrentStorageAccount);
            Assert.AreEqual(defaultAzureSubscription.CurrentStorageAccount, storageAccountKey.StorageAccountName);

            destination = string.Format(@"http://{0}.blob.core.windows.net/vhdstore/{1}", defaultAzureSubscription.CurrentStorageAccount, Utilities.GetUniqueShortName("PSTestAzureVhd"));
            patchDestination = string.Format(@"http://{0}.blob.core.windows.net/vhdstore/{1}", defaultAzureSubscription.CurrentStorageAccount, Utilities.GetUniqueShortName("PSTestAzureVhd"));

            destinationSasUri = string.Format(@"http://{0}.blob.core.windows.net/vhdstore/{1}", defaultAzureSubscription.CurrentStorageAccount, Utilities.GetUniqueShortName("PSTestAzureVhd"));
            patchDestinationSasUri = string.Format(@"http://{0}.blob.core.windows.net/vhdstore/{1}", defaultAzureSubscription.CurrentStorageAccount, Utilities.GetUniqueShortName("PSTestAzureVhd"));
            var destinationBlob = new CloudPageBlob(new Uri(destinationSasUri), new StorageCredentials(storageAccountKey.StorageAccountName, storageAccountKey.Primary));
            var patchDestinationBlob = new CloudPageBlob(new Uri(patchDestinationSasUri), new StorageCredentials(storageAccountKey.StorageAccountName, storageAccountKey.Primary));
            var policy = new SharedAccessBlobPolicy()
            {
                Permissions =
                    SharedAccessBlobPermissions.Delete |
                    SharedAccessBlobPermissions.Read |
                    SharedAccessBlobPermissions.Write |
                    SharedAccessBlobPermissions.List,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromHours(1)
            };
            var destinationBlobToken = destinationBlob.GetSharedAccessSignature(policy);
            var patchDestinationBlobToken = patchDestinationBlob.GetSharedAccessSignature(policy);
            destinationSasUri += destinationBlobToken;
            patchDestinationSasUri += patchDestinationBlobToken;
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDisk()
        {
            DoUploadDynamicDisk(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDiskWithSasUri()
        {
            DoUploadDynamicDisk(destinationSasUri);
        }

        private void DoUploadDynamicDisk(string destination)
        {
            var localFile = new FileInfo(@".\dynamic_50.vhd");
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDiskSecondAttempWithoutOverwriteShouldFail()
        {
            DoUploadDynamicDiskSecondAttempWithoutOverwriteShouldFail(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDiskSecondAttempWithoutOverwriteShouldFailWithSasUri()
        {
            DoUploadDynamicDiskSecondAttempWithoutOverwriteShouldFail(destinationSasUri);
        }

        private void DoUploadDynamicDiskSecondAttempWithoutOverwriteShouldFail(string destination)
        {
            var localFile = new FileInfo(@".\dynamic_50.vhd");
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));
            try
            {
                vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));
                Assert.Fail("Must have failed!");
            }
            catch (Exception)
            {
            }

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDiskWithExplicitlySpecifiedUploaderThread()
        {
            DoUploadDynamicDiskWithExplicitlySpecifiedUploaderThread(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDiskWithExplicitlySpecifiedUploaderThreadSasUri()
        {
            DoUploadDynamicDiskWithExplicitlySpecifiedUploaderThread(destinationSasUri);
        }

        private void DoUploadDynamicDiskWithExplicitlySpecifiedUploaderThread(string destination)
        {
            var localFile = new FileInfo(@".\dynamic_50.vhd");
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName, 16, false));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDiskWithOverwrite()
        {
            DoUploadDynamicDiskWithOverwrite(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDiskWithOverwriteWithSasUri()
        {
            DoUploadDynamicDiskWithOverwrite(destinationSasUri);
        }

        private void DoUploadDynamicDiskWithOverwrite(string destination)
        {
            var localFile = new FileInfo(@".\dynamic_50.vhd");
            vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName, true));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDiskWithExplicitySpecifiedUploaderThreadWithOverwrite()
        {
            DoUploadDynamicDiskWithExplicitySpecifiedUploaderThreadWithOverwrite(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDynamicDiskWithExplicitySpecifiedUploaderThreadWithOverwriteWithSasUri()
        {
            DoUploadDynamicDiskWithExplicitySpecifiedUploaderThreadWithOverwrite(destinationSasUri);
        }

        private void DoUploadDynamicDiskWithExplicitySpecifiedUploaderThreadWithOverwrite(string destination)
        {
            var localFile = new FileInfo(@".\dynamic_50.vhd");
            vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName, 16, true));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDifferencingDiskOfDynamicDisk()
        {
            DoUploadDifferencingDiskOfDynamicDisk(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDifferencingDiskOfDynamicDiskWithSasUri()
        {
            DoUploadDifferencingDiskOfDynamicDisk(destinationSasUri);
        }

        private void DoUploadDifferencingDiskOfDynamicDisk(string destination)
        {
            var localFile = new FileInfo(@".\dynamic_50_child01.vhd");
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadSecondLevelDifferencingDiskOfDynamicDisk()
        {
            DoUploadSecondLevelDifferencingDiskOfDynamicDisk(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadSecondLevelDifferencingDiskOfDynamicDiskWithSasUri()
        {
            DoUploadSecondLevelDifferencingDiskOfDynamicDisk(destinationSasUri);
        }

        private void DoUploadSecondLevelDifferencingDiskOfDynamicDisk(string destination)
        {
            var localFile = new FileInfo(@".\dynamic_50_child02.vhd");
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadFixedDisk()
        {
            DoUploadFixedDisk(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadFixedDiskWithSasUri()
        {
            DoUploadFixedDisk(destinationSasUri);
        }

        private void DoUploadFixedDisk(string destination)
        {
            var localFile = new FileInfo(@".\fixed_50.vhd");
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDifferencingDiskOfFixedDisk()
        {
            DoUploadDifferencingDiskOfFixedDisk(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadDifferencingDiskOfFixedDiskWithSasUri()
        {
            DoUploadDifferencingDiskOfFixedDisk(destinationSasUri);
        }

        private void DoUploadDifferencingDiskOfFixedDisk(string destination)
        {
            var localFile = new FileInfo(@".\fixed_50_child01.vhd");
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadSecondLevelDifferencingDiskOfFixedDisk()
        {
            DoUploadSecondLevelDifferencingDiskOfFixedDisk(destination);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void UploadSecondLevelDifferencingDiskOfFixedDiskWithSasUri()
        {
            DoUploadSecondLevelDifferencingDiskOfFixedDisk(destinationSasUri);
        }

        private void DoUploadSecondLevelDifferencingDiskOfFixedDisk(string destination)
        {
            var localFile = new FileInfo(@".\fixed_50_child02.vhd");
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext);
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void PatchFirstLevelDifferencingDisk()
        {
            DoPatchFirstLevelDifferencingDisk(destination, patchDestination);
        }

        [Ignore(), TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        public void PatchFirstLevelDifferencingDiskWithSasUri()
        {
            DoPatchFirstLevelDifferencingDisk(destinationSasUri, patchDestinationSasUri);
        }

        private void DoPatchFirstLevelDifferencingDisk(string destination, string patchDestination)
        {
            var localFile = new FileInfo(@".\fixed_50.vhd");
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destination, localFile.FullName, true));

            AssertUploadContextAndContentMD5(destination, localFile, vhdUploadContext, false);

            var patchLocalFile = new FileInfo(@".\fixed_50_child01.vhd");
            var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(patchDestination, patchLocalFile.FullName, destination));

            AssertUploadContextAndContentMD5(patchDestination, patchLocalFile, patchVhdUploadContext);
        }

        private void AssertUploadContextAndContentMD5(string destination, FileInfo localFile, VhdUploadContext vhdUploadContext, bool deleteBlob = true)
        {
            AssertUploadContext(destination, localFile, vhdUploadContext);
            BlobUri blobPath;
            Assert.IsTrue(BlobUri.TryParseUri(new Uri(destination), out blobPath));
            AssertContentMD5(blobPath.BlobPath, deleteBlob);
        }

        private void AssertContentMD5(string destination, bool deleteBlob)
        {
            string downloadedFile = DownloadToFile(destination);

            var calculateMd5Hash = CalculateContentMd5(File.OpenRead(downloadedFile));

            BlobUri blobUri2;
            Assert.IsTrue(BlobUri.TryParseUri(new Uri(destination), out blobUri2));
            var blobHandle = new BlobHandle(blobUri2, storageAccountKey.Primary);

            Assert.AreEqual(calculateMd5Hash, blobHandle.Blob.Properties.ContentMD5);

            if(deleteBlob)
            {
                blobHandle.Blob.Delete();
            }
        }

        private void AssertUploadContext(string destination, FileInfo localFile, VhdUploadContext vhdUploadContext)
        {
            Assert.IsNotNull(vhdUploadContext);
            Assert.AreEqual(new Uri(destination), vhdUploadContext.DestinationUri);
            Assert.AreEqual(vhdUploadContext.LocalFilePath.FullName, localFile.FullName);
        }

        private string DownloadToFile(string destination)
        {
            BlobUri blobUri;
            BlobUri.TryParseUri(new Uri(destination), out blobUri);

            var downloader = new Downloader(blobUri, storageAccountKey.Primary);
            var downloadedFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            downloader.Download(downloadedFile);
            return downloadedFile;
        }

        private static string CalculateContentMd5(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                using (var bs = new BufferedStream(stream))
                {
                    var md5Hash = md5.ComputeHash(bs);
                    return Convert.ToBase64String(md5Hash);
                }
            }
        }

    }
}
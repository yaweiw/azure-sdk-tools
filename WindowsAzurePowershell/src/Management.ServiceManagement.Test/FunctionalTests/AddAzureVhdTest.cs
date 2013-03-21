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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Sync.Download;

    [TestClass]
    public class AddAzureVhdTest
    {
        private ServiceManagementCmdletTestHelper vmPowershellCmdlets;
        private SubscriptionData defaultAzureSubscription;
        private StorageServiceKeyOperationContext storageAccountKey;
        private string perfFile;
        private string blobUrlRoot;
        
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return TestContext;
            }
            set
            {
                TestContext = value;
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();
            vmPowershellCmdlets.ImportAzurePublishSettingsFile();
            defaultAzureSubscription = vmPowershellCmdlets.SetDefaultAzureSubscription(Resource.DefaultSubscriptionName);
            Assert.AreEqual(Resource.DefaultSubscriptionName, defaultAzureSubscription.SubscriptionName);
            storageAccountKey = vmPowershellCmdlets.GetAzureStorageAccountKey(defaultAzureSubscription.CurrentStorageAccount);
            Assert.AreEqual(defaultAzureSubscription.CurrentStorageAccount, storageAccountKey.StorageAccountName);            

            blobUrlRoot = string.Format(@"http://{0}.blob.core.windows.net/", defaultAzureSubscription.CurrentStorageAccount);

            perfFile = "perf.csv";
        }


        /// <summary>
        /// UploadDisk: 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\upload_VHD.csv", "upload_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDisk()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            
            // Choose the vhd file from local machine
            string vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            FileInfo vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Start uploading...
            Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName));
            Console.WriteLine("uploading completed: {0}", vhdName);

            // Verify the upload.
            AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, vhdLocalPath, vhdUploadContext, md5hash);
            
            Console.WriteLine("{0} test passed.", testName);            
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\upload_VHD.csv", "upload_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskSasUri()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            int i = 0;
            //while ( i = 0; i < 16; i++)
            while (i < 16)
            {
                string destinationSasUri2 = CreateSasUriWithPermission(vhdName, i);
                try
                {
                    Console.WriteLine("uploads {0} to {1}", vhdName, destinationSasUri2);
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName));
                    Console.WriteLine("Finished uploading: {0}", destinationSasUri2);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUri2, vhdLocalPath, vhdUploadContext, md5hash);
                    Console.WriteLine("Test success with permission: {0}", i);
                    i++;
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("already running"))
                    {
                        Console.WriteLine(e.InnerException.Message);
                        continue;
                    }
                    if (i != 3 && i != 7 && i != 11 && i != 15)
                    {
                        Console.WriteLine("Error as expected.  Permission: {0}", i);
                        Console.WriteLine("Error message: {0}", e.InnerException.Message);
                        i++;
                        continue;
                    }
                    else
                    {
                        Assert.Fail("Test failed Permission: {0} \n {1}", i, e.ToString());
                    }
                }
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0} {1},{2}", testName, vhdName, (testEndTime - testStartTime).TotalSeconds) });

        }

        private string CreateSasUriWithPermission(string vhdName, int p)
        {
            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;


            var destinationBlob2 = new CloudPageBlob(new Uri(vhdDestUri), new StorageCredentials(storageAccountKey.StorageAccountName, storageAccountKey.Primary));
            var policy2 = new SharedAccessBlobPolicy()
            {
                Permissions = (SharedAccessBlobPermissions)p,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromHours(1)
            };
            var destinationBlobToken2 = destinationBlob2.GetSharedAccessSignature(policy2);
            vhdDestUri += destinationBlobToken2;
            return vhdDestUri;
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\overwrite_VHD.csv", "overwrite_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskOverwrite()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Start uploading...
            Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
            vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName));
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName, true));
            Console.WriteLine("uploading completed: {0}", vhdName);

            // Verify the upload.
            AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, vhdLocalPath, vhdUploadContext, md5hash);

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime-testStartTime).TotalSeconds);
            
            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime-testStartTime).TotalSeconds) });

        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\overwrite_VHD.csv", "overwrite_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskResume()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Start uploading...
            Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
            string result = vmPowershellCmdlets.AddAzureVhdStop(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName), 500);

            if (result.ToLowerInvariant() == "stopped")
            {
                Console.WriteLine("successfully stopped");

                var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName));
                Console.WriteLine("uploading completed: {0}", vhdName);

                // Verify the upload.
                AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, vhdLocalPath, vhdUploadContext, md5hash);
            }
            else
            {
                Console.WriteLine("didn't stop!");
            }            

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }
        

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\overwrite_VHD.csv", "overwrite_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskOverwriteSasUri()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            int i = 0;
            while (i < 16)
            {
                string destinationSasUri2 = CreateSasUriWithPermission(vhdName, i);
                try
                {
                    Console.WriteLine("uploads {0} to {1}", vhdName, destinationSasUri2);
                    vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName));
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName, true));
                    Console.WriteLine("Finished uploading: {0}", destinationSasUri2);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUri2, vhdLocalPath, vhdUploadContext, md5hash);
                    Console.WriteLine("Test success with permission: {0}", i);
                    i++;
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("already running"))
                    {
                        Console.WriteLine(e.InnerException.Message);
                        continue;
                    }
                    if (i != 7 && i != 15)
                    {
                        Console.WriteLine("Error as expected.  Permission: {0}", i);
                        Console.WriteLine("Error message: {0}", e.InnerException.Message);
                        i++;
                        continue;
                    }
                    else
                    {
                        Assert.Fail("Test failed Permission: {0} \n {1}", i, e.ToString());
                    }
                }
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\overwrite_VHD.csv", "overwrite_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskOverwriteNonExist()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Start uploading...
            Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
            //vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName));
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName, true));
            Console.WriteLine("uploading completed: {0}", vhdName);

            // Verify the upload.
            AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, vhdLocalPath, vhdUploadContext, md5hash);

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });

        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\overwrite_VHD.csv", "overwrite_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskOverwriteNonExistSasUri()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            int i = 0;
            while (i < 16)
            {
                string destinationSasUri2 = CreateSasUriWithPermission(vhdName, i);
                try
                {
                    Console.WriteLine("uploads {0} to {1}", vhdName, destinationSasUri2);
                    //vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName));
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName, true));
                    Console.WriteLine("Finished uploading: {0}", destinationSasUri2);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUri2, vhdLocalPath, vhdUploadContext, md5hash);
                    Console.WriteLine("Test success with permission: {0}", i);
                    i++;
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("already running"))
                    {
                        Console.WriteLine(e.InnerException.Message);
                        continue;
                    }
                    if (i != 7 && i != 15)
                    {
                        Console.WriteLine("Error as expected.  Permission: {0}", i);
                        Console.WriteLine("Error message: {0}", e.InnerException.Message);
                        i++;
                        continue;
                    }
                    else
                    {
                        Assert.Fail("Test failed Permission: {0} \n {1}", i, e.ToString());
                    }
                }
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\overwrite_VHD.csv", "overwrite_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskSecondWithoutOverwrite()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Start uploading...
            Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName));

            try
            {
                Console.WriteLine("uploads {0} to {1} second times", vhdName, vhdBlobName);
                vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName));
                Assert.Fail("Must have failed!  Test failed for: {0}", vhdLocalPath.FullName);
            }
            catch (Exception)
            {
                Console.WriteLine("Failed as expected while uploading {0} second time without overwrite", vhdLocalPath.Name);
            }

            // Verify the upload.
            AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, vhdLocalPath, vhdUploadContext, md5hash);

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\overwrite_VHD.csv", "overwrite_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskSecondWithoutOverwriteSasUri()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            
            for (int i = 0; i < 16; i++)            
            {
                string destinationSasUri2 = CreateSasUriWithPermission(vhdName, i);
                try
                {
                    Console.WriteLine("uploads {0} to {1}", vhdName, destinationSasUri2);
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName));

                    try
                    {
                        Console.WriteLine("uploads {0} to {1} second times", vhdName, destinationSasUri2);
                        vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName));
                        Assert.Fail("Must have failed!  Test failed for: {0}", vhdLocalPath.FullName);                        
                    }
                    catch (Exception e)
                    {                        
                        Console.WriteLine("Failed as expected while uploading {0} second time without overwrite: {1}", vhdLocalPath.Name, e.InnerException.Message);

                    }

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUri2, vhdLocalPath, vhdUploadContext, md5hash);
                    Console.WriteLine("Test success with permission: {0}", i);
                }
                catch (Exception e)
                {
                    if (i != 3 && i != 7 && i != 11 && i != 15)
                    {
                        Console.WriteLine("Error as expected.  Permission: {0}", i);
                        Console.WriteLine("Error message: {0}", e.InnerException.Message);
                        continue;
                    }
                    else
                    {
                        Assert.Fail("Test failed.  Permission: {0}", i);
                    }
                }
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }



        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\thread_VHD.csv", "thread_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskThreadNumber()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Start uploading...
            Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName, 16, false));
            Console.WriteLine("uploading completed: {0}", vhdName);

            // Verify the upload.
            AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, vhdLocalPath, vhdUploadContext, md5hash);

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\thread_VHD.csv", "thread_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskThreadNumberSasUri()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            int i = 0;            
            while (i < 16)
            {
                string destinationSasUri2 = CreateSasUriWithPermission(vhdName, i);
                try
                {
                    Console.WriteLine("uploads {0} to {1}", vhdName, destinationSasUri2);
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName, 16, false));
                    Console.WriteLine("uploading completed: {0}", vhdName);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUri2, vhdLocalPath, vhdUploadContext, md5hash);
                    Console.WriteLine("Test success with permission: {0}", i);
                    i++;
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("already running"))
                    {
                        Console.WriteLine(e.InnerException.Message);
                        continue;
                    }
                    if (i != 3 && i != 7 && i != 11 && i != 15)
                    {
                        Console.WriteLine("Error as expected.  Permission: {0}", i);
                        Console.WriteLine("Error message: {0}", e.InnerException.Message);
                        i++;
                        continue;
                    }
                    else
                    {
                        Assert.Fail("Test failed Permission: {0} \n {1}", i, e.ToString());
                    }
                }
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\thread_VHD.csv", "thread_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskThreadNumberOverwrite()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Start uploading...
            Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
            vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName));
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName, 16, true));
            Console.WriteLine("uploading completed: {0}", vhdName);

            // Verify the upload.
            AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, vhdLocalPath, vhdUploadContext, md5hash);

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\thread_VHD.csv", "thread_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskThreadNumberOverwriteSasUri()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            for (int i = 0; i < 16; i++)
            {
                string destinationSasUri2 = CreateSasUriWithPermission(vhdName, i);
                try
                {
                    Console.WriteLine("uploads {0} to {1}", vhdName, destinationSasUri2);
                    vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName));
                    Console.WriteLine("uploaded: {0}", vhdName); 
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, vhdLocalPath.FullName, 16, true));
                    Console.WriteLine("uploading overwrite completed: {0}", vhdName);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUri2, vhdLocalPath, vhdUploadContext, md5hash);
                    Console.WriteLine("Test success with permission: {0}", i);
                }
                catch (Exception e)
                {
                    if (i != 7 && i != 15)
                    {
                        Console.WriteLine("Error as expected.  Permission: {0}", i);
                        Console.WriteLine("Error message: {0}", e.InnerException.Message);
                        continue;
                    }
                    else
                    {
                        Assert.Fail("Test failed Permission: {0} \n {1}", i, e.ToString());
                    }
                }
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\patch_VHD.csv", "patch_VHD#csv", DataAccessMethod.Sequential)]
        public void PatchFirstLevelDifferencingDisk()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["baseImage"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);
            string md5hashBase = Convert.ToString(TestContext.DataRow["MD5hashBase"]);


            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Start uploading...
            Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName, true));
            Console.WriteLine("uploading completed: {0}", vhdName);

            // Verify the upload.
            AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, vhdLocalPath, vhdUploadContext, md5hashBase, false);


            // Choose the vhd file from local machine
            var childVhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var childVhdLocalPath = new FileInfo(@".\" + childVhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", childVhdLocalPath);

            // Set the destination
            string childVhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(childVhdName)));
            string childVhdDestUri = blobUrlRoot + childVhdBlobName;

            // Start uploading the child vhd...
            Console.WriteLine("uploads {0} to {1}", childVhdName, childVhdBlobName);
            var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(childVhdDestUri, childVhdLocalPath.FullName, vhdDestUri));
            Console.WriteLine("uploading completed: {0}", childVhdName);

            // Verify the upload
            AssertUploadContextAndContentMD5UsingSaveVhd(childVhdDestUri, childVhdLocalPath, patchVhdUploadContext, md5hash);

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\patch_VHD.csv", "patch_VHD#csv", DataAccessMethod.Sequential)]
        public void PatchFirstLevelDifferencingDiskSasUri()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the base vhd file from local machine
            var baseVhdName = Convert.ToString(TestContext.DataRow["baseImage"]);
            var baseVhdLocalPath = new FileInfo(@".\" + baseVhdName);
            Assert.IsTrue(File.Exists(baseVhdLocalPath.FullName), "VHD file not exist={0}", baseVhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);
            string md5hashBase = Convert.ToString(TestContext.DataRow["MD5hashBase"]);


            // Choose the child vhd file from the local machine

            var childVhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var childVhdLocalPath = new FileInfo(@".\" + childVhdName);
            Assert.IsTrue(File.Exists(childVhdLocalPath.FullName), "VHD file not exist={0}", childVhdLocalPath);

            for (int i = 0; i < 16; i++)
            {
                string destinationSasUri2 = CreateSasUriWithPermission(baseVhdName, i);
                string destinationSasUri3 = CreateSasUriWithPermission(childVhdName, i);
                try
                {
                    Console.WriteLine("uploads {0} to {1}", baseVhdName, destinationSasUri2);
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri2, baseVhdLocalPath.FullName, true));
                    Console.WriteLine("uploading completed: {0}", baseVhdName);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUri2, baseVhdLocalPath, vhdUploadContext, md5hashBase, false);


                    Console.WriteLine("uploads {0} to {1}", childVhdName, destinationSasUri3);
                    var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUri3, childVhdLocalPath.FullName, destinationSasUri2));
                    Console.WriteLine("uploading completed: {0}", childVhdName);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUri3, childVhdLocalPath, patchVhdUploadContext, md5hash);
                    Console.WriteLine("Test success with permission: {0}", i);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error as expected.  Permission: {0}", i);
                    Console.WriteLine("Error message: {0}", e.InnerException.Message);
                    continue;                    
                }
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\patch_VHD.csv", "patch_VHD#csv", DataAccessMethod.Sequential)]
        public void PatchSasUriNormalBaseShouldFail()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the base vhd file from local machine
            var baseVhdName = Convert.ToString(TestContext.DataRow["baseImage"]);
            var baseVhdLocalPath = new FileInfo(@".\" + baseVhdName);
            Assert.IsTrue(File.Exists(baseVhdLocalPath.FullName), "VHD file not exist={0}", baseVhdLocalPath);

            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(baseVhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);
            string md5hashBase = Convert.ToString(TestContext.DataRow["MD5hashBase"]);


            Console.WriteLine("uploads {0} to {1}", baseVhdName, vhdDestUri);
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, baseVhdLocalPath.FullName, true));
            Console.WriteLine("uploading the parent vhd completed: {0}", baseVhdName);

            // Choose the child vhd file from the local machine
            var childVhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var childVhdLocalPath = new FileInfo(@".\" + childVhdName);
            Assert.IsTrue(File.Exists(childVhdLocalPath.FullName), "VHD file not exist={0}", childVhdLocalPath);

            for (int i = 0; i < 16; i++)
            {
                string destinationSasUriParent = CreateSasUriWithPermission(baseVhdName, i);
                string destinationSasUriChild = CreateSasUriWithPermission(childVhdName, i);
                try
                {
                    Console.WriteLine("uploads {0} to {1} with patching from {2}", childVhdName, destinationSasUriChild, vhdDestUri);
                    var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUriChild, childVhdLocalPath.FullName, vhdDestUri));
                    Console.WriteLine("uploading the child vhd completed: {0}", childVhdName);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUriChild, childVhdLocalPath, patchVhdUploadContext, md5hash);
                    Console.WriteLine("Test success with permission: {0}", i);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error as expected.  Permission: {0}", i);
                    Console.WriteLine("Error message: {0}", e.InnerException.Message);
                    continue;
                }
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\patch_VHD.csv", "patch_VHD#csv", DataAccessMethod.Sequential)]
        public void PatchNormalSasUriBase()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);

            // Choose the base vhd file from local machine
            var baseVhdName = Convert.ToString(TestContext.DataRow["baseImage"]);
            var baseVhdLocalPath = new FileInfo(@".\" + baseVhdName);
            Assert.IsTrue(File.Exists(baseVhdLocalPath.FullName), "VHD file not exist={0}", baseVhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);
            string md5hashBase = Convert.ToString(TestContext.DataRow["MD5hashBase"]);

        
            // Choose the child vhd file from the local machine
            var childVhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var childVhdLocalPath = new FileInfo(@".\" + childVhdName);
            Assert.IsTrue(File.Exists(childVhdLocalPath.FullName), "VHD file not exist={0}", childVhdLocalPath);

            int i = 0;
            while (i < 16)
            {
                string destinationSasUriParent = CreateSasUriWithPermission(baseVhdName, i); // the destination of the parent vhd is a Sas Uri

                // Set the destination of child vhd
                string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(childVhdName)));
                string vhdDestUri = blobUrlRoot + vhdBlobName;

                try
                {
                    // Upload the parent vhd using Sas Uri
                    Console.WriteLine("uploads {0} to {1}", baseVhdName, destinationSasUriParent);
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(destinationSasUriParent, baseVhdLocalPath.FullName, true));
                    Console.WriteLine("uploading completed: {0}", baseVhdName);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUriParent, baseVhdLocalPath, vhdUploadContext, md5hashBase, false);

                    Console.WriteLine("uploads {0} to {1} with patching from {2}", childVhdName, vhdDestUri, destinationSasUriParent);
                    var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, childVhdLocalPath.FullName, destinationSasUriParent));
                    Console.WriteLine("uploading the child vhd completed: {0}", childVhdName);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, childVhdLocalPath, patchVhdUploadContext, md5hash);
                    Console.WriteLine("Test success with permission: {0}", i);
                    i++;
                }
                catch (Exception e)
                {
                    if (i != 3 && i != 7 && i != 11 && i != 15)
                    {
                        Console.WriteLine("Error as expected.  Permission: {0}", i);
                        Console.WriteLine("Error message: {0}", e.InnerException.Message);
                        i++;
                        continue;
                    }
                    else
                    {
                        Assert.Fail("Test failed Permission: {0} \n {1}", i, e.ToString());
                    }                   
                }
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\thread_VHD.csv", "thread_VHD#csv", DataAccessMethod.Sequential)]
        public void WrongProtocolShouldFail()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);


            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(@".\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Set the destination
            string vhdBlobName = string.Format("vhdstore/{0}.vhd", Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string badUrlRoot = string.Format(@"badprotocolhttp://{0}.blob.core.windows.net/", defaultAzureSubscription.CurrentStorageAccount);
            string vhdDestUri = badUrlRoot + vhdBlobName;

            DateTime startTime = DateTime.Now;
            try
            {
                // Start uploading...
                Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
                var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(vhdDestUri, vhdLocalPath.FullName));
                Console.WriteLine("uploading completed: {0}", vhdName);
                Assert.Fail("Should have failed. {0} test failed.", testName);

            }
            catch (Exception e)
            {
                TimeSpan duration = DateTime.Now - startTime;
                Console.WriteLine("error message: {0}", e);
                Console.WriteLine("{0} test passed after {1} seconds", testName, duration.Seconds);

                DateTime testEndTime = DateTime.Now;
                Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
                Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

                System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
            }
        }





        private void AssertUploadContextAndContentMD5UsingSaveVhd(string destination, FileInfo localFile, VhdUploadContext vhdUploadContext, string md5hash, bool deleteBlob = true)
        {
            AssertUploadContext(destination, localFile, vhdUploadContext);

            FileInfo downloadFile = new FileInfo(localFile.FullName + "_download.vhd");            
           
            BlobHandle blobHandle = getBlobHandle(destination);

            Assert.IsTrue(VerifyMD5hash(blobHandle, md5hash));
            SaveVhdAndAssertContent(blobHandle, downloadFile, deleteBlob);            
        }

        private BlobHandle getBlobHandle(string blob)
        {
            BlobUri blobPath;
            Assert.IsTrue(BlobUri.TryParseUri(new Uri(blob), out blobPath));
            return new BlobHandle(blobPath, storageAccountKey.Primary);
        }

        private bool VerifyMD5hash(BlobHandle blobHandle, string md5hash)
        {               
            Console.WriteLine("MD5 hash of blob, {0}, is {1}", blobHandle.Blob.Uri.ToString(), blobHandle.Blob.Properties.ContentMD5);
            Console.WriteLine("MD5 hash of the local file: {0}", md5hash);
            return String.Equals(blobHandle.Blob.Properties.ContentMD5, md5hash);
        }

        private void SaveVhdAndAssertContent(BlobHandle destination, FileInfo localFile, bool deleteBlob)
        {
            SaveVhdAndAssertContent(destination, localFile, null, null, false, deleteBlob);
        }

        private void SaveVhdAndAssertContent(BlobHandle destination, FileInfo localFile, int? numThread, bool deleteBlob)
        {
            SaveVhdAndAssertContent(destination, localFile, numThread, null, false, deleteBlob);
        }

        private void SaveVhdAndAssertContent(BlobHandle destination, FileInfo localFile, string storageKey, bool deleteBlob)
        {
            SaveVhdAndAssertContent(destination, localFile, null, storageKey, false, deleteBlob);
        }

        private void SaveVhdAndAssertContent(BlobHandle destination, FileInfo localFile, bool overwrite, bool deleteBlob)
        {
            SaveVhdAndAssertContent(destination, localFile, null, null, overwrite, deleteBlob);
        }
       

        private void SaveVhdAndAssertContent(BlobHandle destination, FileInfo localFile, int? numThread, string storageKey, bool overwrite, bool deleteBlob)
        {
            try
            {
                Console.WriteLine("Downloading a VHD from {0} to {1}...", destination.Blob.Uri.ToString(), localFile.FullName);
                DateTime startTime = DateTime.Now;
                VhdDownloadContext result = vmPowershellCmdlets.SaveAzureVhd(destination.Blob.Uri, localFile, numThread, storageKey, overwrite);                
                Console.WriteLine("Downloading completed in {0} seconds.", (DateTime.Now - startTime).TotalSeconds);
                               

                string calculateMd5Hash = CalculateContentMd5(File.OpenRead(result.LocalFilePath.FullName));
                                
                Assert.IsTrue(VerifyMD5hash(destination, calculateMd5Hash));

                if (deleteBlob)
                {
                    destination.Blob.Delete();
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.InnerException.ToString());
            }
        }



        private void AssertUploadContext(string destination, FileInfo localFile, VhdUploadContext vhdUploadContext)
        {
            Assert.IsNotNull(vhdUploadContext);
            Assert.AreEqual(new Uri(destination), vhdUploadContext.DestinationUri);
            Assert.AreEqual(vhdUploadContext.LocalFilePath.FullName, localFile.FullName);            
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


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Save-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\download_VHD.csv", "download_VHD#csv", DataAccessMethod.Sequential)]
        public void SaveAzureVhdThreadNumberTest()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);

            // Set the source blob
            string vhdBlobLocation = blobUrlRoot + Convert.ToString(TestContext.DataRow["vhdBlobLocation"]);
            BlobHandle blobHandle = getBlobHandle(vhdBlobLocation);


            // Choose the vhd path in your local machine            
            string vhdName = Convert.ToString(TestContext.DataRow["vhdLocalPath"]) + Utilities.GetUniqueShortName();
            FileInfo vhdLocalPath = new FileInfo(vhdName);


            // Download with 2 threads and verify it.
            SaveVhdAndAssertContent(blobHandle, vhdLocalPath, 2, false);


            // Choose the vhd path in your local machine            
            string vhdName2 = Convert.ToString(TestContext.DataRow["vhdLocalPath"]) + Utilities.GetUniqueShortName();
            FileInfo vhdLocalPath2 = new FileInfo(vhdName2);

            // Download with 16 threads and verify it.
            SaveVhdAndAssertContent(blobHandle, vhdLocalPath2, 16, false);



            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }



        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Save-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\download_VHD.csv", "download_VHD#csv", DataAccessMethod.Sequential)]
        public void SaveAzureVhdStorageKeyTest()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);

            // Set the source blob
            string vhdBlobLocation = blobUrlRoot + Convert.ToString(TestContext.DataRow["vhdBlobLocation"]);
            BlobHandle blobHandle = getBlobHandle(vhdBlobLocation);
            

            // Choose the vhd path in your local machine            
            string vhdName = Convert.ToString(TestContext.DataRow["vhdLocalPath"]) + Utilities.GetUniqueShortName();
            FileInfo vhdLocalPath = new FileInfo(vhdName);


            // Download with a secondary storage key and verify it.
            SaveVhdAndAssertContent(blobHandle, vhdLocalPath, storageAccountKey.Secondary, false);
                        
            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Save-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\download_VHD.csv", "download_VHD#csv", DataAccessMethod.Sequential)]
        public void SaveAzureVhdOverwriteTest()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);

            // Set the source blob
            string vhdBlobLocation = blobUrlRoot + Convert.ToString(TestContext.DataRow["vhdBlobLocation"]);
            BlobHandle blobHandle = getBlobHandle(vhdBlobLocation);


            // Choose the vhd path in your local machine            
            string vhdName = Convert.ToString(TestContext.DataRow["vhdLocalPath"]);// +Utilities.GetUniqueShortName();
            FileInfo vhdLocalPath = new FileInfo(vhdName);


            // Download and verify it.
            SaveVhdAndAssertContent(blobHandle, vhdLocalPath, false);

            // Download with overwrite and verify it.
            SaveVhdAndAssertContent(blobHandle, vhdLocalPath, true, false);

            // Try to download without overwrite.
            try
            {
                SaveVhdAndAssertContent(blobHandle, vhdLocalPath, false);
                Assert.Fail("Must fail");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }



            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Save-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\download_VHD.csv", "download_VHD#csv", DataAccessMethod.Sequential)]
        public void SaveAzureVhdAllTest()
        {            
            string testName = MethodBase.GetCurrentMethod().Name;                        

            // Set the source blob
            string vhdBlobLocation = blobUrlRoot + Convert.ToString(TestContext.DataRow["vhdBlobLocation"]);
            BlobHandle blobHandle = getBlobHandle(vhdBlobLocation);


            // Choose the vhd path in your local machine            
            string vhdName = Convert.ToString(TestContext.DataRow["vhdLocalPath"]) +Utilities.GetUniqueShortName();
            FileInfo vhdLocalPath = new FileInfo(vhdName);


            // Download and verify it.
            SaveVhdAndAssertContent(blobHandle, vhdLocalPath, 16, storageAccountKey.Secondary, true, false);

            // Download with overwrite and verify it.
            SaveVhdAndAssertContent(blobHandle, vhdLocalPath, 32, storageAccountKey.Primary, true, false);


            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);            

            //System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Save-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\download_VHD.csv", "download_VHD#csv", DataAccessMethod.Sequential)]
        public void SaveAzureVhdResumeTest()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);

            // Set the source blob
            string vhdBlobLocation = blobUrlRoot + Convert.ToString(TestContext.DataRow["vhdBlobLocation"]);
            BlobHandle blobHandle = getBlobHandle(vhdBlobLocation);
            //Uri vhdSourceLocation = new Uri(vhdBlobLocation);

            // Choose the vhd path in your local machine            
            string vhdName = Convert.ToString(TestContext.DataRow["vhdLocalPath"]) + Utilities.GetUniqueShortName();
            FileInfo vhdLocalPath = new FileInfo(vhdName);
            Assert.IsFalse(File.Exists(vhdLocalPath.FullName), "VHD file already exist={0}", vhdLocalPath);

            

            // Start uploading and stop after 5 seconds...
            Console.WriteLine("downloading {0} to {1}", vhdBlobLocation, vhdLocalPath);
            string result = vmPowershellCmdlets.SaveAzureVhdStop(blobHandle.Blob.Uri, vhdLocalPath, null, null, false, 5000);

            if (result.ToLowerInvariant() == "stopped")
            {
                Console.WriteLine("successfully stopped");


                SaveVhdAndAssertContent(blobHandle, vhdLocalPath, false);                                
            }
            else
            {
                Console.WriteLine("didn't stop!");
            }

            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Save-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\wrongPara_VHD.csv", "wrongPara_VHD#csv", DataAccessMethod.Sequential)]
        public void SaveAzureVhdWrongParaTest()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            DateTime testStartTime = DateTime.Now;
            Console.WriteLine("{0} test starts at {1}", testName, testStartTime);

            // Set the source blob
            string vhdBlobLocation = blobUrlRoot + Convert.ToString(TestContext.DataRow["vhdBlobLocation"]);
            string vhdName = Convert.ToString(TestContext.DataRow["vhdLocalPath"]);
            string numThreadstr = Convert.ToString(TestContext.DataRow["numThread"]);
            int? numThread = String.IsNullOrWhiteSpace(numThreadstr) ? (int?) null : Int32.Parse(numThreadstr);
            string storageKeystr = Convert.ToString(TestContext.DataRow["storageKey"]);
            string storageKey = String.IsNullOrWhiteSpace(storageKeystr) ? (string)null : storageKeystr;

            // Download and verify it.
            try
            {
                vmPowershellCmdlets.SaveAzureVhd(new Uri(vhdBlobLocation), new FileInfo(vhdName), numThread, storageKey, false);
                Assert.Fail("Should have failed!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred as expected.  Exception: {0}", e.ToString());
            }


            DateTime testEndTime = DateTime.Now;
            Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
        }

        


        //private void AssertUploadContextAndContentMD5(string destination, FileInfo localFile, VhdUploadContext vhdUploadContext, bool deleteBlob = true)
        //{
        //    AssertUploadContext(destination, localFile, vhdUploadContext);
        //    BlobUri blobPath;
        //    Assert.IsTrue(BlobUri.TryParseUri(new Uri(destination), out blobPath));
        //    AssertContentMD5(blobPath, deleteBlob);
        //}

        //private void AssertContentMD5(BlobUri destination, bool deleteBlob)
        //{
        //    string downloadedFile = DownloadToFile(destination);

        //    var calculateMd5Hash = CalculateContentMd5(File.OpenRead(downloadedFile));

        //    var blobHandle = new BlobHandle(destination, storageAccountKey.Primary);

        //    Assert.AreEqual(calculateMd5Hash, blobHandle.Blob.Properties.ContentMD5);

        //    if (deleteBlob)
        //    {
        //        blobHandle.Blob.Delete();
        //    }
        //}


        //private string DownloadToFile(BlobUri destination)
        //{
        //    var downloadedFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
        //    var downloader = new Downloader(destination, storageAccountKey.Primary, downloadedFile);

        //    downloader.Download();
        //    return downloadedFile;
        //}



    }
}
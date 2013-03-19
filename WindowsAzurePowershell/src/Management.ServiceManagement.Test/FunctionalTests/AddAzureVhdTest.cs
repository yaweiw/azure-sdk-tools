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
    using Microsoft.VisualStudio.TestTools.UnitTesting;    
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Sync.Download;

    [TestClass]
    public class AddAzureVhdTest : AzureVhdTest
    {

        [TestInitialize]
        public void Initialize()
        {
            pass = true;
            testStartTime = DateTime.Now;

            // Set the source blob
            //blobHandle = getBlobHandle(vhdBlobLocation);               
        }


        /// <summary>
        /// UploadDisk: 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\upload_VHD.csv", "upload_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDisk()
        {
           
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            
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
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri);
            Console.WriteLine("uploading completed: {0}", vhdName);

            // Verify the upload.
            AssertUploadContextAndContentMD5UsingSaveVhd(vhdDestUri, vhdLocalPath, vhdUploadContext, md5hash);

            pass = true;
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Add-AzureVhd)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\upload_VHD.csv", "upload_VHD#csv", DataAccessMethod.Sequential)]
        public void UploadDiskSasUri()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);


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
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, destinationSasUri2);
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
                        Console.WriteLine(e.ToString());
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
            testName = MethodBase.GetCurrentMethod().Name;
            StartTest(testName, testStartTime);


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
            vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri);
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri, true);
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);


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
            string result = vmPowershellCmdlets.AddAzureVhdStop(vhdLocalPath, vhdDestUri, 500);

            if (result.ToLowerInvariant() == "stopped")
            {
                Console.WriteLine("successfully stopped");

                var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri);
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);


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
                    vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, destinationSasUri2);
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, destinationSasUri2, true);
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
                        Console.WriteLine(e.ToString());
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);


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
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri, true);
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);


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
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, destinationSasUri2, true);
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
                        Console.WriteLine(e.ToString());
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);


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
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri);

            try
            {
                Console.WriteLine("uploads {0} to {1} second times", vhdName, vhdBlobName);
                vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri);
                pass = false;
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, destinationSasUri2);

                    try
                    {
                        Console.WriteLine("uploads {0} to {1} second times", vhdName, destinationSasUri2);
                        vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, destinationSasUri2);
                        pass = false;
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri, 16);
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, destinationSasUri2, 16);
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
                        Console.WriteLine(e.ToString());
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
            vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri);
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri, 16, true);
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
                    vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, destinationSasUri2);
                    Console.WriteLine("uploaded: {0}", vhdName); 
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, destinationSasUri2, 16, true);
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri,true);
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
            //var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(new AddAzureVhdCmdletInfo(childVhdDestUri, childVhdLocalPath.FullName, vhdDestUri));
            var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(childVhdLocalPath, childVhdDestUri, vhdDestUri);
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(baseVhdLocalPath, destinationSasUri2, true);
                    Console.WriteLine("uploading completed: {0}", baseVhdName);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUri2, baseVhdLocalPath, vhdUploadContext, md5hashBase, false);


                    Console.WriteLine("uploads {0} to {1}", childVhdName, destinationSasUri3);
                    var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(childVhdLocalPath, destinationSasUri3, destinationSasUri2);
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
        [Ignore]
        public void PatchSasUriNormalBaseShouldFail()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(baseVhdLocalPath, vhdDestUri, true);
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
                    var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(childVhdLocalPath, destinationSasUriChild, vhdDestUri);
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
                    var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(baseVhdLocalPath, destinationSasUriParent, true);
                    Console.WriteLine("uploading completed: {0}", baseVhdName);

                    // Verify the upload.
                    AssertUploadContextAndContentMD5UsingSaveVhd(destinationSasUriParent, baseVhdLocalPath, vhdUploadContext, md5hashBase, false);

                    Console.WriteLine("uploads {0} to {1} with patching from {2}", childVhdName, vhdDestUri, destinationSasUriParent);
                    var patchVhdUploadContext = vmPowershellCmdlets.AddAzureVhd(childVhdLocalPath, vhdDestUri, destinationSasUriParent);
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
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

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
                var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri);
                Console.WriteLine("uploading completed: {0}", vhdName);
                pass = false;

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





        private void AssertUploadContextAndContentMD5UsingSaveVhd(string destination, FileInfo localFile, VhdUploadContext vhdUploadContext, string md5hash, bool deleteBlob = true, bool deleteLocal = true)
        {
            AssertUploadContext(destination, localFile, vhdUploadContext);

            FileInfo downloadFile = new FileInfo(localFile.FullName + "_download.vhd");            
           
            BlobHandle blobHandle = getBlobHandle(destination);

            Assert.IsTrue(VerifyMD5hash(blobHandle, md5hash));
            SaveVhdAndAssertContent(blobHandle, downloadFile, true, deleteBlob, deleteLocal);            
        }

        private BlobHandle getBlobHandle(string blob)
        {
            BlobUri blobPath;
            Assert.IsTrue(BlobUri.TryParseUri(new Uri(blob), out blobPath));
            return new BlobHandle(blobPath, storageAccountKey.Primary);
        }


        private void AssertUploadContext(string destination, FileInfo localFile, VhdUploadContext vhdUploadContext)
        {
            Assert.IsNotNull(vhdUploadContext);
            Assert.AreEqual(new Uri(destination), vhdUploadContext.DestinationUri);
            Assert.AreEqual(vhdUploadContext.LocalFilePath.FullName, localFile.FullName);            
        }

        

        [TestCleanup]
        public virtual void CleanUp()
        {
            Console.WriteLine("Test {0}", pass ? "passed" : "failed");
            
            //if (pass)
            //{                
            //    Console.WriteLine("Test passed.");

            //    //DateTime testEndTime = DateTime.Now;
            //    //Console.WriteLine("{0} test passed at {1}.", testName, testEndTime);
            //    //Console.WriteLine("Duration of the test pass: {0} seconds", (testEndTime - testStartTime).TotalSeconds);

            //    //System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("{0},{1}", testName, (testEndTime - testStartTime).TotalSeconds) });
            //}
            //else
            //{
            //    Assert.Fail("Test failed.");
            //}
        }
    }
}
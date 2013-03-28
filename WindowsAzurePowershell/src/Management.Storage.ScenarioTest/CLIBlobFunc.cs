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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CLITest.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MS.Test.Common.MsTestLib;
using StorageTestLib;

namespace CLITest
{
    /// <summary>
    /// this class contains all the functional test cases for PowerShell Blob cmdlets
    /// </summary>
    [TestClass]
    class CLIBlobFunc
    {
        private static CloudStorageAccount StorageAccount;
        private static CloudBlobHelper BlobHelper;
        private static string BlockFilePath;
        private static string PageFilePath;

        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            Trace.WriteLine("ClassInit");
            Test.FullClassName = testContext.FullyQualifiedTestClassName;

            string ConnectionString = Test.Data.Get("StorageConnectionString");
            StorageAccount = CloudStorageAccount.Parse(ConnectionString);

            //init the blob helper for blob related operations
            BlobHelper = new CloudBlobHelper(StorageAccount);

            // import module
            string moduleFilePath = Test.Data.Get("ModuleFilePath");
            if (moduleFilePath.Length > 0)
                PowerShellAgent.ImportModule(moduleFilePath);

            // $context = New-AzureStorageContext -ConnectionString ...
            PowerShellAgent.SetStorageContext(ConnectionString);

            BlockFilePath = Test.Data.Get("BlockFilePath");
            PageFilePath = Test.Data.Get("PageFilePath");

            // Generate block file and page file which are used for uploading
            Helper.GenerateMediumFile(BlockFilePath, 1);
            Helper.GenerateMediumFile(PageFilePath, 1);
        }

        //
        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            Trace.WriteLine("ClasssCleanup");
        }

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            Trace.WriteLine("TestInit");
            Test.Start(TestContext.FullyQualifiedTestClassName, TestContext.TestName);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            Trace.WriteLine("TestCleanup");
            // do not clean up the blobs here for investigation
            // every test case should do cleanup in its init
            Test.End(TestContext.FullyQualifiedTestClassName, TestContext.TestName);
        }

        #endregion

        [TestMethod]
        [TestCategory(Tag.Function)]
        public void RootBlobOperations()
        {
            string DownloadDirPath = Test.Data.Get("DownloadDirPath");

            RootBlobOperations(new PowerShellAgent(), BlockFilePath, DownloadDirPath, Microsoft.WindowsAzure.Storage.Blob.BlobType.BlockBlob);
            RootBlobOperations(new PowerShellAgent(), PageFilePath, DownloadDirPath, Microsoft.WindowsAzure.Storage.Blob.BlobType.PageBlob);
        }

        [TestMethod]
        [TestCategory(Tag.Function)]
        public void CreateInvalidBlob()
        {
            CreateInvalidBlob(new PowerShellAgent());
        }

        [TestMethod]
        [TestCategory(Tag.Function)]
        public void CreateExistingBlob()
        {
            CreateExistingBlob(new PowerShellAgent());
        }

        [TestMethod]
        [TestCategory(Tag.Function)]
        public void BlobListOperations()
        {
            BlobListOperations(new PowerShellAgent());
        }

        [TestMethod]
        [TestCategory(Tag.Function)]
        public void GetNonExistingBlob()
        {
            GetNonExistingBlob(new PowerShellAgent());
        }

        [TestMethod]
        [TestCategory(Tag.Function)]
        public void EnumerateAllBlobs()
        {
            EnumerateAllBlobs(new PowerShellAgent());
        }

        [TestMethod]
        [TestCategory(Tag.Function)]
        public void RemoveNonExistingBlob()
        {
            RemoveNonExistingBlob(new PowerShellAgent());
        }

        [TestMethod]
        [TestCategory(Tag.Function)]
        public void RemoveBlobWithoutForce()
        {
            RemoveBlobWithoutForce(new PowerShellAgent());
        }

        /// <summary>
        /// Functional Cases:
        /// 1. Upload a new blob file in the root container     (Set-AzureStorageBlobContent Positive 2)
        /// 2. Get an existing blob in the root container       (Get-AzureStorageBlob Positive 2)
        /// 3. Download an existing blob in the root container  (Get-AzureStorageBlobContent Positive 2)
        /// 4. Remove an existing blob in the root container    (Remove-AzureStorageBlob Positive 2)
        /// </summary>
        internal void RootBlobOperations(Agent agent, string UploadFilePath, string DownloadDirPath, Microsoft.WindowsAzure.Storage.Blob.BlobType Type)
        {
            const string ROOT_CONTAINER_NAME = "$root";
            string blobName = Path.GetFileName(UploadFilePath);
            string downloadFilePath = Path.Combine(DownloadDirPath, blobName);

            Collection<Dictionary<string, object>> comp = new Collection<Dictionary<string, object>>();
            Dictionary<string, object> dic = Utility.GenComparisonData(StorageObjectType.Blob, blobName);

            dic["BlobType"] = Type;
            comp.Add(dic);

            // create the container
            CloudBlobContainer container = StorageAccount.CreateCloudBlobClient().GetRootContainerReference();
            container.CreateIfNotExists();

            //--------------Upload operation--------------
            Test.Assert(agent.SetAzureStorageBlobContent(UploadFilePath, ROOT_CONTAINER_NAME, Type), Utility.GenComparisonData("SendAzureStorageBlob", true));
            ICloudBlob blob = BlobHelper.QueryBlob(ROOT_CONTAINER_NAME, blobName);
            blob.FetchAttributes();
            // Verification for returned values
            CloudBlobUtil.PackBlobCompareData(blob, dic);
            agent.OutputValidation(comp);

            Test.Assert(blob.Exists(), "blob " + blobName + " should exist!");

            // validate the ContentType value for GetAzureStorageBlob operation
            if (Type == Microsoft.WindowsAzure.Storage.Blob.BlobType.BlockBlob)
            {
                dic["ContentType"] = "application/octet-stream";
            }

            //--------------Get operation--------------
            Test.Assert(agent.GetAzureStorageBlob(blobName, ROOT_CONTAINER_NAME), Utility.GenComparisonData("GetAzureStorageBlob", true));
            // Verification for returned values
            agent.OutputValidation(comp);

            //--------------Download operation--------------
            downloadFilePath = Path.Combine(DownloadDirPath, blobName);    
            Test.Assert(agent.GetAzureStorageBlobContent(blobName, downloadFilePath, ROOT_CONTAINER_NAME),
                Utility.GenComparisonData("GetAzureStorageBlobContent", true));
            // Verification for returned values
            agent.OutputValidation(comp);

            Test.Assert(Helper.CompareTwoFiles(downloadFilePath, UploadFilePath),
                String.Format("File '{0}' should be bit-wise identicial to '{1}'", downloadFilePath, UploadFilePath));

            //--------------Remove operation--------------
            Test.Assert(agent.RemoveAzureStorageBlob(blobName, ROOT_CONTAINER_NAME), Utility.GenComparisonData("RemoveAzureStorageBlob", true));
            blob = BlobHelper.QueryBlob(ROOT_CONTAINER_NAME, blobName);
            Test.Assert(blob == null, "blob {0} should not exist!", blobName);
        }

        /// <summary>
        /// Functional Cases : for Set-AzureStorageBlobContent
        /// 1. Upload a list of new Blobs (Positive 2)
        /// 2. Upload a list of Blobs that already exist (Negative 4)
        /// 3. Upload a list of Blobs that some of them already exist (Negative 5)
        /// 
        /// Functional Cases : for Get-AzureStorageBlob
        /// 4.	Get a list of Blobs by using wildcards in the name (Positive 4)
        /// 5.	Get a list of blobs by using Prefix parameter (Positive 2)
        /// 
        /// Functional Cases : for Remove-AzureStorageBlob
        /// 6.	Remove a list of existing Blobs by using pipeline (Positive 4)
        /// </summary>
        internal void BlobListOperations(Agent agent)
        {
            //string PREFIX = Tool.GenNameString("uniqueprefix");
            //string[] BLOB_NAMES = new string[] { Tool.GenNameString(PREFIX), Tool.GenNameString(PREFIX), Tool.GenNameString(PREFIX) };

            //// PART_EXISTING_NAMES differs only the last element with Blob_NAMES
            //string[] PARTLY_EXISTING_NAMES = new string[BLOB_NAMES.Length];
            //Array.Copy(BLOB_NAMES, PARTLY_EXISTING_NAMES, BLOB_NAMES.Length - 1);
            //PARTLY_EXISTING_NAMES[BLOB_NAMES.Length - 1] = Tool.GenNameString(PREFIX);

            //string[] MERGED_NAMES = BLOB_NAMES.Union(PARTLY_EXISTING_NAMES).ToArray();
            //Array.Sort(MERGED_NAMES);

            //// Generate the comparison data
            //Collection<Dictionary<string, object>> comp = new Collection<Dictionary<string, object>>();
            //foreach (string name in MERGED_NAMES)
            //{
            //    comp.Add(Tool.GenComparisonData(StorageObjectType.Blob, name));
            //}

            //CloudBlobClient blobClient = _StorageAccount.CreateCloudBlobClient();

            //// Check if all the above Blobs have been removed
            //foreach (string name in MERGED_NAMES)
            //{
            //    CloudBlob Blob = blobClient.GetBlobReference(name);
            //    Blob.DeleteIfExists();
            //}

            ////--------------1. New operation--------------
            //Test.Assert(agent.NewAzureStorageBlob(BLOB_NAMES), Tool.GenComparisonData("NewAzureStorageBlob", true));
            //// Verification for returned values
            //Test.Assert(agent.Output.Count == 3, "3 row returned : {0}", agent.Output.Count);

            //// Check if all the above blobs have been created
            //foreach (string name in BLOB_NAMES)
            //{
            //    CloudBlob blob = blobClient.GetBlobReference(name);
            //    Test.Assert(blob.Exists(), "blob {0} should exist", name);
            //}

            //try
            //{
            //    //--------------2. New operation--------------
            //    Test.Assert(!agent.NewAzureStorageBlob(BLOB_NAMES), Tool.GenComparisonData("NewAzureStorageBlob", false));
            //    // Verification for returned values
            //    Test.Assert(agent.Output.Count == 0, "0 row returned : {0}", agent.Output.Count);
            //    int i = 0;
            //    foreach (string name in BLOB_NAMES)
            //    {
            //        Test.Assert(agent.ErrorMessages[i].Equals(String.Format("Blob '{0}' already exists.", name)), agent.ErrorMessages[i]);
            //        ++i;
            //    }

            //    //--------------3. New operation--------------
            //    Test.Assert(!agent.NewAzureStorageBlob(PARTLY_EXISTING_NAMES), Tool.GenComparisonData("NewAzureStorageBlob", false));
            //    // Verification for returned values
            //    Test.Assert(agent.Output.Count == 1, "1 row returned : {0}", agent.Output.Count);

            //    // Check if all the above blobs have been created
            //    foreach (string name in BLOB_NAMES)
            //    {
            //        CloudBlob blob = blobClient.GetBlobReference(name);
            //        Test.Assert(blob.Exists(), "blob {0} should exist", name);
            //    }

            //    //--------------4. Get operation--------------
            //    Test.Assert(agent.GetAzureStorageBlob("*" + PREFIX + "*"), Tool.GenComparisonData("GetAzureStorageBlob", true));
            //    // Verification for returned values
            //    agent.OutputValidation(_StorageAccount.CreateCloudBlobClient().ListBlobs(PREFIX));

            //    // failed due to bug#634898
            //    // use Prefix parameter
            //    //Test.Assert(agent.GetAzureStorageBlobByPrefix(PREFIX), Tool.GenComparisonData("GetAzureStorageBlobByPrefix", true));
            //    // Verification for returned values
            //    //agent.OutputValidation(_StorageAccount.CreateCloudBlobClient().ListBlobs(PREFIX));
            //}
            //finally { }

            ////--------------5. Remove operation--------------
            //Test.Assert(agent.RemoveAzureStorageBlob(MERGED_NAMES), Tool.GenComparisonData("RemoveAzureStorageBlob", true));
            //// Check if all the above blobs have been removed
            //foreach (string name in BLOB_NAMES)
            //{
            //    CloudBlob blob = blobClient.GetBlobReference(name);
            //    Test.Assert(!blob.Exists(), "blob {0} should not exist", name);
            //}
        }

        /// <summary>
        /// Negative Functional Cases : for Set-AzureStorageBlobContent 
        /// 1. Upload a Blob that already exists (Negative 3)
        /// </summary>
        internal void CreateExistingBlob(Agent agent)
        {
            //string BLOB_NAME = Tool.GenNameString("existing");

            //// create blob if not exists
            //CloudBlob blob = _StorageAccount.CreateCloudBlobClient().GetBlobReference(BLOB_NAME);
            //blob.CreateIfNotExists();

            //try
            //{
            //    //--------------New operation--------------
            //    Test.Assert(!agent.NewAzureStorageBlob(BLOB_NAME), Tool.GenComparisonData("NewAzureStorageBlob", false));
            //    // Verification for returned values
            //    Test.Assert(agent.Output.Count == 0, "Only 0 row returned : {0}", agent.Output.Count);
            //    Test.Assert(agent.ErrorMessages[0].Equals(String.Format("Blob '{0}' already exists.", BLOB_NAME)), agent.ErrorMessages[0]);
            //}
            //finally
            //{
            //    // Recover the environment
            //    blob.DeleteIfExists();
            //}
        }

        /// <summary>
        /// Negative Functional Cases : for Set-AzureStorageBlobContent 
        /// 1. Upload a new blob with an invalid blob name (Negative 1)
        /// </summary>
        internal void CreateInvalidBlob(Agent agent)
        {
            //string BLOB_NAME = Tool.GenNameString("abc_");

            ////--------------New operation--------------
            //Test.Assert(!agent.NewAzureStorageBlob(BLOB_NAME), Tool.GenComparisonData("NewAzureStorageBlob", false));
            //// Verification for returned values
            //Test.Assert(agent.Output.Count == 0, "Only 0 row returned : {0}", agent.Output.Count);
            //Test.Assert(agent.ErrorMessages[0].Equals(String.Format("Blob name '{0}' is invalid.", BLOB_NAME)), agent.ErrorMessages[0]);
        }


        /// <summary>
        /// Negative Functional Cases : for Get-AzureStorageBlob 
        /// 1. Get a non-existing blob (Negative 1)
        /// </summary>
        internal void GetNonExistingBlob(Agent agent)
        {
            string CONTAINER_NAME = Utility.GenNameString("upload-");

            // create the container
            CloudBlobContainer container = StorageAccount.CreateCloudBlobClient().GetContainerReference(CONTAINER_NAME);
            container.CreateIfNotExists();

            try
            {
                string BLOB_NAME = Utility.GenNameString("nonexisting");

                // Delete the blob if it exists
                ICloudBlob blob = BlobHelper.QueryBlob(CONTAINER_NAME, BLOB_NAME);
                if (blob != null)
                    blob.DeleteIfExists();

                //--------------Get operation--------------
                Test.Assert(!agent.GetAzureStorageBlob(BLOB_NAME, CONTAINER_NAME), Utility.GenComparisonData("GetAzureStorageBlob", false));
                // Verification for returned values
                Test.Assert(agent.Output.Count == 0, "Only 0 row returned : {0}", agent.Output.Count);
                Test.Assert(agent.ErrorMessages[0].Equals(String.Format("Can not find blob '{0}' in container '{1}'.", BLOB_NAME, CONTAINER_NAME)), agent.ErrorMessages[0]);
            }
            finally
            {
                // cleanup
                container.DeleteIfExists();
            }
        }

        /// <summary>
        /// Functional Cases : for Get-AzureStorageBlob
        /// 1. Validate that all the blobs can be enumerated (Positive 5)
        /// </summary>
        internal void EnumerateAllBlobs(Agent agent)
        {
            ////--------------Get operation--------------
            //Test.Assert(agent.GetAzureStorageBlob(""), Tool.GenComparisonData("EnumerateAllBlobs", false));

            //// Verification for returned values
            //agent.OutputValidation(_StorageAccount.CreateCloudBlobClient().ListBlobs());
        }

        /// <summary>
        /// Negative Functional Cases : for Remove-AzureStorageBlob 
        /// 1. Remove a non-existing blob (Negative 2)
        /// </summary>
        internal void RemoveNonExistingBlob(Agent agent)
        {
            string CONTAINER_NAME = Utility.GenNameString("upload-");
            string BLOB_NAME = Utility.GenNameString("nonexisting");

            // create the container
            CloudBlobContainer container = StorageAccount.CreateCloudBlobClient().GetContainerReference(CONTAINER_NAME);
            container.CreateIfNotExists();

            try
            {
                // Delete the blob if it exists
                ICloudBlob blob = BlobHelper.QueryBlob(CONTAINER_NAME, BLOB_NAME);
                if (blob != null)
                    blob.DeleteIfExists();

                //--------------Remove operation--------------
                Test.Assert(!agent.RemoveAzureStorageBlob(BLOB_NAME, CONTAINER_NAME), Utility.GenComparisonData("RemoveAzureStorageBlob", false));
                // Verification for returned values
                Test.Assert(agent.Output.Count == 0, "Only 0 row returned : {0}", agent.Output.Count);
                Test.Assert(agent.ErrorMessages[0].Equals(String.Format("Can not find blob '{0}' in container '{1}'.", BLOB_NAME, CONTAINER_NAME)), agent.ErrorMessages[0]);
            }
            finally
            {
                container.DeleteIfExists();
            }
        }

        /// <summary>
        /// Negative Functional Cases : for Remove-AzureStorageBlob 
        /// 1. Remove the blob without by force (Negative 3)
        /// </summary>
        internal void RemoveBlobWithoutForce(Agent agent)
        {
            //string BLOB_NAME = Tool.GenNameString("withoutforce");

            //// create blob if not exists
            //CloudBlob blob = _StorageAccount.CreateCloudBlobClient().GetBlobReference(BLOB_NAME);
            //blob.CreateIfNotExists();

            //try
            //{
            //    //--------------Remove operation--------------
            //    Test.Assert(!agent.RemoveAzureStorageBlob(BLOB_NAME, false), Tool.GenComparisonData("RemoveAzureStorageBlob", false));
            //    // Verification for returned values
            //    Test.Assert(agent.Output.Count == 0, "Only 0 row returned : {0}", agent.Output.Count);
            //    Test.Assert(agent.ErrorMessages[0].StartsWith("A command that prompts the user failed because"), agent.ErrorMessages[0]);
            //}
            //finally
            //{
            //    // Recover the environment
            //    blob.DeleteIfExists();
            //}
        }
    }
}

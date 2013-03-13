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
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using MS.Test.Common.MsTestLib;
using StorageTestLib;
using Microsoft.WindowsAzure.Management.ScenarioTest.Common;
using System.Management.Automation;

namespace CLITest.BVT
{
    /// <summary>
    /// this class contain all the bvt cases for the full functional storage context such as local/connectionstring/namekey, anonymous and sas token are excluded.
    /// </summary>
    [TestClass]
    public class CLICommonBVT: WindowsAzurePowerShellTest
    {
        private static CloudBlobHelper CommonBlobHelper;
        private static CloudStorageAccount CommonStorageAccount;
        private static string CommonBlockFilePath;
        private static string CommonPageFilePath;

        //env connection string
        private static string SavedEnvString;
        public static string EnvKey;

        /// <summary>
        /// the storage account which is used to set up the unit tests.
        /// </summary>
        protected static CloudStorageAccount SetUpStorageAccount
        {
            get 
            {
                return CommonStorageAccount;
            }

            set
            {
                CommonStorageAccount = value;
            }
        }
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

        public CLICommonBVT()
        { 
        }
        
        /// <summary>
        /// Init test resources for bvt class
        /// </summary>
        /// <param name="testContext">TestContext object</param>
        [ClassInitialize()]
        public static void CLICommonBVTInitialize(TestContext testContext)
        {
            Test.Info(string.Format("{0} Class  Initialize", testContext.FullyQualifiedTestClassName));
            Test.FullClassName = testContext.FullyQualifiedTestClassName;
            EnvKey = Test.Data.Get("EnvContextKey");
            SaveAndCleanSubScriptionAndEnvConnectionString();

            //Clean Storage Context
            Test.Info("Clean storage context in PowerShell");
            PowerShellAgent.CleanStorageContext();

            PowerShellAgent.ImportModule(@".\Microsoft.WindowsAzure.Management.Storage.dll");
            

            // import module
            string moduleFilePath = Test.Data.Get("ModuleFilePath");
            PowerShellAgent.ImportModule(moduleFilePath);

            GenerateBvtTempFiles();
        }

        /// <summary>
        /// Save azure subscription and env connection string. So the current settings can't impact our tests.
        /// </summary>
        //TODO move to TestBase
        public static void SaveAndCleanSubScriptionAndEnvConnectionString()
        {
            Test.Info("Clean Azure Subscription and save env connection string");
            //can't restore the azure subscription files
            PowerShellAgent.RemoveAzureSubscriptionIfExists();

            //set env connection string
            //TODO A little bit trivial, move to CLITestBase class
            if (string.IsNullOrEmpty(EnvKey))
            {
                EnvKey = Test.Data.Get("EnvContextKey");
            }

            SavedEnvString = System.Environment.GetEnvironmentVariable(EnvKey);
            System.Environment.SetEnvironmentVariable(EnvKey, string.Empty);
        }

        /// <summary>
        /// Restore the previous subscription and env connection string before testing.
        /// </summary>
        public static void RestoreSubScriptionAndEnvConnectionString()
        {
            Test.Info("Restore env connection string and skip restore subscription");
            System.Environment.SetEnvironmentVariable(EnvKey, SavedEnvString);
        }

        /// <summary>
        /// Generate temp files
        /// </summary>
        private static void GenerateBvtTempFiles()
        {
            CommonBlockFilePath = Test.Data.Get("BlockFilePath");
            CommonPageFilePath = Test.Data.Get("PageFilePath");

            CreateDirIfNotExits(Path.GetDirectoryName(CommonBlockFilePath));
            CreateDirIfNotExits(Path.GetDirectoryName(CommonPageFilePath));

            // Generate block file and page file which are used for uploading
            Helper.GenerateMediumFile(CommonBlockFilePath, 1);
            Helper.GenerateMediumFile(CommonPageFilePath, 1);
        }

        private static void CreateDirIfNotExits(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        /// <summary>
        /// Clean up test resources of  bvt class
        /// </summary>
        [ClassCleanup()]
        public static void CLICommonBVTCleanup()
        {
            Test.Info(string.Format("BVT Test Class Cleanup"));
            RestoreSubScriptionAndEnvConnectionString();
        }

        /// <summary>
        /// init test resources for one single unit test.
        /// </summary>
        [TestInitialize()]
        public void StorageTestInitialize()
        {
            SetTestStorageAccount(powershell);
            PowerShellAgent.SetPowerShellInstance(powershell);
            Test.Start(TestContext.FullyQualifiedTestClassName, TestContext.TestName);
        }

        private string EnvConnectionStringInPowerShell;

        private void SetTestStorageAccount(PowerShell powershell)
        {
            if (String.IsNullOrEmpty(EnvConnectionStringInPowerShell))
            {
                PSCommand currentCommand = powershell.Commands.Clone();
                string envConnStringScript = string.Format("$env:{0}", Test.Data.Get("EnvContextKey"));
                powershell.AddScript(envConnStringScript);
                Collection<PSObject> output = powershell.Invoke();

                if (output.Count == 1)
                {
                    EnvConnectionStringInPowerShell = output[0].BaseObject.ToString();
                    powershell.Commands = currentCommand;
                }
                else
                {
                    Test.AssertFail("Can not find the environment variable 'AZURE_STORAGE_CONNECTION_STRING' in powershell instance");
                }
            }

            if (String.IsNullOrEmpty(EnvConnectionStringInPowerShell))
            {
                throw new ArgumentException("Please set the StorageConnectionString element of TestData.xml");
            }

            CommonStorageAccount = CloudStorageAccount.Parse(EnvConnectionStringInPowerShell);

            CommonBlobHelper = new CloudBlobHelper(CommonStorageAccount);
        }

        /// <summary>
        /// clean up the test resources for one single unit test.
        /// </summary>
        [TestCleanup()]
        public void StorageTestCleanup()
        {
            Trace.WriteLine("Unit Test Cleanup");
            Test.End(TestContext.FullyQualifiedTestClassName, TestContext.TestName);
        }

        #endregion

        /// <summary>
        /// BVT case : for New-AzureStorageContainer
        /// </summary>
        [TestMethod]
        [TestCategory(Tag.BVT)]
        [TestCategory(PsTag.FastEnv)]
        [TestCategory(Category.All)]
        [TestCategory(Category.Storage)]
        public void NewContainerTest()
        {
            NewContainerTest(new PowerShellAgent());
        }

        internal void NewContainerTest(Agent agent)
        {
            string NEW_CONTAINER_NAME = Utility.GenNameString("astoria-");

            Dictionary<string, object> dic = Utility.GenComparisonData(StorageObjectType.Container, NEW_CONTAINER_NAME);
            Collection<Dictionary<string, object>> comp = new Collection<Dictionary<string, object>>{dic};

            // delete container if it exists
            CloudBlobContainer container = CommonStorageAccount.CreateCloudBlobClient().GetContainerReference(NEW_CONTAINER_NAME);
            container.DeleteIfExists();

            try
            {
                //--------------New operation--------------
                Test.Assert(agent.NewAzureStorageContainer(NEW_CONTAINER_NAME), Utility.GenComparisonData("NewAzureStorageContainer", true));
                // Verification for returned values
                CloudBlobUtil.PackContainerCompareData(container, dic);
                agent.OutputValidation(comp);
                Test.Assert(container.Exists(), "container {0} should exist!", NEW_CONTAINER_NAME);
            }
            finally
            {
                // clean up
                container.DeleteIfExists();
            }
        }
    }
}

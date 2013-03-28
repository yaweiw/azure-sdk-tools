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

using CLITest.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MS.Test.Common.MsTestLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CLITest.Common
{
    /// <summary>
    /// general settings for container related tests
    /// </summary>
    [TestClass]
    public abstract class TestBase
    {
        protected static CloudBlobUtil blobUtil;
        protected static CloudQueueUtil queueUtil;
        protected static CloudTableUtil tableUtil;
        protected static CloudStorageAccount StorageAccount;
        protected static Random random;
        private static int ContainerInitCount = 0;
        private static int QueueInitCount = 0;
        private static int TableInitCount = 0;

        protected Agent agent;

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

        /// <summary>
        /// Use ClassInitialize to run code before running the first test in the class
        /// the derived class should use it's custom class initialize
        /// first init common bvt
        /// second set storage context in powershell
        /// </summary>
        /// <param name="testContext">Test context object</param>
        [ClassInitialize()]
        public static void TestClassInitialize(TestContext testContext)
        {
            Test.Info(string.Format("{0} Class Initialize", testContext.FullyQualifiedTestClassName));
            Test.FullClassName = testContext.FullyQualifiedTestClassName;

            string ConnectionString = Test.Data.Get("StorageConnectionString");
            StorageAccount = CloudStorageAccount.Parse(ConnectionString);

            //init the blob helper for blob related operations
            blobUtil = new CloudBlobUtil(StorageAccount);
            queueUtil = new CloudQueueUtil(StorageAccount);
            tableUtil = new CloudTableUtil(StorageAccount);

            // import module
            string moduleFilePath = Test.Data.Get("ModuleFilePath");
            PowerShellAgent.ImportModule(moduleFilePath);

            //set the default storage context
            PowerShellAgent.SetStorageContext(ConnectionString);

            random = new Random();
            ContainerInitCount = blobUtil.GetExistingContainerCount();
            QueueInitCount = queueUtil.GetExistingQueueCount();
            TableInitCount = tableUtil.GetExistingTableCount();
        }

        //
        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void TestClassCleanup()
        {
            int count = blobUtil.GetExistingContainerCount();

            string message = string.Format("there are {0} containers before running mutiple unit tests, after is {1}", ContainerInitCount, count);
            AssertCleanupOnStorageObject("containers", ContainerInitCount, count);
            
            count = queueUtil.GetExistingQueueCount();
            AssertCleanupOnStorageObject("queues", QueueInitCount, count);

            count = tableUtil.GetExistingTableCount();
            
            AssertCleanupOnStorageObject("tables", TableInitCount, count);

            Test.Info("Test Class Cleanup");
        }

        private static void AssertCleanupOnStorageObject(string name, int initCount, int cleanUpCount)
        {
            string message = string.Format("there are {0} {1} before running mutiple unit tests, after is {2}", initCount, name, cleanUpCount);

            if (initCount == cleanUpCount)
            {
                Test.Info(message);
            }
            else
            {
                Test.Warn(message);
            }
        }

        /// <summary>
        /// on test setup
        /// the derived class could use it to run it owned set up settings.
        /// </summary>
        public virtual void OnTestSetup()
        { 
        }

        /// <summary>
        /// on test clean up
        /// the derived class could use it to run it owned clean up settings.
        /// </summary>
        public virtual void OnTestCleanUp()
        { 
        }

        /// <summary>
        /// test initialize
        /// </summary>
        [TestInitialize()]
        public void InitAgent()
        {
            agent = new PowerShellAgent();
            Test.Start(TestContext.FullyQualifiedTestClassName, TestContext.TestName);
            OnTestSetup();
        }

        /// <summary>
        /// test clean up
        /// </summary>
        [TestCleanup()]
        public void CleanAgent()
        {
            OnTestCleanUp();
            agent = null;
            Test.End(TestContext.FullyQualifiedTestClassName, TestContext.TestName);
        }

        #endregion

        public void ExpectedEqualErrorMessage(string errorMessage)
        {
            Test.Assert(agent.ErrorMessages.Count > 0, "Should return error message");
            
            if (agent.ErrorMessages.Count == 0)
            {
                return;
            }

            Test.Assert(errorMessage == agent.ErrorMessages[0], String.Format("Expected error message: {0}, and actually it's {1}", errorMessage, agent.ErrorMessages[0]));
        }

        public void ExpectedStartsWithErrorMessage(string errorMessage)
        {
            Test.Assert(agent.ErrorMessages.Count > 0, "Should return error message");
            
            if (agent.ErrorMessages.Count == 0)
            {
                return;
            }
            
            Test.Assert(agent.ErrorMessages[0].StartsWith(errorMessage), String.Format("Expected error message should start with {0}, and actualy it's {1}", errorMessage, agent.ErrorMessages[0]));
        }

        public void ExpectedContainErrorMessage(string errorMessage)
        {
            Test.Assert(agent.ErrorMessages.Count > 0, "Should return error message");
            
            if (agent.ErrorMessages.Count == 0)
            {
                return;
            }

            Test.Assert(agent.ErrorMessages[0].IndexOf(errorMessage) != -1, String.Format("Expected error message should contain {0}, and actualy it's {1}", errorMessage, agent.ErrorMessages[0]));
        }
    }
}

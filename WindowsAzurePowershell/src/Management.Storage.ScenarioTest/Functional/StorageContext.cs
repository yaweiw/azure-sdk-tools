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
using System.Management.Automation;
using System.Text;
using CLITest.BVT;
using CLITest.Common;
using CLITest.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MS.Test.Common.MsTestLib;
using StorageTestLib;

namespace CLITest.Functional
{
    /// <summary>
    /// function test for storage context
    /// </summary>
    [TestClass]
    class StorageContext: TestBase
    {
        [ClassInitialize()]
        public static void GetBlobClassInit(TestContext testContext)
        {
            TestBase.TestClassInitialize(testContext);
        }

        [ClassCleanup()]
        public static void GetBlobClassCleanup()
        {
            TestBase.TestClassCleanup();
        }

        /// <summary>
        /// get containers from multiple storage contexts
        /// 8.19	New-AzureStorageContext Cmdlet Parameters Positive Functional Cases
        ///     9.	Use pipeline to run PowerShell cmdlets for two valid accounts
        /// </summary>
        //TODO should add more test about context and pipeline in each cmdlet
        [TestMethod()]
        [TestCategory(Tag.Function)]
        [TestCategory(PsTag.StorageContext)]
        public void GetContainerFromMultipleStorageContext()
        {
            string connectionString1 = Test.Data.Get("StorageConnectionString1");
            string connectionString2 = Test.Data.Get("StorageConnectionString2");
            Test.Assert(connectionString1 != connectionString2, "Use two different connection string {0} != {1}", connectionString1, connectionString2);
            CloudStorageAccount account1 = CloudStorageAccount.Parse(connectionString1);
            CloudStorageAccount account2 = CloudStorageAccount.Parse(connectionString2);
            CloudBlobUtil blobUtil1 = new CloudBlobUtil(account1);
            CloudBlobUtil blobUtil2 = new CloudBlobUtil(account2);
            string containerName = Utility.GenNameString("container");

            try
            {
                CloudBlobContainer container1 = blobUtil1.CreateContainer(containerName);
                CloudBlobContainer container2 = blobUtil2.CreateContainer(containerName);
                int containerCount = 2;

                string cmd = String.Format("$context1 = new-azurestoragecontext -connectionstring '{0}';$context2 = new-azurestoragecontext -connectionstring '{1}';($context1, $context2)", connectionString1, connectionString2);
                agent.UseContextParam = false;
                ((PowerShellAgent)agent).AddPipelineScript(cmd);

                Test.Assert(agent.GetAzureStorageContainer(containerName), Utility.GenComparisonData("Get-AzureStorageContainer using multiple storage contexts", true));
                Test.Assert(agent.Output.Count == containerCount, String.Format("Want to retrieve {0} page blob, but retrieved {1} page blobs", containerCount, agent.Output.Count));

                agent.OutputValidation(new List<CloudBlobContainer>() { container1, container2 });
            }
            finally
            {
                blobUtil1.RemoveContainer(containerName);
                blobUtil2.RemoveContainer(containerName);
            }
        }

        /// <summary>
        /// get containers from valid and invalid storage contexts
        /// 8.19 New-AzureStorageContext Negative Functional Cases
        ///     3.	Use pipeline to run PowerShell cmdlets for one valid account and one invalid account
        /// </summary>
        //TODO should add more test about context and pipeline in each cmdlet
        [TestMethod()]
        [TestCategory(Tag.Function)]
        [TestCategory(PsTag.StorageContext)]
        public void GetContainerFromValidAndInvalidStorageContext()
        {
            string connectionString1 = Test.Data.Get("StorageConnectionString");
            string randomAccountName = Utility.GenNameString("account");
            string randomAccountKey = Utility.GenNameString("key");
            randomAccountKey = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(randomAccountKey));
            CloudStorageAccount account1 = CloudStorageAccount.Parse(connectionString1);

            string containerName = Utility.GenNameString("container");

            try
            {
                CloudBlobContainer container1 = blobUtil.CreateContainer(containerName);
                string cmd = String.Format("$context1 = new-azurestoragecontext -connectionstring '{0}';$context2 = new-azurestoragecontext -StorageAccountName '{1}' -StorageAccountKey '{2}';($context1, $context2)",
                    connectionString1, randomAccountName, randomAccountKey);
                agent.UseContextParam = false;
                ((PowerShellAgent)agent).AddPipelineScript(cmd);

                Test.Assert(!agent.GetAzureStorageContainer(containerName), Utility.GenComparisonData("Get-AzureStorageContainer using valid and invalid storage contexts", false));
                Test.Assert(agent.Output.Count == 1, "valid storage context should return 1 container");
                Test.Assert(agent.ErrorMessages.Count == 1, "invalid storage context should return error");

                //the same error may output different error messages in different environments
                bool expectedError = agent.ErrorMessages[0].StartsWith("The remote server returned an error: (502) Bad Gateway") ||
                    agent.ErrorMessages[0].StartsWith("The remote name could not be resolved") || agent.ErrorMessages[0].StartsWith("The operation has timed out");
                Test.Assert(expectedError, "use invalid storage account should return 502 or could not be resolved exception or The operation has timed out, actully {0}", agent.ErrorMessages[0]);
            }
            finally
            {
                //TODO test the invalid storage account in subscription
                blobUtil.RemoveContainer(containerName);
            }
        }

        /// <summary>
        /// run cmdlet without storage context
        /// 8.19 New-AzureStorageContext Negative Functional Cases
        ///     1. Do not specify the context parameter in the parameter set for each cmdlet
        /// </summary>
        [TestMethod()]
        [TestCategory(Tag.Function)]
        [TestCategory(PsTag.StorageContext)]
        public void RunCmdletWithoutStorageContext()
        {
            PowerShellAgent.RemoveAzureSubscriptionIfExists();

            CLICommonBVT.SaveAndCleanSubScriptionAndEnvConnectionString();

            string containerName = Utility.GenNameString("container");
            CloudBlobContainer container = blobUtil.CreateContainer(containerName);

            try
            {
                bool terminated = false;

                try
                {
                    agent.GetAzureStorageContainer(containerName);
                }
                catch (CmdletInvocationException e)
                {
                    terminated = true;
                    Test.Info(e.Message);
                    Test.Assert(e.Message.StartsWith("Can not find your azure storage credential."), "Can not find your azure storage credential.");
                }
                finally
                {
                    if (!terminated)
                    {
                        Test.AssertFail("without storage context should return a terminating error");
                    }
                }
            }
            finally
            {
                blobUtil.RemoveContainer(containerName);
            }

            CLICommonBVT.RestoreSubScriptionAndEnvConnectionString();
        }
    }
}

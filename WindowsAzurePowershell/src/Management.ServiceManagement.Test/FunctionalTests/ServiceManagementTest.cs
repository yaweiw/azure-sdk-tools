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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;    
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;    
    using System.Linq;

    [TestClass]
    public class ServiceManagementTest
    {

        protected const string serviceNamePrefix = "PSTestService";
        protected const string vmNamePrefix = "PSTestVM";
        protected const string password = "p@ssw0rd";
        protected const string username = "pstestuser";
        protected const string localFile = @"\\watest\public\PSTest\oneGBFixedWS2008R2.vhd";
       

        protected static ServiceManagementCmdletTestHelper vmPowershellCmdlets;
        protected static SubscriptionData defaultAzureSubscription;
        protected static StorageServiceKeyOperationContext storageAccountKey;
        protected static string blobUrlRoot;

        protected static string locationName;
        protected static string imageName;
        
        protected bool pass;
        protected string testName;
        protected DateTime testStartTime;

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

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();
            vmPowershellCmdlets.ImportAzurePublishSettingsFile();
            defaultAzureSubscription = vmPowershellCmdlets.SetDefaultAzureSubscription(Resource.DefaultSubscriptionName);
            Assert.AreEqual(Resource.DefaultSubscriptionName, defaultAzureSubscription.SubscriptionName);
            if (defaultAzureSubscription.CurrentStorageAccount == null)
            {
                string defaultStorage = Utilities.GetUniqueShortName("storage");                
                vmPowershellCmdlets.NewAzureStorageAccount(defaultStorage, Resource.Location);
                defaultAzureSubscription = vmPowershellCmdlets.SetAzureSubscription(defaultAzureSubscription.SubscriptionName, defaultStorage);
            }
            
            storageAccountKey = vmPowershellCmdlets.GetAzureStorageAccountKey(defaultAzureSubscription.CurrentStorageAccount);
            Assert.AreEqual(defaultAzureSubscription.CurrentStorageAccount, storageAccountKey.StorageAccountName);
            blobUrlRoot = (vmPowershellCmdlets.GetAzureStorageAccount(defaultAzureSubscription.CurrentStorageAccount)[0].Endpoints.ToArray())[0];

            locationName = vmPowershellCmdlets.GetAzureLocationName(new[] { Resource.Location }, false); // Get-AzureLocation
            Console.WriteLine("Location Name: {0}", locationName);
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows", "testvmimage" }, false); // Get-AzureVMImage
            Console.WriteLine("Image Name: {0}", imageName);
        }

        protected void StartTest(string testname, DateTime testStartTime)
        {            
            Console.WriteLine("{0} test starts at {1}", testname, testStartTime);
        }
    }
}
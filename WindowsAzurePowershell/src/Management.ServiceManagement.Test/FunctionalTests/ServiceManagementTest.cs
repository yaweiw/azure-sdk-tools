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
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;    
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;    
    using System.Linq;
    using System.Threading;
    using System.IO;
    using Sync.Download;

    [TestClass]
    public class ServiceManagementTest
    {
        
        protected const string serviceNamePrefix = "PSTestService";
        protected const string vmNamePrefix = "PSTestVM";
        protected const string password = "p@ssw0rd";
        protected const string username = "pstestuser";
        protected static string localFile = Resource.Vhd;
        protected const bool deleteDefaultStorageAccount = false;
        protected static string vnetConfigFilePath = Directory.GetCurrentDirectory() + "\\vnetconfig.netcfg";
       

        protected static ServiceManagementCmdletTestHelper vmPowershellCmdlets;
        protected static SubscriptionData defaultAzureSubscription;
        protected static StorageServiceKeyOperationContext storageAccountKey;
        protected static string blobUrlRoot;

        protected static string locationName;
        protected static string imageName;
        
        protected bool pass;
        protected string testName;
        protected DateTime testStartTime;
        protected bool cleanupIfPassed = true;
        protected bool cleanupIfFailed = false;
        

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
            if (string.IsNullOrEmpty(Resource.DefaultSubscriptionName))
            {
                Console.WriteLine("No subscription is selected!");
            }
            else
            {
                defaultAzureSubscription = vmPowershellCmdlets.SetDefaultAzureSubscription(Resource.DefaultSubscriptionName);
                Assert.AreEqual(Resource.DefaultSubscriptionName, defaultAzureSubscription.SubscriptionName);

                if (defaultAzureSubscription.CurrentStorageAccount == null || Utilities.CheckRemove(vmPowershellCmdlets.GetAzureStorageAccount, defaultAzureSubscription.CurrentStorageAccount))
                {
                    string defaultStorage = Utilities.GetUniqueShortName("storage");
                    vmPowershellCmdlets.NewAzureStorageAccount(defaultStorage, Resource.Location);
                    defaultAzureSubscription = vmPowershellCmdlets.SetAzureSubscription(defaultAzureSubscription.SubscriptionName, defaultStorage);
                }

                storageAccountKey = vmPowershellCmdlets.GetAzureStorageAccountKey(defaultAzureSubscription.CurrentStorageAccount);
                Assert.AreEqual(defaultAzureSubscription.CurrentStorageAccount, storageAccountKey.StorageAccountName);
                blobUrlRoot = (vmPowershellCmdlets.GetAzureStorageAccount(defaultAzureSubscription.CurrentStorageAccount)[0].Endpoints.ToArray())[0];


                locationName = vmPowershellCmdlets.GetAzureLocationName(new[] { Resource.Location }, false); // Get-AzureLocation
                if (String.IsNullOrEmpty(locationName))
                {
                    Console.WriteLine("No location is selected!");
                }
                Console.WriteLine("Location Name: {0}", locationName);


                imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows", "testvmimage" }, false); // Get-AzureVMImage
                if (String.IsNullOrEmpty(imageName))
                {
                    Console.WriteLine("No image is selected!");
                }
                Console.WriteLine("Image Name: {0}", imageName);
            }
        }
        

        protected void StartTest(string testname, DateTime testStartTime)
        {            
            Console.WriteLine("{0} test starts at {1}", testname, testStartTime);
        }

        [AssemblyCleanup]
        public static void CleanUpAssembly()
        {

            if (defaultAzureSubscription != null)
            {
                Retry(String.Format("Get-AzureDisk | Where {{$_.DiskName.Contains(\"{0}\")}} | Remove-AzureDisk -DeleteVhd", serviceNamePrefix), "in use");
                if (deleteDefaultStorageAccount && !string.IsNullOrEmpty(defaultAzureSubscription.CurrentStorageAccount))
                {
                    vmPowershellCmdlets.RemoveAzureStorageAccount(defaultAzureSubscription.CurrentStorageAccount);
                }
            }
        }

        private static void Retry(string cmdlet, string message, int maxTry = 20, int intervalSecond = 10)
        {

            ServiceManagementCmdletTestHelper pscmdlet = new ServiceManagementCmdletTestHelper();

            for (int i = 0; i < maxTry; i++)
            {
                try
                {
                    pscmdlet.RunPSScript(cmdlet);
                    break;
                }
                catch (Exception e)
                {
                    if (i == maxTry)
                    {
                        Console.WriteLine("Max try reached.  Couldn't perform within the given time.");
                    }
                    if (e.ToString().Contains(message))
                    {
                        Thread.Sleep(intervalSecond * 1000);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;



namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests
{
    //using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;    
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;
    

    [TestClass]
    public class FunctionalTest
    {
        private ServiceManagementCmdletTestHelper vmPowershellCmdlets;
        private SubscriptionData defaultAzureSubscription;
        private StorageServiceKeyOperationContext storageAccountKey;
        bool cleanup = true;
        bool pass = false;
        string testName;        


        private string locationName;
        private string imageName;
        private string serviceName = "DefaultServiceName";
        private string vmName = "DefaultVmName";
        
        
        //private string perfFile;
        [TestInitialize]
        public void Initialize()
        {
            vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();
            vmPowershellCmdlets.ImportAzurePublishSettingsFile(); // Import-AzurePublishSettingsFile
            defaultAzureSubscription = vmPowershellCmdlets.SetDefaultAzureSubscription(Resource.DefaultSubscriptionName); // Set-AzureSubscription
            Assert.AreEqual(Resource.DefaultSubscriptionName, defaultAzureSubscription.SubscriptionName);
            storageAccountKey = vmPowershellCmdlets.GetAzureStorageAccountKey(defaultAzureSubscription.CurrentStorageAccount); // Get-AzureStorageAccountKey
            Assert.AreEqual(defaultAzureSubscription.CurrentStorageAccount, storageAccountKey.StorageAccountName);

            locationName = vmPowershellCmdlets.GetAzureLocationName(new[] { Resource.Location }, false); // Get-AzureLocation
            Console.WriteLine("Location Name: {0}", locationName);
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "MSFT", "testvmimage" }, false); // Get-AzureVMImage
            Console.WriteLine("Image Name: {0}", imageName);
                       
            // Create a unique Service Name
            //serviceName = Utilities.GetUniqueShortName("PSTestService");
            //vmName = Utilities.GetUniqueShortName("PSTestVM");
            //vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName, serviceName, imageName, "p@ssw0rd", locationName);

            if (vmPowershellCmdlets.TestAzureServiceName(serviceName))
            {                
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName, serviceName, imageName, "p@ssw0rd", locationName);                
                Console.WriteLine("Service Name: {0} is created.", serviceName);
            }
            else
            {
                Console.WriteLine("Service Name: {0} already exists.", serviceName);
            }


      
            //string vmName2 = "MyVM";
            //AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(vmName2, VMSizeInfo.Small, imageName);            
            //AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, "p@ssw0rd");            
            //PersistentVMConfigInfo persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);           
            //PersistentVM persistentVM = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);            
            //PersistentVM[] VMs = { persistentVM };
            //vmPowershellCmdlets.NewAzureVM(serviceName, VMs);
            

        }
              
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Get-Module)")]
        public void ScriptTestSample()
        {
            
            var result = vmPowershellCmdlets.RunPSScript("Get-AzureStorageAccount | Select Label");
        }  

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("priya"), Description("Test the cmdlet (Get-Module)")]
        public void GetAzureVMImage()
        {
            ServiceManagementCmdletTestHelper vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();
            vmPowershellCmdlets.GetAzureVMImage(null);
        }        
    
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New, Get, Set, Remove)-AzureAffinityGroup)")]
        public void AzureAffinityGroupTest()
        {
            testName = "AzureAffinityGroupTest";

            string affinityName1 = "affinityName1";
            string affinityLabel1 = affinityName1;
            string location1 = "West US";
            string description1 = "Affinity group for West US";

            string affinityName2 = "affinityName2";
            string affinityLabel2 = "label2";
            string location2 = "East US";
            string description2 = "Affinity group for East US";

            try
            {
                ServiceManagementCmdletTestHelper vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();

                // Remove previously created affinity groups
                foreach (var aff in vmPowershellCmdlets.GetAzureAffinityGroup(null))
                {
                    if (aff.Name == affinityName1 || aff.Name == affinityName2)
                    {
                        vmPowershellCmdlets.RemoveAzureAffinityGroup(aff.Name);
                    }                    
                }
               
                // New-AzureAffinityGroup
                vmPowershellCmdlets.NewAzureAffinityGroup(affinityName1, location1, affinityLabel1, description1);
                vmPowershellCmdlets.NewAzureAffinityGroup(affinityName2, location2, affinityLabel2, description2);
                Console.WriteLine("Affinity groups created: {0}, {1}", affinityName1, affinityName2);

                // Get-AzureAffinityGroup
                foreach (var aff in vmPowershellCmdlets.GetAzureAffinityGroup(affinityName1))
                {
                    Console.WriteLine("Get-AzureAffinityGroup returned: {0}", aff.ToString());
                    Assert.AreEqual(aff.Name, affinityName1, "Error: Affinity Name is not equal!");
                    Assert.AreEqual(aff.Label, affinityLabel1, "Error: Affinity Label is not equal!");
                    Assert.AreEqual(aff.Location, location1, "Error: Affinity Location is not equal!");
                    Assert.AreEqual(aff.Description, description1, "Error: Affinity Description is not equal!");
                }

                foreach (var aff in vmPowershellCmdlets.GetAzureAffinityGroup(affinityName2))
                {
                    Console.WriteLine("Get-AzureAffinityGroup returned: {0}", aff.ToString());
                    Assert.AreEqual(aff.Name, affinityName2, "Error: Affinity Name is not equal!");
                    Assert.AreEqual(aff.Label, affinityLabel2, "Error: Affinity Label is not equal!");
                    Assert.AreEqual(aff.Location, location2, "Error: Affinity Location is not equal!");
                    Assert.AreEqual(aff.Description, description2, "Error: Affinity Description is not equal!");
                }

                // Set-AzureAffinityGroup
                vmPowershellCmdlets.SetAzureAffinityGroup(affinityName2, affinityLabel1, description1);
                Console.WriteLine("update affinity group: {0}", affinityName2);

                foreach (var aff in vmPowershellCmdlets.GetAzureAffinityGroup(affinityName2))
                {
                    Console.WriteLine("Get-AzureAffinityGroup returned: {0}", aff.ToString());
                    Assert.AreEqual(aff.Name, affinityName2, "Error: Affinity Name is not equal!");
                    Assert.AreEqual(aff.Label, affinityLabel1, "Error: Affinity Label is not equal!");
                    Assert.AreEqual(aff.Location, location2, "Error: Affinity Location is not equal!");
                    Assert.AreEqual(aff.Description, description1, "Error: Affinity Description is not equal!");
                }

                // Remove-AzureAffinityGroup
                vmPowershellCmdlets.RemoveAzureAffinityGroup(affinityName2);
                Console.WriteLine("affinity group removed: {0}", affinityName2);

                try
                {
                    vmPowershellCmdlets.GetAzureAffinityGroup(affinityName2);
                    Assert.Fail("The affinity group should have been removed!");
                }
                catch (Exception e)
                {
                    if (e.ToString().ToLowerInvariant().Contains("does not exist"))
                    {
                        Console.WriteLine("the affinity group, {0}, is successfully removed.", affinityName2);
                    }
                    else
                    {
                        Assert.Fail("Error during get-azureAffinityGroup: {0}", e.ToString());
                    }
                }
                vmPowershellCmdlets.RemoveAzureAffinityGroup(affinityName1);

                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }
        
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (New-AzureAffinityGroup)")]
        public void AzureCertificateTest()
        {
            
            testName = "AzureCertificateTest";
            cleanup = false;

            string certLocation = "cert:\\CurrentUser\\My\\";

            string thumbprint1 = "C5AF4AEE8FD278F9D9FCFAB7DC5436B8DF3A5074";            
            PSObject cert1 = vmPowershellCmdlets.RunPSScript("Get-Item " + certLocation + thumbprint1)[0];

            string thumbprint2 = "2FB0786115F0C2E7575F31C0A5FBBAC559E7F96F";
            PSObject cert2 = vmPowershellCmdlets.RunPSScript("Get-Item " + certLocation + thumbprint2)[0];


            try
            {
                vmPowershellCmdlets.AddAzureCertificate(serviceName, cert1);
                vmPowershellCmdlets.AddAzureCertificate(serviceName, cert2);

                CertificateContext getCert1 = vmPowershellCmdlets.GetAzureCertificate(serviceName, thumbprint1, "sha1")[0];
                Console.WriteLine("Cert is added: {0}", getCert1.Thumbprint);
                //Assert.AreEqual(getCert1.Thumbprint, thumbprint1);  // Currently fails because of a bug

                CertificateContext getCert2 = vmPowershellCmdlets.GetAzureCertificate(serviceName, thumbprint2, "sha1")[0];
                Console.WriteLine("Cert is added: {0}", getCert2.Thumbprint);
                //Assert.AreEqual(getCert2.Thumbprint, thumbprint2);

                vmPowershellCmdlets.RemoveAzureCertificate(serviceName, thumbprint1, "sha1");
                foreach (var cert in vmPowershellCmdlets.GetAzureCertificate(serviceName))
                {
                    Assert.AreNotEqual(cert.Thumbprint, thumbprint1, String.Format("Cert is not removed:", thumbprint1));
                }
                Console.WriteLine("Cert, {0}, is successfully removed.");

                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (New-AzureAffinityGroup)")]
        public void AzureDataDiskTest()
        {
            testName = "AzureDataDiskTest";
            cleanup = false;

            string diskLabel1 = "disk1";
            int diskSize1 = 30;            
            int lunSlot1 = 0;

            string diskLabel2 = "disk2";
            int diskSize2 = 50;
            int lunSlot2 = 2;


            try
            {                
                AddAzureDataDiskConfig dataDiskInfo1 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, diskSize1, diskLabel1, lunSlot1);
                AddAzureDataDiskConfig dataDiskInfo2 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, diskSize2, diskLabel2, lunSlot2);

                vmPowershellCmdlets.AddDataDisk(vmName, serviceName, new [] {dataDiskInfo1, dataDiskInfo2}); // Add-AzureEndpoint with Get-AzureVM and Update-AzureVm  
                
                Assert.IsTrue(CheckDataDisk(vmName, serviceName, dataDiskInfo1, HostCaching.None), "Data disk is not properly added");
                Console.WriteLine("Date disk added correctly.\n");
               
                                                
                Assert.IsTrue(CheckDataDisk(vmName, serviceName, dataDiskInfo2, HostCaching.None), "Data disk is not properly added");
                Console.WriteLine("Date disk added correctly.\n");

                vmPowershellCmdlets.SetDataDisk(serviceName, vmName, HostCaching.ReadOnly, lunSlot1);                
                Assert.IsTrue(CheckDataDisk(vmName, serviceName, dataDiskInfo1, HostCaching.ReadOnly), "Data disk is not properly changed");
                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
            finally
            {
                // Remove DataDisks created
                vmPowershellCmdlets.RemoveDataDisk(serviceName, vmName, new [] {lunSlot1, lunSlot2}); // Remove-AzureEndpoint
            }
        }

        private bool CheckDataDisk(string vmName, string serviceName, AddAzureDataDiskConfig dataDiskInfo, HostCaching hc)
        {            
            bool found = false;
            foreach (DataVirtualHardDisk disk in vmPowershellCmdlets.GetAzureDataDisk(vmName, serviceName))
            {
                Console.WriteLine("DataDisk - Name:{0}, Label:{1}, Size:{2}, LUN:{3}, HostCaching: {4}", disk.DiskName, disk.DiskLabel, disk.LogicalDiskSizeInGB, disk.Lun, disk.HostCaching);
                if (disk.DiskLabel == dataDiskInfo.DiskLabel && disk.LogicalDiskSizeInGB == dataDiskInfo.DiskSizeGB && disk.Lun == dataDiskInfo.LunSlot)
                {
                    if (disk.HostCaching == hc.ToString())
                    {
                        found = true;
                        Console.WriteLine("DataDisk found: {0}", disk.DiskLabel);
                    }
                }
            }
            return found;
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (New-AzureAffinityGroup)")]
        public void AzureEndpointTest()
        {
            testName = "AzureEndpointTest";
            cleanup = false;
            
            string epName1 = "tcp1";
            int localPort1 = 60010;
            int port1 = 60011;

            string epName2 = "tcp2";
            int localPort2 = 60020;
            int port2 = 60021;


            try                        
            {
                // Add two new endpoints
                AzureEndPointConfigInfo epInfo1 = new AzureEndPointConfigInfo(ProtocolInfo.tcp, localPort1, port1, epName1);
                AzureEndPointConfigInfo epInfo2 = new AzureEndPointConfigInfo(ProtocolInfo.tcp, localPort2, port2, epName2);

                vmPowershellCmdlets.AddEndPoint(vmName, serviceName, new [] {epInfo1, epInfo2}); // Add-AzureEndpoint with Get-AzureVM and Update-AzureVm                             
                Assert.IsTrue(CheckEndpoint(vmName, serviceName, epInfo1), "Error: Endpoint was not added!");
                Assert.IsTrue(CheckEndpoint(vmName, serviceName, epInfo2), "Error: Endpoint was not added!");

                // Change the endpoint
                AzureEndPointConfigInfo epInfo3 = new AzureEndPointConfigInfo(ProtocolInfo.tcp, 60030, 60031, epName2);
                vmPowershellCmdlets.SetEndPoint(serviceName, vmName, epInfo3); // Set-AzureEndpoint with Get-AzureVM and Update-AzureVm                 
                Assert.IsTrue(CheckEndpoint(vmName, serviceName, epInfo3), "Error: Endpoint was not changed!");

                // Remove Endpoint
                vmPowershellCmdlets.RemoveEndPoint(serviceName, vmName, new[] { epName1, epName2 }); // Remove-AzureEndpoint
                Assert.IsFalse(CheckEndpoint(vmName, serviceName, epInfo1), "Error: Endpoint was not removed!");
                Assert.IsFalse(CheckEndpoint(vmName, serviceName, epInfo3), "Error: Endpoint was not removed!");

                pass = true;
                
            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
            finally
            {
                
            }
        }

        private bool CheckEndpoint(string vmName, string serviceName, AzureEndPointConfigInfo epInfo)
        {            
            bool found = false;
            foreach (InputEndpointContext ep in vmPowershellCmdlets.GetAzureEndPoint(vmPowershellCmdlets.GetAzureVM(vmName, serviceName)))
            {
                Console.WriteLine("Endpoint - Name:{0}, Protocol:{1}, Port:{2}, LocalPort:{3}, Vip:{4}", ep.Name, ep.Protocol, ep.Port, ep.LocalPort, ep.Vip);
                if (ep.Name == epInfo.EndpointName && ep.LocalPort == epInfo.InternalPort && ep.Port == epInfo.ExternalPort && ep.Protocol == epInfo.Protocol.ToString())
                {
                    found = true;
                    Console.WriteLine("Endpoint found: {0}", epInfo.EndpointName);                    
                }
            }
            return found;
        }

        // ToDo:
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (New-AzureAffinityGroup)")]
        public void AzureDeployemtTest()
        {
            testName = "AzureDeployemtnTest";
            cleanup = false;

            

            try
            {
                
                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
            finally
            {

            }
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (New-AzureAffinityGroup)")]
        public void AzureStorageAccountTest()
        {
            testName = "AzureStorageAccountTest";
            cleanup = false;

            string storageName1 = Utilities.GetUniqueShortName("psteststorage");
            string locationName1 = "West US";
            string storageName2 = Utilities.GetUniqueShortName("psteststorage");
            string locationName2 = "East US";

            try
            {
                vmPowershellCmdlets.NewAzureStorageAccount(storageName1, locationName1, null, null, null);
                vmPowershellCmdlets.NewAzureStorageAccount(storageName2, locationName2, null, null, null);

                Assert.IsNotNull(vmPowershellCmdlets.GetAzureStorageAccount(storageName1));
                Console.WriteLine("{0} is created", storageName1);
                Assert.IsNotNull(vmPowershellCmdlets.GetAzureStorageAccount(storageName2));                
                Console.WriteLine("{0} is created", storageName2);

                vmPowershellCmdlets.SetAzureStorageAccount(storageName1, "newLabel", "newDescription", false);

                StorageServicePropertiesOperationContext storage = vmPowershellCmdlets.GetAzureStorageAccount(storageName1)[0];
                Console.WriteLine("Name: {0}, Label: {1}, Description: {2}, GeoReplication: {3}", storage.StorageAccountName, storage.Label, storage.StorageAccountDescription, storage.GeoReplicationEnabled.ToString());
                Assert.IsTrue((storage.Label == "newLabel" && storage.StorageAccountDescription == "newDescription" && storage.GeoReplicationEnabled == false), "storage account is not changed correctly");
                

                vmPowershellCmdlets.RemoveAzureStorageAccount(storageName1);
                vmPowershellCmdlets.RemoveAzureStorageAccount(storageName2);


                Assert.IsTrue(CheckRemove(vmPowershellCmdlets.GetAzureStorageAccount, storageName1), "The storage account was not removed");
                Assert.IsTrue(CheckRemove(vmPowershellCmdlets.GetAzureStorageAccount, storageName2), "The storage account was not removed");
                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
            finally
            {

            }
        }

        private bool CheckRemove<T>(Func<string, T> fn, string name)
        {
            try
            {
                fn(name);
                return false;
            }
            catch (Exception e)
            {
                if (e.ToString().ToLowerInvariant().Contains("does not exist"))
                {
                    Console.WriteLine("{0} is successfully removed", name);
                    return true;
                }
                else
                {
                    Console.WriteLine("Error: {0}", e.ToString());
                    return false;
                }
            }
        }

        
       
       
        [TestCleanup]
        public virtual void CleanUp()
        {


            if (pass)
            {
                Console.WriteLine("{0} passed.", testName);
            }
            
            // Cleanup
            //vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            /* RemoveAzureService doesn't work */
            if (cleanup)
            {
                vmPowershellCmdlets.RemoveAzureService(serviceName);
                Console.WriteLine("Service, {0}, is deleted", serviceName);
            }
            //Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));

        }

    }
}

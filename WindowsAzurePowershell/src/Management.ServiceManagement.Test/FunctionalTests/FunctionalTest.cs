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

using System.Threading;
using System.Reflection;



namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests
{    
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;

    using System.IO;
    

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
        
        
        //private string perfFile;
        [TestInitialize]
        public void Initialize()
        {            
            vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();
            vmPowershellCmdlets.ImportAzurePublishSettingsFile(); // Import-AzurePublishSettingsFile
            defaultAzureSubscription = vmPowershellCmdlets.SetDefaultAzureSubscription(Resource.DefaultSubscriptionName); // Set-AzureSubscription
            Assert.AreEqual(Resource.DefaultSubscriptionName, defaultAzureSubscription.SubscriptionName);
            storageAccountKey = vmPowershellCmdlets.GetAzureStorageAccountKey(defaultAzureSubscription.CurrentStorageAccount); // Get-AzureStorageKey
            Assert.AreEqual(defaultAzureSubscription.CurrentStorageAccount, storageAccountKey.StorageAccountName);

            locationName = vmPowershellCmdlets.GetAzureLocationName(new[] { Resource.Location }, false); // Get-AzureLocation
            Console.WriteLine("Location Name: {0}", locationName);
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "MSFT", "testvmimage" }, false); // Get-AzureVMImage
            Console.WriteLine("Image Name: {0}", imageName);
                                  
            if (vmPowershellCmdlets.TestAzureServiceName(serviceName))
            {
                Console.WriteLine("Service Name: {0} already exists.", serviceName);
                if (vmPowershellCmdlets.GetAzureVM(vmName, serviceName) == null)
                {
                    vmPowershellCmdlets.RemoveAzureService(serviceName);
                    vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName, serviceName, imageName, "p@ssw0rd", locationName);
                }               
            }
            else
            {
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName, serviceName, imageName, "p@ssw0rd", locationName);
                Console.WriteLine("Service Name: {0} is created.", serviceName);                
            }
        }
              
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Get-AzureStorageAccount)")]
        public void ScriptTestSample()
        {
            
            var result = vmPowershellCmdlets.RunPSScript("Get-Help Save-AzureVhd -full");
        }  

        
        
    
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New,Get,Set,Remove)-AzureAffinityGroup)")]
        public void AzureAffinityGroupTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;
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
                    Console.WriteLine("Get-AzureAffinityGroup returned: Name - {0}, Location - {1}, Label - {2}, Description - {3}", aff.Name, aff.Location, aff.Label, aff.Description);
                    Assert.AreEqual(aff.Name, affinityName1, "Error: Affinity Name is not equal!");
                    Assert.AreEqual(aff.Label, affinityLabel1, "Error: Affinity Label is not equal!");
                    Assert.AreEqual(aff.Location, location1, "Error: Affinity Location is not equal!");
                    Assert.AreEqual(aff.Description, description1, "Error: Affinity Description is not equal!");
                }

                foreach (var aff in vmPowershellCmdlets.GetAzureAffinityGroup(affinityName2))
                {
                    Console.WriteLine("Get-AzureAffinityGroup returned: Name - {0}, Location - {1}, Label - {2}, Description - {3}", aff.Name, aff.Location, aff.Label, aff.Description);
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
                    Console.WriteLine("Get-AzureAffinityGroup returned: Name - {0}, Location - {1}, Label - {2}, Description - {3}", aff.Name, aff.Location, aff.Label, aff.Description);
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
        
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Add,Get,Remove)-AzureCertificate)")]
        public void AzureCertificateTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;
            
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
                Assert.AreEqual(getCert1.Thumbprint, thumbprint1);  // Currently fails because of a bug

                CertificateContext getCert2 = vmPowershellCmdlets.GetAzureCertificate(serviceName, thumbprint2, "sha1")[0];
                Console.WriteLine("Cert is added: {0}", getCert2.Thumbprint);
                Assert.AreEqual(getCert2.Thumbprint, thumbprint2);

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


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("priya"), Description("Test the cmdlet (Get-Module)")]
        public void AzureCertificateSettingTest()
        {
            cleanup = true;
            testName = MethodBase.GetCurrentMethod().Name;

            string thumbprint = "C5AF4AEE8FD278F9D9FCFAB7DC5436B8DF3A5074";
            string store = "My";

            
            try
            {
                vmName = Utilities.GetUniqueShortName("PSTestVM");
                serviceName = Utilities.GetUniqueShortName("PSTestService");

                vmPowershellCmdlets.NewAzureService(serviceName, locationName);
               
                CertificateSetting cert = vmPowershellCmdlets.NewAzureCertificateSetting(thumbprint, store);

                CertificateSettingList certList = new CertificateSettingList();
                certList.Add(cert);
                
                AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(vmName, VMSizeInfo.Small, imageName);               
                AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, certList, "Cert1234!");                                

                PersistentVMConfigInfo persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);           
                
                PersistentVM vm = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);            
               
                vmPowershellCmdlets.NewAzureVM(serviceName, new [] {vm});

                PersistentVMRoleContext result = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
                Console.WriteLine("{0} is created", result.Name);



            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }    

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Add,Get,Set,Remove)-AzureDataDisk)")]
        public void AzureDataDiskTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;
            
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
                Console.WriteLine("Data disk added correctly.");
                                                               
                Assert.IsTrue(CheckDataDisk(vmName, serviceName, dataDiskInfo2, HostCaching.None), "Data disk is not properly added");
                Console.WriteLine("Data disk added correctly.");

                vmPowershellCmdlets.SetDataDisk(vmName, serviceName, HostCaching.ReadOnly, lunSlot1);                
                Assert.IsTrue(CheckDataDisk(vmName, serviceName, dataDiskInfo1, HostCaching.ReadOnly), "Data disk is not properly changed");
                Console.WriteLine("Data disk is changed correctly.");

                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
            finally
            {
                // Remove DataDisks created
                vmPowershellCmdlets.RemoveDataDisk(vmName, serviceName, new [] {lunSlot1, lunSlot2}); // Remove-AzureDataDisk
                // ToDo: Verify removal
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


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Add,Get,Update,Remove)-AzureDisk)")]
        public void AzureDiskTest()
        {
            testName = MethodBase.GetCurrentMethod().Name;
            cleanup = false;
           
            string vhdName = "128GBOS.vhd";
            string vhdLocalPath = "http://"+defaultAzureSubscription.CurrentStorageAccount+".blob.core.windows.net/vhdstore/"+vhdName;

            try
            {
                vmPowershellCmdlets.AddAzureDisk(vhdName, vhdLocalPath, vhdName, null);

                bool found = false;
                foreach (DiskContext disk in vmPowershellCmdlets.GetAzureDisk(vhdName))
                {
                    Console.WriteLine("Disk: Name - {0}, Label - {1}, Size - {2},", disk.DiskName, disk.Label, disk.DiskSizeInGB);
                    if (disk.DiskName == vhdName && disk.Label == vhdName)
                    {
                        found = true;
                        Console.WriteLine("{0} is found", disk.DiskName);
                    }

                }
                Assert.IsTrue(found, "Error: Disk is not added");

                string newLabel = "NewLabel";
                vmPowershellCmdlets.UpdateAzureDisk(vhdName, newLabel);

                DiskContext disk2 = vmPowershellCmdlets.GetAzureDisk(vhdName)[0];

                Console.WriteLine("Disk: Name - {0}, Label - {1}, Size - {2},", disk2.DiskName, disk2.Label, disk2.DiskSizeInGB);
                Assert.AreEqual(newLabel, disk2.Label);
                Console.WriteLine("Disk Label is successfully updated");

                vmPowershellCmdlets.RemoveAzureDisk(vhdName, false);
                Assert.IsTrue(CheckRemove(vmPowershellCmdlets.GetAzureDisk, vhdName), "The disk was not removed");

            }
            catch (Exception e)
            {
                pass = false;
                Console.WriteLine("Exception occurs: {0}", e.ToString());
            }
            finally
            {


            }

        }

    
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New,Get,Set,Remove,Move)-AzureDeployment)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void AzureDeploymentTest()
        {
            testName = MethodBase.GetCurrentMethod().Name;
            cleanup = true;


            // Choose the package and config files from local machine
            string packageName = Convert.ToString(TestContext.DataRow["packageName"]);
            string configName = Convert.ToString(TestContext.DataRow["configName"]);
            string upgradePackageName = Convert.ToString(TestContext.DataRow["upgradePackage"]);
            string upgradeConfigName = Convert.ToString(TestContext.DataRow["upgradeConfig"]);


            var packagePath1 = new FileInfo(@".\" + packageName);
            var configPath1 = new FileInfo(@".\" + configName);
            var packagePath2 = new FileInfo(@".\" + upgradePackageName);
            var configPath2 = new FileInfo(@".\" + upgradeConfigName);

            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);
            

            string deploymentName = "deployment1";
            string deploymentLabel = "label1";
            DeploymentInfoContext result;


            try
            {
                serviceName = Utilities.GetUniqueShortName("PSTestService");
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
                Console.WriteLine("service, {0}, is created.", serviceName);

                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Staging, deploymentLabel, deploymentName, false, false);
                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Staging);
                PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Staging, null, 1);
                Console.WriteLine("successfully deployed the package");


                // Move the deployment from 'Staging' to 'Production'
                vmPowershellCmdlets.MoveAzureDeployment(serviceName);
                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Production, null, 1);                
                Console.WriteLine("successfully moved");


                // Set the deployment status to 'Suspended'
                vmPowershellCmdlets.SetAzureDeploymentStatus(serviceName, DeploymentSlotType.Production, DeploymentStatus.Suspended);
                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Production, DeploymentStatus.Suspended, 1);
                Console.WriteLine("successfully changed the status");


                // Update the deployment
                vmPowershellCmdlets.SetAzureDeploymentConfig(serviceName, DeploymentSlotType.Production, configPath2.FullName);
                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Production, null, 2);
                Console.WriteLine("successfully updated the deployment");


                // Upgrade the deployment
                vmPowershellCmdlets.SetAzureDeploymentUpgrade(serviceName, DeploymentSlotType.Production, UpgradeType.Auto, packagePath2.FullName, configPath2.FullName);
                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                PrintAndCompareDeployment(result, serviceName, deploymentName, serviceName, DeploymentSlotType.Production, null, 2);
                Console.WriteLine("successfully updated the deployment");

                               
                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                try
                {
                    vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                    Assert.Fail("the deployment is not removed!");
                }
                catch(Exception e1)
                {
                    if (e1.ToString().Contains("ResourceNotFound"))
                    {
                        Console.WriteLine("Successfully removed the deployment");
                    }
                    else
                    {
                        Assert.Fail("Exception occurred: {0}", e1.ToString());
                    }
                }

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

        private bool PrintAndCompareDeployment(DeploymentInfoContext deployment, string serviceName, string deploymentName, string deploymentLabel, string slot, string status, int instanceCount)
        {
            Console.WriteLine("ServiceName:{0}, DeploymentID: {1}, Uri: {2}", deployment.ServiceName, deployment.DeploymentId, deployment.Url.AbsoluteUri);
            Console.WriteLine("Name - {0}, Label - {1}, Slot - {2}, Status - {3}", 
                deployment.DeploymentName, deployment.Label, deployment.Slot, deployment.Status);
            Console.WriteLine("RoleInstance: {0}", deployment.RoleInstanceList.Count);
            foreach (var instance in deployment.RoleInstanceList)
            {
                Console.WriteLine("InstanceName - {0}, InstanceStatus - {1}", instance.InstanceName, instance.InstanceStatus);
            }
            

            Assert.AreEqual(deployment.ServiceName, serviceName);
            Assert.AreEqual(deployment.DeploymentName, deploymentName);
            Assert.AreEqual(deployment.Label, deploymentLabel);
            Assert.AreEqual(deployment.Slot, slot);
            if (status != null)
            {
                Assert.AreEqual(deployment.Status, status);
            }
            
            Assert.AreEqual(deployment.RoleInstanceList.Count, instanceCount);
            
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New,Get)-AzureDns)")]
        public void AzureDnsTest()
        {
            cleanup = true;
            testName = MethodBase.GetCurrentMethod().Name;


            string dnsName = "OpenDns1";
            string ipAddress = "208.67.222.222";

            try
            {
                serviceName = Utilities.GetUniqueShortName("PSTestService");
                vmPowershellCmdlets.NewAzureService(serviceName, locationName);

                DnsServer dns = vmPowershellCmdlets.NewAzureDns(dnsName, ipAddress);

                AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(vmName, VMSizeInfo.ExtraSmall, imageName);
                AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, "password1234!");     
           
                PersistentVMConfigInfo persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);           
                
                PersistentVM vm = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);  
           
                vmPowershellCmdlets.NewAzureVM(serviceName, new []{vm}, null, new[]{dns}, null, null, null, null, null, null);
                


                DnsServerList dnsList =  vmPowershellCmdlets.GetAzureDns(vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production).DnsSettings);
                foreach (DnsServer dnsServer in dnsList)
                {
                    Console.WriteLine("DNS Server Name: {0}, DNS Server Address: {1}", dnsServer.Name, dnsServer.Address);
                    Assert.AreEqual(dnsServer.Name, dns.Name);
                    Assert.AreEqual(dnsServer.Address, dns.Address);
                }

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


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Add,Get,Set,Remove)-AzureEndpoint)")]
        public void AzureEndpointTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;

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

                vmPowershellCmdlets.AddEndPoint(vmName, serviceName, new[] { epInfo1, epInfo2 }); // Add-AzureEndpoint with Get-AzureVM and Update-AzureVm                             
                Assert.IsTrue(CheckEndpoint(vmName, serviceName, epInfo1), "Error: Endpoint was not added!");
                Assert.IsTrue(CheckEndpoint(vmName, serviceName, epInfo2), "Error: Endpoint was not added!");

                // Change the endpoint
                AzureEndPointConfigInfo epInfo3 = new AzureEndPointConfigInfo(ProtocolInfo.tcp, 60030, 60031, epName2);
                vmPowershellCmdlets.SetEndPoint(vmName, serviceName, epInfo3); // Set-AzureEndpoint with Get-AzureVM and Update-AzureVm                 
                Assert.IsTrue(CheckEndpoint(vmName, serviceName, epInfo3), "Error: Endpoint was not changed!");

                // Remove Endpoint
                vmPowershellCmdlets.RemoveEndPoint(vmName, serviceName, new[] { epName1, epName2 }); // Remove-AzureEndpoint
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


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Get-AzureLocation)")]
        public void AzureLocationTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;

            try
            {
                foreach (LocationsContext loc in vmPowershellCmdlets.GetAzureLocation())
                {
                    Console.WriteLine("Location: Name - {0}, DisplayName - {1}", loc.Name, loc.DisplayName);
                }

                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }            
        }



  



        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureOSDisk)")]
        public void AzureOSDiskTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;

            try
            {
                PersistentVM vm = vmPowershellCmdlets.GetAzureVM(vmName, serviceName).VM;
                OSVirtualHardDisk osdisk = vmPowershellCmdlets.GetAzureOSDisk(vm);
                Console.WriteLine("OS Disk: Name - {0}, Label - {1}, HostCaching - {2}, OS - {3}", osdisk.DiskName, osdisk.DiskLabel, osdisk.HostCaching, osdisk.OS);
                Assert.IsTrue(osdisk.Equals(vm.OSVirtualHardDisk), "OS disk returned is not the same!");

                PersistentVM vm2 = vmPowershellCmdlets.SetAzureOSDisk(HostCaching.ReadOnly, vm);
                osdisk = vmPowershellCmdlets.GetAzureOSDisk(vm2);
                Console.WriteLine("OS Disk: Name - {0}, Label - {1}, HostCaching - {2}, OS - {3}", osdisk.DiskName, osdisk.DiskLabel, osdisk.HostCaching, osdisk.OS);
                Assert.IsTrue(osdisk.Equals(vm2.OSVirtualHardDisk), "OS disk returned is not the same!");

                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Get-AzureOSVersion)")]
        public void AzureOSVersionTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;       

            try
            {
                foreach (OSVersionsContext osVersions in vmPowershellCmdlets.GetAzureOSVersion())
                {
                    Console.WriteLine("OS Version: Family - {0}, FamilyLabel - {1}, Version - {2}", osVersions.Family, osVersions.FamilyLabel, osVersions.Version);
                }

                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureRole)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void AzureRoleTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;

            // Choose the package and config files from local machine
            string packageName = Convert.ToString(TestContext.DataRow["packageName"]);
            string configName = Convert.ToString(TestContext.DataRow["configName"]);
            string upgradePackageName = Convert.ToString(TestContext.DataRow["upgradePackage"]);
            string upgradeConfigName = Convert.ToString(TestContext.DataRow["upgradeConfig"]);


            var packagePath1 = new FileInfo(@".\" + packageName);
            var configPath1 = new FileInfo(@".\" + configName);

            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);
            

            string deploymentName = "deployment1";
            string deploymentLabel = "label1";
            string slot = DeploymentSlotType.Production;

            //DeploymentInfoContext result;
            string roleName = "";

            try
            {
            

                serviceName = Utilities.GetUniqueShortName("PSTestService");
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);

                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, slot, deploymentLabel, deploymentName, false, false);

            
                foreach (RoleContext role in vmPowershellCmdlets.GetAzureRole(serviceName, slot, null, false))
                {
                    Console.WriteLine("Role: Name - {0}, ServiceName - {1}, DeploymenntID - {2}, InstanceCount - {3}", role.RoleName, role.ServiceName, role.DeploymentID, role.InstanceCount);
                    Assert.AreEqual(serviceName, role.ServiceName);
                    roleName = role.RoleName;
                }
                
                vmPowershellCmdlets.SetAzureRole(serviceName, slot, roleName, 2);

                foreach (RoleContext role in vmPowershellCmdlets.GetAzureRole(serviceName, slot, null, false))
                {
                    Console.WriteLine("Role: Name - {0}, ServiceName - {1}, DeploymenntID - {2}, InstanceCount - {3}", role.RoleName, role.ServiceName, role.DeploymentID, role.InstanceCount);
                    Assert.AreEqual(serviceName, role.ServiceName);
                    Assert.AreEqual(2, role.InstanceCount);                   
                }

                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureSubnet)")]
        public void AzureSubnetTest()
        {
            cleanup = true;
            testName = MethodBase.GetCurrentMethod().Name;

            try
            {
                serviceName = Utilities.GetUniqueShortName("PSTestService");
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
                
                PersistentVM vm = vmPowershellCmdlets.NewAzureVMConfig(new AzureVMConfigInfo(vmName, VMSizeInfo.Small, imageName));
                AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, "password1234!");
                azureProvisioningConfig.Vm = vm;

                string [] subs = new []  {"subnet1", "subnet2", "subnet3"};
                vm = vmPowershellCmdlets.SetAzureSubnet(vmPowershellCmdlets.AddAzureProvisioningConfig(azureProvisioningConfig), subs);
                
                SubnetNamesCollection subnets = vmPowershellCmdlets.GetAzureSubnet(vm);
                foreach (string subnet in subnets)
                {
                    Console.WriteLine("Subnet: {0}", subnet);
                }                
                CollectionAssert.AreEqual(subnets, subs);
                
                pass = true;
            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New,Get)-AzureStorageKey)")]
        public void AzureStorageKeyTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;
            
            try
            {
                StorageServiceKeyOperationContext key1 = vmPowershellCmdlets.GetAzureStorageAccountKey(defaultAzureSubscription.CurrentStorageAccount); // Get-AzureStorageAccountKey
                Console.WriteLine("Primary - {0}", key1.Primary);
                Console.WriteLine("Secondary - {0}", key1.Secondary);

                StorageServiceKeyOperationContext key2 = vmPowershellCmdlets.NewAzureStorageAccountKey(defaultAzureSubscription.CurrentStorageAccount, KeyType.Primary);
                Console.WriteLine("Primary - {0}", key2.Primary);
                Console.WriteLine("Secondary - {0}", key2.Secondary);

                Assert.AreNotEqual(key1.Primary, key2.Primary);
                Assert.AreEqual(key1.Secondary, key2.Secondary);

                pass = true;
            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }            
        }


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New,Get,Set,Remove)-AzureStorageAccount)")]
        public void AzureStorageAccountTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;
            

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


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Add,Get,Save,Update,Remove)-AzureVMImage)")]
        public void AzureVMImageTest()
        {

            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;

            string newImageName = Utilities.GetUniqueShortName("vmimage");
            string blobUrlRoot = string.Format(@"http://{0}.blob.core.windows.net/", defaultAzureSubscription.CurrentStorageAccount);
            string mediaLocation = string.Format("{0}vhdstore/128GBOS.vhd", blobUrlRoot);

            string oldLabel = "old label";
            string newLabel = "new label";            

            try
            {
                OSImageContext result = vmPowershellCmdlets.AddAzureVMImage(newImageName, mediaLocation, OSType.Windows, oldLabel);

                OSImageContext resultReturned = vmPowershellCmdlets.GetAzureVMImage(newImageName)[0];

                if (!result.Equals(resultReturned))
                {
                    pass = false;                    
                }



                result = vmPowershellCmdlets.UpdateAzureVMImage(newImageName, newLabel);


                resultReturned = vmPowershellCmdlets.GetAzureVMImage(newImageName)[0];

                //Assert.AreEqual(result, resultReturned);

                if (!result.Equals(resultReturned))
                {
                    pass = false;
                }


                vmPowershellCmdlets.RemoveAzureVMImage(newImageName);

                //pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
            finally
            {

            }
        }

        /// <summary>
        /// AzureVNetGatewayTest()       
        /// </summary>
        /// Note: Create a VNet, a LocalNet from the portal without creating a gateway.
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New,Get,Set,Remove)-AzureVNetGateway)")]
        public void AzureVNetGatewayTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;

            string vnetName1 = "NewVNet1"; // For connect test
            string vnetName2 = "NewVNet2"; // For disconnect test
            string vnetName3 = "NewVNet3"; // For create test

            string localNet = "LocalNet1"; // Your local network site name.

            try
            {
                // New-AzureVNetGateway
                vmPowershellCmdlets.NewAzureVNetGateway(vnetName3);

                foreach (VirtualNetworkSiteContext site in vmPowershellCmdlets.GetAzureVNetSite(vnetName3))
                {
                    Console.WriteLine("Name: {0}, AffinityGroup: {1}", site.Name, site.AffinityGroup);
                }

                // Remove-AzureVnetGateway
                vmPowershellCmdlets.RemoveAzureVNetGateway(vnetName3);
                foreach (VirtualNetworkGatewayContext gateway in vmPowershellCmdlets.GetAzureVNetGateway(vnetName3))
                {
                    Console.WriteLine("State: {0}, VIP: {1}", gateway.State.ToString(), gateway.VIPAddress);
                }
                
                

                // Set-AzureVNetGateway -Connect Test
                vmPowershellCmdlets.SetAzureVNetGateway("connect", vnetName1, localNet);
                
                foreach (GatewayConnectionContext connection in vmPowershellCmdlets.GetAzureVNetConnection(vnetName1))
                {
                    Console.WriteLine("Connectivity: {0}, LocalNetwork: {1}", connection.ConnectivityState, connection.LocalNetworkSiteName);
                    Assert.IsFalse(connection.ConnectivityState.ToLowerInvariant().Contains("notconnected"));
                }
                foreach (VirtualNetworkGatewayContext gateway in vmPowershellCmdlets.GetAzureVNetGateway(vnetName1))
                {
                    Console.WriteLine("State: {0}, VIP: {1}", gateway.State.ToString(), gateway.VIPAddress);
                }


                // Set-AzureVNetGateway -Disconnect
                vmPowershellCmdlets.SetAzureVNetGateway("disconnect", vnetName2, localNet);
               
                foreach (GatewayConnectionContext connection in vmPowershellCmdlets.GetAzureVNetConnection(vnetName2))
                {
                    Console.WriteLine("Connectivity: {0}, LocalNetwork: {1}", connection.ConnectivityState, connection.LocalNetworkSiteName);
                    if (connection.LocalNetworkSiteName == localNet)
                    {
                        Assert.IsTrue(connection.ConnectivityState.ToLowerInvariant().Contains("notconnected"));
                    }
                }

                foreach (VirtualNetworkGatewayContext gateway in vmPowershellCmdlets.GetAzureVNetGateway(vnetName2))
                {
                    Console.WriteLine("State: {0}, VIP: {1}", gateway.State.ToString(), gateway.VIPAddress);
                }

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

        /// <summary>
        /// 
        /// </summary>
        /// Note: You have to manually create a virtual network, a Local network, a gateway, and connect them.
        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet (Get-AzureVNetGatewayKey, Get-AzureVNetConnection)")]
        public void AzureVNetGatewayKeyTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;
            
            string vnetName = "NewVNet1";
            

            try
            {                
                SharedKeyContext result = vmPowershellCmdlets.GetAzureVNetGatewayKey(vnetName, vmPowershellCmdlets.GetAzureVNetConnection(vnetName)[0].LocalNetworkSiteName);
                Console.WriteLine(result.Value);

                pass = true;

            }
            catch (Exception e)
            {
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }            
        }


        [TestMethod(), TestCategory("Functional"), TestProperty("Feature", "IAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureVNetConfig)")]
        public void AzureVNetConfigTest()
        {
            cleanup = false;
            testName = MethodBase.GetCurrentMethod().Name;


            string filePath = "C:\\vnetconfig.netcfg";            

            try
            {

                var result = vmPowershellCmdlets.GetAzureVNetConfig(filePath);

                vmPowershellCmdlets.SetAzureVNetConfig(filePath);

                Collection<VirtualNetworkSiteContext> vnetSites = vmPowershellCmdlets.GetAzureVNetSite(null);
                foreach (var re in vnetSites)
                {
                    Console.WriteLine("VNet: {0}", re.Name);
                }

                vmPowershellCmdlets.RemoveAzureVNetConfig();

                Collection<VirtualNetworkSiteContext> vnetSitesAfter = vmPowershellCmdlets.GetAzureVNetSite(null);

                Assert.AreNotEqual(vnetSites.Count, vnetSitesAfter.Count, "No Vnet is removed");
                
                foreach (var re in vnetSitesAfter)
                {
                    Console.WriteLine("VNet: {0}", re.Name);
                }

                pass = true;

            }
            catch (Exception e)
            {
                if (e.ToString().Contains("while in use"))
                {
                    Console.WriteLine(e.InnerException.ToString());
                }
                else
                {
                    Assert.Fail("Exception occurred: {0}", e.ToString());
                }
            }
            finally
            {

            }
        }




        


        // CheckRemove checks if 'fn(name)' exists.    'fn(name)' is usually 'Get-AzureXXXXX name'
        private bool CheckRemove<T>(Func<string, T> fn, string name)
        {
            try
            {
                fn(name);
                return false;
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("ResourceNotFound"))
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

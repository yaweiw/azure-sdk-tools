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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;

    [TestClass]
    public class ScenarioTest
    {
        private ServiceManagementCmdletTestHelper vmPowershellCmdlets;
        private SubscriptionData defaultAzureSubscription;
        private StorageServiceKeyOperationContext storageAccountKey;       


        private string locationName;
        private string imageName;
        private string serviceName = "DefaultServiceName";
        private string vmName = "DefaultVmName";

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
            }
            else
            {
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName, serviceName, imageName, "p@ssw0rd", locationName);
                Console.WriteLine("Service Name: {0} is created.", serviceName);
            }


        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,Get-AzureVM,Get-AzureLocation,Import-AzurePublishSettingsFile,Get-AzureSubscription,Set-AzureSubscription)")]
        public void NewWindowsAzureQuickVM()
        {           

            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            string newAzureQuickVMSvcName = Utilities.GetUniqueShortName("PSTestService");

            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, newAzureQuickVMSvcName, imageName, "p@ssw0rd", locationName);

            // Verify
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(newAzureQuickVMName, vmRoleCtxt.Name, true);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);

            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));
        }

        // Basic Provisioning a Virtual Machine	  
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (Get-AzureLocation,Test-AzureName ,Get-AzureVMImage,New-AzureQuickVM,Get-AzureVM ,Restart-AzureVM,Stop-AzureVM , Start-AzureVM)")]
        public void ProvisionLinuxVM()
        {            
            string newAzureQuickVMSvcName = Utilities.GetUniqueShortName("PSTestService");
         
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSLinuxVM");
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux", "testvmimage" }, false);

            vmPowershellCmdlets.NewAzureQuickLinuxVM(OS.Linux, newAzureQuickVMName, newAzureQuickVMSvcName, imageName, "user", "p@ssw0rd", locationName);

            // Verify
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(newAzureQuickVMName, vmRoleCtxt.Name, true);

            // Disabling Stop / start / restart tests for now due to timing isues
            /*
            // Stop & start the VM
            vmPowershellCmdlets.StopAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(vmRoleCtxt.PowerState, VMPowerState.Stopped);
            vmPowershellCmdlets.StartAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(vmRoleCtxt.PowerState, VMPowerState.Started.ToString());

            // Restart the VM
            vmPowershellCmdlets.StopAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(vmRoleCtxt.PowerState, VMPowerState.Stopped);
            vmPowershellCmdlets.RestartAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(vmRoleCtxt.PowerState, VMPowerState.Started.ToString());
             * */

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            
            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));

            //TODO: Need to do proper cleanup of the service
            //            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName);
            //            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureService(newAzureQuickVMSvcName));
        }

        //Verify Advanced Provisioning
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureService,New-AzureVMConfig,Add-AzureProvisioningConfig ,Add-AzureDataDisk ,Add-AzureEndpoint,New-AzureVM)")]
        public void AdvancedProvisioning()
        {
          
            string newAzureVM1Name = Utilities.GetUniqueShortName("PSTestVM");
            string newAzureVM2Name = Utilities.GetUniqueShortName("PSTestVM");

            string newAzureSvcName = Utilities.GetUniqueShortName("PSTestService");
            vmPowershellCmdlets.NewAzureService(newAzureSvcName, newAzureSvcName, locationName);

            AzureVMConfigInfo azureVMConfigInfo1 = new AzureVMConfigInfo(newAzureVM1Name, VMSizeInfo.ExtraSmall, imageName);
            AzureVMConfigInfo azureVMConfigInfo2 = new AzureVMConfigInfo(newAzureVM2Name, VMSizeInfo.ExtraSmall, imageName);
            AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, "p@ssw0rd");
            AddAzureDataDiskConfig azureDataDiskConfigInfo = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk1", 0);
            AzureEndPointConfigInfo azureEndPointConfigInfo = new AzureEndPointConfigInfo(ProtocolInfo.tcp, 80, 80, "web", "lbweb", 80, ProtocolInfo.http, @"/");

            PersistentVMConfigInfo persistentVMConfigInfo1 = new PersistentVMConfigInfo(azureVMConfigInfo1, azureProvisioningConfig, azureDataDiskConfigInfo, azureEndPointConfigInfo);
            PersistentVMConfigInfo persistentVMConfigInfo2 = new PersistentVMConfigInfo(azureVMConfigInfo2, azureProvisioningConfig, azureDataDiskConfigInfo, azureEndPointConfigInfo);

            PersistentVM persistentVM1 = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo1);
            PersistentVM persistentVM2 = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo2);

            PersistentVM[] VMs = { persistentVM1, persistentVM2 };            
            vmPowershellCmdlets.NewAzureVM(newAzureSvcName, VMs);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureVM1Name, newAzureSvcName);
            vmPowershellCmdlets.RemoveAzureVM(newAzureVM2Name, newAzureSvcName);
            
            vmPowershellCmdlets.RemoveAzureService(newAzureSvcName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureVM1Name, newAzureSvcName));
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureVM2Name, newAzureSvcName));
        }

        //Modifying Existing Virtual Machines
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig ,Add-AzureDataDisk ,Add-AzureEndpoint,New-AzureVM)")]
        public void ModifyingVM()
        {
            
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            string newAzureQuickVMSvcName = Utilities.GetUniqueShortName("PSTestService");
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, newAzureQuickVMSvcName, imageName, "p@ssw0rd", locationName);

            AddAzureDataDiskConfig azureDataDiskConfigInfo1 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk1", 0);
            AddAzureDataDiskConfig azureDataDiskConfigInfo2 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk2", 1);
            AzureEndPointConfigInfo azureEndPointConfigInfo = new AzureEndPointConfigInfo(ProtocolInfo.tcp, 1433, 2000, "sql");
            AddAzureDataDiskConfig[] dataDiskConfig = { azureDataDiskConfigInfo1, azureDataDiskConfigInfo2 };
            vmPowershellCmdlets.AddVMDataDisksAndEndPoint(newAzureQuickVMName, newAzureQuickVMSvcName, dataDiskConfig, azureEndPointConfigInfo);

            SetAzureDataDiskConfig setAzureDataDiskConfig1 = new SetAzureDataDiskConfig(HostCaching.ReadWrite, 0);
            SetAzureDataDiskConfig setAzureDataDiskConfig2 = new SetAzureDataDiskConfig(HostCaching.ReadWrite, 0);
            SetAzureDataDiskConfig[] diskConfig = { setAzureDataDiskConfig1, setAzureDataDiskConfig2 };
            vmPowershellCmdlets.SetVMDataDisks(newAzureQuickVMName, newAzureQuickVMSvcName, diskConfig);

            vmPowershellCmdlets.GetAzureDataDisk(newAzureQuickVMName, newAzureQuickVMSvcName);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            
            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));

        }

        // Changes that Require a Reboot
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (Get-AzureVM,Set-AzureDataDisk ,Update-AzureVM,Set-AzureVMSize)")]
        public void UpdateAndReboot()
        {
          
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            string newAzureQuickVMSvcName = Utilities.GetUniqueShortName("PSTestService");
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, newAzureQuickVMSvcName, imageName, "p@ssw0rd", locationName);

            AddAzureDataDiskConfig azureDataDiskConfigInfo1 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk1", 0);
            AddAzureDataDiskConfig azureDataDiskConfigInfo2 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk2", 1);
            AddAzureDataDiskConfig[] dataDiskConfig = { azureDataDiskConfigInfo1, azureDataDiskConfigInfo2 };
            vmPowershellCmdlets.AddVMDataDisks(newAzureQuickVMName, newAzureQuickVMSvcName, dataDiskConfig);

            SetAzureDataDiskConfig setAzureDataDiskConfig1 = new SetAzureDataDiskConfig(HostCaching.ReadOnly, 0);
            SetAzureDataDiskConfig setAzureDataDiskConfig2 = new SetAzureDataDiskConfig(HostCaching.ReadOnly, 0);
            SetAzureDataDiskConfig[] diskConfig = { setAzureDataDiskConfig1, setAzureDataDiskConfig2 };
            vmPowershellCmdlets.SetVMDataDisks(newAzureQuickVMName, newAzureQuickVMSvcName, diskConfig);

            SetAzureVMSizeConfig vmSizeConfig = new SetAzureVMSizeConfig(InstanceSize.Medium);
            vmPowershellCmdlets.SetVMSize(newAzureQuickVMName, newAzureQuickVMSvcName, vmSizeConfig);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            
            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName); 
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (Get-AzureDisk,Remove-AzureVM,Remove-AzureDisk,Get-AzureVMImage)")]
        public void ManagingDiskImages()
        {

            // Create a unique VM name and Service Name
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            string newAzureQuickVMSvcName = Utilities.GetUniqueShortName("PSTestService");

            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, newAzureQuickVMSvcName, imageName, "p@ssw0rd", locationName); // New-AzureQuickVM
            Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, newAzureQuickVMSvcName);

            // starting the test.

            Collection<DiskContext> vmDisks = vmPowershellCmdlets.GetAzureDiskAttachedToRoleName(new[] { newAzureQuickVMName });  // Get-AzureDisk | Where {$_.AttachedTo.RoleName -eq $vmname }

            foreach (var disk in vmDisks)
                Console.WriteLine("The disk, {0}, is created", disk.DiskName);

            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);  // Remove-AzureVM
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));
            Console.WriteLine("The VM, {0}, is successfully removed.", newAzureQuickVMName);

            foreach (var disk in vmDisks)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        vmPowershellCmdlets.RemoveAzureDisk(disk.DiskName, true); // Remove-AzureDisk
                        break;
                    }
                    catch (Exception e)
                    {
                        if (e.ToString().ToLowerInvariant().Contains("currently in use") && i != 2)
                        {
                            Console.WriteLine("The vhd, {0}, is still in the state of being used by the deleted VM", disk.DiskName);
                            Thread.Sleep(120000);
                            continue;
                        }
                        else
                        {
                            Assert.Fail("error during Remove-AzureDisk: {0}", e.ToString());
                        }
                    }
                }

                try
                {
                    vmPowershellCmdlets.GetAzureDisk(disk.DiskName); // Get-AzureDisk -DiskName (try to get the removed disk.)
                    Assert.Fail("Disk is not removed: {0}", disk.DiskName);
                }
                catch (Exception e)
                {
                    if (e.ToString().ToLowerInvariant().Contains("does not exist"))
                    {
                        Console.WriteLine("The disk, {0}, is successfully removed.", disk.DiskName);
                        continue;
                    }
                    else
                    {
                        Assert.Fail("Exception: {0}", e.ToString());
                    }
                }
            }

            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName); // Clean up the service.

            try
            {
                vmPowershellCmdlets.GetAzureService(newAzureQuickVMSvcName);
                Assert.Fail("The service, {0}, is not removed", newAzureQuickVMSvcName);
            }
            catch (Exception e)
            {
                if (e.ToString().ToLowerInvariant().Contains("does not exist"))
                {
                    Console.WriteLine("The service, {0}, is successfully removed", newAzureQuickVMSvcName);
                }
                else
                {
                    Assert.Fail("Error occurred: {0}", e.ToString());
                }
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM,Save-AzureVMImage)")]
        public void CaptureImagingExportingImportingVMConfig()
        {         

            // Create a unique VM name
            string newAzureVMName = Utilities.GetUniqueShortName("PSTestVM");
            Console.WriteLine("VM Name: {0}", newAzureVMName);

            // Create a unique Service Name
            string newAzureSvcName = Utilities.GetUniqueShortName("PSTestService");
            vmPowershellCmdlets.NewAzureService(newAzureSvcName, newAzureSvcName, locationName);
            Console.WriteLine("VM Service Name: {0}", newAzureSvcName);

            // starting the test.

            AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(newAzureVMName, VMSizeInfo.Small, imageName); // parameters for New-AzureVMConfig (-Name -InstanceSize -ImageName)            
            AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, "p@ssw0rd"); // parameters for Add-AzureProvisioningConfig (-Windows -Password)            
            PersistentVMConfigInfo persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
            PersistentVM persistentVM = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo); // New-AzureVMConfig & Add-AzureProvisioningConfig

            PersistentVM[] VMs = { persistentVM };
            vmPowershellCmdlets.NewAzureVM(newAzureSvcName, VMs); // New-AzureVM
            Console.WriteLine("The VM is successfully created: {0}", persistentVM.RoleName);
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(persistentVM.RoleName, newAzureSvcName);
            Assert.AreEqual(vmRoleCtxt.Name, persistentVM.RoleName, true);


            vmPowershellCmdlets.StopAzureVM(newAzureVMName, newAzureSvcName); // Stop-AzureVM
            for (int i = 0; i < 3; i++)
            {
                vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(persistentVM.RoleName, newAzureSvcName);
                if (vmRoleCtxt.InstanceStatus == "StoppedVM")
                    break;
                else
                {
                    Console.WriteLine("The status of the VM {0} : {1}", persistentVM.RoleName, vmRoleCtxt.InstanceStatus);
                    Thread.Sleep(120000);
                }
            }
            Assert.AreEqual(vmRoleCtxt.InstanceStatus, "StoppedVM", true);

            //TODO
            // RDP 

            //TODO:
            // Run sysprep and shutdown 

            // Check the status of VM
            //PersistentVMRoleContext vmRoleCtxt2 = vmPowershellCmdlets.GetAzureVM(newAzureVMName, newAzureSvcName); // Get-AzureVM -Name
            //Assert.AreEqual(newAzureVMName, vmRoleCtxt2.Name, true);  // 

            // Save-AzureVMImage
            //string newImageName = "newImage";
            //string newImageLabel = "newImageLabel";
            //string postAction = "Delete";

            // Save-AzureVMImage -ServiceName -Name -NewImageName -NewImageLabel -PostCaptureAction
            //vmPowershellCmdlets.SaveAzureVMImage(newAzureSvcName, newAzureVMName, newImageName, newImageLabel, postAction);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(persistentVM.RoleName, newAzureSvcName);
            vmPowershellCmdlets.RemoveAzureService(newAzureSvcName); 
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(persistentVM.RoleName, newAzureSvcName));
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (Export-AzureVM,Remove-AzureVM,Import-AzureVM,New-AzureVM)")]
        public void ExportingImportingVMConfigAsTemplateforRepeatableUsage()
        {           

            // Create a unique VM name
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");

            // Create a unique Service Name
            string newAzureQuickVMSvcName = Utilities.GetUniqueShortName("PSTestService");
            
            

            // Create a new Azure quick VM
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, newAzureQuickVMSvcName, imageName, "p@ssw0rd", locationName); // New-AzureQuickVM
            Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, newAzureQuickVMSvcName);

            // starting the test.

            string path = "C:\\temp\\mytestvmconfig1.xml";
            PersistentVMRoleContext vmRole = vmPowershellCmdlets.ExportAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName, path); // Export-AzureVM
            Console.WriteLine("Exporting VM is successfully done: path - {0}  Name - {1}", path, vmRole.Name);

            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName); // Remove-AzureVM
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));
            Console.WriteLine("The VM is successfully removed: {0}", newAzureQuickVMName);

            List<PersistentVM> VMs = new List<PersistentVM>();
            foreach (var pervm in vmPowershellCmdlets.ImportAzureVM(path)) // Import-AzureVM
            {
                VMs.Add(pervm);
                Console.WriteLine("The VM, {0}, is imported.", pervm.RoleName);
            }


            for (int i = 0; i < 3; i++)
            {
                try
                {
                    vmPowershellCmdlets.NewAzureVM(newAzureQuickVMSvcName, VMs.ToArray()); // New-AzureVM
                    Console.WriteLine("All VMs are successfully created.");
                    foreach (var vm in VMs)
                    {
                        Console.WriteLine("created VM: {0}", vm.RoleName);
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (e.ToString().ToLowerInvariant().Contains("currently in use") && i != 2)
                    {
                        Console.WriteLine("The removed VM is still using the vhd");
                        Thread.Sleep(120000);
                        continue;
                    }
                    else
                    {
                        Assert.Fail("error during New-AzureVM: {0}", e.ToString());
                    }
                }
            }

            // Verify
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(newAzureQuickVMName, vmRoleCtxt.Name, true);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));



            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName); // Clean up the service.

            try
            {
                vmPowershellCmdlets.GetAzureService(newAzureQuickVMSvcName);
                Assert.Fail("The service, {0}, is not removed", newAzureQuickVMSvcName);
            }
            catch (Exception e)
            {
                if (e.ToString().ToLowerInvariant().Contains("does not exist"))
                {
                    Console.WriteLine("The service, {0}, is successfully removed", newAzureQuickVMSvcName);
                }
                else
                {
                    Assert.Fail("Error occurred: {0}", e.ToString());
                }
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (Get-AzureVM,Get-AzureEndpoint,Get-AzureRemoteDesktopFile)")]
        public void ManagingRDPSSHConnectivity()
        {            

            // Create a unique VM name
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");

            // Create a unique Service Name
            string newAzureQuickVMSvcName = Utilities.GetUniqueShortName("PSTestService");

            // Create a new Azure quick VM
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, newAzureQuickVMSvcName, imageName, "p@ssw0rd", locationName); // New-AzureQuickVM
            Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, newAzureQuickVMSvcName);

            // starting the test.

            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName); // Get-AzureVM
            InputEndpointContext inputEndpointCtxt = vmPowershellCmdlets.GetAzureEndPoint(vmRoleCtxt)[0]; // Get-AzureEndpoint
            Console.WriteLine("InputEndpointContext Name: {0}", inputEndpointCtxt.Name);
            Console.WriteLine("InputEndpointContext port: {0}", inputEndpointCtxt.Port);
            Console.WriteLine("InputEndpointContext protocol: {0}", inputEndpointCtxt.Protocol);
            Assert.AreEqual(inputEndpointCtxt.Name, "RemoteDesktop", true);

            string path = "C:\\temp\\myvmconnection.rdp";
            vmPowershellCmdlets.GetAzureRemoteDesktopFile(newAzureQuickVMName, newAzureQuickVMSvcName, path, false); // Get-AzureRemoteDesktopFile
            Console.WriteLine("RDP file is successfully created at: {0}", path);

            // ToDo: Automate RDP.
            //vmPowershellCmdlets.GetAzureRemoteDesktopFile(newAzureQuickVMName, newAzureQuickVMSvcName, path, true); // Get-AzureRemoteDesktopFile -Launch

            Console.WriteLine("Test passed");

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, newAzureQuickVMSvcName));
            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName); 
            Console.WriteLine("VM is successfully removed");
            // vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName);
            // Assert.AreEqual(null, vmPowershellCmdlets.GetAzureService(newAzureQuickVMSvcName));
            //Console.WriteLine("The service is successfully removed");


            //TODO: Need to do proper cleanup of the service
            //            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName);
            //            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureService(newAzureQuickVMSvcName));


        }
    }
}

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
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;

    using Microsoft.WindowsAzure.ServiceManagement;
    using System.Reflection;
    using System.Net;
    using System.Net.Cache;
    using System.IO;
    using System.Text;

    [TestClass]
    public class ScenarioTest : ServiceManagementTest
    {        
        private string serviceName;
        
        bool cleanup = false;        
        string perfFile;

        [TestInitialize]
        public void Initialize()
        {           
            serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
            pass = false;
            testStartTime = DateTime.Now;
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,Get-AzureVM,Get-AzureLocation,Import-AzurePublishSettingsFile,Get-AzureSubscription,Set-AzureSubscription)")]
        public void NewWindowsAzureQuickVM()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string newAzureQuickVMName = Utilities.GetUniqueShortName(vmNamePrefix);
            
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName);

            // Verify
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName);
            Assert.AreEqual(newAzureQuickVMName, vmRoleCtxt.Name, true);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);

            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));

            pass = true;
            cleanup = true;
        }

        // Basic Provisioning a Virtual Machine	  
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (Get-AzureLocation,Test-AzureName ,Get-AzureVMImage,New-AzureQuickVM,Get-AzureVM ,Restart-AzureVM,Stop-AzureVM , Start-AzureVM)")]
        public void ProvisionLinuxVM()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
                     
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSLinuxVM");
            string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux", "testvmimage" }, false);

            vmPowershellCmdlets.NewAzureQuickLinuxVM(OS.Linux, newAzureQuickVMName, serviceName, linuxImageName, "user", password, locationName);

            // Verify
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName);
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
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);
                        
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));

            //TODO: Need to do proper cleanup of the service
            //            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName);
            //            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureService(newAzureQuickVMSvcName));

            pass = true;
            cleanup = true;            
        }

        //Verify Advanced Provisioning
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureService,New-AzureVMConfig,Add-AzureProvisioningConfig ,Add-AzureDataDisk ,Add-AzureEndpoint,New-AzureVM)")]
        public void AdvancedProvisioning()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            string newAzureVM1Name = Utilities.GetUniqueShortName("PSTestVM");
            string newAzureVM2Name = Utilities.GetUniqueShortName("PSTestVM");
            
            vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);

            AzureVMConfigInfo azureVMConfigInfo1 = new AzureVMConfigInfo(newAzureVM1Name, VMSizeInfo.ExtraSmall, imageName);
            AzureVMConfigInfo azureVMConfigInfo2 = new AzureVMConfigInfo(newAzureVM2Name, VMSizeInfo.ExtraSmall, imageName);
            AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password);
            AddAzureDataDiskConfig azureDataDiskConfigInfo = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk1", 0);
            AzureEndPointConfigInfo azureEndPointConfigInfo = new AzureEndPointConfigInfo(ProtocolInfo.tcp, 80, 80, "web", "lbweb", 80, ProtocolInfo.http, @"/");

            PersistentVMConfigInfo persistentVMConfigInfo1 = new PersistentVMConfigInfo(azureVMConfigInfo1, azureProvisioningConfig, azureDataDiskConfigInfo, azureEndPointConfigInfo);
            PersistentVMConfigInfo persistentVMConfigInfo2 = new PersistentVMConfigInfo(azureVMConfigInfo2, azureProvisioningConfig, azureDataDiskConfigInfo, azureEndPointConfigInfo);

            PersistentVM persistentVM1 = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo1);
            PersistentVM persistentVM2 = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo2);

            PersistentVM[] VMs = { persistentVM1, persistentVM2 };                        
            vmPowershellCmdlets.NewAzureVM(serviceName, VMs);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureVM1Name, serviceName);
            vmPowershellCmdlets.RemoveAzureVM(newAzureVM2Name, serviceName);            
            
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureVM1Name, serviceName));
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureVM2Name, serviceName));
            cleanup = true;
            pass = true;
        }

        /// <summary>
        /// Modifying Existing Virtual Machines
        /// </summary>
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig ,Add-AzureDataDisk ,Add-AzureEndpoint,New-AzureVM)")]
        public void ModifyingVM()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, username, imageName, password, locationName);

            AddAzureDataDiskConfig azureDataDiskConfigInfo1 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk1", 0);
            AddAzureDataDiskConfig azureDataDiskConfigInfo2 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk2", 1);
            AzureEndPointConfigInfo azureEndPointConfigInfo = new AzureEndPointConfigInfo(ProtocolInfo.tcp, 1433, 2000, "sql");
            AddAzureDataDiskConfig[] dataDiskConfig = { azureDataDiskConfigInfo1, azureDataDiskConfigInfo2 };
            vmPowershellCmdlets.AddVMDataDisksAndEndPoint(newAzureQuickVMName, serviceName, dataDiskConfig, azureEndPointConfigInfo);

            SetAzureDataDiskConfig setAzureDataDiskConfig1 = new SetAzureDataDiskConfig(HostCaching.ReadWrite, 0);
            SetAzureDataDiskConfig setAzureDataDiskConfig2 = new SetAzureDataDiskConfig(HostCaching.ReadWrite, 0);
            SetAzureDataDiskConfig[] diskConfig = { setAzureDataDiskConfig1, setAzureDataDiskConfig2 };
            vmPowershellCmdlets.SetVMDataDisks(newAzureQuickVMName, serviceName, diskConfig);

            vmPowershellCmdlets.GetAzureDataDisk(newAzureQuickVMName, serviceName);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);
                        
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));
            cleanup = true;
            pass = true;
        }

        // Changes that Require a Reboot
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("priya"), Description("Test the cmdlets (Get-AzureVM,Set-AzureDataDisk ,Update-AzureVM,Set-AzureVMSize)")]
        public void UpdateAndReboot()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
          
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");            
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName);

            AddAzureDataDiskConfig azureDataDiskConfigInfo1 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk1", 0);
            AddAzureDataDiskConfig azureDataDiskConfigInfo2 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, 50, "datadisk2", 1);
            AddAzureDataDiskConfig[] dataDiskConfig = { azureDataDiskConfigInfo1, azureDataDiskConfigInfo2 };
            vmPowershellCmdlets.AddVMDataDisks(newAzureQuickVMName, serviceName, dataDiskConfig);

            SetAzureDataDiskConfig setAzureDataDiskConfig1 = new SetAzureDataDiskConfig(HostCaching.ReadOnly, 0);
            SetAzureDataDiskConfig setAzureDataDiskConfig2 = new SetAzureDataDiskConfig(HostCaching.ReadOnly, 0);
            SetAzureDataDiskConfig[] diskConfig = { setAzureDataDiskConfig1, setAzureDataDiskConfig2 };
            vmPowershellCmdlets.SetVMDataDisks(newAzureQuickVMName, serviceName, diskConfig);

            SetAzureVMSizeConfig vmSizeConfig = new SetAzureVMSizeConfig(InstanceSize.Medium);
            vmPowershellCmdlets.SetVMSize(newAzureQuickVMName, serviceName, vmSizeConfig);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);
                        
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));
            cleanup = true;
            pass = true;
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (Get-AzureDisk,Remove-AzureVM,Remove-AzureDisk,Get-AzureVMImage)")]
        public void ManagingDiskImages()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Create a unique VM name and Service Name
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");           

            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName); // New-AzureQuickVM
            Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, serviceName);

            // starting the test.
            Collection<DiskContext> vmDisks = vmPowershellCmdlets.GetAzureDiskAttachedToRoleName(new[] { newAzureQuickVMName });  // Get-AzureDisk | Where {$_.AttachedTo.RoleName -eq $vmname }

            foreach (var disk in vmDisks)
                Console.WriteLine("The disk, {0}, is created", disk.DiskName);

            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);  // Remove-AzureVM
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));
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
                    Console.WriteLine("Disk is not removed: {0}", disk.DiskName);
                    pass = false;
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

            cleanup = true;
            pass = true;           
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM,Save-AzureVMImage)")]
        public void CaptureImagingExportingImportingVMConfig()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Create a unique VM name
            string newAzureVMName = Utilities.GetUniqueShortName("PSTestVM");
            Console.WriteLine("VM Name: {0}", newAzureVMName);

            // Create a unique Service Name            
            vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
            Console.WriteLine("Service Name: {0}", serviceName);

            // starting the test.

            AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(newAzureVMName, VMSizeInfo.Small, imageName); // parameters for New-AzureVMConfig (-Name -InstanceSize -ImageName)            
            AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password); // parameters for Add-AzureProvisioningConfig (-Windows -Password)            
            PersistentVMConfigInfo persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
            PersistentVM persistentVM = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo); // New-AzureVMConfig & Add-AzureProvisioningConfig

            PersistentVM[] VMs = { persistentVM };
            vmPowershellCmdlets.NewAzureVM(serviceName, VMs); // New-AzureVM
            Console.WriteLine("The VM is successfully created: {0}", persistentVM.RoleName);
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(persistentVM.RoleName, serviceName);
            Assert.AreEqual(vmRoleCtxt.Name, persistentVM.RoleName, true);


            vmPowershellCmdlets.StopAzureVM(newAzureVMName, serviceName); // Stop-AzureVM
            for (int i = 0; i < 3; i++)
            {
                vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(persistentVM.RoleName, serviceName);
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
            vmPowershellCmdlets.RemoveAzureVM(persistentVM.RoleName, serviceName);            
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(persistentVM.RoleName, serviceName));
            cleanup = true;
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (Export-AzureVM,Remove-AzureVM,Import-AzureVM,New-AzureVM)")]
        public void ExportingImportingVMConfigAsTemplateforRepeatableUsage()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
           
            // Create a new Azure quick VM
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");            
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName); // New-AzureQuickVM
            Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, serviceName);

            // starting the test.
            string path = ".\\mytestvmconfig1.xml";
            PersistentVMRoleContext vmRole = vmPowershellCmdlets.ExportAzureVM(newAzureQuickVMName, serviceName, path); // Export-AzureVM
            Console.WriteLine("Exporting VM is successfully done: path - {0}  Name - {1}", path, vmRole.Name);

            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName); // Remove-AzureVM
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));
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
                    vmPowershellCmdlets.NewAzureVM(serviceName, VMs.ToArray()); // New-AzureVM
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
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName);
            Assert.AreEqual(newAzureQuickVMName, vmRoleCtxt.Name, true);

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));

            cleanup = true;
            pass = true;
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (Get-AzureVM,Get-AzureEndpoint,Get-AzureRemoteDesktopFile)")]
        public void ManagingRDPSSHConnectivity()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            // Create a new Azure quick VM
            string newAzureQuickVMName = Utilities.GetUniqueShortName("PSTestVM");
            vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, newAzureQuickVMName, serviceName, imageName, username, password, locationName); // New-AzureQuickVM
            Console.WriteLine("VM is created successfully: -Name {0} -ServiceName {1}", newAzureQuickVMName, serviceName);

            // starting the test.
            PersistentVMRoleContext vmRoleCtxt = vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName); // Get-AzureVM
            InputEndpointContext inputEndpointCtxt = vmPowershellCmdlets.GetAzureEndPoint(vmRoleCtxt)[0]; // Get-AzureEndpoint
            Console.WriteLine("InputEndpointContext Name: {0}", inputEndpointCtxt.Name);
            Console.WriteLine("InputEndpointContext port: {0}", inputEndpointCtxt.Port);
            Console.WriteLine("InputEndpointContext protocol: {0}", inputEndpointCtxt.Protocol);
            Assert.AreEqual(inputEndpointCtxt.Name, "RemoteDesktop", true);

            string path = ".\\myvmconnection.rdp";
            vmPowershellCmdlets.GetAzureRemoteDesktopFile(newAzureQuickVMName, serviceName, path, false); // Get-AzureRemoteDesktopFile
            Console.WriteLine("RDP file is successfully created at: {0}", path);

            // ToDo: Automate RDP.
            //vmPowershellCmdlets.GetAzureRemoteDesktopFile(newAzureQuickVMName, newAzureQuickVMSvcName, path, true); // Get-AzureRemoteDesktopFile -Launch

            Console.WriteLine("Test passed");

            // Cleanup
            vmPowershellCmdlets.RemoveAzureVM(newAzureQuickVMName, serviceName);
            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureVM(newAzureQuickVMName, serviceName));

            cleanup = true;
            pass = true;

            //TODO: Need to do proper cleanup of the service
            //            vmPowershellCmdlets.RemoveAzureService(newAzureQuickVMSvcName);
            //            Assert.AreEqual(null, vmPowershellCmdlets.GetAzureService(newAzureQuickVMSvcName));


        }

        // Basic Provisioning a Virtual Machine
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New,Get,Set,Remove,Move)-AzureDeployment)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", ".\\packageScenario.csv", "packageScenario#csv", DataAccessMethod.Sequential)]        
        public void DeploymentUpgrade()        
        {

            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            cleanup = true;
            perfFile = @".\deployment2.csv";


            // Choose the package and config files from local machine
            string path = Convert.ToString(TestContext.DataRow["path"]);
            string packageName = Convert.ToString(TestContext.DataRow["packageName"]);
            string configName = Convert.ToString(TestContext.DataRow["configName"]);
            string upgradePackageName = Convert.ToString(TestContext.DataRow["upgradePackage"]);
            string upgradeConfigName = Convert.ToString(TestContext.DataRow["upgradeConfig"]);
            string upgradeConfigName2 = Convert.ToString(TestContext.DataRow["upgradeConfig2"]);



            var packagePath1 = new FileInfo(@path + packageName);            
            var packagePath2 = new FileInfo(@path + upgradePackageName);
            var configPath1 = new FileInfo(@path + configName); // config with 1 instances
            var configPath2 = new FileInfo(@path + upgradeConfigName); // config with 2 instances
            var configPath3 = new FileInfo(@path + upgradeConfigName2); // config with 4 instances


            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);


            string deploymentName = "deployment1";
            string deploymentLabel = "label1";
            DeploymentInfoContext result;


            try
            {
                
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
                Console.WriteLine("service, {0}, is created.", serviceName);

                // New deployment to Production
                DateTime start = DateTime.Now;
                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false);

                TimeSpan duration = DateTime.Now - start;

                Uri site = Utilities.GetDeploymentAndWaitForReady(serviceName, DeploymentSlotType.Production, 1, 600);

                System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("Deployment, {0}, {1}", duration, DateTime.Now - start) });


                Console.WriteLine("site: {0}", site.ToString());
                Console.WriteLine("Time for all instances to become in ready state: {0}", DateTime.Now - start);


                // Auto-Upgrade the deployment
                //start = DateTime.Now;
                //vmPowershellCmdlets.SetAzureDeploymentUpgrade(serviceName, DeploymentSlotType.Production, UpgradeType.Auto, packagePath1.FullName, configPath1.FullName);
                //duration = DateTime.Now - start;

                //System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("Auto Upgrade, {0}", duration) });

                //Console.WriteLine("Auto upgrade took {0}.", duration);

                //result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                //Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, serviceName, DeploymentSlotType.Production, null, 8);
                //Console.WriteLine("successfully updated the deployment");

                // DISABLED: Upgrade the deployment simultaneously
                //start = DateTime.Now;
                //vmPowershellCmdlets.SetAzureDeploymentUpgrade(serviceName, DeploymentSlotType.Production, UpgradeType.Simultaneous, packagePath1.FullName, configPath1.FullName);
                //TimeSpan duration2 = DateTime.Now - start;                

                //Console.WriteLine("Simultaneous Upgrade took {0}.", duration2);
                //Assert.IsTrue(duration2 < duration, "Simultaneous upgrade took more time!!");


                //Utilities.GetDeploymentAndWaitForReady(serviceName, DeploymentSlotType.Production, 1, 600);
                //System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("Simultaneous Upgrade, {0}, {1}", duration2, DateTime.Now - start) });

                //Console.WriteLine("site: {0}", site.ToString());
                //Console.WriteLine("Time for all instances to become in ready state: {0}", DateTime.Now - start);
  

                //result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                //Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, serviceName, DeploymentSlotType.Production, null, 8);
                //Console.WriteLine("successfully updated the deployment");


                // Manual-Upgrade the deployment
                start = DateTime.Now;
                vmPowershellCmdlets.SetAzureDeploymentUpgrade(serviceName, DeploymentSlotType.Production, UpgradeType.Manual, packagePath1.FullName, configPath1.FullName);
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 0);                
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 1);
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 2);
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 3);
                vmPowershellCmdlets.SetAzureWalkUpgradeDomain(serviceName, DeploymentSlotType.Production, 4);

                duration = DateTime.Now - start;
                Console.WriteLine("Manual upgrade took {0}.", duration);

                result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, serviceName, DeploymentSlotType.Production, null, 8);
                Console.WriteLine("successfully updated the deployment");

                site = Utilities.GetDeploymentAndWaitForReady(serviceName, DeploymentSlotType.Production, 1, 600);

                System.IO.File.AppendAllLines(perfFile, new string[] { String.Format("Manual Upgrade, {0}, {1}", duration, DateTime.Now - start) });



                //Console.WriteLine(GetSiteContent(site, 3, false));



               


                //Utilities.PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Staging, null, 1);
                //Console.WriteLine("successfully deployed the package");

                




              
                //// Update the deployment
                //vmPowershellCmdlets.SetAzureDeploymentConfig(serviceName, DeploymentSlotType.Production, configPath2.FullName);
                //result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                ////PrintAndCompareDeployment(result, serviceName, deploymentName, deploymentLabel, DeploymentSlotType.Production, null, 2);
                //Console.WriteLine("successfully updated the deployment");


                
                //// Upgrade the deployment simultaneously
                //start = DateTime.Now;
                //vmPowershellCmdlets.SetAzureDeploymentUpgrade(serviceName, DeploymentSlotType.Production, UpgradeType.Simultaneous, packagePath2.FullName, configPath2.FullName);
                //TimeSpan duration2 = DateTime.Now - start;
                //Console.WriteLine("Simultaneous Upgrade took {0}.", duration2);
                //Assert.IsTrue(duration2 < duration, "Simultaneous upgrade took more time!!");

                //result = vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                ////PrintAndCompareDeployment(result, serviceName, deploymentName, serviceName, DeploymentSlotType.Production, null, 2);
                //Console.WriteLine("successfully updated the deployment");





                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                pass &= Utilities.CheckRemove(vmPowershellCmdlets.GetAzureDeployment, serviceName);
                //try
                //{
                //    vmPowershellCmdlets.GetAzureDeployment(serviceName, DeploymentSlotType.Production);
                //    Console.WriteLine("the deployment is not removed!");
                //    pass = false;
                //}
                //catch (Exception e1)
                //{
                //    if (e1.ToString().Contains("ResourceNotFound"))
                //    {
                //        Console.WriteLine("Successfully removed the deployment");
                //    }
                //    else
                //    {
                //        Assert.Fail("Exception occurred: {0}", e1.ToString());
                //    }
                //}

                //pass &= true;
                cleanup = true;
                pass = true;

            }
            catch (Exception e)
            {
                pass = false;
                cleanup = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }


       



        [TestCleanup]
        public virtual void CleanUp()
        {

            Console.WriteLine("Test {0}", pass ? "passed" : "failed");            

            // Remove the service
            if (cleanup)
            {
                vmPowershellCmdlets.RemoveAzureService(serviceName);
                try
                {                    
                    vmPowershellCmdlets.GetAzureService(serviceName);
                    Console.WriteLine("The service, {0}, is not removed", serviceName);
                }
                catch (Exception e)
                {
                    if (e.ToString().ToLowerInvariant().Contains("does not exist"))
                    {
                        Console.WriteLine("The service, {0}, is successfully removed", serviceName);
                    }
                    else
                    {
                        Console.WriteLine("Error occurred: {0}", e.ToString());
                    }
                }
            }
        }

        private string GetSiteContent(Uri uri, int maxRetryTimes, bool holdConnection)
        {
            Console.WriteLine("GetSiteContent. uri={0} maxRetryTimes={1}", uri.AbsoluteUri, maxRetryTimes);
            
            HttpWebRequest request;
            HttpWebResponse response = null;

            var noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            HttpWebRequest.DefaultCachePolicy = noCachePolicy;

            int i;
            for (i = 1; i <= maxRetryTimes; i++)
            {
                try
                {
                    request = (HttpWebRequest)WebRequest.Create(uri);                    
                    request.Timeout = 10 * 60 * 1000; //set to 10 minutes, default 100 sec. default IE7/8 is 60 minutes
                    response = (HttpWebResponse)request.GetResponse();
                    break;
                }
                catch (WebException e)
                {
                    Console.WriteLine("Exception Message: " + e.Message);
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        Console.WriteLine("Status Code: {0}", ((HttpWebResponse)e.Response).StatusCode);
                        Console.WriteLine("Status Description: {0}", ((HttpWebResponse)e.Response).StatusDescription);
                    }
                }

                Thread.Sleep(30 * 1000);
            }

            if (i > maxRetryTimes)
            {
                throw new Exception("Web Site has error and reached maxRetryTimes");
            }

            Stream responseStream = response.GetResponseStream();
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[100];
            int length;
            while ((length = responseStream.Read(buf, 0, 100)) != 0)
            {
                if (holdConnection)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
                sb.Append(Encoding.UTF8.GetString(buf, 0, length));
            }

            string responseString = sb.ToString();
            Console.WriteLine("Site content: (IsFromCache={0})", response.IsFromCache);
            Console.WriteLine(responseString);

            return responseString;
        }
    }
}

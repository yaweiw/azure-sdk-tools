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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests
{
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class VMTemplateTests : ServiceManagementTest
    {
        string serviceName;
        string diskLabel1 = "disk1";
        int diskSize1 = 30;
        int lunSlot1 = 0;
        const string CONSTANT_SPECIALIZED = "Specialized";
        const string CONSTANT_GENERALIZED = "Generalized";
        const string CONSTANT_CATEGORY = "User";
        HostCaching cahcing = HostCaching.ReadWrite;
        string vmImageName;
        bool skipCleanup;

        [ClassInitialize]
        public static void ClassIntialize(TestContext context)
        {
            
        }

        [TestInitialize]
        public void TestIntialize()
        {
            pass = false;
            skipCleanup = false;
            serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
        }

        #region TestCases
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(0), Owner("hylee"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,New-AzureVM,New-AzureVMConfig,Add-AzureDataDisk,Stop-AzureVM,Save-AzureVMImage,Get-AzureVM,Get-AzureVMImage,i.	Remove-AzureVMImage)")]
        public  void CaptureSpecializedVMAndDeploy()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string serviceName1 = Utilities.GetUniqueShortName(serviceNamePrefix);
            try
            {
                //      a.	Deploy a new IaaS VM
                string vmName = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("--------------------------------Deploying a new IaaS VM :{0}--------------------------------",vmName);
                var vm =  CreateIaaSVMObjectWithDisk(vmName, InstanceSize.Small, imageName, true, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("--------------------------------Deploying a new IaaS VM :{0} completed.---------------------", vmName);
                //b.	Stop the VM
                Console.WriteLine("--------------------------------Stopping vm :{0}--------------------------------", vmName);
                vmPowershellCmdlets.StopAzureVM(vmName, serviceName,force: true);
                Console.WriteLine("--------------------------------Stopped vm :{0}--------------------------------", vmName);
                //c.	Save the VM image
                Console.WriteLine("--------------------------------Save the VM image--------------------------------");
                vmImageName = vmName + "Image";
                vmPowershellCmdlets.SaveAzureVMImage(serviceName, vmName, vmImageName,  CONSTANT_SPECIALIZED,vmImageName);
                Console.WriteLine("--------------------------------Saved VM image with name {0}----------------------");
                //d.	Verify the VM image by Get-AzureVMImage
                Console.WriteLine("--------------------------------Verify the VM image--------------------------------");
                VerifyVMImage(vmImageName, OS.Windows, vmImageName, CONSTANT_SPECIALIZED, cahcing, lunSlot1, diskSize1,1);
                Console.WriteLine("--------------------------------Verified that the VM image is saved successfully--------------------------------");
                //e.	Deploy a new IaaS VM with the save VM image
                Console.WriteLine("--------------------------------Deploy a new IaaS VM with the saved VM image {0}--------------------------------",vmImageName);
                string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);
                vm = Utilities.CreateIaaSVMObject(vmName1, InstanceSize.Small, vmImageName);
                vmPowershellCmdlets.NewAzureVM(serviceName1, new[] { vm }, locationName);
                Console.WriteLine("--------------------------------Deployed a IaaS VM {0} with the saved VM image {1}--------------------------------", vmName1,vmImageName);
                //f.	Verify the VM by Get-AzureVM
                Console.WriteLine("--------------------------------Verify the VM by Get-AzureVM--------------------------------", vmName1, vmImageName);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName1, serviceName1);
                Utilities.PrintContext(vmRoleContext);
                VerifyVM(vmRoleContext.VM, OS.Windows, HostCaching.ReadWrite, diskSize1, 1);
                Console.WriteLine("--------------------------------Verified the VM {0} successfully--------------------------------", vmName1);
                //g.	Add another IaaS VM with the save VM image to the existing service
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("--------------------------------Deploy a new IaaS VM with the saved VM image {0}--------------------------------", vmImageName);
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows, vmName2, serviceName1, vmImageName);
                Console.WriteLine("--------------------------------Deployed a IaaS VM {0} with the saved VM image {1}--------------------------------", vmName2, vmImageName);
                //h.	Verify the VM by Get-AzureVM
                Console.WriteLine("--------------------------------Verify the VM by Get-AzureVM--------------------------------", vmName2, vmImageName);
                vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName2, serviceName1);
                VerifyVM(vmRoleContext.VM, OS.Windows, HostCaching.ReadWrite, diskSize1, 1);
                Utilities.PrintContext(vmRoleContext);
                Console.WriteLine("--------------------------------Verified the VM {0} successfully--------------------------------", vmName2);
                
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                CleanupService(serviceName1);
                //	Delete the VM image
                Console.WriteLine("------------------------------Delete the VM image---------------------------------");
                DeleteVMImageIfExists(vmImageName);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
            }
        }

        [TestMethod(), TestCategory("Scenario"),  TestProperty("Feature", "IaaS"), Priority(0), Owner("hylee"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,New-AzureVM,New-AzureVMConfig,Add-AzureDataDisk,Stop-AzureVM,Save-AzureVMImage,Get-AzureVM,Get-AzureVMImage,i.	Remove-AzureVMImage)")]
        public void CaptureGeneralizedVMAndDeploy()
        {
            string serviceName1 = Utilities.GetUniqueShortName(serviceNamePrefix);
            try
            {
                //        a.	Deploy a new IaaS VM
                string vmName = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("--------------------------------Deploying a new IaaS VM :{0}--------------------------------", vmName);
                var vm = CreateIaaSVMObjectWithDisk(vmName, InstanceSize.Small, imageName, true, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("--------------------------------Deploying a new IaaS VM :{0} completed.---------------------", vmName);
                //b.	RDP to the VM and sysprep

                //c.	Stop the VM
                Console.WriteLine("--------------------------------Stopping vm :{0}--------------------------------", vmName);
                vmPowershellCmdlets.StopAzureVM(vmName, serviceName, force: true);
                Console.WriteLine("--------------------------------Stopped vm :{0}--------------------------------", vmName);
                //d.	Save the VM image
                Console.WriteLine("--------------------------------Save the VM image as Generalized image --------------------------------");
                vmImageName = vmName + "Image";
                vmPowershellCmdlets.SaveAzureVMImage(serviceName, vmName, vmImageName, CONSTANT_GENERALIZED, vmImageName);
                Console.WriteLine("--------------------------------Saved VM image with name {0}----------------------");
                //e.	Verify the VM image by Get-AzureVMImage
                Console.WriteLine("--------------------------------Verify the VM image--------------------------------");
                VerifyVMImage(vmImageName, OS.Windows, vmImageName, CONSTANT_GENERALIZED, cahcing, lunSlot1, diskSize1, 1);
                Console.WriteLine("--------------------------------Verified that the VM image is saved successfully--------------------------------");
                //f.	Deploy a new IaaS VM with the save VM image
                Console.WriteLine("--------------------------------Deploy a new IaaS VM with the saved VM image {0}--------------------------------", vmImageName);
                string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);
                vmPowershellCmdlets.NewAzureQuickVM(OS.Windows,vmName1, serviceName1, vmImageName, username, password, locationName);
                Console.WriteLine("--------------------------------Deployed a IaaS VM {0} with the saved VM image {1}--------------------------------", vmName1, vmImageName);
                //g.	Verify the VM by Get-AzureVM
                Console.WriteLine("--------------------------------Verify the VM by Get-AzureVM--------------------------------", vmName1, vmImageName);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName1, serviceName1);
                Utilities.PrintContext(vmRoleContext);
                VerifyVM(vmRoleContext.VM, OS.Windows, HostCaching.ReadWrite, diskSize1, 1);
                Console.WriteLine("--------------------------------Verified the VM {0} successfully--------------------------------", vmName1);
                //h.	Add another IaaS VM with the save VM image to the existing service
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                vm = Utilities.CreateIaaSVMObject(vmName2, InstanceSize.Small, vmImageName, true, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName1, new[] { vm });
                //i.	Verify the VM by Get-AzureVM
                Console.WriteLine("--------------------------------Verify the VM by Get-AzureVM--------------------------------", vmName2, vmImageName);
                vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName2, serviceName1);
                Utilities.PrintContext(vmRoleContext);
                VerifyVM(vmRoleContext.VM, OS.Windows, HostCaching.ReadWrite, diskSize1, 1);
                Console.WriteLine("--------------------------------Verified the VM {0} successfully--------------------------------", vmName2);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                CleanupService(serviceName1);
                //Delete the VM image
                Console.WriteLine("------------------------------Delete the VM image---------------------------------");
                vmPowershellCmdlets.RemoveAzureVMImage(vmImageName, true);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
            }
        }

        [TestMethod(), TestCategory("Scenario"),  TestProperty("Feature", "IaaS"), Priority(0), Owner("hylee"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,New-AzureVM,New-AzureVMConfig,Add-AzureDataDisk,Stop-AzureVM,Save-AzureVMImage,Get-AzureVM,Get-AzureVMImage,i.	Remove-AzureVMImage)")]
        public void CaptureSpecializedLinuxVMAndDeploy()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string serviceName1 = Utilities.GetUniqueShortName(serviceNamePrefix);
            string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);
            try
            {
                //                a.	Deploy a new IaaS VM
                string vmName = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("--------------------------------Deploying a new IaaS VM :{0}--------------------------------", vmName);
                var vm = CreateIaaSVMObjectWithDisk(vmName, InstanceSize.Small, linuxImageName, false, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("--------------------------------Deploying a new IaaS VM :{0} completed.---------------------", vmName);
                //b.	Stop the VM
                Console.WriteLine("--------------------------------Stopping vm :{0}--------------------------------", vmName);
                vmPowershellCmdlets.StopAzureVM(vmName, serviceName, force: true);
                Console.WriteLine("--------------------------------Stopped vm :{0}--------------------------------", vmName);
                //c.	Save the VM image
                Console.WriteLine("--------------------------------Save the VM image--------------------------------");
                vmImageName = vmName + "Image";
                vmPowershellCmdlets.SaveAzureVMImage(serviceName, vmName, vmImageName, CONSTANT_SPECIALIZED, vmImageName);
                Console.WriteLine("--------------------------------Saved VM image with name {0}----------------------");
                //d.	Verify the VM image by Get-AzureVMImage
                Console.WriteLine("--------------------------------Verify the VM image--------------------------------");
                VerifyVMImage(vmImageName, OS.Linux, vmImageName, CONSTANT_SPECIALIZED, cahcing, lunSlot1, diskSize1,1);
                Console.WriteLine("--------------------------------Verified that the VM image is saved successfully--------------------------------");
                //e.	Deploy a new IaaS VM with the save VM image
                Console.WriteLine("--------------------------------Deploy a new IaaS VM with the saved VM image {0}--------------------------------", vmImageName);
                string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);
                vmPowershellCmdlets.NewAzureQuickVM(OS.Linux, vmName1, serviceName1, vmImageName,null, null, locationName);
                Console.WriteLine("--------------------------------Deployed a IaaS VM {0} with the saved VM image {1}--------------------------------", vmName1, vmImageName);
                //f.	Verify the VM by Get-AzureVM
                Console.WriteLine("--------------------------------Verify the VM by Get-AzureVM--------------------------------", vmName1, vmImageName);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName1, serviceName1);
                Utilities.PrintContext(vmRoleContext);
                VerifyVM(vmRoleContext.VM, OS.Linux, HostCaching.ReadWrite, diskSize1, 1);
                Console.WriteLine("--------------------------------Verified the VM {0} successfully--------------------------------", vmName1);
                //g.	Add another IaaS VM with the save VM image to the existing service
                Console.WriteLine("--------------------------------Deploy a new IaaS VM with the saved VM image {0}--------------------------------", vmImageName);
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                vm = Utilities.CreateIaaSVMObject(vmName2, InstanceSize.Small, vmImageName);
                vmPowershellCmdlets.NewAzureVM(serviceName1, new[] { vm });
                Console.WriteLine("--------------------------------Deployed a IaaS VM {0} with the saved VM image {1}--------------------------------", vmName2, vmImageName);
                //h.	Verify the VM by Get-AzureVM
                Console.WriteLine("--------------------------------Verify the VM by Get-AzureVM--------------------------------", vmName2, vmImageName);
                vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName2, serviceName1);
                VerifyVM(vmRoleContext.VM, OS.Linux, HostCaching.ReadWrite, diskSize1, 1);
                Utilities.PrintContext(vmRoleContext);
                Console.WriteLine("--------------------------------Verified the VM {0} successfully--------------------------------", vmName2);
                
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                CleanupService(serviceName1);
                //Delete the VM image
                Console.WriteLine("------------------------------Delete the VM image---------------------------------");
                vmPowershellCmdlets.RemoveAzureVMImage(vmImageName, true);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
            }
        }

        [TestMethod(), TestCategory("Scenario"),  TestProperty("Feature", "IaaS"), Priority(0), Owner("hylee"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,New-AzureVM,New-AzureVMConfig,Add-AzureDataDisk,Stop-AzureVM,Save-AzureVMImage,Get-AzureVM,Get-AzureVMImage,i.	Remove-AzureVMImage)")]
        public void CaptureGeneralizedLinuxVMAndDeploy()
        {
            string serviceName1 = Utilities.GetUniqueShortName(serviceNamePrefix);
            string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);
            try
            {
                //                a.	Deploy a new IaaS VM
                string vmName = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("--------------------------------Deploying a new IaaS VM :{0} completed.---------------------", vmName);
                PersistentVM vm = CreateIaaSVMObjectWithDisk(vmName, InstanceSize.Small, linuxImageName, false, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("--------------------------------Deploying a new IaaS VM :{0} completed.---------------------", vmName);
                //b.	Stop the VM
                Console.WriteLine("--------------------------------Stopping vm :{0}--------------------------------", vmName);
                vmPowershellCmdlets.StopAzureVM(vmName, serviceName, force: true);
                Console.WriteLine("--------------------------------Stopped vm :{0}--------------------------------", vmName);
                //c.	Save the VM image
                Console.WriteLine("--------------------------------Save the VM image--------------------------------");
                vmImageName = vmName + "Image";
                vmPowershellCmdlets.SaveAzureVMImage(serviceName, vmName, vmImageName, CONSTANT_GENERALIZED, vmImageName);
                Console.WriteLine("--------------------------------Saved VM image with name {0}----------------------");
                //d.	Verify the VM image by Get-AzureVMImage
                Console.WriteLine("--------------------------------Verify the VM image--------------------------------");
                VerifyVMImage(vmImageName, OS.Linux, vmImageName, CONSTANT_GENERALIZED, cahcing, lunSlot1, diskSize1, 1);
                Console.WriteLine("--------------------------------Verified that the VM image is saved successfully--------------------------------");
                //e.	Deploy a new IaaS VM with the save VM image
                Console.WriteLine("--------------------------------Deploy a new IaaS VM with the saved VM image {0}--------------------------------", vmImageName);
                string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);
                vm = Utilities.CreateIaaSVMObject(vmName1, InstanceSize.Small, vmImageName,false,username,password);
                vmPowershellCmdlets.NewAzureVM(serviceName1, new[] { vm }, locationName);
                Console.WriteLine("--------------------------------Deployed a IaaS VM {0} with the saved VM image {1}--------------------------------", vmName1, vmImageName);
                //f.	Verify the VM by Get-AzureVM
                Console.WriteLine("--------------------------------Verify the VM by Get-AzureVM--------------------------------", vmName1, vmImageName);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName1, serviceName1);
                Utilities.PrintContext(vmRoleContext);
                Console.WriteLine("--------------------------------Verified the VM {0} successfully--------------------------------", vmName1);
                //g.	Add another IaaS VM with the save VM image to the existing service
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("--------------------------------Deploy a new IaaS VM with the saved VM image {0}--------------------------------", vmImageName);
                vmPowershellCmdlets.NewAzureQuickVM(OS.Linux, vmName2, serviceName1, vmImageName,username,password);
                Console.WriteLine("--------------------------------Deployed a IaaS VM {0} with the saved VM image {1}--------------------------------", vmName2, vmImageName);
                //h.	Verify the VM by Get-AzureVM
                Console.WriteLine("--------------------------------Verify the VM by Get-AzureVM--------------------------------", vmName2, vmImageName);
                vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName2, serviceName1);
                Utilities.PrintContext(vmRoleContext);
                Console.WriteLine("--------------------------------Verified the VM {0} successfully--------------------------------", vmName2);

                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                CleanupService(serviceName1);
                //Delete the VM image
                Console.WriteLine("------------------------------Delete the VM image---------------------------------");
                vmPowershellCmdlets.RemoveAzureVMImage(vmImageName, true);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IaaS"), Priority(0), Owner("hylee"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,New-AzureVM,New-AzureVMConfig,Add-AzureDataDisk,Stop-AzureVM,Save-AzureVMImage,Get-AzureVM,Get-AzureVMImage,i.	Remove-AzureVMImage)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\overwrite_VHD.csv", "overwrite_VHD#csv", DataAccessMethod.Sequential)]
        public void AzureVMImageListRemoveTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            string newImageName = Utilities.GetUniqueShortName("vmimage");
            string oldLabel = "old label";
            string newLabel = Utilities.GetUniqueShortName("vmimage");
            string vmName = Utilities.GetUniqueShortName(vmNamePrefix);
            string serviceName1 = Utilities.GetUniqueShortName("Pstestsvc"); 

            try
            {
                string mediaLocation = UploadVhdFile();
                Console.WriteLine("------------------------------Add an OS image---------------------------------");
                //      a.	Add an OS image
                var result = vmPowershellCmdlets.AddAzureVMImage(newImageName, mediaLocation, OS.Windows, newImageName);
                Console.WriteLine("------------------------------Add an OS image: Completed---------------------------------");
                //b.	Deploy a new IaaS VM
                Console.WriteLine("------------------------------Deploy a new IaaS VM---------------------------------");
                var vm = CreateIaaSVMObjectWithDisk(vmName, InstanceSize.Small, newImageName, true, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("------------------------------Deploy a new IaaS VM: Completed---------------------------------");
                //c.	Stop the VM
                Console.WriteLine("------------------------------Stop the VM---------------------------------");
                vmPowershellCmdlets.StopAzureVM(vm, serviceName, true);
                Console.WriteLine("------------------------------Stop the VM: Completed---------------------------------");
                //d.	Try to save the OS image with an existing os image name. (should fail)
                Console.WriteLine("------------------------------Try to save the OS image with an existing os image name---------------------------------");
                Utilities.VerifyFailure(() => vmPowershellCmdlets.SaveAzureVMImage(serviceName, vmName, oldLabel, CONSTANT_SPECIALIZED, oldLabel), BadRequestException);
                Console.WriteLine("------------------------------Try to save the OS image with an existing os image name: Completed---------------------------------");
                //e.	Save the OS image with a new image name.
                Console.WriteLine("------------------------------Save the OS image with a new image name.---------------------------------");
                vmPowershellCmdlets.SaveAzureVMImage(serviceName, vmName, newLabel);
                Console.WriteLine("------------------------------Save the OS image with a new image name: Completed---------------------------------");
                //f.	Deploy a new IaaS VM
                Console.WriteLine("------------------------------Deploy a new IaaS VM---------------------------------");
                string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);
                vm = CreateIaaSVMObjectWithDisk(vmName1, InstanceSize.Small, newLabel, true, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName1, new[] { vm }, locationName);
                var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName1, serviceName1);
                Console.WriteLine("------------------------------Deploy a new IaaS VM: Completed---------------------------------");

                //g.	Stop the VM
                Console.WriteLine("------------------------------Stop the VM---------------------------------");
                vmPowershellCmdlets.StopAzureVM(vm, serviceName1, true);
                Console.WriteLine("------------------------------Stop the VM: Completed---------------------------------");
                //h.	Save the VM image with the existing os image name (should fail)
                Console.WriteLine("------------------------------Save the VM image with the existing os image name---------------------------------");
                vmImageName = vmName1 + "Image";
                Utilities.VerifyFailure(() => vmPowershellCmdlets.SaveAzureVMImage(serviceName1, vmName1, newLabel, CONSTANT_SPECIALIZED, vmImageName), "OSImage");
                Console.WriteLine("------------------------------Save the VM image with the existing os image name: Completed---------------------------------");
                //i.	List VM Images
                //i.	Get-AzureVMImage
                //VerifyVMImage(vmImageName, OS.Windows, vmImageName, CONSTANT_SPECIALIZED, cahcing, lunSlot1, diskSize1, 1);
                Console.WriteLine("------------------------------Get-AzureVMImage---------------------------------");
                VerifyOsImage(newLabel, new OSImageContext()
                {  
                    ImageName = newLabel,
                    Category = CONSTANT_CATEGORY,
                    Location = locationName,
                    Label = newLabel,
                    OSImageName = newLabel,
                    OS = OS.Windows.ToString()
               });
                Console.WriteLine("------------------------------Get-AzureVMImage: Completed---------------------------------");
                //j.	Try to remove a wrong vm
                Console.WriteLine("------------------------------Try to remove a wrong vm---------------------------------");
                Utilities.VerifyFailure(() => vmPowershellCmdlets.RemoveAzureVMImage(Utilities.GetUniqueShortName()),ResourceNotFoundException);
                Console.WriteLine("------------------------------Try to remove a wrong vm: Completed---------------------------------");
                pass = true;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                //k.	Remove the VM Images
                DeleteVMImageIfExists(newLabel);
                vmPowershellCmdlets.RemoveAzureService(serviceName1);
            }
        }

        [TestMethod(), TestCategory("Scenario"),  TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,New-AzureVM,New-AzureVMConfig,Add-AzureDataDisk,Stop-AzureVM,Save-AzureVMImage,Get-AzureVM,Get-AzureVMImage,i.	Remove-AzureVMImage)")]
        public void GetAzureVMImageNegativeTest()
        {
            try
            {
                skipCleanup = true;
                //  Try to get a wrong vm image.
                Utilities.VerifyFailure(() => vmPowershellCmdlets.GetAzureVMImage(Utilities.GetUniqueShortName(vmNamePrefix)), ResourceNotFoundException);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        [TestMethod(),Ignore(), TestCategory("Scenario"),  TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,New-AzureVM,New-AzureVMConfig,Add-AzureDataDisk,Stop-AzureVM,Save-AzureVMImage,Get-AzureVM,Get-AzureVMImage,i.	Remove-AzureVMImage)")]
        public void RemoveAzureVMImageNegativeTest()
        {
            try
            {
                //      a.	Try to remove a wrong vm image
                //i.	Remove-AzureVMImage –VMImageName $wrongimgname
                //ii.	Remove-AzureVMImage –OSImageName $wrongimgname

                //Not Applicable yet as VMImageName,OSImageName parameters are not yet provided.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        
        [TestMethod(), TestCategory("Scenario"),TestProperty("Feature", "IaaS"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureQuickVM,Get-AzureVMImage,New-AzureVM,New-AzureVMConfig,Add-AzureDataDisk,Stop-AzureVM,Save-AzureVMImage,Get-AzureVM,Get-AzureVMImage,i.	Remove-AzureVMImage)")]
        public void SaveAzureVMImageNegativeTest()
        {
            try
            {
                string vmName = Utilities.GetUniqueShortName(vmNamePrefix);
                //      Deploy a new IaaS VM
                Console.WriteLine("------------------------------Deploy a new IaaS VM---------------------------------");
                var vm = CreateIaaSVMObjectWithDisk(vmName, InstanceSize.Small, imageName, true, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("------------------------------Deploy a new IaaS VM: completed---------------------------------");
                //b.	Stop the VM
                Console.WriteLine("------------------------------Stop the VM---------------------------------");
                vmPowershellCmdlets.StopAzureVM(vm, serviceName,force:true);
                Console.WriteLine("------------------------------Stop the VM: completed---------------------------------");
                //c.	Save the VM image
                Console.WriteLine("------------------------------Save the VM image---------------------------------");
                vmImageName = vmName + "Image";
                vmPowershellCmdlets.SaveAzureVMImage(serviceName, vmName, vmImageName,  CONSTANT_SPECIALIZED,vmImageName);
                Console.WriteLine("------------------------------Save the VM image: completed---------------------------------");
                //d.	Deploy another new IaaS VM
                Console.WriteLine("------------------------------Deploy another new IaaS VM---------------------------------");
                string vmName1 = Utilities.GetUniqueShortName(vmNamePrefix);
                vm = CreateIaaSVMObjectWithDisk(vmName1, InstanceSize.Small, imageName, true, username, password);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm });
                Console.WriteLine("------------------------------Deploy another new IaaS VM: completed---------------------------------");
                //e.	Stop the VM
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                vmPowershellCmdlets.StopAzureVM(vm, serviceName,force:true);
                string testImageName = Utilities.GetUniqueShortName(vmNamePrefix);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                //f.	Try to save the VM image with the existing name (must fail)
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                Utilities.VerifyFailure(() => vmPowershellCmdlets.SaveAzureVMImage(serviceName, vmName1, vmImageName, CONSTANT_SPECIALIZED, vmImageName),ConflictErrorException);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                //g.	Try to save the VM image with the wrong vm name (must fail)
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                Utilities.VerifyFailure(() => vmPowershellCmdlets.SaveAzureVMImage(serviceName, Utilities.GetUniqueShortName(vmNamePrefix), testImageName, CONSTANT_SPECIALIZED, testImageName), ResourceNotFoundException);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                //h.	Try to save the VM image with the wrong service name (must fail)
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                string testVMIMage = Utilities.GetUniqueShortName("VMImage");
                vmPowershellCmdlets.SaveAzureVMImage(Utilities.GetUniqueShortName(vmNamePrefix), vmName1, testVMIMage, CONSTANT_SPECIALIZED, testVMIMage);
                Utilities.VerifyFailure(() => vmPowershellCmdlets.GetAzureVMImage(testVMIMage),ResourceNotFoundException);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                //i.	Try to save the VM image with the label longer than maximum length of string (must fail)
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                string LongImageName = Utilities.GetUniqueShortName(length:30) + Utilities.GetUniqueShortName(length:30)+  Guid.NewGuid().ToString() + Guid.NewGuid().ToString() ;
                Console.WriteLine("Attempting to save a VMImage with name {0} of {1} characters and expecting it to fail.", LongImageName,LongImageName.Length);
                Utilities.VerifyFailure(() => vmPowershellCmdlets.SaveAzureVMImage(serviceName, vmName1, testImageName, CONSTANT_SPECIALIZED, LongImageName), BadRequestException);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                Console.WriteLine("------------------------------Delete the VM image---------------------------------");
                DeleteVMImageIfExists(vmImageName);
                Console.WriteLine("------------------------------Deleted the VM image---------------------------------");
            }
        }

        

        #endregion TestCases

        [TestCleanup]
        public void TestCleanUp()
        {
            if (!skipCleanup)
                CleanupService(serviceName);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }
        #region Helper Methods
        public void VerifyVMImage(string vmImageName, OS ImageFamily, string imageLabel, string osState, HostCaching hostCaching, int LUN, int LogicalDiskSizeInGB,int noOfDataDisks)
        {
            var vmImages = vmPowershellCmdlets.GetAzureVMImageReturningVMImages(vmImageName);
            Assert.IsTrue(vmImages.Count >= 1);
            var vmImageInfo = vmImages[0];
            Utilities.PrintContext(vmImageInfo);
            Utilities.PrintContext(vmImageInfo.OSDiskConfiguration);
            Utilities.PrintContext(vmImageInfo.DataDiskConfigurations[0]);
            //Verify ImageName
            Assert.IsTrue(vmImageName.Equals(vmImageInfo.ImageName));
            Assert.IsTrue(vmImageInfo.Label.Equals(imageLabel));
            //Verify Category
            Assert.IsTrue("User".Equals(vmImageInfo.Category,StringComparison.CurrentCultureIgnoreCase));
            //Verify LogicalDiskSizeInGB, HostCaching
            Assert.AreEqual(hostCaching.ToString(),vmImageInfo.OSDiskConfiguration.HostCaching,"Property HostCaching is not matching.");
            Assert.AreEqual(hostCaching.ToString(), vmImageInfo.DataDiskConfigurations[0].HostCaching,"Data disk HostCaching iproperty is not matching.");
            //Verify LogicalDiskSizeInGB,
            Assert.AreEqual(LogicalDiskSizeInGB, vmImageInfo.DataDiskConfigurations[0].LogicalDiskSizeInGB);
            //Verify OSstate
            Assert.AreEqual(osState, vmImageInfo.OSDiskConfiguration.OSState,"OsState is not matching.");
            //Verify OS
            Assert.AreEqual(ImageFamily.ToString(), vmImageInfo.OSDiskConfiguration.OS,"Os Family is not matching.");
            //Verify  LUN
            Assert.AreEqual(LUN, vmImageInfo.DataDiskConfigurations[0].Lun);
            //Verify the no of the data disks 
            Assert.AreEqual(noOfDataDisks, vmImageInfo.DataDiskConfigurations.Count);
            
        }

        public void VerifyOsImage(string ImageName, OSImageContext imageContext)
        {
            var vmImage = vmPowershellCmdlets.GetAzureVMImage(ImageName);
            Utilities.PrintContext(vmImage[0]);
            Assert.AreEqual(imageContext.ImageName, vmImage[0].ImageName, "ImageName property of the saved os image is not matching.");
            Assert.AreEqual(imageContext.Category, vmImage[0].Category, "Category property of the saved os image is not matching.");
            Assert.AreEqual(imageContext.Location, vmImage[0].Location, "Location property of the saved os image is not matching.");
            Assert.AreEqual(imageContext.Label, vmImage[0].Label, "Label property of the saved os image is not matching.");
            Assert.AreEqual(imageContext.OSImageName, vmImage[0].OSImageName, "OSImageName property of the saved os image is not matching.");
            Assert.AreEqual(imageContext.OS, vmImage[0].OS, "OS property of the saved os image is not matching.");
        }


        public PersistentVM CreateIaaSVMObjectWithDisk(string vmName, InstanceSize size, string imageName, bool isWindows, string username, string password)
        {
            PersistentVM vm = Utilities.CreateIaaSVMObject(vmName, InstanceSize.Small, imageName, isWindows, username, password);
            AddAzureDataDiskConfig azureDataDiskConfigInfo1 = new AddAzureDataDiskConfig(DiskCreateOption.CreateNew, diskSize1, diskLabel1, lunSlot1,cahcing.ToString());
            azureDataDiskConfigInfo1.Vm = vm;
            return vmPowershellCmdlets.AddAzureDataDisk(azureDataDiskConfigInfo1);
        }

        public string UploadVhdFile()
        {
            // Choose the vhd file from local machine
            var vhdName = Convert.ToString(TestContext.DataRow["vhdName"]);
            var vhdLocalPath = new FileInfo(Directory.GetCurrentDirectory() + "\\" + vhdName);
            Assert.IsTrue(File.Exists(vhdLocalPath.FullName), "VHD file not exist={0}", vhdLocalPath);

            // Get the pre-calculated MD5 hash of the fixed vhd that was converted from the original vhd.
            string md5hash = Convert.ToString(TestContext.DataRow["MD5hash"]);


            // Set the destination
            string vhdBlobName = string.Format("{0}/{1}.vhd", vhdContainerName, Utilities.GetUniqueShortName(Path.GetFileNameWithoutExtension(vhdName)));
            string vhdDestUri = blobUrlRoot + vhdBlobName;

            // Start uploading...
            Console.WriteLine("uploads {0} to {1}", vhdName, vhdBlobName);
            vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri);
            var vhdUploadContext = vmPowershellCmdlets.AddAzureVhd(vhdLocalPath, vhdDestUri, true);
            Console.WriteLine("uploading completed: {0}", vhdName);

            return vhdDestUri;
        }

        public void DeleteVMImageIfExists(string vmImageName)
        {
            try
            {
                var vmImages = vmPowershellCmdlets.GetAzureVMImage(vmImageName);
                if (vmImages.Count > 0)
                {
                    vmPowershellCmdlets.RemoveAzureVMImage(vmImageName, true);
                }
            }
            catch (Exception)
            {
                /*GetAzureVMImage throws image if it doesnt not find any vm images with the given name.
                 Since it is an expected behaviour we cathc the exception here.*/
            }
            
            
        }

        public void VerifyVM(PersistentVM vm, OS ImageFamily, HostCaching hostCaching, int LogicalDiskSizeInGB, int noOfDataDisks)
        {
            //Verify OS Disk
            Console.WriteLine("VM OS Virtual Hard Disk properties:");
            Utilities.PrintContext(vm.OSVirtualHardDisk);
            Assert.AreEqual(HostCaching.ReadWrite.ToString(), vm.OSVirtualHardDisk.HostCaching, "Os disk Property HostCaching is not matching.");
            Assert.AreEqual(ImageFamily.ToString(), vm.OSVirtualHardDisk.OS,"ImageFamily property is not matching.");
            //Verify Data Disk
            Console.WriteLine("VM Data Hard Disk properties:");
            Utilities.PrintContext(vm.DataVirtualHardDisks[0]);
            Assert.AreEqual(hostCaching.ToString(), vm.DataVirtualHardDisks[0].HostCaching, "Data disk Property HostCaching is not matching.");
            Assert.AreEqual(LogicalDiskSizeInGB, vm.DataVirtualHardDisks[0].LogicalDiskSizeInGB,"Data disk size is not matching.");
            Assert.AreEqual(noOfDataDisks, vm.DataVirtualHardDisks.Count, "Data disks count is not matching.");
        }

        #endregion Helper Methods

    }
}

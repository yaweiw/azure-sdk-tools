using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests
{
    [TestClass]
    public class NewAzureVmTests:ServiceManagementTest
    {
        private string serviceName;
        private string imageName;
        private string linuxImageName;
        string cerFileName = "testcert.cer";
        PSObject certToUpload;
        X509Certificate2 installedCert;


        [TestInitialize]
        public void Intialize()
        {
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
            linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);
            InstallCertificate();
        }

        public NewAzureVmTests()
        {
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "Iaas"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM)")]
        public void NewAzureVMWithLinuxAndNoSSHEnpoint()
        {
            try
            {
                // New-AzureVMConfig
                //
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                string newAzureLinuxVMName = Utilities.GetUniqueShortName("PSLinuxVM");
                AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(newAzureLinuxVMName, InstanceSize.Small, linuxImageName);
                PersistentVM vm = vmPowershellCmdlets.NewAzureVMConfig(azureVMConfigInfo);

                //
                // Add-AzureProvisioningConfig
                //
                AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password, true, false, null, null);
                azureProvisioningConfig.Vm = vm;
                vm = vmPowershellCmdlets.AddAzureProvisioningConfig(azureProvisioningConfig);

                //
                // New-AzureVM
                //
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);
                Collection<InputEndpointContext> endpoints = vmPowershellCmdlets.GetAzureEndPoint(vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, serviceName));
                Assert.AreEqual(endpoints.Count, 1);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                pass = false;
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "Iaas"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM)")]
        public void NewAzureVMWithLinuxAndDisableSSH()
        {
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                // New-AzureVMConfig
                //
                string newAzureLinuxVMName = Utilities.GetUniqueShortName("PSLinuxVM");
                string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);
                AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(newAzureLinuxVMName, InstanceSize.Small, linuxImageName);
                PersistentVM vm = vmPowershellCmdlets.NewAzureVMConfig(azureVMConfigInfo);

                //
                // Add-AzureProvisioningConfig
                //
                AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password, false, true, null, null);
                azureProvisioningConfig.Vm = vm;
                vm = vmPowershellCmdlets.AddAzureProvisioningConfig(azureProvisioningConfig);

                //
                // New-AzureVM
                //
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);
                Collection<InputEndpointContext> endpoints = vmPowershellCmdlets.GetAzureEndPoint(vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, serviceName));
                Assert.AreEqual(endpoints.Count, 1);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                pass = false;
            }

        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "Iaas"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM)")]
        public void NewAzureVMWithLinux()
        {
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                // New-AzureVMConfig
                //
                string newAzureLinuxVMName = Utilities.GetUniqueShortName("PSLinuxVM");
                string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);
                AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(newAzureLinuxVMName, InstanceSize.Small, linuxImageName);
                PersistentVM vm = vmPowershellCmdlets.NewAzureVMConfig(azureVMConfigInfo);

                //
                // Add-AzureProvisioningConfig
                //
                AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password, false, false, null, null);
                azureProvisioningConfig.Vm = vm;
                vm = vmPowershellCmdlets.AddAzureProvisioningConfig(azureProvisioningConfig);

                //
                // New-AzureVM
                //
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);
                Collection<InputEndpointContext> endpoints = vmPowershellCmdlets.GetAzureEndPoint(vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, serviceName));
                Assert.AreEqual(endpoints.Count, 1);
                pass = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                pass = false;
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "Iaas"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM)")]
        public void NewAzureVMWithLinuxAndNoSSHEnpointAndDisableSSH()
        {
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                // New-AzureVMConfig
                //
                string newAzureLinuxVMName = Utilities.GetUniqueShortName("PSLinuxVM");
                string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);
                AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(newAzureLinuxVMName, InstanceSize.Small, linuxImageName);
                PersistentVM vm = vmPowershellCmdlets.NewAzureVMConfig(azureVMConfigInfo);

                //
                // Add-AzureProvisioningConfig
                //
                AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password, true, true, null, null);
                azureProvisioningConfig.Vm = vm;
                vm = vmPowershellCmdlets.AddAzureProvisioningConfig(azureProvisioningConfig);

                //
                // New-AzureVM
                //
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);
                Collection<InputEndpointContext> endpoints = vmPowershellCmdlets.GetAzureEndPoint(vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, serviceName));
                Assert.AreEqual(endpoints.Count, 1);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                pass = false;
            }
        }

        [TestMethod(),Ignore(), TestCategory("Scenario"),TestProperty("Feature","Iaas"),Priority(1),Owner("hylee"),Description("Test the cmdlets(New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM) with a WinRMCert")]
        public void NewAzureVMWithWinRMCertificateTest()
        {
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                // New-AzureVMConfig
                //
                string newAzureVMName = Utilities.GetUniqueShortName("PSWinVM");
                AzureVMConfigInfo azureVMConfigInfo = new AzureVMConfigInfo(newAzureVMName, InstanceSize.Small, imageName);
                PersistentVM vm = vmPowershellCmdlets.NewAzureVMConfig(azureVMConfigInfo);

                //
                // Add-AzureProvisioningConfig
                //
                AzureProvisioningConfigInfo azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password, installedCert);
                azureProvisioningConfig.Vm = vm;
                vm = vmPowershellCmdlets.AddAzureProvisioningConfig(azureProvisioningConfig);

                //
                // New-AzureVM
                //
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, null);
                Console.WriteLine("New Azure service with name:{0} created successfully.", serviceName);
                var result = vmPowershellCmdlets.GetAzureVM(newAzureVMName, serviceName);
                Assert.AreEqual(installedCert.Thumbprint, result.VM.WinRMCertificate.Thumbprint);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                pass = false;
            }
        }

        private void InstallCertificate()
        {
            // Create a certificate
            X509Certificate2 certCreated = Utilities.CreateCertificate(password);
            byte[] certData2 = certCreated.Export(X509ContentType.Cert);
            File.WriteAllBytes(cerFileName, certData2);

            // Install the .cer file to local machine.
            StoreLocation certStoreLocation = StoreLocation.CurrentUser;
            StoreName certStoreName = StoreName.My;
            installedCert = Utilities.InstallCert(cerFileName, certStoreLocation, certStoreName);

            certToUpload = vmPowershellCmdlets.RunPSScript(
                String.Format("Get-Item cert:\\{0}\\{1}\\{2}", certStoreLocation.ToString(), certStoreName.ToString(), installedCert.Thumbprint))[0];

        }

        [TestCleanup]
        public virtual void CleanUp()
        {
            Console.WriteLine("Test {0}", pass ? "passed" : "failed");

            // Remove the service
            if ((cleanupIfPassed && pass) || (cleanupIfFailed && !pass))
            {
                CleanupService(serviceName);
            }
        }

    }
}

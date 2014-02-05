using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests
{
    [TestClass]
    public class NewAzureVmTests:ServiceManagementTest
    {
        private string _serviceName;
        private string _linuxImageName;
        const string CerFileName = "testcert.cer";
        X509Certificate2 _installedCert;

        [TestInitialize]
        public void Intialize()
        {
            pass = false;
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
            _linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);
            InstallCertificate();
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "Iaas"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM)")]
        public void NewAzureVMWithLinuxAndNoSSHEnpoint()
        {
            try
            {
                _serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                string newAzureLinuxVMName = Utilities.GetUniqueShortName("PSLinuxVM");

                // Add-AzureProvisioningConfig with NoSSHEndpoint
                var azureVMConfigInfo = new AzureVMConfigInfo(newAzureLinuxVMName, InstanceSize.Small.ToString(), _linuxImageName);
                var azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password, true);
                var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
                PersistentVM vm = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);

                // New-AzureVM
                vmPowershellCmdlets.NewAzureVM(_serviceName, new[] { vm }, locationName);
                Console.WriteLine("New Azure service with name:{0} created successfully.", _serviceName);
                Collection<InputEndpointContext> endpoints = vmPowershellCmdlets.GetAzureEndPoint(vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, _serviceName));

                Console.WriteLine("The number of endpoints: {0}", endpoints.Count);
                foreach (var ep in endpoints)
                {
                    Utilities.PrintContext(ep);
                }
                Assert.AreEqual(0, endpoints.Count);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "Iaas"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM)")]
        public void NewAzureVMWithLinuxAndDisableSSH()
        {
            try
            {
                _serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                string newAzureLinuxVMName = Utilities.GetUniqueShortName("PSLinuxVM");
                string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);

                // Add-AzureProvisioningConfig with DisableSSH option
                var azureVMConfigInfo = new AzureVMConfigInfo(newAzureLinuxVMName, InstanceSize.Small.ToString(), linuxImageName);
                var azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password, false, true);
                var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
                PersistentVM vm = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);

                // New-AzureVM
                vmPowershellCmdlets.NewAzureVM(_serviceName, new[] { vm }, locationName);
                Console.WriteLine("New Azure service with name:{0} created successfully.", _serviceName);
                Collection<InputEndpointContext> endpoints = vmPowershellCmdlets.GetAzureEndPoint(vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, _serviceName));

                Console.WriteLine("The number of endpoints: {0}", endpoints.Count);
                foreach (var ep in endpoints)
                {
                    Utilities.PrintContext(ep);
                }
                Assert.AreEqual(0, endpoints.Count);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "Iaas"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM)")]
        public void NewAzureVMWithLinux()
        {
            try
            {
                _serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);

                string newAzureLinuxVMName = Utilities.GetUniqueShortName("PSLinuxVM");
                string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);

                // Add-AzureProvisioningConfig without NoSSHEndpoint or DisableSSH option
                var azureVMConfigInfo = new AzureVMConfigInfo(newAzureLinuxVMName, InstanceSize.Small.ToString(), linuxImageName);
                var azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password);
                var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
                PersistentVM vm = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);

                // New-AzureVM
                vmPowershellCmdlets.NewAzureVM(_serviceName, new[] { vm }, locationName);
                Console.WriteLine("New Azure service with name:{0} created successfully.", _serviceName);
                Collection<InputEndpointContext> endpoints = vmPowershellCmdlets.GetAzureEndPoint(vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, _serviceName));

                Console.WriteLine("The number of endpoints: {0}", endpoints.Count);
                foreach (var ep in endpoints)
                {
                    Utilities.PrintContext(ep);
                }
                Assert.AreEqual(1, endpoints.Count);
                pass = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "Iaas"), Priority(1), Owner("hylee"), Description("Test the cmdlets (New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM)")]
        public void NewAzureVMWithLinuxAndNoSSHEnpointAndDisableSSH()
        {
            try
            {
                _serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);

                // New-AzureVMConfig
                string newAzureLinuxVMName = Utilities.GetUniqueShortName("PSLinuxVM");
                string linuxImageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Linux" }, false);

                // Add-AzureProvisioningConfig
                var azureVMConfigInfo = new AzureVMConfigInfo(newAzureLinuxVMName, InstanceSize.Small.ToString(), linuxImageName);
                var azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password, true, true);
                var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
                PersistentVM vm = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);

                // New-AzureVM
                vmPowershellCmdlets.NewAzureVM(_serviceName, new[] { vm }, locationName);
                Console.WriteLine("New Azure service with name:{0} created successfully.", _serviceName);
                Collection<InputEndpointContext> endpoints = vmPowershellCmdlets.GetAzureEndPoint(vmPowershellCmdlets.GetAzureVM(newAzureLinuxVMName, _serviceName));

                Console.WriteLine("The number of endpoints: {0}", endpoints.Count);
                foreach (var ep in endpoints)
                {
                    Utilities.PrintContext(ep);
                }
                Assert.AreEqual(0, endpoints.Count);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [TestMethod(), TestCategory("Scenario"),TestProperty("Feature","Iaas"),Priority(1),Owner("hylee"),Description("Test the cmdlets(New-AzureVMConfig,Add-AzureProvisioningConfig,New-AzureVM) with a WinRMCert")]
        [Ignore]
        public void NewAzureVMWithWinRMCertificateTest()
        {
            try
            {
                _serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                string newAzureVMName = Utilities.GetUniqueShortName("PSWinVM");

                // Add-AzureProvisioningConfig with X509Certificate
                var azureVMConfigInfo = new AzureVMConfigInfo(newAzureVMName, InstanceSize.Small.ToString(), imageName);
                var azureProvisioningConfig = new AzureProvisioningConfigInfo(username, password, _installedCert);
                var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
                PersistentVM vm = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);

                // New-AzureVM
                vmPowershellCmdlets.NewAzureVM(_serviceName, new[] { vm }, null);
                Console.WriteLine("New Azure service with name:{0} created successfully.", _serviceName);
                var result = vmPowershellCmdlets.GetAzureVM(newAzureVMName, _serviceName);
                Assert.AreEqual(_installedCert.Thumbprint, result.VM.WinRMCertificate.Thumbprint);
                pass = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void InstallCertificate()
        {
            // Create a certificate
            X509Certificate2 certCreated = Utilities.CreateCertificate(password);
            byte[] certData2 = certCreated.Export(X509ContentType.Cert);
            File.WriteAllBytes(CerFileName, certData2);

            // Install the .cer file to local machine.
            var certStoreLocation = StoreLocation.CurrentUser;
            var certStoreName = StoreName.My;
            _installedCert = Utilities.InstallCert(CerFileName, certStoreLocation, certStoreName);

            //vmPowershellCmdlets.RunPSScript(
            //   String.Format("Get-Item cert:\\{0}\\{1}\\{2}", certStoreLocation.ToString(), certStoreName.ToString(), installedCert.Thumbprint))[0];
        }

        [TestCleanup]
        public virtual void CleanUp()
        {
            Console.WriteLine("Test {0}", pass ? "passed" : "failed");

            // Remove the service
            if ((cleanupIfPassed && pass) || (cleanupIfFailed && !pass))
            {
                CleanupService(_serviceName);
            }
        }

    }
}

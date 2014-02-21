using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests
{
    [TestClass]
    public class GenericIaaSExtensionTests:ServiceManagementTest
    {
        private string serviceName;
        private string vmName;
        private const string referenceNamePrefix = "Reference";
        private string vmAccessUserName;
        private string vmAccessPassword;
        private string completeConfiguraion;
        private string publicConfiguration;
        private string privateConfiguration;
        private string publicConfigPath;
        private string privateConfigPath;
        private VirtualMachineExtensionImageContext vmAccessExtension;
        private string version = "1.0";
        string rdpPath = @".\AzureVM.rdp";
        string dns;
        int port;
        private string referenceName;
        string localPath;

        [ClassInitialize]
        public static void Intialize(TestContext context)
        {
            imageName = vmPowershellCmdlets.GetAzureVMImageName(new[] { "Windows" }, false);
        }

        [TestInitialize]
        public void TestIntialize()
        {
            pass = false;
            serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
            vmName = Utilities.GetUniqueShortName(vmNamePrefix);
            testStartTime = DateTime.Now;
            GetVmAccessConfiguration();
            referenceName = Utilities.GetUniqueShortName(referenceNamePrefix);
            localPath = Path.Combine(Environment.CurrentDirectory, serviceName + ".xml").ToString();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            CleanupService(serviceName);
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureVMExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void AzureVMExtensionTest()
        {
            try
            {
                
                //Get the available VM Extension 

                var availableExtensions =  vmPowershellCmdlets.GetAzureVMAvailableExtension();
                vmAccessExtension = availableExtensions[2];
                if (availableExtensions.Count > 0)
                {
                    
                    var VMExtensionConfigTemplate = vmPowershellCmdlets.GetAzureVMExtensionConfigTemplate(vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, localPath, version);

                    //Deploy a new IaaS VM with Extension using Add-AzureVMExtension
                    Console.WriteLine("Create a new VM with VM access extension.");
                    var vm = CreateIaaSVMObject(vmName);
                    vm = vmPowershellCmdlets.SetAzureVMExtension(vm, vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, version, referenceName, publicConfigPath: publicConfigPath, privateConfigPath:privateConfigPath, disable: false);
                    
                    vmPowershellCmdlets.NewAzureVM(serviceName,new[] {vm},locationName,true);
                    Console.WriteLine("Created a new VM {0} with VM access extension. Service Name : {1}",vmName,serviceName);

                    var vmExtension = GetAzureVMExtesnion(vmName,serviceName);

                    //Verify that the extension actually work
                    VerifyRDPExtension(vmName, serviceName);

                    //Disbale extesnion
                    DisableExtension(vmName, serviceName);

                    VerifyDisabledExtension(vmName, serviceName);
                    pass = true;
                }
                else
                {
                    Console.WriteLine("There are no Azure VM extension available");
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
        
        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureVMExtension)")]
        public void UpdateVMWithExtensionTest()
        {
            try
            {
                var availableExtensions =  vmPowershellCmdlets.GetAzureVMAvailableExtension();
                if (availableExtensions.Count > 0)
                {
                    
                    vmAccessExtension = availableExtensions[1];

                    //Deploy a new IaaS VM with Extension using Add-AzureVMExtension
                    var vm = CreateIaaSVMObject(vmName);
                    vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);

                    vm = GetAzureVM(vmName, serviceName);
                    //Set extension without version
                    vm = vmPowershellCmdlets.SetAzureVMExtension(vm, vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, null,referenceName, publicConfiguration, privateConfiguration);
                    vmPowershellCmdlets.UpdateAzureVM(vmName, serviceName, vm);

                    var vmExtension = GetAzureVMExtesnion(vmName, serviceName);

                    //Verify that the extension actually work
                    VerifyRDPExtension(vmName, serviceName);

                    vmPowershellCmdlets.RemoveAzureVMExtension(GetAzureVM(vmName, serviceName), vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, referenceName, false);
                    pass = true;
                }
                else
                {
                    Console.WriteLine("There are no Azure VM extension available");
                }
                pass = true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        //[TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureVMExtension)")]
        //public void DisableVMExtensionTest()
        //{
        //    try
        //    {

        //        //Get the available VM Extension 
        //        var availableExtensions = vmPowershellCmdlets.GetAzureVMAvailableExtension();
        //        if (availableExtensions.Count > 0)
        //        {
        //            //string publicConfiguration = GetVmAccessConfiguration();
        //            string privateConfiguration = string.Empty;
        //            string version = "0.1";
        //            var vmAccessExtension = availableExtensions[1];
        //            string referenceName = Utilities.GetUniqueShortName(referenceNamePrefix);
        //            //Get the extension config template
        //            string localPath = Path.Combine(Environment.CurrentDirectory, serviceName + ".xml").ToString();
        //            vmPowershellCmdlets.GetAzureVMExtensionConfigTemplate(vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, localPath, version);

        //            //Deploy a new IaaS VM with Extension using Add-AzureVMExtension
        //            var vm = CreateIaaSVMObject(vmName);
        //            vm = vmPowershellCmdlets.SetAzureVMExtension(vm, vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, version, referenceName, publicConfiguration, privateConfiguration, disable:false);
        //            //string extension, string publisher, string version, string referenceName, string publicConfiguraion, string privateConfiguraion, bool state

        //            vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);

        //            var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
        //            vm = vmPowershellCmdlets.SetAzureVMExtension(vm, vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, version, referenceName, publicConfiguration, privateConfiguration, disable: true);
        //            vmPowershellCmdlets.UpdateAzureVM(vmName, serviceName, vm);

        //            vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
        //            var vmExtension = vmPowershellCmdlets.GetAzureVMExtension(vmRoleContext.VM, vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, version);
        //            Utilities.PrintContext(vmExtension);

        //            //Verify that the extension actually work
        //        }
        //        else
        //        {
        //            Console.WriteLine("There are no Azure VM extension available");
        //        }
        //        pass = true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        throw;
        //    }
        //}

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureVMExtension)")]
        public void AddRoleWithExtensionTest()
        {
            try
            {
                var availableExtensions = vmPowershellCmdlets.GetAzureVMAvailableExtension();
                vmAccessExtension = availableExtensions[2];
                //Create an deployment
                
                var vm1 = CreateIaaSVMObject(vmName);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm1 }, locationName);
                //Add a role with extension enabled.

                string referenceName = Utilities.GetUniqueShortName(referenceNamePrefix);
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                var vm2 = CreateIaaSVMObject(vmName2);
                vm2 = vmPowershellCmdlets.SetAzureVMExtension(vm2, vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, version, referenceName, publicConfiguration, privateConfiguration, disable: false);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm2 }, locationName);

                var vmExtension = GetAzureVMExtesnion(vmName, serviceName);

                //Verify that the extension actually work
                VerifyRDPExtension(vmName, serviceName);
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            

        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "IAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set)-AzureVMExtension)")]
        public void UpdateRoleWithExtensionTest()
        {
            try
            {
                var availableExtensions = vmPowershellCmdlets.GetAzureVMAvailableExtension();
                var vmAccessExtension = availableExtensions[2];
                string referenceName = Utilities.GetUniqueShortName(referenceNamePrefix);
                var vm1 = CreateIaaSVMObject(vmName);
                string vmName2 = Utilities.GetUniqueShortName(vmNamePrefix);
                var vm2 = CreateIaaSVMObject(vmName2);
                vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm1, vm2 }, locationName);
                var vmroleContext = vmPowershellCmdlets.GetAzureVM(vmName2, serviceName);
                vmPowershellCmdlets.SetAzureVMExtension(vmroleContext.VM, vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, vmAccessExtension.Version, referenceName, publicConfiguration, privateConfiguration, disable: false);
                vmPowershellCmdlets.UpdateAzureVM(vmName2, serviceName, vm2);
                var vmExtension = GetAzureVMExtesnion(vmName, serviceName);

                //Verify that the extension actually work
                VerifyRDPExtension(vmName, serviceName);
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            
        }
        

        private PersistentVM CreateIaaSVMObject(string vmName)
        {
            //Create an IaaS VM with a static CA.
            var azureVMConfigInfo = new AzureVMConfigInfo(vmName, InstanceSize.Small.ToString(), imageName);
            var azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password);
            var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
            return vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);
        }

        private void CreateNewAzureVM()
        {
            var azureVMConfigInfo = new AzureVMConfigInfo(vmName, InstanceSize.Small.ToString(), imageName);
            var azureProvisioningConfig = new AzureProvisioningConfigInfo(OS.Windows, username, password);
            var persistentVMConfigInfo = new PersistentVMConfigInfo(azureVMConfigInfo, azureProvisioningConfig, null, null);
            PersistentVM vm = vmPowershellCmdlets.GetPersistentVM(persistentVMConfigInfo);
            vmPowershellCmdlets.NewAzureVM(serviceName, new[] { vm }, locationName);
        }

        private void VerifyRDPExtension()
        {
            vmPowershellCmdlets.GetAzureRemoteDesktopFile(vmName, serviceName, rdpPath, false);
            using (StreamReader stream = new StreamReader(rdpPath))
            {
                string firstLine = stream.ReadLine();
                dns = Utilities.FindSubstring(firstLine, ':', 2);
            }

            Assert.IsTrue((Utilities.RDPtestIaaS(dns, 0, vmAccessUserName, vmAccessPassword, true)), "Cannot RDP to the instance!!");
        }

        private void GetVmAccessConfiguration()
        {
            privateConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateConfig.xml");
            publicConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "PublicConfig.xml");
            privateConfiguration = File.ReadAllText(privateConfigPath);
            publicConfiguration = File.ReadAllText(publicConfigPath);
            
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(publicConfiguration);
            vmAccessUserName = doc.GetElementsByTagName("UserName")[0].InnerText;
            doc.LoadXml(privateConfiguration);
            vmAccessPassword = doc.GetElementsByTagName("Password")[0].InnerText;
        }

        private VirtualMachineExtensionContext GetAzureVMExtesnion(string vmName, string serviceName)
        {
            Console.WriteLine("Get Azure VM's extension");
            var vmExtension = vmPowershellCmdlets.GetAzureVMExtension(GetAzureVM(vmName, serviceName));
            Utilities.PrintContext(vmExtension);
            Console.WriteLine("Azure VM's extension info retrieved successfully.");
            return vmExtension;
        }

        private PersistentVM GetAzureVM(string vmName, string serviceName)
        {
            Console.WriteLine("Fetch Azure VM details");
            var vmRoleContext = vmPowershellCmdlets.GetAzureVM(vmName, serviceName);
            Console.WriteLine("Azure VM details retreived successfully");
            return vmRoleContext.VM;
        }

        private void ValidateLogin(string dns, int port, string vmAccessUserName, string vmAccessPassword)
        {
            Assert.IsTrue((Utilities.RDPtestIaaS(dns, port, vmAccessUserName, vmAccessPassword, true)), "Cannot RDP to the instance!!");
        }

        private void VerifyRDPExtension(string vmName, string serviceName)
        {
            Console.WriteLine("Fetching Azure VM RDP file");
            vmPowershellCmdlets.GetAzureRemoteDesktopFile(vmName, serviceName, rdpPath, false);
            using (StreamReader stream = new StreamReader(rdpPath))
            {
                string firstLine = stream.ReadLine();
                var dnsAndport = Utilities.FindSubstring(firstLine, ':', 2).Split(new char[] { ':' });
                dns = dnsAndport[0];
                port = int.Parse(dnsAndport[1]);
            }
            Console.WriteLine("Azure VM RDP file downloaded.");

            Console.WriteLine("Waiting for a minute vefore trying to connect to VM");
            Thread.Sleep(60000);
            Utilities.RetryActionUntilSuccess(() => ValidateLogin(dns, port, vmAccessUserName, vmAccessPassword), "Cannot RDP to the instance!!", 5, 10000);

        }

        private void DisableExtension(string vmName, string serviceName)
        {
            var vm = GetAzureVM(vmName, serviceName);
            Console.WriteLine("Disabling the VM Access extesnion for the vm {0}", vmName);
            vm = vmPowershellCmdlets.SetAzureVMExtension(vm, vmAccessExtension.ExtensionName, vmAccessExtension.Publisher, version, referenceName, disable: true);
            vmPowershellCmdlets.UpdateAzureVM(vmName, serviceName, vm);
            Console.WriteLine("Disabled VM Access extesnion for the vm {0}", vmName);
        }


        private void VerifyDisabledExtension(string vmName, string serviceName)
        {
            Console.WriteLine("Verifying the disabled extension");
            var vmExtension = GetAzureVMExtesnion(vmName, serviceName);
            Utilities.PrintContext(vmExtension);
            Assert.AreEqual("Disable", vmExtension.State, "State is not Disable");
            Assert.IsTrue(string.IsNullOrEmpty(vmExtension.PrivateConfiguration), "PrivateConfiguration is not empty.");
            Assert.IsTrue(string.IsNullOrEmpty(vmExtension.PublicConfiguration), "PublicConfiguration is not empty.");
            Console.WriteLine("Verifed the disabled extension successfully.");
        }

    }
}

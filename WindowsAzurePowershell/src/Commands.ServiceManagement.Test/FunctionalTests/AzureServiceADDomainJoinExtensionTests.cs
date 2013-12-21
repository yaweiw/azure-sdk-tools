
namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Extensions;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
    using Microsoft.WindowsAzure.ServiceManagement;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    [TestClass]
    public class AzureServiceADDomainJoinExtensionTests:ServiceManagementTest
    {
        private string serviceName;
        string cerFileName = "testcert.cer";
        PSObject certToUpload;
        X509Certificate2 installedCert;

        [TestInitialize]
        public void Initialize()
        {
            serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
            pass = false;
            testStartTime = DateTime.Now;
            InstallCertificate();
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensions()
        {

            // Choose the package and config files from local machine
            string packageName = Convert.ToString(TestContext.DataRow["upgradePackage"]);
            string configName = Convert.ToString(TestContext.DataRow["upgradeConfig"]);
            var packagePath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + packageName);
            var configPath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + configName);

            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);

            string deploymentName = "deployment1";
            string deploymentLabel = "label1";
            PSCredential cred = new PSCredential(username, Utilities.convertToSecureString(password));
            
            string storageAccount = "djteststrg";
            string vmName1 = Utilities.GetUniqueShortName("DjExtVM1");
            string vmName2 = Utilities.GetUniqueShortName("DjExtVM2");
            string[] role = { "WebRole1" };
            string domainName = "djtest.com";
            string thumbprintAlgorithm = "sha1";
            SwitchParameter restart = new SwitchParameter();

            try
            {

                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
                Console.WriteLine("service, {0}, is created.", serviceName);
                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false);
               
                //Join Domain with default parameter set.
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, null, null, role, DeploymentSlotType.Production.ToString(), serviceName, thumbprintAlgorithm, restart, cred, null, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully.", serviceName, domainName);
                vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                Console.WriteLine("Service domain join extension fetched successfully.");
                vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);

                //Join Domian with DomainJoinParmaterSet with JoinOptions.JoinDomain
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, null, JoinOptions.JoinDomain, role, DeploymentSlotType.Production.ToString(), serviceName, thumbprintAlgorithm, null, cred, null, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully using join option 35.", serviceName, domainName);
                vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                Console.WriteLine("Service domain join extension fetched successfully.");
                vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);

                ////Join Domian with DomainParmaterSet
                //vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName,installedCert, null, role, DeploymentSlotType.Production.ToString(), serviceName, thumbprintAlgorithm, null, cred, null, null);
                //Console.WriteLine("Servie {0} added to domain {1} successfully.", serviceName, domainName);
                //vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                //Console.WriteLine("Service domain join extension fetched successfully.");
                //vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                //Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);

                ////Join Domian with DomainJoinParmaterSet and certificate
                //vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, installedCert, JoinOptions.JoinDomain, role, DeploymentSlotType.Production.ToString(), serviceName, thumbprintAlgorithm, null, cred, null, null);
                //Console.WriteLine("Servie {0} added to domain {1} successfully.", serviceName, domainName);
                //vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                //Console.WriteLine("Service domain join extension fetched successfully.");
                //vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                //Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);

                //vmPowershellCmdlets.AddAzureCertificate(serviceName, certToUpload, password);

                ////Join domain with DomainThumbprintParameterSet
                //vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, installedCert.Thumbprint, null, role, null, DeploymentSlotType.Production, serviceName, thumbprintAlgorithm, null, cred, null);
                //Console.WriteLine("Servie {0} added to domain {1} successfully.", serviceName, domainName);
                //vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                //Console.WriteLine("Service domain join extension fetched successfully.");
                //vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                //Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);

                //Join domain with DomainJoinThumbprintParameterSet
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName,null, null, role, 35, DeploymentSlotType.Production, serviceName, thumbprintAlgorithm, null, cred, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully", serviceName, domainName);
                vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                Console.WriteLine("Service domain join extension fetched successfully.");
                vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);

                //Join domain with DomainJoinThumbprintParameterSet and join oprtion 35
                //vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, installedCert.Thumbprint, null, role, 35, DeploymentSlotType.Production, serviceName, thumbprintAlgorithm, null, cred, null);
                //Console.WriteLine("Servie {0} added to domain {1} successfully using join option 35", serviceName, domainName);
                //vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                //Console.WriteLine("Service domain join extension fetched successfully.");
                //vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                //Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                vmPowershellCmdlets.RemoveAzureService(serviceName, true);

            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void NewAzureServiceDomainJoinExtensionConfigTests()
        {

            // Choose the package and config files from local machine
            string packageName = Convert.ToString(TestContext.DataRow["upgradePackage"]);
            string configName = Convert.ToString(TestContext.DataRow["upgradeConfig"]);
            var packagePath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + packageName);
            var configPath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + configName);

            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);

            string deploymentName = "deployment1";
            string deploymentLabel = "label1";

            PSCredential cred = new PSCredential(username, Utilities.convertToSecureString(password));
            //string affinityGroup = "djtestuswest";
            string storageAccount = "djteststrg";
            string vmName1 = Utilities.GetUniqueShortName("DjExtVM1");
            string vmName2 = Utilities.GetUniqueShortName("DjExtVM2");
            string[] role = { "WebRole1" };
            string domainName = "djtest.com";
            string thumbprintAlgorithm = "sha1";
            SwitchParameter restart = new SwitchParameter();
            ExtensionConfigurationInput domainJoinExtensionConfig;
            try
            {

                string subscriptionName = vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName;
                vmPowershellCmdlets.SetAzureSubscription(subscriptionName, storageAccount);

                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);

                Console.WriteLine("service, {0}, is created.", serviceName);
                
                //Prepare a new domain join config with default parameter set
                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, null, null, null, null, role, thumbprintAlgorithm, restart, cred);
                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false, domainJoinExtensionConfig);
                Console.WriteLine("{0}:New deployment{1} with domain join {2} created successfully.", DateTime.Now, serviceName, domainName);
                vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                Console.WriteLine("{0}:Service domain join extension fetched successfully.", DateTime.Now);
                vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                Console.WriteLine("{0}Removed domain join extension for the deployment {1} succefully.", DateTime.Now, deploymentName);
                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

                //Prepare a new domain join config with default parameter set and one of the join options
                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, null, JoinOptions.JoinDomain, null, null, null, null, null, cred);
                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Staging, deploymentLabel, deploymentName, false, false, domainJoinExtensionConfig);
                Console.WriteLine("New deployment with domain join created successfully.", serviceName, domainName);
                vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Staging.ToString());
                Console.WriteLine("Service domain join extension fetched successfully.");
                vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Staging.ToString(), role, null);
                Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);
                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Staging, true);

                //Prepare a new domain join config with DomainParameterSet (using only X509certicate2)
                //domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, installedCert, null, null, null, null, null, null, cred);
                //vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false, domainJoinExtensionConfig);
                //Console.WriteLine("New deployment with domain join created successfully.", serviceName, domainName);
                //vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                //Console.WriteLine("Service domain join extension fetched successfully.");
                //vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                //Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);
                //vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

                ////Prepare a new domain join config with DomainParameterSet (using X509certicate2 and Options)
                //domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, installedCert, JoinOptions.JoinDomain, null, null, null, null, null, cred);
                //vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Staging, deploymentLabel, deploymentName, false, false, domainJoinExtensionConfig);
                //Console.WriteLine("New deployment with domain join created successfully.", serviceName, domainName);
                //vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Staging.ToString());
                //Console.WriteLine("Service domain join extension fetched successfully.");
                //vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Staging.ToString(), role, null);
                //Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);
                //vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Staging, true);

                vmPowershellCmdlets.AddAzureCertificate(serviceName, certToUpload,password);

                //Prepare a new domain join config with DomianThumbprintParameterSet
                //domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName,installedCert.Thumbprint, null, null, role, thumbprintAlgorithm,null, null, cred);
                //vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Staging, deploymentLabel, deploymentName, false, false, domainJoinExtensionConfig);
                //Console.WriteLine("{0}:New deployment{1} with domain join {2} created successfully.", DateTime.Now, serviceName, domainName);
                //vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Staging.ToString());
                //Console.WriteLine("{0}:Service domain join extension fetched successfully.", DateTime.Now);
                //vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Staging.ToString(), role, null);
                //Console.WriteLine("{0}Removed domain join extension for the deployment {1} succefully.", DateTime.Now, deploymentName);
                //vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Staging, true);

                //Prepare a new domain join config with default parameter set and joinOption 35
                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, null, null, null, role, thumbprintAlgorithm, 35, null, cred);
                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false, domainJoinExtensionConfig);
                Console.WriteLine("{0}:New deployment{1} with domain join {2} created successfully.", DateTime.Now, serviceName, domainName);
                vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
                Console.WriteLine("{0}:Service domain join extension fetched successfully.", DateTime.Now);
                vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
                Console.WriteLine("{0}Removed domain join extension for the deployment {1} succefully.", DateTime.Now, deploymentName);
                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

                ////Prepare a new domain join config with DomianJoinThumbprintParameterSet
                //domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName,installedCert.Thumbprint, null, null, role, thumbprintAlgorithm, 35, null, cred);
                //vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Staging, deploymentLabel, deploymentName, false, false, domainJoinExtensionConfig);
                //Console.WriteLine("{0}:New deployment{1} with domain join {2} created successfully.", DateTime.Now, serviceName, domainName);
                //vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Staging.ToString());
                //Console.WriteLine("{0}:Service domain join extension fetched successfully.", DateTime.Now);
                //vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Staging.ToString(), role, null);
                //Console.WriteLine("{0}Removed domain join extension for the deployment {1} succefully.", DateTime.Now, deploymentName);
                //vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Staging, true);

                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.
                
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
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

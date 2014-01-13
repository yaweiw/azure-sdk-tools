
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
        string domainName = "djtest.com";
        string thumbprintAlgorithm = "sha1";
        string storageAccount = "djteststrg";
        
        string deploymentName = "deployment1";
        string deploymentLabel = "label1";

        // Choose the package and config files from local machine
        string packageName;
        string configName;
        FileInfo packagePath1;
        FileInfo configPath1;
        PSCredential cred;
        string[] role = { "WebRole1" };


        [TestInitialize]
        public void Initialize()
        {
            serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
            pass = false;
            InstallCertificate();
            // Choose the package and config files from local machine
            packageName = Convert.ToString(TestContext.DataRow["upgradePackage"]);
            configName = Convert.ToString(TestContext.DataRow["upgradeConfig"]);
            packagePath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + packageName);
            configPath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + configName);
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDefaultParamterSetTest()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);
                
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();
                                
                //Join Domain with default parameter set.
                Console.Write("Joining domain with default parameter set");
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, null, null, role, DeploymentSlotType.Production.ToString(), serviceName, null, false, cred, null, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully.", serviceName, domainName);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                pass = true;

            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainJoinParmaterSetAndJoinOptionsTest()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();
            
            string[] role = { "WebRole1" };

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);
                
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();

                //Join Domian with DomainJoinParmaterSet with JoinOptions.JoinDomain
                Console.Write("Joining domain with domain join parameter set with JoinOptions.JoinDomain");
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, null, JoinOptions.JoinDomain, role, DeploymentSlotType.Production.ToString(), serviceName, null, false, cred, null, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully using join option 35.", serviceName, domainName);
                
                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                pass = true;
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        
        [TestMethod(),Ignore(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainParmaterSetTest()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            string[] role = { "WebRole1" };

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);
                NewAzureDeployment();

                //Join Domian with DomainParmaterSet
                Console.Write("Joining domain with domian parameter set");
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, installedCert, null, role, DeploymentSlotType.Production.ToString(), serviceName, thumbprintAlgorithm, false, cred, null, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully.", serviceName, domainName);
                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                pass = true;
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(),Ignore(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainJoinParmaterSetAndCertificateTest()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            string[] role = { "WebRole1" };

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);
                NewAzureDeployment();
                
                //Join Domian with DomainJoinParmaterSet and certificate
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, installedCert, JoinOptions.JoinDomain, role, DeploymentSlotType.Production.ToString(), serviceName, thumbprintAlgorithm, false, cred, null, null);
                
                Console.WriteLine("Servie {0} added to domain {1} successfully.", serviceName, domainName);
                GetAzureServiceDomainJoinExtension();
                
                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                pass = true;
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(),Ignore(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainThumbprintParameterSetTest()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            string[] role = { "WebRole1" };

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();

                vmPowershellCmdlets.AddAzureCertificate(serviceName, certToUpload, password);

                //Join domain with DomainThumbprintParameterSet
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, installedCert.Thumbprint, null, role, null, DeploymentSlotType.Production, serviceName, thumbprintAlgorithm, false, cred, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully.", serviceName, domainName);
                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                vmPowershellCmdlets.RemoveAzureService(serviceName, true);
                pass = true;
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDefaultParameterSet35Test()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            string[] role = { "WebRole1" };

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();

                vmPowershellCmdlets.AddAzureCertificate(serviceName, certToUpload, password);

                //Join domain with DomainJoinThumbprintParameterSet
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, null, null, role, 35, DeploymentSlotType.Production, serviceName, null, false, cred, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully", serviceName, domainName);
                
                GetAzureServiceDomainJoinExtension();
                
                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);
                pass = true;
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(),Ignore(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainJoinThumbprintParameterSetAndJoinOption35Test()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            string[] role = { "WebRole1" };

            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();

                vmPowershellCmdlets.AddAzureCertificate(serviceName, certToUpload, password);

                //Join domain with DomainJoinThumbprintParameterSet and join oprtion 35
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(domainName, installedCert.Thumbprint, null, role, 35, DeploymentSlotType.Production, serviceName, thumbprintAlgorithm, false, cred, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully using join option 35", serviceName, domainName);
                
                GetAzureServiceDomainJoinExtension();
                
                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void NewAzureServiceDomainJoinExtensionConfigWithDefaultParmateSetTests()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();
            
            string[] role = { "WebRole1" };

            ExtensionConfigurationInput domainJoinExtensionConfig;
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);

                //Prepare a new domain join config with default parameter set
                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, null, null, null, null, role, null, false, cred);
                
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment(domainJoinExtensionConfig);
                
                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Staging, true);
                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.
                
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDefaultParmateSetAndJoinOptionsTests()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            string[] role = { "WebRole1" };
            ExtensionConfigurationInput domainJoinExtensionConfig;
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);

                //Prepare a new domain join config with default parameter set and one of the join options
                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, null, JoinOptions.JoinDomain, null, null, null, null, false, cred);

                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment(domainJoinExtensionConfig);
                GetAzureServiceDomainJoinExtension();
                RemoveAzureServiceDomainJoinExtesnion();
                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Staging, true);

                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.

            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(),Ignore(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDomainParmateSetTests()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            string[] role = { "WebRole1" };
            ExtensionConfigurationInput domainJoinExtensionConfig;
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);

                //Prepare a new domain join config with DomainParameterSet (using only X509certicate2)
                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, installedCert, null, null, null, null, null, false, cred);

                NewAzureDeployment(domainJoinExtensionConfig);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();
                
                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.

            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(),Ignore(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDomainParameterSetAndCertTest()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            string[] role = { "WebRole1" };

            ExtensionConfigurationInput domainJoinExtensionConfig;
            try
            {

                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);

                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, installedCert, JoinOptions.JoinDomain, null, null, null, null, false, cred);
                
                NewAzureDeployment(domainJoinExtensionConfig);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Staging, true);

            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(),Ignore(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDomianThumbprintParameterSetTest()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            string[] role = { "WebRole1" };
            ExtensionConfigurationInput domainJoinExtensionConfig;
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);

                //Prepare a new domain join config with DomianThumbprintParameterSet
                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, installedCert.Thumbprint, null, null, role, thumbprintAlgorithm, null, false, cred);

                NewAzureDeployment(domainJoinExtensionConfig);
                
                GetAzureServiceDomainJoinExtension();
                
                RemoveAzureServiceDomainJoinExtesnion();
                
                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Staging, true);
                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.

            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDefaultParmateSetAndJoinOption35Test()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            ExtensionConfigurationInput domainJoinExtensionConfig;
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);

                //Prepare a new domain join config with default parameter set and joinOption 35
                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, null, null, null, role, null, 35, false, cred);
                NewAzureDeployment(domainJoinExtensionConfig);


                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();
                
                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Production, true);

                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.
            }
            catch (Exception e)
            {
                pass = false;
                Assert.Fail("Exception occurred: {0}", e.ToString());
            }
        }

        [TestMethod(),Ignore(), TestCategory("Scenario"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\package.csv", "package#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDomianJoinThumbprintParameterSetTests()
        {
            testStartTime = DateTime.Now;
            CheckIfPackageAndConfigFilesExists();

            ExtensionConfigurationInput domainJoinExtensionConfig;
            try
            {
                serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
                vmPowershellCmdlets.SetAzureSubscription(vmPowershellCmdlets.GetCurrentAzureSubscription().SubscriptionName, storageAccount);

                domainJoinExtensionConfig = vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(domainName, installedCert.Thumbprint, null, null, role, thumbprintAlgorithm, 35, false, cred);
                
                NewAzureDeployment(domainJoinExtensionConfig);

                GetAzureServiceDomainJoinExtension();
                
                RemoveAzureServiceDomainJoinExtesnion();
                
                vmPowershellCmdlets.RemoveAzureDeployment(serviceName, DeploymentSlotType.Staging, true);

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

        #region Helper Methods
        private void NewAzureDeployment(ExtensionConfigurationInput domainJoinExtensionConfig = null)
        {
            //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
            Console.WriteLine("Creating a new Azure Iaas VM");
            vmPowershellCmdlets.NewAzureService(serviceName, serviceName, locationName);
            Console.WriteLine("Service, {0}, is created.", serviceName);
            if (domainJoinExtensionConfig == null)
            {
                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false);
                Console.WriteLine("New deployment created successfully.");
            }
            else
            {
                vmPowershellCmdlets.NewAzureDeployment(serviceName, packagePath1.FullName, configPath1.FullName, DeploymentSlotType.Production, deploymentLabel, deploymentName, false, false, domainJoinExtensionConfig);
                Console.WriteLine("{0}:New deployment{1} with domain join {2} created successfully.", DateTime.Now, serviceName, domainName);
            }
        }

        private void GetAzureServiceDomainJoinExtension()
        {
            var domianContext = vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString());
            Utilities.PrintContext<ADDomainExtensionContext>(domianContext);
            Assert.IsFalse(string.IsNullOrEmpty(domianContext.Extension), "Extension is empty or null.");
            Console.WriteLine("Service domain join extension fetched successfully.");
        }

        private void RemoveAzureServiceDomainJoinExtesnion()
        {
            Console.WriteLine("Removing the domian join extension.");
            vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(serviceName, DeploymentSlotType.Production.ToString(), role, null);
            Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", deploymentName);
        }

        private void CheckIfPackageAndConfigFilesExists()
        {
            Assert.IsTrue(File.Exists(packagePath1.FullName), "VHD file not exist={0}", packagePath1);
            Assert.IsTrue(File.Exists(configPath1.FullName), "VHD file not exist={0}", configPath1);
        }
        #endregion Helper Methods
    }
}

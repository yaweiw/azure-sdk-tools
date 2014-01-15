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
    using VisualStudio.TestTools.UnitTesting;
    using Extensions;
    using WindowsAzure.ServiceManagement;
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using System.Reflection;

    [TestClass]
    public class AzureServiceADDomainJoinExtensionTests:ServiceManagementTest
    {
        private string _serviceName;
        PSObject _certToUpload;
        X509Certificate2 _installedCert;
        private string _deploymentName;
        private string _deploymentLabel;
        private string _packageName;
        private string _configName;
        private string _rdpCertName;

        private FileInfo _packagePath1;
        private FileInfo _configPath1;
        private FileInfo _rdpCertPath;
        private PSCredential _cred;

        const string CerFileName = "testcert.cer";
        const string DomainName = "djtest.com";
        const string ThumbprintAlgorithm = "sha1";

        const string DeploymentNamePrefix = "psdeployment";
        const string DeploymentLabelPrefix = "psdeploymentlabel";
        private const string DomainUserName = "pstestuser@djtest.com";
        private const string AffinityGroupName = "WestUsAffinityGroup";

        // Choose the package and config files from local machine
        readonly string[] _role = { "WebRole1" };

        [TestInitialize]
        public void Initialize()
        {
            _serviceName = Utilities.GetUniqueShortName(serviceNamePrefix);
            _deploymentName = Utilities.GetUniqueShortName(DeploymentNamePrefix);
            _deploymentLabel = Utilities.GetUniqueShortName(DeploymentLabelPrefix);

            pass = false;
            InstallCertificate();

            // Choose the package and config files from local machine
            _packageName = Convert.ToString(TestContext.DataRow["packageName"]);
            _configName = Convert.ToString(TestContext.DataRow["configName"]);
            _rdpCertName = Convert.ToString(TestContext.DataRow["rdpCertName"]);
            _packagePath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + _packageName);
            _configPath1 = new FileInfo(Directory.GetCurrentDirectory() + "\\" + _configName);
            _rdpCertPath = new FileInfo(Directory.GetCurrentDirectory() + "\\" + _rdpCertName);
            _cred = new PSCredential(DomainUserName, Utilities.convertToSecureString(password));

            CheckIfPackageAndConfigFilesExists();
            testStartTime = DateTime.Now;
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDefaultParamterSetTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);
            Console.WriteLine(_cred.UserName);
            try
            {
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();

                //Join Domain with default parameter set.
                Console.Write("Joining domain with default parameter set");
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(DomainName, _cred, 35, true, _serviceName,
                    DeploymentSlotType.Production, null);
                Console.WriteLine("Servie {0} added to domain {1} successfully.", _serviceName, DomainName);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Production, true);
                pass = true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainJoinParmaterSetAndJoinOptionsTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();

                //Join Domian with DomainJoinParmaterSet with JoinOptions.JoinDomain
                Console.Write("Joining domain with domain join parameter set with JoinOptions.JoinDomain");

                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(DomainName, _cred, JoinOptions.JoinDomain, true,
                    _serviceName, DeploymentSlotType.Production, _role);

                Console.WriteLine("Servie {0} added to domain {1} successfully using join option.", _serviceName, DomainName);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Production, true);
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainParmaterSetTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                NewAzureDeployment();

                //Join Domian with DomainParmaterSet
                Console.Write("Joining domain with domian parameter set");
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(DomainName, _cred, null, true, _serviceName,
                    DeploymentSlotType.Production, _role, _installedCert, ThumbprintAlgorithm);
                Console.WriteLine("Servie {0} added to domain {1} successfully.", _serviceName, DomainName);
                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Production, true);
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainJoinParmaterSetAndCertificateTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                NewAzureDeployment();

                //Join Domian with DomainJoinParmaterSet and certificate
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(DomainName, _cred, JoinOptions.JoinDomain, true,
                    _serviceName, DeploymentSlotType.Production, _role, _installedCert, ThumbprintAlgorithm);

                Console.WriteLine("Servie {0} added to domain {1} successfully.", _serviceName, DomainName);
                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Production, true);
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainThumbprintParameterSetTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();

                vmPowershellCmdlets.AddAzureCertificate(_serviceName, _certToUpload, password);

                //Join domain with DomainThumbprintParameterSet
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(DomainName, _cred, null, true,
                    _serviceName, DeploymentSlotType.Production, _role,
                    _installedCert.Thumbprint, ThumbprintAlgorithm);
                Console.WriteLine("Servie {0} added to domain {1} successfully.", _serviceName, DomainName);
                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Production, true);
                vmPowershellCmdlets.RemoveAzureService(_serviceName, true);
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(0), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDefaultParameterSet35Test()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();

                vmPowershellCmdlets.AddAzureCertificate(_serviceName, _certToUpload, password);

                //Join domain with DomainJoinThumbprintParameterSet
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(DomainName, _cred, 35, true,
                    _serviceName, DeploymentSlotType.Production, _role);
                Console.WriteLine("Servie {0} added to domain {1} successfully", _serviceName, DomainName);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Production, true);
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((Get,Set,Remove)-AzureServiceADDomainExtension)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void SetAzureServiceDomainJoinExtensionwithDomainJoinThumbprintParameterSetAndJoinOption35Test()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment();

                vmPowershellCmdlets.AddAzureCertificate(_serviceName, _certToUpload, password);

                //Join domain with DomainJoinThumbprintParameterSet and join oprtion 35
                vmPowershellCmdlets.SetAzureServiceDomainJoinExtension(DomainName, _cred, 35, true,
                    _serviceName, DeploymentSlotType.Production, _role,
                    _installedCert.Thumbprint, ThumbprintAlgorithm);
                Console.WriteLine("Servie {0} added to domain {1} successfully using join option 35", _serviceName, DomainName);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Production, true);
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void NewAzureServiceDomainJoinExtensionConfigWithDefaultParmateSetTests()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                //Prepare a new domain join config with default parameter set
                ExtensionConfigurationInput domainJoinExtensionConfig =
                    vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(DomainName, null, null, null, null,
                        _role, null, false, _cred);

                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment(domainJoinExtensionConfig);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Staging, true);
                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDefaultParmateSetAndJoinOptionsTests()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                //Prepare a new domain join config with default parameter set and one of the join options
                ExtensionConfigurationInput domainJoinExtensionConfig =
                    vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(DomainName, null,
                        JoinOptions.JoinDomain, null, null, null, null, false, _cred);

                //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
                NewAzureDeployment(domainJoinExtensionConfig);
                GetAzureServiceDomainJoinExtension();
                RemoveAzureServiceDomainJoinExtesnion();
                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Staging, true);

                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDomainParmateSetTests()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                //Prepare a new domain join config with DomainParameterSet (using only X509certicate2)
                ExtensionConfigurationInput domainJoinExtensionConfig =
                    vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(DomainName, _installedCert, null, null,
                        null, null, null, false, _cred);

                NewAzureDeployment(domainJoinExtensionConfig);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Production, true);

                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDomainParameterSetAndCertTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                ExtensionConfigurationInput domainJoinExtensionConfig =
                    vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(DomainName, _installedCert,
                        JoinOptions.JoinDomain, null, null, null, null, true, _cred);

                NewAzureDeployment(domainJoinExtensionConfig);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion();

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Staging, true);
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDomianThumbprintParameterSetTest()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                //Prepare a new domain join config with DomianThumbprintParameterSet
                ExtensionConfigurationInput domainJoinExtensionConfig =
                    vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(DomainName, _installedCert.Thumbprint,
                        null, null, _role, ThumbprintAlgorithm, null, false, _cred);

                NewAzureDeployment(domainJoinExtensionConfig);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Staging, true);
                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDefaultParmateSetAndJoinOption35Test()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                //Prepare a new domain join config with default parameter set and joinOption 35
                ExtensionConfigurationInput domainJoinExtensionConfig =
                    vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(DomainName, null, null, null, _role,
                        null, 35, false, _cred);
                NewAzureDeployment(domainJoinExtensionConfig);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Production, true);

                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
                throw;
            }
        }

        [TestMethod(), TestCategory("ADDomain"), TestProperty("Feature", "PAAS"), Priority(1), Owner("hylee"), Description("Test the cmdlet ((New)-AzureServiceADDomainExtensionConfig)")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\Resources\\packageADDomain.csv", "packageADDomain#csv", DataAccessMethod.Sequential)]
        public void NewDomainJoinExtConfigWithDomianJoinThumbprintParameterSetTests()
        {
            StartTest(MethodBase.GetCurrentMethod().Name, testStartTime);

            try
            {
                ExtensionConfigurationInput domainJoinExtensionConfig =
                    vmPowershellCmdlets.NewAzureServiceDomainJoinExtensionConfig(DomainName, _installedCert.Thumbprint,
                        null, null, _role, ThumbprintAlgorithm, 35, false, _cred);

                NewAzureDeployment(domainJoinExtensionConfig);

                GetAzureServiceDomainJoinExtension();

                RemoveAzureServiceDomainJoinExtesnion(_role);

                vmPowershellCmdlets.RemoveAzureDeployment(_serviceName, DeploymentSlotType.Staging, true);

                //TO DO: add test cases to test cmdlet with UnjoinCrednetial patameter.
                pass = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: {0}", e);
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
            const StoreLocation certStoreLocation = StoreLocation.CurrentUser;
            const StoreName certStoreName = StoreName.My;
            _installedCert = Utilities.InstallCert(CerFileName);

            _certToUpload = vmPowershellCmdlets.RunPSScript(
                String.Format("Get-Item cert:\\{0}\\{1}\\{2}", certStoreLocation, certStoreName, _installedCert.Thumbprint))[0];
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

        #region Helper Methods
        private void NewAzureDeployment(ExtensionConfigurationInput domainJoinExtensionConfig = null)
        {
            //Create a new Azure Iaas VM and set Domain Join extension, get domain join extension and then remove domain join extension
            Console.WriteLine("Creating a new Azure Iaas VM");

            vmPowershellCmdlets.NewAzureService(_serviceName, _serviceName, null, AffinityGroupName);

            Console.WriteLine("Service, {0}, is created.", _serviceName);

            vmPowershellCmdlets.AddAzureCertificate(_serviceName, _rdpCertPath.FullName, password);

            if (domainJoinExtensionConfig == null)
            {
                vmPowershellCmdlets.NewAzureDeployment(_serviceName, _packagePath1.FullName, _configPath1.FullName, DeploymentSlotType.Production, _deploymentLabel, _deploymentName, false, false);
                Console.WriteLine("New deployment created successfully.");
            }
            else
            {
                vmPowershellCmdlets.NewAzureDeployment(_serviceName, _packagePath1.FullName, _configPath1.FullName, DeploymentSlotType.Production, _deploymentLabel, _deploymentName, false, false, domainJoinExtensionConfig);
                Console.WriteLine("{0}:New deployment {1} with domain join {2} created successfully.", DateTime.Now, _serviceName, domainJoinExtensionConfig.Type);
            }
        }

        private void GetAzureServiceDomainJoinExtension()
        {
            var domianContext = vmPowershellCmdlets.GetAzureServiceDomainJoinExtension(_serviceName, DeploymentSlotType.Production);
            Utilities.PrintContext(domianContext);
            Assert.IsFalse(string.IsNullOrEmpty(domianContext.Extension), "Extension is empty or null.");
            Console.WriteLine("Service domain join extension fetched successfully.");
        }

        private void RemoveAzureServiceDomainJoinExtesnion(string[] roles = null, bool uninstallConfig = false)
        {
            Console.WriteLine("Removing the domian join extension.");
            vmPowershellCmdlets.RemoveAzureServiceDomainJoinExtension(_serviceName, DeploymentSlotType.Production, roles, uninstallConfig);
            Console.WriteLine("Removed domain join extension for the deployment {0} succefully.", _deploymentName);
        }

        private void CheckIfPackageAndConfigFilesExists()
        {
            Assert.IsTrue(File.Exists(_packagePath1.FullName), "VHD file not exist={0}", _packagePath1);
            Assert.IsTrue(File.Exists(_configPath1.FullName), "VHD file not exist={0}", _configPath1);
        }
        #endregion Helper Methods
    }
}

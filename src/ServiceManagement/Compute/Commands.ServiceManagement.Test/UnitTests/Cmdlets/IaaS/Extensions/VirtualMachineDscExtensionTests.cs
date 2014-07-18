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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.UnitTests.Cmdlets.IaaS.Extensions
{
    using System;
    using System.Linq;
    using Commands.Test.Utilities.Common;
    using ServiceManagement.IaaS.Extensions.DSC;
    using VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for DSC ConfigurationNameHelper class.
    /// </summary>
    /// <remarks>
    /// ConfigurationNameHelper.ExtractConfigurationNames() API requires tests to be run in x64 host.
    /// </remarks>
    [TestClass]
    public class VirtualMachineDscExtensionTests : TestBase
    {
        private const string CorporateClientConfigurationPath = @"DSC\Configurations\CorporateClientConfiguration.ps1";
        private const string DomainControllerConfigurationPath = @"DSC\Configurations\DomainControllerConfiguration.ps1";
        private const string SHMulptiConfigurationsPath = @"DSC\Configurations\SHMulptiConfigurations.ps1";


        [TestMethod]
        [TestCategory("Scenario")]
        [DeploymentItem(CorporateClientConfigurationPath)]
        public void TestExtractConfigurationNames1()
        {
            ConfigurationParseResult results = ConfigurationNameHelper.ExtractConfigurationNames(CorporateClientConfigurationPath);
            Assert.AreEqual(0, results.Errors.Count());
            Assert.AreEqual(0, results.RequiredModules.Count);
            Assert.AreEqual(1, results.Configurations.Count);
            Assert.AreEqual("CorpClientVMConfiguration", results.Configurations[0]);
        }

        [TestMethod]
        [TestCategory("Scenario")]
        [DeploymentItem(DomainControllerConfigurationPath)]
        public void TestExtractConfigurationNames2()
        {
            ConfigurationParseResult results = ConfigurationNameHelper.ExtractConfigurationNames(DomainControllerConfigurationPath);
            Assert.AreEqual(0, results.Errors.Count());
            Assert.AreEqual(0, results.RequiredModules.Count);
            Assert.AreEqual(1, results.Configurations.Count);
            Assert.AreEqual("DomainController", results.Configurations[0]);
        }

        [TestMethod]
        [TestCategory("Scenario")]
        [DeploymentItem(SHMulptiConfigurationsPath)]
        public void TestExtractConfigurationNamesMulti()
        {
            ConfigurationParseResult results = ConfigurationNameHelper.ExtractConfigurationNames(SHMulptiConfigurationsPath);
            Assert.AreEqual(0, results.Errors.Count());
            Assert.AreEqual(0, results.RequiredModules.Count);
            Assert.AreEqual(3, results.Configurations.Count);
            Assert.AreEqual("FileServerConfiguration", results.Configurations[0]);
            Assert.AreEqual("MgmtSrv", results.Configurations[1]);
            Assert.AreEqual("SHPullServerConfiguration", results.Configurations[2]);
        }
    }
}

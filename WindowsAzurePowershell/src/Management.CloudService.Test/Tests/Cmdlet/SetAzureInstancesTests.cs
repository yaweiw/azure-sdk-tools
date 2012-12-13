// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.CloudService.Cmdlet;
    using Microsoft.WindowsAzure.Management.CloudService.Model;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;

    [TestClass]
    public class SetAzureInstancesTests : TestBase
    {
        private const string serviceName = "AzureService";

        private FakeWriter writer;

        private SetAzureServiceProjectRoleCommand cmdlet;

        [TestInitialize]
        public void TestSetup()
        {
            writer = new FakeWriter();
            cmdlet = new SetAzureServiceProjectRoleCommand();
            cmdlet.Writer = writer;
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsNode()
        {
            int newRoleInstances = 10;

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Resources.NodeScaffolding);
                cmdlet.SetAzureInstancesProcess("WebRole1", newRoleInstances, service.Paths.RootPath);
                service = new AzureService(service.Paths.RootPath, null);

                Assert.AreEqual<int>(newRoleInstances, service.Components.CloudConfig.Role[0].Instances.count);
                Assert.AreEqual<int>(newRoleInstances, service.Components.LocalConfig.Role[0].Instances.count);
                Assert.AreEqual<string>(roleName, ((PSObject)writer.OutputChannel[0]).Members[Parameters.RoleName].Value.ToString());
            }
        }


        [TestMethod]
        public void SetAzureInstancesProcessTestsPHP()
        {
            int newRoleInstances = 10;

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                string roleName = "WebRole1";
                service.AddWebRole(Resources.PHPScaffolding);
                cmdlet.SetAzureInstancesProcess("WebRole1", newRoleInstances, service.Paths.RootPath);
                service = new AzureService(service.Paths.RootPath, null);

                Assert.AreEqual<int>(newRoleInstances, service.Components.CloudConfig.Role[0].Instances.count);
                Assert.AreEqual<int>(newRoleInstances, service.Components.LocalConfig.Role[0].Instances.count);
                Assert.AreEqual<string>(roleName, ((PSObject)writer.OutputChannel[0]).Members[Parameters.RoleName].Value.ToString());
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsRoleNameDoesNotExistFail()
        {
            string roleName = "WebRole1";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, roleName, 10), string.Format(Resources.RoleNotFoundMessage, roleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsNodeRoleNameDoesNotExistServiceContainsWebRoleFail()
        {
            string roleName = "WebRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                service.AddWebRole(Resources.NodeScaffolding, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, invalidRoleName, 10), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsPHPRoleNameDoesNotExistServiceContainsWebRoleFail()
        {
            string roleName = "WebRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                service.AddWebRole(Resources.PHPScaffolding, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, invalidRoleName, 10), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsNodeRoleNameDoesNotExistServiceContainsWorkerRoleFail()
        {
            string roleName = "WorkerRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                service.AddWorkerRole(Resources.NodeScaffolding, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, invalidRoleName, 10), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsPHPRoleNameDoesNotExistServiceContainsWorkerRoleFail()
        {
            string roleName = "WorkerRole1";
            string invalidRoleName = "foo";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                service.AddWorkerRole(Resources.PHPScaffolding, roleName, 1);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, invalidRoleName, 10), string.Format(Resources.RoleNotFoundMessage, invalidRoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsEmptyRoleNameFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, string.Empty, 10), string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.RoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsNullRoleNameFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, null, 10), string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.RoleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessTestsLargeRoleInstanceFail()
        {
            string roleName = "WebRole1";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, roleName, 2000), string.Format(Resources.InvalidInstancesCount, roleName));
            }
        }

        [TestMethod]
        public void SetAzureInstancesProcessNegativeRoleInstanceFail()
        {
            string roleName = "WebRole1";

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                Testing.AssertThrows<ArgumentException>(() => service.SetRoleInstances(service.Paths, roleName, -1), string.Format(Resources.InvalidInstancesCount, roleName));
            }
        }
    }
}
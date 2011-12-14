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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using AzureDeploymentCmdlets.Cmdlet;
using AzureDeploymentCmdlets.Model;
using AzureDeploymentCmdlets.Node.Cmdlet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureDeploymentCmdlets.Test
{
    /// <summary>
    /// Basic unit tests for the Enable-AzureRemoteDesktop command.
    /// </summary>
    [TestClass]
    public class DisableAzureRemoteDesktopCommandTest : TestBase
    {
        /// <summary>
        /// Invoke the Enable-AzureRemoteDesktop command.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        private static void EnableRemoteDesktop(string username, string password)
        {
            SecureString securePassword = null;
            if (password != null)
            {
                securePassword = new SecureString();
                foreach (char ch in password)
                {
                    securePassword.AppendChar(ch);
                }
                securePassword.MakeReadOnly();
            }

            EnableAzureRemoteDesktopCommand command = new EnableAzureRemoteDesktopCommand();
            command.Username = username;
            command.Password = securePassword;
            command.EnableRemoteDesktop();
        }

        private static void VerifyRoleSettings(AzureService service)
        {
            IEnumerable<ServiceConfigurationSchema.RoleSettings> settings =
                Enumerable.Concat(
                    service.Components.CloudConfig.Role,
                    service.Components.LocalConfig.Role);
            foreach (ServiceConfigurationSchema.RoleSettings roleSettings in settings)
            {
                Assert.AreEqual(
                    1,
                    roleSettings
                        .Certificates
                        .Where(c => c.name == "Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption")
                        .Count());
            }
        }

        /// <summary>
        /// Invoke the Disable-AzureRemoteDesktop command
        /// </summary>
        private static void DisableRemoteDesktop()
        {
            DisableAzureRemoteDesktopCommand command = new DisableAzureRemoteDesktopCommand();
            command.DisableRemoteDesktop();
        }

        private static void VerifyWebRole(ServiceDefinitionSchema.WebRole role, bool isForwarder)
        {
            Assert.AreEqual(isForwarder ? 1 : 0, role.Imports.Where(i => i.moduleName == "RemoteForwarder").Count());
            Assert.AreEqual(1, role.Imports.Where(i => i.moduleName == "RemoteAccess").Count());
        }

        private static void VerifyWorkerRole(ServiceDefinitionSchema.WorkerRole role, bool isForwarder)
        {
            Assert.AreEqual(isForwarder ? 1 : 0, role.Imports.Where(i => i.moduleName == "RemoteForwarder").Count());
            Assert.AreEqual(1, role.Imports.Where(i => i.moduleName == "RemoteAccess").Count());
        }

        private static void VerifyDisableRoleSettings(AzureService service)
        {
            IEnumerable<ServiceConfigurationSchema.RoleSettings> settings =
                Enumerable.Concat(
                    service.Components.CloudConfig.Role,
                    service.Components.LocalConfig.Role);
            foreach (ServiceConfigurationSchema.RoleSettings roleSettings in settings)
            {
                Assert.AreEqual(
                    1,
                    roleSettings.ConfigurationSettings
                        .Where(c => c.name == "Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled" && c.value == "false")
                        .Count());
            }
        }

        /// <summary>
        /// Enable remote desktop for an empty service.
        /// </summary>
        [TestMethod]
        public void DisableRemoteDesktopForEmptyService()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                files.CreateNewService("NEW_SERVICE");
                DisableRemoteDesktop();
            }
        }

        /// <summary>
        /// Disable remote desktop for a simple web role.
        /// </summary>
        [TestMethod]
        public void DisableRemoteDesktopForWebRole()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                string root = files.CreateNewService("NEW_SERVICE");
                new AddAzureNodeWebRoleCommand().AddAzureNodeWebRoleProcess("WebRole", 1, root);
                DisableRemoteDesktop();
            }
        }

        /// <summary>
        /// Disable remote desktop for web and worker roles.
        /// </summary>
        [TestMethod]
        public void DisableRemoteDesktopForWebAndWorkerRoles()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                string root = files.CreateNewService("NEW_SERVICE");
                new AddAzureNodeWebRoleCommand().AddAzureNodeWebRoleProcess("WebRole", 1, root);
                new AddAzureNodeWorkerRoleCommand().AddAzureNodeWorkerRoleProcess("WorkerRole", 1, root);
                DisableRemoteDesktop();
            }
        }

        /// <summary>
        /// Enable then disable remote desktop for a simple web role.
        /// </summary>
        [TestMethod]
        public void EnableDisableRemoteDesktopForWebRole()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                string root = files.CreateNewService("NEW_SERVICE");
                new AddAzureNodeWebRoleCommand().AddAzureNodeWebRoleProcess("WebRole", 1, root);
                EnableRemoteDesktop("user", "GoodPassword!");
                DisableRemoteDesktop();
                // Verify the role has been setup with forwarding, access,
                // and certs
                AzureService service = new AzureService(root, null);
                VerifyWebRole(service.Components.Definition.WebRole[0], true);
                VerifyDisableRoleSettings(service);
            }
        }

        /// <summary>
        /// Enable then disable remote desktop for web and worker roles.
        /// </summary>
        [TestMethod]
        public void EnableDisableRemoteDesktopForWebAndWorkerRoles()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                string root = files.CreateNewService("NEW_SERVICE");
                new AddAzureNodeWebRoleCommand().AddAzureNodeWebRoleProcess("WebRole", 1, root);
                new AddAzureNodeWorkerRoleCommand().AddAzureNodeWorkerRoleProcess("WorkerRole", 1, root);
                EnableRemoteDesktop("user", "GoodPassword!");
                DisableRemoteDesktop();
                // Verify the roles have been setup with forwarding, access,
                // and certs
                AzureService service = new AzureService(root, null);
                VerifyWebRole(service.Components.Definition.WebRole[0], false);
                VerifyWorkerRole(service.Components.Definition.WorkerRole[0], true);
                VerifyDisableRoleSettings(service);
            }
        }

        /// <summary>
        /// Enable then disable remote desktop for web and worker roles.
        /// </summary>
        [TestMethod]
        public void EnableDisableEnableRemoteDesktopForWebAndWorkerRoles()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                string root = files.CreateNewService("NEW_SERVICE");
                new AddAzureNodeWebRoleCommand().AddAzureNodeWebRoleProcess("WebRole", 1, root);
                new AddAzureNodeWorkerRoleCommand().AddAzureNodeWorkerRoleProcess("WorkerRole", 1, root);
                EnableRemoteDesktop("user", "GoodPassword!");
                DisableRemoteDesktop();
                EnableRemoteDesktop("user", "GoodPassword!");
                // Verify the roles have been setup with forwarding, access,
                // and certs
                AzureService service = new AzureService(root, null);
                VerifyWebRole(service.Components.Definition.WebRole[0], false);
                VerifyWorkerRole(service.Components.Definition.WorkerRole[0], true);
                VerifyRoleSettings(service);
            }
        }    
    }
}

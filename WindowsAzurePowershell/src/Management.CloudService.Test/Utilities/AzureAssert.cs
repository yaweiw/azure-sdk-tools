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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Utilities
{
    using System;
    using System.IO;
    using CloudService.Model;
    using CloudService.Properties;
    using Scaffolding;
    using ServiceConfigurationSchema;
    using ServiceDefinitionSchema;
    using TestData;
    using VisualStudio.TestTools.UnitTesting;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;
    using DefConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema.ConfigurationSetting;

    internal static class AzureAssert
    {
        public static void AreEqualServiceSettings(ServiceSettings expected, ServiceSettings actual)
        {
            AreEqualServiceSettings(expected.Location, expected.Slot, expected.StorageAccountName, expected.Subscription, actual);
        }

        public static void AreEqualServiceSettings(string location, string slot, string storageAccountName, string subscriptionName, ServiceSettings actual)
        {
            Assert.AreEqual<string>(location, actual.Location);
            Assert.AreEqual<string>(slot, actual.Slot);
            Assert.AreEqual<string>(storageAccountName, actual.StorageAccountName);
            Assert.AreEqual<string>(subscriptionName, actual.Subscription);
        }

        public static void AreEqualDeploymentSettings(DeploymentSettings expected, DeploymentSettings actual)
        {
            AreEqualDeploymentSettings(expected.ServiceSettings, expected.ConfigPath, expected.DeploymentName, expected.Label, expected.PackagePath, expected.SubscriptionId, actual);
        }

        public static void AreEqualDeploymentSettings(ServiceSettings settings, string configPath, string deploymentName, string label, string packagePath, string subscriptionId, DeploymentSettings actual)
        {
            AreEqualServiceSettings(settings, actual.ServiceSettings);
            Assert.AreEqual<string>(configPath, actual.ConfigPath);
            Assert.AreEqual<string>(deploymentName, actual.DeploymentName);
            Assert.AreEqual<string>(label, actual.Label);
            Assert.AreEqual<string>(packagePath, actual.PackagePath);
            Assert.AreEqual<string>(subscriptionId, actual.SubscriptionId);

            Assert.IsTrue(File.Exists(actual.ConfigPath));
            Assert.IsTrue(File.Exists(actual.PackagePath));
        }

        public static void AreEqualServicePathInfo(ServicePathInfo expected, ServicePathInfo actual)
        {
            AreEqualServicePathInfo(expected.CloudConfiguration, expected.CloudPackage, expected.Definition, expected.LocalConfiguration,
                expected.LocalPackage, expected.RootPath, expected.Settings, actual);
        }

        public static void AreEqualServicePathInfo(string cloudConfig, string cloudPackage, string def, string localConfig, string localPackage, string root, string settings, ServicePathInfo actual)
        {
            throw new NotImplementedException();
        }

        public static void AreEqualServicePathInfo(string rootPath, ServicePathInfo actual)
        {
            Assert.AreEqual<string>(rootPath, actual.RootPath);
            Assert.AreEqual<string>(Path.Combine(rootPath, Resources.ServiceDefinitionFileName), actual.Definition);
            Assert.AreEqual<string>(Path.Combine(rootPath, Resources.CloudServiceConfigurationFileName), actual.CloudConfiguration);
            Assert.AreEqual<string>(Path.Combine(rootPath, Resources.LocalServiceConfigurationFileName), actual.LocalConfiguration);
            Assert.AreEqual<string>(Path.Combine(rootPath, Resources.SettingsFileName), actual.Settings);
            Assert.AreEqual<string>(Path.Combine(rootPath, Resources.CloudPackageFileName), actual.CloudPackage);
            Assert.AreEqual<string>(Path.Combine(rootPath, Resources.LocalPackageFileName), actual.LocalPackage);
        }

        public static void AreEqualServiceComponents(ServiceComponents actual)
        {
            Assert.IsNotNull(actual.CloudConfig);
            Assert.IsNotNull(actual.Definition);
            Assert.IsNotNull(actual.LocalConfig);
            Assert.IsNotNull(actual.Settings);
        }

        public static void ScaffoldingExists(string destinationDirectory, string scaffoldFilePath, string roleName = "WebRole")
        {
            Scaffold scaffold = Scaffold.Parse(Path.Combine(Data.TestResultDirectory, scaffoldFilePath, Resources.ScaffoldXml));

            foreach (ScaffoldFile file in scaffold.Files)
            {
                if (file.Copy)
                {

                    string elementPath;
                    if (string.IsNullOrEmpty(file.PathExpression))
                    {
                        elementPath = string.IsNullOrEmpty(file.TargetPath) ? Path.Combine(destinationDirectory, file.Path) : Path.Combine(destinationDirectory, file.TargetPath);
                        elementPath = elementPath.Replace("$RoleName$", roleName);
                        Assert.IsTrue(File.Exists(elementPath));
                    }
                    else
                    {
                        string substring = file.PathExpression.Substring(0, file.PathExpression.LastIndexOf('\\'));
                        elementPath = string.IsNullOrEmpty(file.TargetPath) ? Path.Combine(destinationDirectory, substring) : Path.Combine(destinationDirectory, file.TargetPath);
                        elementPath = elementPath.Replace("$RoleName$", roleName);
                        Assert.IsTrue(Directory.Exists(elementPath));
                    }
                }
            }
        }

        public static void AzureServiceExists(string serviceRootPath, string scaffoldFilePath, string serviceName, ServiceSettings settings = null, WebRoleInfo[] webRoles = null, WorkerRoleInfo[] workerRoles = null, string webScaff = null, string workerScaff = null, RoleInfo[] roles = null)
        {
            ServiceComponents components = new Microsoft.WindowsAzure.Management.CloudService.Model.ServiceComponents(new Microsoft.WindowsAzure.Management.CloudService.Model.ServicePathInfo(serviceRootPath));

            ScaffoldingExists(serviceRootPath, scaffoldFilePath);

            if (webRoles != null)
            {
                for (int i = 0; i < webRoles.Length; i++)
                {
                    ScaffoldingExists(Path.Combine(serviceRootPath, webRoles[i].Name), webScaff);
                }
            }

            if (workerRoles != null)
            {
                for (int i = 0; i < workerRoles.Length; i++)
                {
                    ScaffoldingExists(Path.Combine(serviceRootPath, workerRoles[i].Name), workerScaff);
                }
            }

            AreEqualServiceConfiguration(components.LocalConfig, serviceName, roles);
            AreEqualServiceConfiguration(components.CloudConfig, serviceName, roles);
            IsValidServiceDefinition(components.Definition, serviceName, webRoles, workerRoles);
            AreEqualServiceSettings(settings ?? new ServiceSettings(), components.Settings);
        }

        /// <summary>
        /// Validates that given service definition is valid for a service. Validation steps:
        /// 1. Validates name element matches serviceName
        /// 2. Validates web role element has all webRoles with same configuration.
        /// 3. Validates worker role element has all workerRoles with same configuration.
        /// </summary>
        /// <param name="actual">Service definition to be checked</param>
        /// <param name="serviceName">New created service name</param>
        public static void IsValidServiceDefinition(ServiceDefinition actual, string serviceName, WebRoleInfo[] webRoles = null, WorkerRoleInfo[] workerRoles = null)
        {
            Assert.AreEqual<string>(serviceName, actual.name);

            if (webRoles != null)
            {
                Assert.AreEqual<int>(webRoles.Length, actual.WebRole.Length);
                int length = webRoles.Length;

                for (int i = 0; i < length; i++)
                {
                    Assert.IsTrue(webRoles[i].Equals(actual.WebRole[i]));
                }
            }
            else
            {
                Assert.IsNull(actual.WebRole);
            }

            if (workerRoles != null)
            {
                Assert.AreEqual<int>(workerRoles.Length, actual.WorkerRole.Length);
                int length = workerRoles.Length;

                for (int i = 0; i < length; i++)
                {
                    Assert.IsTrue(workerRoles[i].Equals(actual.WorkerRole[i]));
                }
            }
            else
            {
                Assert.IsNull(actual.WorkerRole);
            }
        }

        /// <summary>
        /// Validates that given service definition is valid against list of web/worker roles. Validation steps:
        /// 1. Make sure that name element 
        /// </summary>
        /// <param name="actual">Service definition to be checked</param>
        /// <param name="serviceName">New created service name</param>
        public static void IsValidServiceDefinition(ServiceDefinitionSchema.ServiceDefinition actual, string serviceName)
        {
            Assert.AreEqual<string>(serviceName, actual.name);
            Assert.IsNull(actual.WebRole);
            Assert.IsNull(actual.WorkerRole);
        }

        public static void AreEqualServiceDefinition(ServiceDefinitionSchema.ServiceDefinition expected, ServiceDefinitionSchema.ServiceDefinition actual)
        {
            throw new NotImplementedException();
        }

        public static void AreEqualServiceConfiguration(ServiceConfiguration expected, ServiceConfiguration actual)
        {
            throw new NotImplementedException();
        }

        public static void AreEqualServiceConfiguration(ServiceConfiguration actual, string serviceName, RoleInfo[] roles = null)
        {
            Assert.AreEqual<string>(actual.serviceName, serviceName);

            if (roles != null)
            {
                Assert.AreEqual<int>(actual.Role.Length, roles.Length);
                int length = roles.Length;

                for (int i = 0; i < length; i++)
                {
                    Assert.IsTrue(roles[i].Equals(actual.Role[i]));
                }
            }
        }

        public static void WorkerRoleImportsExists(Import expected, WorkerRole actual)
        {
            Assert.IsTrue(Array.Exists<Import>(actual.Imports, i => i.moduleName.Equals(expected.moduleName)));
        }

        public static void LocalResourcesLocalStoreExists(LocalStore expected, LocalResources actual)
        {
            Assert.IsTrue(Array.Exists<LocalStore>(actual.LocalStorage, l => l.name.Equals(expected.name) && 
                l.cleanOnRoleRecycle.Equals(expected.cleanOnRoleRecycle) && l.sizeInMB.Equals(expected.sizeInMB)));
        }

        public static void ConfigurationSettingExist(DefConfigurationSetting expected, DefConfigurationSetting[] actual)
        {
            Assert.IsTrue(Array.Exists<DefConfigurationSetting>(actual, c => c.name == expected.name));
        }

        public static void ConfigurationSettingExist(ConfigConfigurationSetting expected, ConfigConfigurationSetting[] actual)
        {
            Assert.IsTrue(Array.Exists<ConfigConfigurationSetting>(actual, c => c.name == expected.name));
        }

        public static void RuntimeExists(Task[] tasks, string runtimeValue)
        {
            Assert.IsTrue(Array.Exists<Task>(tasks, t => Array.Exists<Variable>(t.Environment,
                e => e.value != null && e.value.Contains(runtimeValue))));
            Assert.IsTrue(Array.Exists<Task>(tasks, t => Array.Exists<Variable>(t.Environment,
                e => e.value != null && e.value.Contains(string.Format("http://nodertncu.blob.core.windows.net/{0}/", runtimeValue)))));
        }

        public static void StartupTaskExists(Task[] tasks, string startupCommand)
        {
            Assert.IsTrue(Array.Exists<Task>(tasks, t => t.commandLine == startupCommand));
        }

        public static void InternalEndpointExists(InternalEndpoint[] internalEndpoints, InternalEndpoint internalEndpoint)
        {
            Assert.IsTrue(Array.Exists<InternalEndpoint>(internalEndpoints, i => i.name == internalEndpoint.name && 
                i.port == internalEndpoint.port && i.protocol == internalEndpoint.protocol));
        }
    }
}
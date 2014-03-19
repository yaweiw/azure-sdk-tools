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

namespace Microsoft.WindowsAzure.Commands.Profile
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Profile;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Microsoft.PowerShell.Commands;
    using System.Collections.Generic;
    using System.Management.Automation.Runspaces;
    using System.Linq;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Switches between ServiceManagement and ResourceManagement modules.
    /// </summary>
    [Cmdlet(VerbsCommon.Switch, "AzureModule")]
    public class SwitchAzureAccount : CmdletBase
    {
        private const string ProfileModuleName = "AzureProfile";
        
        private const string ServiceManagementModuleName = "AzureServiceManagement";
        
        private const string ResourceManagementModuleName = "AzureResourceManagement";

        private string resourceManagementModulePath;

        private string serviceManagementModulePath;

        private string profileModulePath;

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Name of the module to switch to. Valid values are AzureServiceManagement and AzureResourceManagement")]
        public AzureModule Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, HelpMessage = "If specified, save the module switch at machine level")]
        public SwitchParameter Global { get; set; }

        public SwitchAzureAccount()
        {
            string rootInstallationPath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
            serviceManagementModulePath = Path.Combine(rootInstallationPath, ServiceManagementModuleName);
            resourceManagementModulePath = Path.Combine(rootInstallationPath, ResourceManagementModuleName);
            profileModulePath = Path.Combine(serviceManagementModulePath, ProfileModuleName);
        }

        public override void ExecuteCmdlet()
        {
            List<PSModuleInfo> modules = this.GetModules();
            bool serviceManagementModuleLoaded = modules.Exists(m => m.Name.Equals(ServiceManagementModuleName));
            bool resourceManagementModuleLoaded = modules.Exists(m => m.Name.Equals(ResourceManagementModuleName));

            if (serviceManagementModuleLoaded && resourceManagementModuleLoaded)
            {
                string warningMessage = string.Format(
                    "{0} module and {1} module are loaded in the current session please consider removing one of them.",
                    ServiceManagementModuleName,
                    ResourceManagementModuleName);
            }
            else if (serviceManagementModuleLoaded && Name == AzureModule.AzureResourceManagement)
            {
                RemoveAzureModule(ServiceManagementModuleName, serviceManagementModulePath);

                if (!resourceManagementModuleLoaded)
                {
                    ImportAzureModule(ResourceManagementModuleName, resourceManagementModulePath);
                    ImportAzureModule(ProfileModuleName, profileModulePath);
                }
            }
            else if (resourceManagementModuleLoaded && Name == AzureModule.AzureServiceManagement)
            {
                RemoveAzureModule(ResourceManagementModuleName, resourceManagementModulePath);
                RemoveAzureModule(ProfileModuleName, profileModulePath);

                if (!serviceManagementModuleLoaded)
                {
                    ImportAzureModule(ServiceManagementModuleName, serviceManagementModulePath);
                }
            }
        }

        private void ImportAzureModule(string name, string path)
        {
            WriteVerbose(string.Format("Importing {0} module...", name));
            this.ImportModule(Path.Combine(path, name + ".psd1"));

            WriteVerbose(string.Format("Adding {0} module path to PSModulePath...", path));
            PowerShellUtilities.AddModuleToPSModulePath(path);

            if (Global)
            {
                PowerShellUtilities.AddModuleToPSModulePath(path, EnvironmentVariableTarget.Machine);
            }
        }

        private void RemoveAzureModule(string name, string path)
        {
            WriteVerbose(string.Format("Removing {0} module...", name));
            this.RemoveModule(name);

            WriteVerbose(string.Format("Removing {0} module path from PSModulePath...", path));
            PowerShellUtilities.RemoveModuleFromPSModulePath(path);

            if (Global)
            {
                PowerShellUtilities.RemoveModuleFromPSModulePath(path, EnvironmentVariableTarget.Machine);
            }
        }
    }
}
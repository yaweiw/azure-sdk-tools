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
        private const string ProfileManagementModuleName = "Microsoft.Azure.Commands.Profile";
        
        private const string ServiceManagementModuleName = "Microsoft.Azure.Commands.ServiceManagement";
        
        private const string ResourceManagementModuleName = "Microsoft.Azure.Commands.ResourceManagement";

        private string resourceManagementModulePath;

        private string serviceManagementModulePath;

        private string profileModulePath;

        public SwitchAzureAccount()
        {
            string rootInstallationPath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
            serviceManagementModulePath = Path.Combine(rootInstallationPath, ServiceManagementModuleName);
            resourceManagementModulePath = Path.Combine(rootInstallationPath, ResourceManagementModuleName);
            profileModulePath = Path.Combine(rootInstallationPath, ProfileManagementModuleName);
        }

        public override void ExecuteCmdlet()
        {
            List<PSModuleInfo> modules = this.ExecuteScript<PSModuleInfo>("Get-Module");
            bool serviceManagementModuleLoaded = modules.Exists(m => m.Name.Equals(ServiceManagementModuleName));
            bool resourceManagementModuleLoaded = modules.Exists(m => m.Name.Equals(ResourceManagementModuleName));
            bool profileModuleLoaded = modules.Exists(m => m.Name.Equals(ProfileManagementModuleName));

            if (serviceManagementModuleLoaded && resourceManagementModuleLoaded)
            {
                string warningMessage = string.Format(
                    "{0} module and {1} module are loaded in the current session please consider removing one of them.",
                    ServiceManagementModuleName,
                    ResourceManagementModuleName);
            }
            else if (serviceManagementModuleLoaded)
            {
                SwitchModules(ServiceManagementModuleName,
                    ResourceManagementModuleName,
                    serviceManagementModulePath,
                    resourceManagementModulePath);

                this.ImportModule(profileModulePath);
            }
            else if (resourceManagementModuleLoaded)
            {
                this.RemoveModule(ProfileManagementModuleName);

                SwitchModules(ResourceManagementModuleName,
                    ServiceManagementModuleName,
                    resourceManagementModulePath,
                    serviceManagementModulePath);
            }
            else
            {
                WriteVerbose("There are no loaded Azure modules to switch");
            }
        }

        private void SwitchModules(string oldName, string newName, string oldPath, string newPath)
        {
            WriteVerbose(string.Format("Removing {0} module...", oldName));
            this.RemoveModule(oldName);

            WriteVerbose(string.Format("Removing {0} module path from PSModulePath...", oldPath));
            PowerShellUtilities.RemoveModuleFromPSModulePath(oldPath);

            WriteVerbose(string.Format("Importing {0} module...", newName));
            this.ImportModule(newPath);

            WriteVerbose(string.Format("Adding {0} module path to PSModulePath...", newPath));
            PowerShellUtilities.AddModuleToPSModulePath(newPath);
        }
    }
}
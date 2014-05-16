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
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using System;
    using System.IO;
    using System.Management.Automation;

    /// <summary>
    /// Switches between ServiceManagement and ResourceManager modes.
    /// </summary>
    [Cmdlet(VerbsCommon.Switch, "AzureMode")]
    public class SwitchAzureMode : CmdletBase
    {
        private const string ServiceManagementModuleName = "Azure";
        
        private const string ResourceManagerModuleName = "AzureResourceManager";

        private const string ServiceManagementFolderName = "ServiceManagement";

        private const string ResourceManagerFolderName = "ResourceManager";

        private string ResourceManagerModulePath;

        private string serviceManagementModulePath;

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Name of the mode to switch to. Valid values are AzureServiceManagement and AzureResourceManager")]
        public AzureModule Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, HelpMessage = "If specified, save the module switch at machine level")]
        public SwitchParameter Global { get; set; }

        public SwitchAzureMode()
        {
            string rootInstallationPath = Directory.GetParent(Directory.GetParent(FileUtilities.GetAssemblyDirectory()).FullName).FullName;
            serviceManagementModulePath = Path.Combine(rootInstallationPath, ServiceManagementFolderName);
            ResourceManagerModulePath = Path.Combine(rootInstallationPath, ResourceManagerFolderName);
        }

        public override void ExecuteCmdlet()
        {
            if (Name == AzureModule.AzureResourceManager)
            {
                RemoveAzureModule(ServiceManagementModuleName, serviceManagementModulePath);
                ImportAzureModule(ResourceManagerModuleName, ResourceManagerModulePath);
            }
            else if (Name == AzureModule.AzureServiceManagement)
            {
                RemoveAzureModule(ResourceManagerModuleName, ResourceManagerModulePath);
                ImportAzureModule(ServiceManagementModuleName, serviceManagementModulePath);
            }
        }

        private void ImportAzureModule(string name, string path)
        {
            WriteVerbose(string.Format("Adding {0} module path to PSModulePath...", path));
            PowerShellUtilities.AddModuleToPSModulePath(path);

            if (!IsLoaded(name))
            {
                WriteVerbose(string.Format("Importing {0} module...", name));
                this.ImportModule(name);

                if (name.Equals(ResourceManagerModuleName))
                {
                    this.RemoveAzureAliases();
                }
            }

            if (Global)
            {
                PowerShellUtilities.AddModuleToPSModulePath(path, EnvironmentVariableTarget.Machine);
            }
        }

        private bool IsLoaded(string moduleName)
        {
            return this.GetLoadedModules().Exists(m => m.Name.Equals(moduleName));
        }

        private void RemoveAzureModule(string name, string path)
        {
            if (IsLoaded(name))
            {
                WriteVerbose(string.Format("Removing {0} module...", name));
                this.RemoveModule(name);

                if (name.Equals(ServiceManagementModuleName))
                {
                    this.RemoveAzureAliases();
                }
            }

            WriteVerbose(string.Format("Removing {0} module path from PSModulePath...", path));
            PowerShellUtilities.RemoveModuleFromPSModulePath(path);

            if (Global)
            {
                PowerShellUtilities.RemoveModuleFromPSModulePath(path, EnvironmentVariableTarget.Machine);
            }
        }
    }
}
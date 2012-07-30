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

namespace Microsoft.WindowsAzure.Management.CloudService.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Net;
    using System.Xml;
    using System.Diagnostics;
    using System.Web.Script.Serialization;
    using System.Reflection;

    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema;

    public abstract class CloudRuntime
    {
        public string Version
        {
            get;
            protected set;
        }

        public Runtime Runtime
        {
            get;
            protected set;
        }

        public string FilePath
        {
            get;
            set;
        }

        public string RoleName
        {
            get;
            set;
        }

        public bool ShouldPrompt
        {
            get;
            private set;
        }

        protected CloudRuntime()
        {
        }

        private static CloudRuntime CreateRuntimeInternal(Runtime runtimeType, string roleName)
        {
            CloudRuntime runtime;
            switch(runtimeType)
            {
                case Runtime.Null:
                    runtime = new NullCloudRuntime();
                    break;
                case Runtime.PHP:
                    runtime = new PHPCloudRuntime();
                    break;
                case Runtime.IISNode:
                    runtime = new NullCloudRuntime();
                    break;
                case Runtime.Node:
                default:
                    runtime = new NodeCloudRuntime();
                    break;
            }

            runtime.RoleName = roleName;
            return runtime;
        }

        private static Collection<CloudRuntime> GetRuntimes(Dictionary<string, string> settings, string roleName)
        {
            Collection<CloudRuntime> runtimes = new Collection<CloudRuntime>(new List<CloudRuntime>());
            foreach (Runtime runtimeType in GetRuntimeTypes(settings))
            {
                CloudRuntime runtime = CreateRuntimeInternal(runtimeType, roleName);
                runtime.ConfigureRuntimeOverrides(runtime, settings);
                runtime.Configure();
                runtimes.Add(runtime);
            }

            return runtimes;
        }

        public static Collection<CloudRuntime> CreateRuntime(WebRole webRole)
        {
            return GetRuntimes(GetStartupEnvironment(webRole), webRole.name);
        }

        public static Collection<CloudRuntime> CreateRuntime(WorkerRole workerRole)
        {
            return GetRuntimes(GetStartupEnvironment(workerRole), workerRole.name);
        }

        protected void ConfigureRuntimeOverrides(CloudRuntime runtime, Dictionary<string, string> environmentSettings)
        {
        }

        private static Collection<Runtime> GetRuntimeTypes(Dictionary<string, string> environmentSettings)
        {
            Collection<Runtime> runtimes = new Collection<Runtime>(new List<Runtime>());
            Runtime runtimeType = Runtime.Null;
            if (environmentSettings.ContainsKey(Resources.RuntimeOverrideKey) && !string.IsNullOrEmpty(environmentSettings[Resources.RuntimeOverrideKey]))
            {
                runtimes.Add(Runtime.Null);
            }
            else if (environmentSettings.ContainsKey(Resources.RuntimeTypeKey) && !string.IsNullOrEmpty(environmentSettings[Resources.RuntimeTypeKey]))
            {
                foreach (string runtimeSpec in environmentSettings[Resources.RuntimeTypeKey].Split(';'))
                {
                    if (string.Equals(runtimeSpec, Resources.NodeRuntimeValue, StringComparison.OrdinalIgnoreCase))
                    {
                        runtimes.Add(Runtime.Node);
                    }
                    else if (string.Equals(runtimeSpec, Resources.IISNodeRuntimeValue, StringComparison.OrdinalIgnoreCase))
                    {
                        runtimes.Add(Runtime.IISNode);
                    }
                    else if (string.Equals(runtimeSpec, Resources.PhpRuntimeValue, StringComparison.OrdinalIgnoreCase))
                    {
                        runtimes.Add(Runtime.PHP);
                    }
                }
                runtimeType = Runtime.Node;
            }

            return runtimes;
        }

        private static Dictionary<string, string> GetStartupEnvironment(WebRole webRole)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            foreach (Variable variable in webRole.Startup.Task[0].Environment) 
            {
                settings[variable.name] = variable.value;
            }

            return settings;
        }

        private static Dictionary<string, string> GetStartupEnvironment(WorkerRole workerRole)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            foreach (Variable variable in workerRole.Startup.Task[0].Environment)
            {
                settings[variable.name] = variable.value;
            }

            return settings;
        }

        /// <summary>
        /// Determine if this is a variable that should be set directly or should be merged with the new value
        /// </summary>
        /// <param name="variableToCheck"></param>
        /// <returns>True if the variable should be merged, false if the value should be replaced instead</returns>
        private static bool ShouldMerge(Variable variableToCheck)
        {
            return string.Equals(variableToCheck.name, Resources.RuntimeTypeKey, 
                StringComparison.OrdinalIgnoreCase) || string.Equals(variableToCheck.name, 
                Resources.RuntimeUrlKey, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Set the variable value given a new value. Replace or add the value to the variable, as appropriate
        /// </summary>
        /// <param name="inVariable">The variable to change</param>
        /// <param name="addedValue">The new value to </param>
        private static void SetVariableValue(Variable inVariable, string addedValue)
        {
            if (!string.IsNullOrEmpty(inVariable.value) && ShouldMerge(inVariable))
            {
                if (inVariable.value.IndexOf(addedValue, 0, inVariable.value.Length, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    inVariable.value = string.Concat(inVariable.value, ";", addedValue);
                }
            }
            else
            {
                inVariable.value = addedValue;
            }
        }

        private static Variable[] ApplySettingChanges(Dictionary<string, string> settings, Variable[] roleVariables)
        {
            int roleVariableCount = (roleVariables == null ? 0 : roleVariables.Length);
            if (roleVariableCount > 0)
            {
                for (int j = 0; j < roleVariables.Length; ++j)
                {
                    if (settings.ContainsKey(roleVariables[j].name))
                    {
                        SetVariableValue(roleVariables[j], settings[roleVariables[j].name]);
                        settings.Remove(roleVariables[j].name);
                    }
                }
            }

            int settingsCount = (settings == null ? 0 : settings.Count);
            Variable[] newSettings = new Variable[settingsCount + roleVariableCount];
            int i = 0;
            foreach (string key in settings.Keys) 
            {
                newSettings[i] = new Variable {name = key, value = settings[key]};
                ++i;
            }

            for (int j = 0; j < roleVariables.Length ; ++j) 
            {
                newSettings[i+j] = roleVariables[j];
            }

            return newSettings;
        }

        private static void ApplyRoleXmlChanges(Dictionary<string, string> changes, WebRole webRole)
        {
            webRole.Startup.Task[0].Environment = ApplySettingChanges(changes, webRole.Startup.Task[0].Environment);
        }

        private static void ApplyRoleXmlChanges(Dictionary<string, string> changes, WorkerRole workerRole)
        {
            workerRole.Startup.Task[0].Environment = ApplySettingChanges(changes, workerRole.Startup.Task[0].Environment);
        }

        public virtual void ApplyRuntime(CloudRuntimePackage package, AzureService service, WebRole webRole)
        {

            Dictionary<string, string> changes;
            if( this.GetChanges(package, out changes)) {
                ApplyRoleXmlChanges(changes, webRole);
            }

            ApplyScaffoldingChanges(package, service, webRole.name);
        }

        public virtual void ApplyRuntime(CloudRuntimePackage package, AzureService service, WorkerRole workerRole)
        {
            Dictionary<string, string> changes;
            if (this.GetChanges(package, out changes))
            {
                ApplyRoleXmlChanges(changes, workerRole);
            }

            ApplyScaffoldingChanges(package, service, workerRole.name);
        }

        protected abstract void ApplyScaffoldingChanges(CloudRuntimePackage package, AzureService service, string roleName);

        public abstract bool Match(CloudRuntimePackage runtime);

        public virtual bool ValidateMatch(CloudRuntimePackage runtime, out string warningText)
        {
            warningText = null;
            bool result = this.Match(runtime);
            if (!result)
            {
                warningText = this.GenerateWarningText(runtime);
            }

            return result;
        }

        protected abstract void Configure();
        protected abstract string GenerateWarningText(CloudRuntimePackage package);
        protected virtual bool GetChanges(CloudRuntimePackage package, out Dictionary<string, string> changes)
        {
            changes = new Dictionary<string, string>();
            changes[Resources.RuntimeTypeKey] = package.Runtime.ToString().ToLower();
            changes[Resources.RuntimeUrlKey] = package.PackageUri.ToString();
            return true;
        }

        private class IISNodeCloudRuntime : JavaScriptCloudRuntime
        {
            protected override void Configure()
            {
                this.Runtime = Runtime.IISNode;
                if (string.IsNullOrEmpty(this.Version))
                {
                    this.Version = GetIISNodeVersion();
                }
            }

            public override bool Match(CloudRuntimePackage runtime)
            {
                // here is where we would put in semver semantics
                return string.Equals(this.Version, runtime.Version, StringComparison.OrdinalIgnoreCase);
            }

            private string GetIISNodeVersion()
            {
                string fileVersion = Resources.DefaultFileVersion;
                string iisnodePath = Path.Combine(GetProgramFilesDirectoryPathx86(), Path.Combine(Resources.IISNodePath, Resources.IISNodeDll));
                if (File.Exists(iisnodePath))
                {
                    fileVersion = GetFileVersion(iisnodePath);
                }

                return fileVersion;
            }

            protected override string GenerateWarningText(CloudRuntimePackage package)
            {
                return string.Format(Resources.IISNodeVersionWarningText, package.Version,
                        this.RoleName, this.Version);
            }

            protected override void ApplyScaffoldingChanges(CloudRuntimePackage package, AzureService service, string roleName)
            {
                ApplyChangesToPackageJson(package, service, roleName, Resources.IISNodeRuntimeValue);
            }
        }

        abstract class JavaScriptCloudRuntime : CloudRuntime
        {

            public override bool Match(CloudRuntimePackage runtime)
            {
                // here is where we would put in semver semantics
                return string.Equals(this.Version, runtime.Version, StringComparison.OrdinalIgnoreCase);
            }

            protected static string GetFileVersion(string filePath)
            {
                return FileVersionInfo.GetVersionInfo(filePath).ProductVersion;
            }

            protected static string GetProgramFilesDirectoryPathx86()
            {
                string returnPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                if (string.IsNullOrEmpty(returnPath))
                {
                    returnPath = GetProgramFilesDirectory();
                }

                return returnPath;
            }

            protected static string GetProgramFilesDirectory()
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }

            protected void ApplyChangesToPackageJson(CloudRuntimePackage package, AzureService service, string roleName, string engineKey)
            {
                string packageJsonPath = GetPackageJsonPath(service, roleName);
                StreamReader reader = new StreamReader(packageJsonPath);
                string jsonText = reader.ReadToEnd();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
            }

            private string GetPackageJsonPath(AzureService service, string roleName)
            {
                throw new NotImplementedException();
            }
        }

        private class NodeCloudRuntime : JavaScriptCloudRuntime
        {
            protected override void Configure()
            {
                this.Runtime = Runtime.Node;
                if (string.IsNullOrEmpty(this.Version))
                {
                    this.Version = GetNodeVersion();
                }
            }

            protected override string GenerateWarningText(CloudRuntimePackage package)
            {
                return string.Format(Resources.NodeVersionWarningText, package.Version,
                        this.RoleName, this.Version);
            }

            private string GetNodeVersion()
            {
                string fileVersion = Resources.DefaultFileVersion;
                string nodePath = Path.Combine(GetProgramFilesDirectoryPathx86(), Path.Combine(Resources.NodeDirectory, Resources.NodeExe));
                if (!File.Exists(nodePath))
                {
                    nodePath = Path.Combine(GetProgramFilesDirectory(), Path.Combine(Resources.NodeDirectory, Resources.NodeExe));
                }
                if (File.Exists(nodePath)) 
                {
                    fileVersion = GetFileVersion(nodePath);
                }

                return fileVersion;
            }

            protected override void ApplyScaffoldingChanges(CloudRuntimePackage package, AzureService service, string roleName)
            {
                ApplyChangesToPackageJson(package, service, roleName, Resources.NodeRuntimeValue);
            }
        }

        private class PHPCloudRuntime : CloudRuntime
        {
            protected override void Configure()
            {
                this.Runtime = Runtime.PHP;
                if (string.IsNullOrEmpty(this.Version))
                {
                    this.Version = Resources.PHPRuntimeVersion;
                }
            }

            public override bool Match(CloudRuntimePackage runtime)
            {
                return this.Version.Equals(runtime.Version, StringComparison.OrdinalIgnoreCase);
            }

            protected override string GenerateWarningText(CloudRuntimePackage package)
            {
                return string.Format(Resources.PHPVersionWarningText, package.Version, this.RoleName, 
                    this.Version);
            }
        }

        private class NullCloudRuntime : CloudRuntime
        {
            public override bool Match(CloudRuntimePackage runtime)
            {
                return true;
            }

            protected override void Configure()
            {
            }

            protected override string GenerateWarningText(CloudRuntimePackage package)
            {
                return null;
            }

            protected override bool GetChanges(CloudRuntimePackage package, out Dictionary<string, string> changes)
            {
                changes = null;
                return false;
            }
        }
    }
}

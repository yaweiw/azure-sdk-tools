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

namespace AzureDeploymentCmdlets.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Net;
    using System.Xml;
    using System.Diagnostics;
using AzureDeploymentCmdlets.ServiceDefinitionSchema;

    public abstract class CloudRuntime
    {
        public const string PHPRuntimeVersion = "1.5";
        public const string PHPRuntimeValue = "php";
        public const string RuntimeTypeKey = "RUNTIMEID";
        public const string RuntimePrimaryVersionKey = "RUNTIMEVERSIONPRIMARYKEY";
        public const string RuntimeSecondaryVersionKey = "RUNTIMEVERSIONSECONDARYKEY";
        public const string RuntimeOverrideKey = "RUNTIMEURLOVERRIDE";
        public const string RuntimeUrlKey = "RUNTIMEURL";
        public const string NodePath = "nodejs";
        public const string NodeExe = "node.exe";
        public const string IISNodePath = "iisnode";
        public const string IISNodeDLL = "iisnode.dll";
        public const string DefaultFileVersion = "none";

        public const string NodeWebRoleVersionWarningText = "Installing Node version {0} and IISNode version {1} in Azure for WebRole '{2}' (the versions locally installed are: Node {3}, IISNode: {4})";
        public const string NodeWorkerRoleVersionWarningText = "Installing Node version {0} in Azure for WorkerRole '{1}' (the Node version locally installed is: {2})";
        public const string PHPVersionWarningText = "Installing PHP version {0} for {1}Role '{2}' (the PHP version locally installed is: {3})";

        public string PrimaryVersionKey
        {
            get;
            protected set;
        }

        public string SecondaryVersionKey
        {
            get;
            protected set;
        }

        public RoleType Role
        {
            get;
            private set;
        }

        public string RoleName
        {
            get;
            private set;
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

        public bool ShouldPrompt
        {
            get;
            private set;
        }

        protected CloudRuntime()
        {
        }

        private static CloudRuntime CreateRuntimeInternal(Runtime runtimeType, RoleType roleType, string roleName)
        {
            CloudRuntime runtime;
            if (runtimeType == Runtime.Override)
            {
                runtime = new NullCloudRuntime();
            }
            else if (runtimeType == Runtime.PHP)
            {
                runtime = new PHPCloudRuntime();
            }
            else
            {
                runtime = new NodeCloudRuntime();
            }

            runtime.Role = roleType;
            runtime.RoleName = roleName;
            return runtime;
        }

        public static CloudRuntime CreateRuntime(WebRole webRole)
        {
            Dictionary<string, string> settings = GetStartupEnvironment(webRole);
            Runtime runtimeType = GetRuntimeType(settings);
            CloudRuntime runtime = CreateRuntimeInternal(runtimeType, RoleType.WebRole, webRole.name);
            ConfigureRuntimeOverrides(runtime, settings);
            runtime.Configure();
            return runtime;
        }

        public static CloudRuntime CreateRuntime(WorkerRole workerRole)
        {
            Dictionary<string, string> settings = GetStartupEnvironment(workerRole);
            Runtime runtimeType = GetRuntimeType(settings);
            CloudRuntime runtime = CreateRuntimeInternal(runtimeType, RoleType.WorkerRole, workerRole.name);
            ConfigureRuntimeOverrides(runtime, settings);
            runtime.Configure();
            return runtime;
        }

        private static void ConfigureRuntimeOverrides(CloudRuntime runtime, Dictionary<string, string> environmentSettings)
        {
            if (environmentSettings.ContainsKey(RuntimePrimaryVersionKey))
            {
                runtime.PrimaryVersionKey = environmentSettings[RuntimePrimaryVersionKey];
            }
            if (environmentSettings.ContainsKey(RuntimeSecondaryVersionKey))
            {
                runtime.SecondaryVersionKey = environmentSettings[RuntimeSecondaryVersionKey];
            }
        }

        private static Runtime GetRuntimeType(Dictionary<string, string> environmentSettings)
        {
            Runtime runtimeType = Runtime.Node;
            if (environmentSettings.ContainsKey(RuntimeOverrideKey) && !string.IsNullOrEmpty(environmentSettings[RuntimeOverrideKey]))
            {
                runtimeType = Runtime.Override;
            }
            else if (environmentSettings.ContainsKey(RuntimeTypeKey) && !string.IsNullOrEmpty(environmentSettings[RuntimeTypeKey]) && string.Equals(environmentSettings[RuntimeTypeKey], PHPRuntimeValue, StringComparison.OrdinalIgnoreCase))
            {
                runtimeType = Runtime.PHP;
            }

            return runtimeType;
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

        private static Variable[] ApplySettingChanges(Dictionary<string, string> settings, Variable[] roleVariables)
        {
            int roleVariableCount = (roleVariables == null ? 0 : roleVariables.Length);
            if (roleVariableCount > 0)
            {
                for (int j = 0; j < roleVariables.Length; ++j)
                {
                    if (settings.ContainsKey(roleVariables[j].name))
                    {
                        roleVariables[j].value = settings[roleVariables[j].name];
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

        private static void ApplyRoleSettingChanges(Dictionary<string, string> changes, WebRole webRole)
        {
            webRole.Startup.Task[0].Environment = ApplySettingChanges(changes, webRole.Startup.Task[0].Environment);
        }

        private static void ApplyRoleSettingChanges(Dictionary<string, string> changes, WorkerRole workerRole)
        {
            workerRole.Startup.Task[0].Environment = ApplySettingChanges(changes, workerRole.Startup.Task[0].Environment);
        }

        public virtual void ApplyRuntime(CloudRuntimePackage package, WebRole webRole)
        {
            Dictionary<string, string> changes;
            if( this.GetChanges(package, out changes)) {
                ApplyRoleSettingChanges(changes, webRole);
            }
        }

        public virtual void ApplyRuntime(CloudRuntimePackage package, WorkerRole workerRole)
        {
            Dictionary<string, string> changes;
            if (this.GetChanges(package, out changes))
            {
                ApplyRoleSettingChanges(changes, workerRole);
            }
        }

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
        protected abstract bool GetChanges(CloudRuntimePackage package, out Dictionary<string, string> changes);
        

        private class NodeCloudRuntime : CloudRuntime
        {
            protected override void Configure()
            {
                this.Runtime = Runtime.Node;
                if (string.IsNullOrEmpty(this.PrimaryVersionKey))
                {
                    this.PrimaryVersionKey = GetNodeVersion();
                    if (this.Role == RoleType.WebRole)
                    {
                        this.SecondaryVersionKey = GetIISNodeVersion();
                    }
                }
            }

            public override bool Match(CloudRuntimePackage runtime)
            {
                return string.Equals(this.PrimaryVersionKey, runtime.PrimaryVersionKey, StringComparison.OrdinalIgnoreCase) &&
                    (this.Role == RoleType.WorkerRole || string.Equals(this.SecondaryVersionKey, runtime.SecondaryVersionKey, StringComparison.OrdinalIgnoreCase));
            }

            protected override string GenerateWarningText(CloudRuntimePackage package)
            {
                string returnValue;
                if (this.Role == RoleType.WebRole)
                {
                    returnValue = string.Format(CloudRuntime.NodeWebRoleVersionWarningText, package.PrimaryVersionKey, 
                        package.SecondaryVersionKey, this.RoleName, this.PrimaryVersionKey, this.SecondaryVersionKey);
                }
                else
                {
                    returnValue = string.Format(CloudRuntime.NodeWorkerRoleVersionWarningText, package.PrimaryVersionKey,
                        this.RoleName, this.PrimaryVersionKey);
                }

                return returnValue;
            }

            private string GetNodeVersion()
            {
                string fileVersion = DefaultFileVersion;
                string nodePath = Path.Combine(GetProgramFilesDirectoryPathx86(), Path.Combine(NodePath, NodeExe));
                if (!File.Exists(nodePath))
                {
                    nodePath = Path.Combine(GetProgramFilesDirectory(), Path.Combine(NodePath, NodeExe));
                }
                if (File.Exists(nodePath)) 
                {
                    fileVersion = GetFileVersion(nodePath);
                }

                return fileVersion;
            }

            private string GetIISNodeVersion()
            {
                string fileVersion = DefaultFileVersion;
                string nodePath = Path.Combine(GetProgramFilesDirectory(), Path.Combine("iisnode", "iisnode.dll"));
                if (File.Exists(nodePath))
                {
                    fileVersion = GetFileVersion(nodePath);
                }

                return fileVersion;
            }

            protected override bool GetChanges(CloudRuntimePackage package, out Dictionary<string, string> changes)
            {
                changes = new Dictionary<string, string>();
                changes[RuntimeTypeKey] = package.Runtime.ToString().ToLower();
                changes[RuntimeUrlKey] = package.PackageUri.ToString();
                return true;
            }

            private static string GetFileVersion(string filePath)
            {
                return FileVersionInfo.GetVersionInfo(filePath).ProductVersion;
            }

            private static string GetProgramFilesDirectoryPathx86()
            {
                string returnPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                if (string.IsNullOrEmpty(returnPath)) {
                    returnPath = GetProgramFilesDirectory();
                }

                return returnPath;
            }

            private static string GetProgramFilesDirectory()
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }

        }

        private class PHPCloudRuntime : CloudRuntime
        {
            protected override void Configure()
            {
                this.Runtime = Runtime.PHP;
                if (string.IsNullOrEmpty(this.PrimaryVersionKey))
                {
                    this.PrimaryVersionKey = CloudRuntime.PHPRuntimeVersion;
                }
            }

            public override bool Match(CloudRuntimePackage runtime)
            {
                return this.PrimaryVersionKey.Equals(runtime.PrimaryVersionKey, StringComparison.OrdinalIgnoreCase);
            }

            protected override string GenerateWarningText(CloudRuntimePackage package)
            {
                return string.Format(CloudRuntime.PHPVersionWarningText, package.PrimaryVersionKey, this.Role, this.RoleName, 
                    this.PrimaryVersionKey);
            }

            protected override bool GetChanges(CloudRuntimePackage package, out Dictionary<string, string> changes)
            {
                changes = new Dictionary<string, string>();
                changes[RuntimeTypeKey] = package.Runtime.ToString().ToLower();
                changes[RuntimeUrlKey] = package.PackageUri.ToString();
                return true;
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

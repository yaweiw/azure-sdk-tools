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

namespace AzureDeploymentCmdlets.Test.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AzureDeploymentCmdlets;
    using AzureDeploymentCmdlets.ServiceDefinitionSchema;
    using AzureDeploymentCmdlets.Properties;

    public class RuntimeHelper
    {

        public static bool SetRoleRuntime(ServiceDefinition definition, string roleName, string primaryVersion = null, string secondaryversion = null, string overrideUrl = null)
        {
            bool changed = false;
            Variable[] environment = GetRoleRuntimeEnvironment(definition, roleName);
            if (primaryVersion != null)
            {
                environment = SetRuntimeEnvironment(environment, Resources.RuntimePrimaryVersionKey, primaryVersion);
                changed = true;
            }

            if (secondaryversion != null)
            {
                environment = SetRuntimeEnvironment(environment, Resources.RuntimeSecondaryVersionKey, secondaryversion);
                changed = true;
            }

            if (overrideUrl != null)
            {
                environment = SetRuntimeEnvironment(environment, Resources.RuntimeOverrideKey, overrideUrl);
                changed = true;
            }

            return changed && ApplyRuntimeChanges(definition, roleName, environment);   
        }

        public static string GetRoleRuntimeUrl(ServiceDefinition definition, string roleName)
        {
            Variable v = GetRoleRuntimeEnvironment(definition, roleName).FirstOrDefault<Variable>(variable => string.Equals(variable.name, Resources.RuntimeUrlKey));
            return (null == v ? null : v.value);
        }

        public static string GetRoleRuntim.eOverrideUrl(ServiceDefinition definition, string roleName)
        {
            Variable v = GetRoleRuntimeEnvironment(definition, roleName).FirstOrDefault<Variable>(variable => string.Equals(variable.name, Resources.RuntimeOverrideKey));
            return (null == v ? null : v.value);
        }

        private static bool ApplyRuntimeChanges(ServiceDefinition definition, string roleName, Variable[] environment)
        {
            WebRole webRole;
            if (TryGetWebRole(definition, roleName, out webRole))
            {
                webRole.Startup.Task[0].Environment = environment;
                return true;
            }

            WorkerRole workerRole;
            if (TryGetWorkerRole(definition, roleName, out workerRole))
            {
                workerRole.Startup.Task[0].Environment = environment;
                return true;
            }

            return false;
        }

        private static Variable[] SetRuntimeEnvironment(IEnumerable<Variable> environment, string keyName, string keyValue)
        {
            Variable v = environment.FirstOrDefault<Variable>( variable => string.Equals(variable.name, keyName, StringComparison.OrdinalIgnoreCase));
            if (v != null)
            {
                v.value = keyValue;
                return environment.ToArray<Variable>();
            }
            else
            {
                v = new Variable { name = keyName, value = keyValue };
                return environment.Concat<Variable>(new List<Variable>{v}).ToArray<Variable>();
            }
        }

        private static Variable[] GetRoleRuntimeEnvironment(ServiceDefinition definition, string roleName)
        {
            WebRole webRole;
            if (TryGetWebRole(definition, roleName, out webRole))
            {
                return webRole.Startup.Task[0].Environment;
            }

            WorkerRole workerRole;
            if (TryGetWorkerRole(definition, roleName, out workerRole)) 
            {
                return workerRole.Startup.Task[0].Environment;
            }

            return null;
        }

        private static bool TryGetWebRole(ServiceDefinition definition, string roleName, out WebRole role)
        {
            role = definition.WebRole.FirstOrDefault<WebRole>(r => string.Equals(r.name, roleName, StringComparison.OrdinalIgnoreCase));
            return role != null;
        }

        private static bool TryGetWorkerRole(ServiceDefinition definition, string roleName, out WorkerRole role)
        {
            role = definition.WorkerRole.FirstOrDefault<WorkerRole>(r => string.Equals(r.name, roleName, StringComparison.OrdinalIgnoreCase));
            return role != null;
        }

        
        private bool TryGetEnvironmentValue(ServiceDefinitionSchema.Variable[] environment, string key, out string value)
        {
            value = null;
            bool found = false;
            if (environment != null)
            {
                foreach (ServiceDefinitionSchema.Variable setting in environment)
                {
                    if (string.Equals(setting.name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        value = setting.value;
                        found = true;
                    }
                }
            }

            return found;
        }

        private bool VerifyWebRoleRuntime(string rootPath, string roleName, string expectedUrl)
        {
            bool result = false;
            AzureService service = new AzureService(rootPath, null);
            ServiceDefinitionSchema.ServiceDefinition definition = service.Components.Definition;
            ServiceDefinitionSchema.WebRole role;
            if (TryGetWebRole(definition, roleName, out role))
            {
                string actualUrl;
                if (TryGetEnvironmentValue(role.Startup.Task[0].Environment, Resources.RuntimeUrlKey, out actualUrl))
                {
                    result = string.Equals(expectedUrl, actualUrl, StringComparison.OrdinalIgnoreCase);
                }
            }

            return result;
        }

        private bool VerifyWorkerRoleRuntime(string rootPath, string roleName, string expectedUrl)
        {
            bool result = false;
            AzureService service = new AzureService(rootPath, null);
            ServiceDefinitionSchema.ServiceDefinition definition = service.Components.Definition;
            ServiceDefinitionSchema.WorkerRole role;
            if (TryGetWorkerRole(definition, roleName, out role))
            {
                string actualUrl;
                if (TryGetEnvironmentValue(role.Startup.Task[0].Environment, Resources.RuntimeUrlKey, out actualUrl))
                {
                    result = string.Equals(expectedUrl, actualUrl, StringComparison.OrdinalIgnoreCase);
                }
            }

            return result;
        }

    }
}

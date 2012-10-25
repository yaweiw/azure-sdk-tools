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
    using System.Linq;
    using Properties;
    using ServiceConfigurationSchema;
    using ServiceDefinitionSchema;
    using Utilities;

    public class ServiceComponents
    {
        public ServiceDefinition Definition { get; private set; }
        public ServiceConfiguration CloudConfig { get; private set; }
        public ServiceConfiguration LocalConfig { get; private set; }
        public ServiceSettings Settings { get; private set; }

        public ServiceComponents(ServicePathInfo paths)
        {
            LoadComponents(paths);
        }

        private void LoadComponents(ServicePathInfo paths)
        {
            Validate.ValidateNullArgument(paths, string.Format(Resources.NullObjectMessage, "paths"));
            Validate.ValidateFileFull(paths.CloudConfiguration, Resources.ServiceConfiguration);
            Validate.ValidateFileFull(paths.LocalConfiguration, Resources.ServiceConfiguration);
            Validate.ValidateFileFull(paths.Definition, Resources.ServiceDefinition);
            Validate.ValidateFileFull(paths.Settings, Resources.ServiceSettings);

            Definition = General.DeserializeXmlFile<ServiceDefinition>(paths.Definition);
            CloudConfig = General.DeserializeXmlFile<ServiceConfiguration>(paths.CloudConfiguration);
            LocalConfig = General.DeserializeXmlFile<ServiceConfiguration>(paths.LocalConfiguration);
            Settings = ServiceSettings.Load(paths.Settings);
        }

        public void Save(ServicePathInfo paths)
        {
            if (paths == null) throw new ArgumentNullException("paths");
            // Validate directory exists and it's valid

            General.SerializeXmlFile<ServiceDefinition>(Definition, paths.Definition);
            General.SerializeXmlFile<ServiceConfiguration>(CloudConfig, paths.CloudConfiguration);
            General.SerializeXmlFile<ServiceConfiguration>(LocalConfig, paths.LocalConfiguration);
            Settings.Save(paths.Settings);
        }

        public void SetRoleInstances(string roleName, int instances)
        {
            Validate.ValidateStringIsNullOrEmpty(roleName, Resources.RoleName);
            if (instances <= 0 || instances > int.Parse(Resources.RoleMaxInstances))
            {
                throw new ArgumentException(string.Format(Resources.InvalidInstancesCount, roleName));
            }

            if (!RoleExists(roleName))
            {
                throw new ArgumentException(string.Format(Resources.RoleNotFoundMessage, roleName));
            }

            CloudConfig.Role.First<RoleSettings>(r => r.name.Equals(roleName)).Instances.count = instances;
            LocalConfig.Role.First<RoleSettings>(r => r.name.Equals(roleName)).Instances.count = instances;
        }

        /// <summary>
        /// Gets the worker role if exists otherwise return null.
        /// </summary>
        /// <param name="name">The worker role name</param>
        /// <returns>The worker role object from service definition</returns>
        public WorkerRole GetWorkerRole(string name)
        {
            if (Definition.WorkerRole != null)
            {
                try { return Definition.WorkerRole.First<WorkerRole>(r => r.name.Equals(name)); }
                catch { return null; }
            }

            return null;
        }

        /// <summary>
        /// Gets the role if exists otherwise return null.
        /// </summary>
        /// <param name="name">The role name</param>
        /// <returns>The role object from cloud configuration</returns>
        public RoleSettings GetCloudConfigRole(string name)
        {
            if (CloudConfig.Role != null)
            {
                try { return CloudConfig.Role.First<RoleSettings>(r => r.name.Equals(name)); }
                catch { return null; }
            }

            return null;
        }

        /// <summary>
        /// Gets all role settings that matches the given name.
        /// </summary>
        /// <param name="roleNames">Role names collection</param>
        /// <returns>Matched items</returns>
        public IEnumerable<RoleSettings> GetRoles(IEnumerable<string> roleNames)
        {
            if (CloudConfig.Role != null)
            {
                return Array.FindAll<RoleSettings>(CloudConfig.Role, r => Array.Exists<string>(
                    roleNames.ToArray<string>(), s => s.Equals(r.name)));
            }

            return null;
        }

        /// <summary>
        /// Gets all worker roles that matches given predicate.
        /// </summary>
        /// <param name="predicate">The matching predicate</param>
        /// <returns>Matched items</returns>
        public IEnumerable<WorkerRole> GetWorkerRoles(Predicate<WorkerRole> predicate)
        {
            return Array.FindAll<WorkerRole>(Definition.WorkerRole, predicate);
        }

        /// <summary>
        /// Applied given action to all matching 
        /// </summary>
        /// <param name="roleNames"></param>
        /// <param name="action"></param>
        public void ForEachRoleSettings(Func<RoleSettings, bool> predicate, Action<RoleSettings> action)
        {
            if (CloudConfig.Role != null)
            {
                IEnumerable<RoleSettings> matchedRoles = CloudConfig.Role.Where<RoleSettings>(predicate);
                matchedRoles.ForEach<RoleSettings>(action);
            }
        }

        public int GetNextPort()
        {
            if (Definition.WebRole == null && Definition.WorkerRole == null)
            {
                // First role will have port #80
                //
                return int.Parse(Resources.DefaultWebPort);
            }
            else
            {
                int maxWeb = 0;
                int maxWorker = 0;

                maxWeb = Definition.WebRole.MaxOrDefault<WebRole, int>(wr => (wr.Endpoints?? new Endpoints()).InputEndpoint.MaxOrDefault<InputEndpoint, int>(ie => ie.port, 0), 0);
                maxWorker = Definition.WorkerRole.MaxOrDefault<WorkerRole, int>(wr => (wr.Endpoints ?? new Endpoints()).InputEndpoint.MaxOrDefault<InputEndpoint, int>(ie => ie.port, 0), 0);
                int maxPort = Math.Max(maxWeb, maxWorker);

                if (maxPort == 0)
                {
                    // If this is first external endpoint, use default web role port
                    return int.Parse(Resources.DefaultWebPort);
                }
                else if (maxPort == int.Parse(Resources.DefaultWebPort))
                {
                    // This is second role to be added
                    return int.Parse(Resources.DefaultPort);
                }
                else
                {
                    // Increase max port and return it
                    return (maxPort + 1);
                }
            }
        }

        public void AddRoleToConfiguration(RoleSettings role, DevEnv env)
        {
            Validate.ValidateNullArgument(role, string.Format(Resources.NullRoleSettingsMessage, "ServiceConfiguration"));

            ServiceConfiguration config = (env == DevEnv.Cloud) ? CloudConfig : LocalConfig;

            if (config.Role == null)
            {
                config.Role = new RoleSettings[] { role };
            }
            else
            {
                config.Role = config.Role.Concat(new RoleSettings[] { role }).ToArray();
            }
        }

        /// <summary>
        /// Determines if a specified role exists in service components (*.csdef, local and cloud *cscfg) or not.
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <returns>bool value indicating whether this role is found or not </returns>
        public bool RoleExists(string roleName)
        {
            // If any one of these fields doesn't have elements then this means no roles added at all or
            // there's inconsistency between service components.
            //
            if ((Definition.WebRole == null && Definition.WorkerRole == null) || CloudConfig.Role == null || LocalConfig.Role == null)
                return false;
            else
            {
                return
                   ((Definition.WebRole != null && Definition.WebRole.Any<WebRole>(wr => wr.name.Equals(roleName))) || (Definition.WorkerRole != null && Definition.WorkerRole.Any<WorkerRole>(wr => wr.name.Equals(roleName)))) &&
                    CloudConfig.Role.Any<RoleSettings>(rs => rs.name.Equals(roleName)) &&
                    LocalConfig.Role.Any<RoleSettings>(rs => rs.name.Equals(roleName));
            }
        }

        /// <summary>
        /// Validates if given role name is valid or not
        /// </summary>
        /// <param name="roleName">Role name to be checked</param>
        /// <returns></returns>
        /// <remarks>This method doesn't check if the role exists in service components or not. To check for role existence use RoleExists</remarks>
        public static bool ValidRoleName(string roleName)
        {
            try
            {
                Validate.ValidateFileName(roleName);
                Validate.HasWhiteCharacter(roleName);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
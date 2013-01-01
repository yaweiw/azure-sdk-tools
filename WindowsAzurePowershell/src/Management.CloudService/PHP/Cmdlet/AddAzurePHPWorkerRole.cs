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

namespace Microsoft.WindowsAzure.Management.CloudService.PHP.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema;
    using Model;
    using Properties;

    /// <summary>
    /// Create scaffolding for a new php worker role, change cscfg file and csdef to include the added worker role
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzurePHPWorkerRole")]
    public class AddAzurePHPWorkerRoleCommand : AddRole
    {
        internal void AddAzurePHPWorkerRoleProcess(string workerRoleName, int instances, string rootPath)
        {
            AzureService service = new AzureService(rootPath, null);
            RoleInfo workerRole = service.AddWorkerRole(Resources.PHPScaffolding, workerRoleName, instances);

            try
            {
                service.ChangeRolePermissions(workerRole);
                SafeWriteOutputPSObject(typeof(RoleSettings).FullName, Parameters.RoleName, workerRole.Name);
                WriteVerbose(string.Format(Resources.AddRoleMessageCreate, rootPath, workerRole.Name));
            }
            catch (UnauthorizedAccessException)
            {
                WriteWarning(Resources.AddRoleMessageInsufficientPermissions);
            }
        }

        public override void ExecuteCmdlet()
        {
            AddAzurePHPWorkerRoleProcess(Name, Instances, GetServiceRootPath());
        }
    }
}
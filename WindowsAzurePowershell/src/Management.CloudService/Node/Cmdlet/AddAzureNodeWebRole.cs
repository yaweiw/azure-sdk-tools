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

namespace Microsoft.WindowsAzure.Management.CloudService.Node.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema;
    using Model;
    using Properties;

    /// <summary>
    /// Create scaffolding for a new node web role, change cscfg file and csdef to include the added web role
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureNodeWebRole")]
    public class AddAzureNodeWebRoleCommand : AddRole
    {
        internal void AddAzureNodeWebRoleProcess(string webRoleName, int instances, string rootPath)
        {
            AzureService service = new AzureService(rootPath, null);
            RoleInfo webRole = service.AddWebRole(Resources.NodeScaffolding, webRoleName, instances);

            try
            {
                service.ChangeRolePermissions(webRole);
                SafeWriteOutputPSObject(typeof(RoleSettings).FullName, Parameters.RoleName, webRole.Name);
                WriteVerbose(string.Format(Resources.AddRoleMessageCreate, rootPath, webRole.Name));
            }
            catch (UnauthorizedAccessException)
            {
                WriteWarning(Resources.AddRoleMessageInsufficientPermissions);
            }
        }

        public override void ExecuteCmdlet()
        {
            AddAzureNodeWebRoleProcess(Name, Instances, GetServiceRootPath());
        }
    }
}
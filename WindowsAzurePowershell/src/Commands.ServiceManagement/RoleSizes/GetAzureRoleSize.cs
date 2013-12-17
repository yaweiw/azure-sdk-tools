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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.HostedServices
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Management;
    using Management.Models;
    using Model;
    using Utilities.Common;

    /// <summary>
    /// Retrieve a Windows Azure Role Size.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRoleSize"), OutputType(typeof(RoleSizeContext))]
    public class AzureRoleSizeCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "The Role Size Name.")]
        [ValidateNotNullOrEmpty]
        public string RoleSizeName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            if (string.IsNullOrEmpty(this.RoleSizeName))
            {
                ExecuteClientActionNewSM(
                    null,
                    CommandRuntime.ToString(),
                    () => this.ManagementClient.RoleSizes.List(),
                    (op, response) => response.RoleSizes.Select(roleSize => ContextFactory<RoleSizesListResponse.RoleSize, RoleSizeContext>(roleSize, op)));
            }
            else
            {
                ExecuteClientActionNewSM(
                    null,
                    CommandRuntime.ToString(),
                    () => this.ManagementClient.RoleSizes.List(),
                    (op, response) => response.RoleSizes.Where(roleSize => string.Equals(roleSize.Name, this.RoleSizeName, StringComparison.OrdinalIgnoreCase))
                                                        .Select(roleSize => ContextFactory<RoleSizesListResponse.RoleSize, RoleSizeContext>(roleSize, op)));
            }
        }
    }
}

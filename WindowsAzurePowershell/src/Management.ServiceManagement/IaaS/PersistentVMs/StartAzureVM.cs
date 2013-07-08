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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
    using System.Management.Automation;
    using WindowsAzure.ServiceManagement;
    using Utilities.Common;
    using Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Helpers;
    using System;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Properties;


    [Cmdlet(VerbsLifecycle.Start, "AzureVM", DefaultParameterSetName = "ByName"), OutputType(typeof(ManagementOperationContext))]
    public class StartAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        public StartAzureVMCommand()
        {
        }

        public StartAzureVMCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Virtual Machine to start.", ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Virtual Machine to restart.", ParameterSetName = "Input")]
        [ValidateNotNullOrEmpty]
        [Alias("InputObject")]
        public PersistentVM VM
        {
            get;
            set;
        }

        internal override void ExecuteCommand()
        {
            base.ExecuteCommand();

            if (CurrentDeployment == null)
            {
                return;
            }

            string roleName = (this.ParameterSetName == "ByName") ? this.Name : this.VM.RoleName;

            // Create a regular expression based on user input.
            var regex = PersistentVMHelper.GetRegexFromRoleName(roleName);

            // Generate a list of role names matching regular expressions or
            // the exact name specified in the -Name parameter.
            var roleNames = PersistentVMHelper.GetRoleNames(CurrentDeployment.RoleInstanceList, regex);

            // Insure at least one of the role name instances can be found.
            if (roleNames.Count == 0)
                throw new ArgumentOutOfRangeException(String.Format(Resources.RoleInstanceCanNotBeFoundWithName, Name));

            var startRolesOperation = new StartRolesOperation();
            startRolesOperation.Roles = roleNames;

            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.StartRoles(s, this.ServiceName, CurrentDeployment.Name, startRolesOperation));
        
        }

    }
}

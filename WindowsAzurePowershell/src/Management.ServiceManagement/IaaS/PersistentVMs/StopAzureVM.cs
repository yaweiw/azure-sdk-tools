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
    using System;
    using System.Linq;
    using System.Globalization;
    using System.Management.Automation;
    using WindowsAzure.ServiceManagement;
    using Utilities.Common;
    using Model;
    using Properties;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Helpers;

    [Cmdlet(VerbsLifecycle.Stop, "AzureVM", DefaultParameterSetName = "ByName"), OutputType(typeof(ManagementOperationContext))]
    public class StopAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        public StopAzureVMCommand()
        {
        }

        public StopAzureVMCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Virtual Machine to stop.", ParameterSetName = "ByName")]
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

        [Parameter(Position = 2, HelpMessage = "Keeps the VM provisioned")]
        public SwitchParameter StayProvisioned
        {
            get;
            set;
        }

        [Parameter(Position = 3, HelpMessage = "Allows the deallocation of last VM in a deployment")]
        public SwitchParameter Force
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

            // Create a regular expression based on Name parameter
            var regex = PersistentVMHelper.GetRegexFromRoleName(roleName);

            // Insure the Force switch is specified for wildcard operations
            if (!roleName.Equals(regex.ToString()))
                if (!Force.IsPresent)
                    throw new ArgumentException(Resources.MustSpecifyForceParameterWhenUsingWildcards);

            // Generate a list of role names matching regular expressions or
            // the exact name specified in the -Name parameter.
            var roleNames = PersistentVMHelper.GetRoleNames(CurrentDeployment.RoleInstanceList, regex);

            // Insure at least one of the role name instances can be found.
            if (roleNames.Count == 0)
                throw new ArgumentOutOfRangeException(String.Format(Resources.RoleInstanceCanNotBeFoundWithName, Name));

            var shutdownRolesOperation = new ShutdownRolesOperation();
            shutdownRolesOperation.Roles = roleNames;

            if(StayProvisioned.IsPresent)
            {
                shutdownRolesOperation.PostShutdownAction = PostShutdownAction.Stopped;
                ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.ShutdownRoles(s, this.ServiceName, CurrentDeployment.Name, shutdownRolesOperation));
            }
            else
            {
                
                if(!Force.IsPresent && IsLastVmInDeployment())
                {
                    shutdownRolesOperation.PostShutdownAction = PostShutdownAction.StoppedDeallocated;
                    ConfirmAction(false,
                        Resources.DeploymentVIPLossWarning,
                        string.Format(Resources.DeprovisioningVM, roleName),
                        String.Empty,
                        () =>
                            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.ShutdownRoles(s, this.ServiceName, CurrentDeployment.Name, shutdownRolesOperation))
                        );
                }
                else
                {
                    shutdownRolesOperation.PostShutdownAction = PostShutdownAction.StoppedDeallocated;
                    ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.ShutdownRoles(s, this.ServiceName, CurrentDeployment.Name, shutdownRolesOperation));
                }
            }
        }

        private bool IsLastVmInDeployment()
        {
            Func<RoleInstance, bool> roleNotStoppedDeallocated = r => String.Compare(r.InstanceStatus, PostShutdownAction.StoppedDeallocated.ToString(), true, CultureInfo.InvariantCulture) != 0;
            bool result = CurrentDeployment.RoleInstanceList.Count(roleNotStoppedDeallocated) <= 1;
            return result;
        }
    }
}

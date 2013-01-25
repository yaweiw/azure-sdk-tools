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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
    using System;
    using System.Management.Automation;
    using Samples.WindowsAzure.ServiceManagement;
    using Management.Model;
    using Cmdlets.Common;

    /// <summary>
    /// Deletes the specified deployment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureDeployment"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureDeploymentCommand : CloudBaseCmdlet<IServiceManagement>
    {
        public RemoveAzureDeploymentCommand()
        {
        }
        
        public RemoveAzureDeploymentCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot. Staging | Production")]
        [ValidateSet("Staging", "Production", IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Do not confirm deletion of deployment")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        public void RemoveDeploymentProcess()
        {
            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => ServiceManagementExtensionMethods.DeleteDeploymentBySlot(this.Channel, s, this.ServiceName, this.Slot), WaitForOperation);
        }

        protected override void OnProcessRecord()
        {
            if (this.Force.IsPresent || this.ShouldContinue("This cmdlet will remove deployed applications including VMs from the specified deployment slot. Do you want to continue?", "Deployment Deletion"))
            {
                this.RemoveDeploymentProcess();
            }
        }
    }
}

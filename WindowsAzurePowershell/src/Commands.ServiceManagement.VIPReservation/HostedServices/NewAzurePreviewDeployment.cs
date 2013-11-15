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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.VIPReservation.HostedServices
{
    using System.Management.Automation;
    using ServiceManagement.HostedServices;
    using Utilities.Common;

    /// <summary>
    /// Create a new deployment. Note that there shouldn't be a deployment 
    /// of the same name or in the same slot when executing this command.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureDeployment", DefaultParameterSetName = "PaaS"), OutputType(typeof(ManagementOperationContext))]
    public class NewAzurePreviewDeploymentCommand : NewAzureDeploymentCommand
    {
        [Parameter(Mandatory = false, HelpMessage = "Reserved VIP Name.")]
        [ValidateNotNullOrEmpty]
        public string ReservedIPName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();
            ValidateParameters();
            NewPaaSDeploymentProcess();
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            ReservedIPName = !string.IsNullOrEmpty(ReservedIPName) ? ReservedIPName.Trim() : null;
        }

        public override void NewPaaSDeploymentProcess()
        {
            base.NewPaaSDeploymentProcess();
        }
    }
}

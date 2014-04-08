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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Network
{
    using System.Management.Automation;
    using Management.Network.Models;
    using Model;
    using Utilities.Common;

    [Cmdlet(
        VerbsCommon.New,
        ReservedIPConstants.CmdletNoun,
        DefaultParameterSetName = ReserveNewIPParamSet),
    OutputType(
        typeof(ManagementOperationContext))]
    public class NewAzureReservedIPCmdlet : ServiceManagementBaseCmdlet
    {
        protected const string ReserveNewIPParamSet = "CreateNewReservedIP";
        protected const string ReserveInUseIPParamSet = "CreateInUseReservedIP";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Reserved IP Name.")]
        [ValidateNotNullOrEmpty]
        public string ReservedIPName { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Reserved IP Label.")]
        [ValidateNotNullOrEmpty]
        public string Label { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Affinity Group Name.")]
        [ValidateNotNullOrEmpty]
        public string AffinityGroup { get; set; }

        [Parameter(
            ParameterSetName = ReserveInUseIPParamSet,
            Position = 3,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName { get; set; }

        [Parameter(
            ParameterSetName = ReserveInUseIPParamSet,
            Position = 4,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Deployment Name.")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName { get; set; }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            var parameters = new NetworkReservedIPCreateParameters
            {
                Name           = this.ReservedIPName,
                Label          = this.Label,
                AffinityGroup  = this.AffinityGroup,
                ServiceName    = this.ServiceName,
                DeploymentName = this.DeploymentName
            };

            ExecuteClientActionNewSM(
                null,
                CommandRuntime.ToString(),
                () => NetworkClient.ReservedIPs.Create(parameters));
        }
    }
}

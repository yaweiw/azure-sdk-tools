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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Preview.Network
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using AutoMapper;
    using Management.VirtualNetworks;
    using Management.VirtualNetworks.Models;
    using Model;
    using Utilities.Common;


    [Cmdlet(VerbsCommon.New, ReservedIPConstants.CmdletNoun, DefaultParameterSetName = ReserveNewIPParamSet), OutputType(typeof(ManagementOperationContext))]
    public class NewAzureReservedIPCmdlet : ServiceManagementBaseCmdlet
    {
        protected const string ReserveNewIPParamSet = "CreateNewReservedIP";
        protected const string ReserveInUseIPParamSet = "CreateInUseReservedIP";

        [Parameter(Mandatory = true, ParameterSetName = ReserveNewIPParamSet, HelpMessage = "Reserved IP Name.")]
        [Parameter(Mandatory = true, ParameterSetName = ReserveInUseIPParamSet, HelpMessage = "Reserved IP Name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = ReserveNewIPParamSet, HelpMessage = "Reserved IP Label.")]
        [Parameter(Mandatory = false, ParameterSetName = ReserveInUseIPParamSet, HelpMessage = "Reserved IP Label.")]
        [ValidateNotNullOrEmpty]
        public string Label
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = ReserveNewIPParamSet, HelpMessage = "Affinity Group Name.")]
        [Parameter(Mandatory = true, ParameterSetName = ReserveInUseIPParamSet, HelpMessage = "Affinity Group Name.")]
        [ValidateNotNullOrEmpty]
        public string AffinityGroup
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = ReserveInUseIPParamSet, HelpMessage = "Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = ReserveInUseIPParamSet, HelpMessage = "Deployment Name.")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName
        {
            get;
            set;
        }

        public void ExecuteCommand()
        {
            var parameters = new NetworkReservedIPCreateParameters
            {
                Name = Name,
                Label = Label,
                AffinityGroup = AffinityGroup,
                ServiceName = ServiceName,
                DeploymentName = DeploymentName
            };

            ExecuteClientActionNewSM(null,
                CommandRuntime.ToString(),
                () => NetworkClient.Networks.CreateReservedIP(parameters));
        }

        protected override void OnProcessRecord()
        {
            ServiceManagementPreviewProfile.Initialize();
            this.ExecuteCommand();
        }
    }
}

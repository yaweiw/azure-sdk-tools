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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.Management.Automation;
    using Model;
    using Service.Gateway;

    [Cmdlet(VerbsCommon.Get, "AzureVNetGateway"), OutputType(typeof(VirtualNetworkGatewayContext))]
    public class GetAzureVNetGatewayCommand : GatewayCmdletBase
    {
        public GetAzureVNetGatewayCommand()
        {
        }

        public GetAzureVNetGatewayCommand(IGatewayServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Virtual network name.")]
        public string VNetName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteClientActionInOCS(
                null, this.CommandRuntime.ToString(),
                s => this.Channel.GetVirtualNetworkGateway(s, this.VNetName), 
                this.WaitForNewGatewayOperation,
                (operation, operationResponse) => new VirtualNetworkGatewayContext
                    {
                        OperationId = operation.OperationTrackingId,
                        OperationStatus = operation.Status.ToString(),
                        OperationDescription = this.CommandRuntime.ToString(),
                        LastEventData = (operationResponse.LastEvent != null) ? operationResponse.LastEvent.Data : null,
                        LastEventMessage = (operationResponse.LastEvent != null) ? operationResponse.LastEvent.Message : null,
                        LastEventID = (operationResponse.LastEvent != null) ? operationResponse.LastEvent.Id : -1,
                        LastEventTimeStamp = (operationResponse.LastEvent != null) ? (DateTime?)operationResponse.LastEvent.Timestamp : null,
                        State = operationResponse.State,
                        VIPAddress = operationResponse.VIPAddress
                    });
        }
    }
}
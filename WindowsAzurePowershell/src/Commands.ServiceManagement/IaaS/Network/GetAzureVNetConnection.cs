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
    using System.Linq;
    using System.Management.Automation;
    using Model;
    using Service.Gateway;

    [Cmdlet(VerbsCommon.Get, "AzureVNetConnection"), OutputType(typeof(GatewayConnectionContext))]
    public class GetAzureVNetConnectionCommand : GatewayCmdletBase
    {
        public GetAzureVNetConnectionCommand()
        {
        }

        public GetAzureVNetConnectionCommand(IGatewayServiceManagement channel)
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
            ExecuteClientActionInOCS(
                null,
                CommandRuntime.ToString(),
                s => this.Channel.ListVirtualNetworkConnections(s, this.VNetName),
                WaitForNewGatewayOperation,
                (operation, connections) => connections.Select(c => new GatewayConnectionContext
                {
                    OperationId = operation.OperationTrackingId,
                    OperationDescription = this.CommandRuntime.ToString(),
                    OperationStatus = operation.Status,
                    ConnectivityState = c.ConnectivityState,
                    EgressBytesTransferred = c.EgressBytesTransferred,
                    IngressBytesTransferred = c.IngressBytesTransferred,
                    LastConnectionEstablished = c.LastConnectionEstablished,
                    LastEventID = c.LastEvent != null ? c.LastEvent.Id.ToString() : null,
                    LastEventMessage = c.LastEvent != null ? c.LastEvent.Message.ToString() : null,
                    LastEventTimeStamp = c.LastEvent != null ? c.LastEvent.Timestamp.ToString() : null,
                    LocalNetworkSiteName = c.LocalNetworkSiteName
                }));
        }
    }
}
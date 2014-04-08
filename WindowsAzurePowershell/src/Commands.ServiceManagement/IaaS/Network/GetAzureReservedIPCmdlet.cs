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
    using Management.Network.Models;
    using Model;
    using Utilities.Common;

    [Cmdlet(
        VerbsCommon.Get,
        ReservedIPConstants.CmdletNoun,
        DefaultParameterSetName = GetReservedIPParamSet),
    OutputType(
        typeof(ReservedIPContext))]
    public class GetAzureReservedIPCmdlet : ServiceManagementBaseCmdlet
    {
        protected const string GetReservedIPParamSet = "GetReservedIP";

        [Parameter(
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Reserved IP Name.")]
        [ValidateNotNullOrEmpty]
        public string ReservedIPName { get; set; }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            if (!string.IsNullOrEmpty(this.ReservedIPName))
            {
                ExecuteClientActionNewSM(
                    null,
                    CommandRuntime.ToString(),
                    () => NetworkClient.ReservedIPs.Get(this.ReservedIPName),
                    (s, r) => new int[1].Select(
                         i => ContextFactory<NetworkReservedIPGetResponse, ReservedIPContext>(r, s)));
            }
            else
            {
                ExecuteClientActionNewSM(
                    null,
                    CommandRuntime.ToString(),
                    () => NetworkClient.ReservedIPs.List(),
                    (s, r) => r.ReservedIPs.Select(
                         p => ContextFactory<NetworkReservedIPListResponse.ReservedIP, ReservedIPContext>(p, s)));
            }
        }
    }
}

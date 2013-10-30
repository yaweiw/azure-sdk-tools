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

namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.VirtualMachine
{
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Operations;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;

    [Cmdlet(VerbsCommon.Get, "WAPackVNet", DefaultParameterSetName = WAPackCmdletParameterSets.Empty)]
    public class GetWAPackVNet : IaaSCmdletBase
    {
        [Parameter(Position = 0, Mandatory = false, ParameterSetName = WAPackCmdletParameterSets.FromId, ValueFromPipelineByPropertyName = true, HelpMessage = "VNetwork ID.")]
        [ValidateNotNullOrEmpty]
        public Guid ID
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = WAPackCmdletParameterSets.FromName, ValueFromPipelineByPropertyName = true, HelpMessage = "VNetwork Name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        protected override void ExecuteCommand()
        {
            IEnumerable<VMNetwork> results = null;
            var vmNetworkOperations = new VMNetworkOperations(this.WebClientFactory);

            if (this.ParameterSetName == WAPackCmdletParameterSets.Empty)
            {
                results = vmNetworkOperations.Read();
            }
            else if (this.ParameterSetName == WAPackCmdletParameterSets.FromId)
            {
                VMNetwork vmNetwork = null;
                vmNetwork = vmNetworkOperations.Read(ID);
                results = new List<VMNetwork>() { vmNetwork };
            }
            else if (this.ParameterSetName == WAPackCmdletParameterSets.FromName)
            {
                results = vmNetworkOperations.Read(new Dictionary<string, string>()
                {
                    {"Name", Name}
                });
            }

            this.GenerateCmdletOutput(results);
        }
    }
}

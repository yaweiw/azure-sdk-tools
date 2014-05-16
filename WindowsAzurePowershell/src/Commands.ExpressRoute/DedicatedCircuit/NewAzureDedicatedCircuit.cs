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

namespace Microsoft.WindowsAzure.Commands.ExpressRoute
{
    using Microsoft.WindowsAzure.Management.ExpressRoute.Models;
    using System;
    using System.Management.Automation;
    using Utilities.ExpressRoute;

    [Cmdlet(VerbsCommon.New, "AzureDedicatedCircuit"), OutputType(typeof(AzureDedicatedCircuit))]
    public class NewAzureDedicatedCircuitCommand : ExpressRouteBaseCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true,
            HelpMessage = "New Azure Dedicated Circuit Name")]
        [ValidateNotNullOrEmpty]
        public string CircuitName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Circuit Bandwidth")]
        [ValidateNotNullOrEmpty]
        public UInt32 Bandwidth { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Circuit Location")]
        public string Location { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Circuit Service Provider Name")]
        public string ServiceProviderName { get; set; }
        
        public override void ExecuteCmdlet()
        {
            var circuit = ExpressRouteClient.NewAzureDedicatedCircuit(CircuitName, Bandwidth, Location,
                ServiceProviderName);
            WriteObject(circuit);         
        }
    }
}

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

namespace Microsoft.WindowsAzure.Commands.TrafficManager
{
    using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager;
    using System.Management.Automation;

    [Cmdlet(VerbsDiagnostic.Test, "AzureTrafficManagerDomainName"), OutputType(typeof(bool))]
    public class TestAzureTrafficManagerDomainName : TrafficManagerBaseCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string DomainName { get; set; }

        public override void ExecuteCmdlet()
        {
            bool result = TrafficManagerClient.TestDomainAvailability(DomainName);
            WriteObject(result);
        }
    }
}

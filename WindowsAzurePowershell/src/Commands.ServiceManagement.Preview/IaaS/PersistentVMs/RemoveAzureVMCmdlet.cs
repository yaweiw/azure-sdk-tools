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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Preview.IaaS
{
    using System.Management.Automation;
    using ServiceManagement.IaaS;
    using Model;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.Remove, "AzureVM"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureVMCmdlet : RemoveAzureVMCommand
    {
        [Parameter(Position = 2, HelpMessage = "To delete a reserved IP on this deployment, if any.")]
        public override SwitchParameter DeleteReservedVIP
        {
            get;
            set;
        }

        protected override void ExecuteCommand()
        {
            base.ExecuteCommand();
        }
    }
}
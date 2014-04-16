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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System.Management.Automation;
    using Model;

    [Cmdlet(
        VerbsCommon.Set,
        VirtualMachineAccessExtensionNoun,
        DefaultParameterSetName = EnableExtensionParamSetName),
    OutputType(
        typeof(IPersistentVM))]
    public class SetAzureVMAccessExtensionCommand : VirtualMachineAccessExtensionCmdletBase
    {
        public const string EnableExtensionParamSetName = "EnableAccessExtension";
        public const string DisableExtensionParamSetName = "DisableAccessExtension";

        [Parameter(
            ParameterSetName = EnableExtensionParamSetName,
            Mandatory = false,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "New or Existing User Name")]
        public override string UserName { get; set; }

        [Parameter(
            ParameterSetName = EnableExtensionParamSetName,
            Mandatory = false,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "New or Existing User Password")]
        public override string Password { get; set; }

        [Parameter(
            ParameterSetName = DisableExtensionParamSetName,
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Disable VM Access Extension")]
        public override SwitchParameter Disable { get; set; }

        [Parameter(
            ParameterSetName = EnableExtensionParamSetName,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Reference Name.")]
        [Parameter(
            ParameterSetName = DisableExtensionParamSetName,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Reference Name.")]
        [ValidateNotNullOrEmpty]
        public override string ReferenceName { get; set; }

        [Parameter(
            ParameterSetName = EnableExtensionParamSetName,
            Position = 4,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Version.")]
        [Parameter(
            ParameterSetName = DisableExtensionParamSetName,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Version.")]
        [ValidateNotNullOrEmpty]
        public override string Version { get; set; }

        internal void ExecuteCommand()
        {
            ValidateParameters();
            RemovePredicateExtensions();
            AddResourceExtension();
            WriteObject(VM);
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            if (IsLegacyExtension())
            {
                this.PublicConfiguration = GetLegacyConfiguration();
            }
            else
            {
                this.ReferenceName = this.ReferenceName ?? LegacyReferenceName;
                this.PublicConfiguration = GetPublicConfiguration();
                this.PrivateConfiguration = GetPrivateConfiguration();
            }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ExecuteCommand();
        }
    }
}

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
    using Model;
    using Model.PersistentVMModel;
    using System;
    using System.Management.Automation;

    [Cmdlet(
        VerbsCommon.Add,
        VirtualMachineExtensionNoun,
        DefaultParameterSetName = AddExtensionParamSetName),
    OutputType(
        typeof(IPersistentVM))]
    public class AddAzureVMExtensionCommand : VirtualMachineExtensionCmdletBase
    {
        protected const string AddExtensionParamSetName = "AddExtension";

        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Name.")]
        [ValidateNotNullOrEmpty]
        public override string ExtensionName
        {
            get;
            set;
        }

        [Parameter(
            Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Publisher.")]
        [ValidateNotNullOrEmpty]
        public override string Publisher
        {
            get;
            set;
        }

        [Parameter(
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Version.")]
        [ValidateNotNullOrEmpty]
        public override string Version
        {
            get;
            set;
        }

        [Parameter(
            Position = 4,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Reference Name.")]
        [ValidateNotNullOrEmpty]
        public override string ReferenceName
        {
            get;
            set;
        }

        [Parameter(
            Position = 5,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Public Configuration.")]
        [ValidateNotNullOrEmpty]
        public override string PublicConfiguration
        {
            get;
            set;
        }

        [Parameter(
            Position = 6,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Private Configuration.")]
        [ValidateNotNullOrEmpty]
        public override string PrivateConfiguration
        {
            get;
            set;
        }

        [Parameter(
            Position = 7,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "To Set the Extension State to 'Disable'.")]
        public override SwitchParameter Disable
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            ResourceExtensionReferences.Add(NewResourceExtension());
            WriteObject(VM);
        }

        protected override void ProcessRecord()
        {
            ServiceManagementProfile.Initialize();
            try
            {
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}

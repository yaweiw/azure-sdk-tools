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
    using System;
    using System.Management.Automation;
    using Model;
    using Model.PersistentVMModel;
    using Properties;

    [Cmdlet(
        VerbsCommon.Set,
        "AzureVMEnableVMAccessExtension",
        DefaultParameterSetName = EnableExtensionWithNewOrExistingCredentialParameterSet),
    OutputType(
        typeof(IPersistentVM))]
    public class SetAzureVMEnableVMAccessExtensionCommand : VirtualMachineExtensionCmdletBase
    {
        public const string EnableExtensionWithNewOrExistingCredentialParameterSet = "EnableExtensionWithNewOrExistingCredential";
        public const string DisableExtensionParameterSet = "DisableExtension";

        [Parameter(
            ParameterSetName = EnableExtensionWithNewOrExistingCredentialParameterSet,
            Mandatory = false,
            Position = 1,
            HelpMessage = "New or Existing User Name")]
        public string UserName
        {
            get;
            set;
        }

        [Parameter(
            ParameterSetName = EnableExtensionWithNewOrExistingCredentialParameterSet,
            Mandatory = false,
            Position = 2,
            HelpMessage = "New or Existing User Password")]
        public string Password
        {
            get;
            set;
        }

        [Parameter(
            ParameterSetName = DisableExtensionParameterSet,
            Mandatory = true,
            Position = 1,
            HelpMessage = "Disable VM Access Extension")]
        public SwitchParameter Disabled
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            ValidateParameters();

            ResourceExtensionReferences.RemoveAll(
                e => e.Publisher == VMEnableVMAccessExtensionBuilder.ExtensionDefaultPublisher
                  && e.Name == VMEnableVMAccessExtensionBuilder.ExtensionDefaultName);

            ResourceExtensionReferences.Add(
                Disabled.IsPresent ? new VMEnableVMAccessExtensionBuilder().GetResourceReference()
                                   : new VMEnableVMAccessExtensionBuilder(this.UserName, this.Password).GetResourceReference());

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

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
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Exceptions;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Operations;
    using System;
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Set, "WAPackVM", DefaultParameterSetName = WAPackCmdletParameterSets.UpdateVMSizeProfile)]
    public class SetWAPackVM : IaaSCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = WAPackCmdletParameterSets.UpdateVMSizeProfile, ValueFromPipeline = true, HelpMessage = "Existing VirtualMachine Object.")]
        [ValidateNotNullOrEmpty]
        public VirtualMachine VM
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = WAPackCmdletParameterSets.UpdateVMSizeProfile, HelpMessage = "Existing VMSizeProfile Object.")]
        [ValidateNotNullOrEmpty]
        public HardwareProfile VMSizeProfile
        {
            get;
            set;
        }

        protected override void ExecuteCommand()
        {
            var virtualMachineOperations = new VirtualMachineOperations(this.WebClientFactory);
            Guid? jobId = Guid.Empty;

            var vmToUpdate = virtualMachineOperations.Read(VM.ID);
            
            this.SetSizeProfile(vmToUpdate);
            var updatedVirtualMachine = virtualMachineOperations.Update(vmToUpdate, out jobId);

            if (!jobId.HasValue)
            {
                throw new WAPackOperationException(String.Format(Resources.OperationFailedErrorMessage, Resources.Update, VM.ID));
            }

            var jobInfo = new JobOperations(this.WebClientFactory).WaitOnJob(jobId.Value);
            if (jobInfo.jobStatus == JobStatusEnum.Failed)
            {
                this.WriteErrorDetails(new Exception(jobInfo.errorMessage));
            }

            updatedVirtualMachine = virtualMachineOperations.Read(updatedVirtualMachine.ID);
            WriteObject(updatedVirtualMachine);
        }

        private void SetSizeProfile(VirtualMachine vm)
        {
            vm.CPUCount = VMSizeProfile.CPUCount;
            vm.Memory = VMSizeProfile.Memory;
        }
    }
}

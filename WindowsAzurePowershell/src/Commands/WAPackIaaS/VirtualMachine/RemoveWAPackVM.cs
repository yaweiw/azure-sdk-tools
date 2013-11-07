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

    [Cmdlet(VerbsCommon.Remove, "WAPackVM", DefaultParameterSetName = WAPackCmdletParameterSets.FromVirtualMachineObject, SupportsShouldProcess = true)]
    public class RemoveWAPackVM : IaaSCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = WAPackCmdletParameterSets.FromVirtualMachineObject, ValueFromPipeline = true, HelpMessage = "Existing VirtualMachine Object.")]
        [ValidateNotNullOrEmpty]
        public VirtualMachine VM
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position = 2, HelpMessage = "Confirm the removal of the VM")]
        public SwitchParameter Force { get; set; }

        protected override void ExecuteCommand()
        {
            var virtualMachineOperations = new VirtualMachineOperations(this.WebClientFactory);
            Guid? jobId = null;

            ConfirmAction(
                        Force.IsPresent,
                        string.Format(Resources.RemoveVMConfirmationMessage, VM.Name),
                        string.Format(Resources.RemoveVMMessage),
                        VM.Name,
                        () =>
                        {
                            virtualMachineOperations.Delete(VM.ID, out jobId);

                            if (!jobId.HasValue)
                            {
                                throw new WAPackOperationException(String.Format(Resources.OperationFailedErrorMessage, Resources.Delete, VM.ID));
                            }

                            var jobInfo = new JobOperations(this.WebClientFactory).WaitOnJob(jobId.Value);
                            if (jobInfo.jobStatus == JobStatusEnum.Failed)
                            {
                                this.WriteErrorDetails(new Exception(jobInfo.errorMessage));
                            }

                            if (PassThru)
                            {
                                WriteObject(jobInfo.jobStatus != JobStatusEnum.Failed);
                            }

                        });
        }
    }
}

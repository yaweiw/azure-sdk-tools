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
    using System.Collections.Generic;
    using System.Management.Automation;

    public class VMOperationsCmdlet : IaaSCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = WAPackCmdletParameterSets.FromVirtualMachineObject, ValueFromPipeline = true, HelpMessage = "Existing VirtualMachine Object.")]
        [ValidateNotNullOrEmpty]
        public VirtualMachine VM
        {
            get;
            set;
        }

        protected void ExecuteVMOperation(VMOperationsEnum operation)
        {
            var virtualMachineOperations = new VirtualMachineOperations(this.WebClientFactory);
            Guid? job = null;
            VirtualMachine virtualMachine = null;

            switch (operation)
            {
                case VMOperationsEnum.Start:
                    virtualMachine = virtualMachineOperations.Start(VM.ID, out job);
                    break;

                case VMOperationsEnum.Stop:
                    virtualMachine = virtualMachineOperations.Stop(VM.ID, out job);
                    break;

                case VMOperationsEnum.Restart:
                    virtualMachine = virtualMachineOperations.Restart(VM.ID, out job);
                    break;

                case VMOperationsEnum.Shutdown:
                    virtualMachine = virtualMachineOperations.Shutdown(VM.ID, out job);
                    break;

                case VMOperationsEnum.Suspend:
                    virtualMachine = virtualMachineOperations.Suspend(VM.ID, out job);
                    break;

                case VMOperationsEnum.Resume:
                    virtualMachine = virtualMachineOperations.Resume(VM.ID, out job);
                    break;
            }

            if (!job.HasValue)
            {
                throw new WAPackOperationException(String.Format(Resources.OperationFailedErrorMessage, operation, VM.ID));
            }
            
            var jobInfo = new JobOperations(this.WebClientFactory).WaitOnJob(job.Value);
            if (jobInfo.jobStatus == JobStatusEnum.Failed)
            {
                this.WriteErrorDetails(new Exception(jobInfo.errorMessage));
            }
               
            var updatedVMObject = virtualMachineOperations.Read(virtualMachine.ID);
            WriteObject(updatedVMObject);
        }

        protected override void ExecuteCommand()
        {
            // no-op
        }

    }
}

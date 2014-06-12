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

namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.VMRole
{
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Exceptions;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Operations;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Remove, "WAPackVMRole", DefaultParameterSetName = WAPackCmdletParameterSets.FromVMRoleObject, SupportsShouldProcess = true)]
    public class RemoveWAPackVMRole : IaaSCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = WAPackCmdletParameterSets.FromVMRoleObject, ValueFromPipeline = true, HelpMessage = "Existing VMRole Object.")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = WAPackCmdletParameterSets.FromCloudService, ValueFromPipeline = true, HelpMessage = "Existing VMRole Object.")]
        [ValidateNotNullOrEmpty]
        public VMRole VMRole
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = WAPackCmdletParameterSets.FromCloudService, ValueFromPipeline = true, HelpMessage = "VMRole's CloudServiceName Name.")]
        [ValidateNotNullOrEmpty]
        public string CloudServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Position = 2, HelpMessage = "Confirm the removal of the VMRole.")]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            Guid? jobId = null;
            Guid? cloudJobId = null;
            var vmRoleOperations = new VMRoleOperations(this.WebClientFactory);

            ConfirmAction(
            Force.IsPresent,
            string.Format(Resources.RemoveVMRoleConfirmationMessage, VMRole.Name),
            string.Format(Resources.RemoveVMRoleMessage),
            VMRole.Name,
            () =>
            {
                VMRole deletedVMRole = null;
                if (this.ParameterSetName == WAPackCmdletParameterSets.FromVMRoleObject)
                {
                    deletedVMRole = vmRoleOperations.Read(VMRole.Name, VMRole.Name);
                    vmRoleOperations.Delete(VMRole.Name, VMRole.Name, out jobId);
                    WaitForJobCompletion(jobId);

                    var cloudServiceOperations = new CloudServiceOperations(this.WebClientFactory);
                    cloudServiceOperations.Delete(VMRole.Name, out cloudJobId);
                    WaitForJobCompletion(cloudJobId);
                }
                if (this.ParameterSetName == WAPackCmdletParameterSets.FromCloudService)
                {
                    deletedVMRole = vmRoleOperations.Read(this.CloudServiceName, VMRole.Name);
                    vmRoleOperations.Delete(this.CloudServiceName, VMRole.Name, out jobId);
                    WaitForJobCompletion(jobId);
                }

                if (this.PassThru)
                {
                    IEnumerable<VMRole> results = null;
                    results = new List<VMRole>() { deletedVMRole };
                    GenerateCmdletOutput(results);
                }
            });
        }
    }
}

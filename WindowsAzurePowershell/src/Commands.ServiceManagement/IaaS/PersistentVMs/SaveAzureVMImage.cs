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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Helpers;
    using Management.Compute.Models;
    using Model;
    using Properties;
    using Utilities.Common;

    [Cmdlet(
        VerbsData.Save,
        AzureVMImageNoun),
    OutputType(
        typeof(ManagementOperationContext))]
    public class SaveAzureVMImageCommand : IaaSDeploymentManagementCmdletBase
    {
        protected const string AzureVMImageNoun = "AzureVMImage";
        protected const string GeneralizedStr = "Generalized";
        protected const string SpecializedStr = "Specialized";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The service name.")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the virtual machine to export.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Alias("NewImageName")]
        [Parameter(
            Position = 2,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name for the new image.")]
        [ValidateNotNullOrEmpty]
        public string ImageName
        {
            get;
            set;
        }

        [Alias("NewImageLabel")]
        [Parameter(
            Position = 3,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The label for the new image.")]
        [ValidateNotNullOrEmpty]
        public string ImageLabel
        {
            get;
            set;
        }

        [Parameter(
            Position = 4,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The OS state.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(GeneralizedStr, SpecializedStr, IgnoreCase = true)]
        public string OSState
        {
            get;
            set;
        }
        
        protected override void ExecuteCommand()
        {
            ServiceManagementProfile.Initialize(this);
            
            base.ExecuteCommand();

            if (CurrentDeploymentNewSM == null)
            {
                WriteWarning(string.Format(Resources.NoDeploymentFoundByServiceAndVMName, this.ServiceName, this.Name));
                return;
            }

            Func<OperationStatusResponse> action = null;

            var imageType = new VirtualMachineImageHelper(this.ComputeClient).GetImageType(this.ImageName);

            if (string.IsNullOrEmpty(this.OSState) && ValidateNoImageInOtherType(imageType))
            {
                action = () => this.ComputeClient.VirtualMachines.CaptureOSImage(
                    this.ServiceName,
                    CurrentDeploymentNewSM.Name,
                    this.Name,
                    new VirtualMachineCaptureOSImageParameters
                    {
                        PostCaptureAction = PostCaptureAction.Delete,
                        TargetImageLabel = string.IsNullOrEmpty(this.ImageLabel) ? this.ImageName : this.ImageLabel,
                        TargetImageName = this.ImageName
                    });
            }
            else if (!string.IsNullOrEmpty(this.OSState) && ValidateNoImageInOtherType(imageType))
            {
                if (string.Equals(GetRoleInstanceStatus(), RoleInstanceStatus.ReadyRole))
                {
                    WriteWarning(Resources.CaptureVMImageOperationWhileVMIsStillRunning);
                }

                action = () => this.ComputeClient.VirtualMachines.CaptureVMImage(
                    this.ServiceName,
                    CurrentDeploymentNewSM.Name,
                    this.Name, new VirtualMachineCaptureVMImageParameters
                    {
                        VMImageName = this.ImageName,
                        VMImageLabel = string.IsNullOrEmpty(this.ImageLabel) ? this.ImageName : this.ImageLabel,
                        OSState = this.OSState
                    });
            }

            if (action != null)
            {
                ExecuteClientActionNewSM(null, CommandRuntime.ToString(), action);
            }
        }

        protected bool ValidateNoImageInOtherType(VirtualMachineImageType imageType)
        {
            var otherType = string.IsNullOrEmpty(this.OSState) ? VirtualMachineImageType.OSImage
                                                               : VirtualMachineImageType.VMImage;

            var valid = !imageType.HasFlag(otherType);

            if (!valid)
            {
                // If there is another type of image with the same name, 
                // WAPS will stop here to avoid duplicates and potential conflicts
                WriteErrorWithTimestamp(
                    string.Format(
                        Resources.ErrorAnotherImageTypeFoundWithTheSameName,
                        otherType,
                        this.ImageName));
            }

            return valid;
        }

        protected string GetRoleInstanceStatus()
        {
            var role = CurrentDeploymentNewSM.RoleInstances
                        .FirstOrDefault(
                            r => string.Equals(
                                r.RoleName,
                                this.Name,
                                StringComparison.InvariantCultureIgnoreCase));

            return role == null ? string.Empty : role.InstanceStatus;
        }
    }
}

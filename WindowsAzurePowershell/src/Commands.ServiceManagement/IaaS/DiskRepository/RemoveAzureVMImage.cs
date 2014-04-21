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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.DiskRepository
{
    using System.Management.Automation;
    using Properties;
    using Utilities.Common;

    [Cmdlet(
        VerbsCommon.Remove,
        AzureVMImageNoun),
    OutputType(
        typeof(ManagementOperationContext))]
    public class RemoveAzureVMImage : ServiceManagementBaseCmdlet
    {
        protected const string AzureVMImageNoun = "AzureVMImage";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the image in the image library to remove.")]
        [ValidateNotNullOrEmpty]
        public string ImageName { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            HelpMessage = "Specify to remove the underlying VHD from the blob storage.")]
        public SwitchParameter DeleteVHD { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = false,
            DontShow = true,
            HelpMessage = "Force to delete all images with the specified name.")]
        public SwitchParameter Force { get; set; }

        public void RemoveVMImageProcess()
        {
            ServiceManagementProfile.Initialize(this);
            
            this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () =>
                    {
                        OperationResponse op = null;

                        var imageType = new VirtualMachineImageHelper(this.ComputeClient).GetImageType(this.ImageName);
                        bool isOSImage = imageType.HasFlag(VirtualMachineImageType.OSImage);
                        bool isVMImage = imageType.HasFlag(VirtualMachineImageType.VMImage);

                        if (isOSImage && isVMImage)
                        {
                            if (this.Force.IsPresent)
                            {
                                op = this.ComputeClient.VirtualMachineOSImages.Delete(this.ImageName, this.DeleteVHD.IsPresent);
                            }
                            else
                            {
                                WriteErrorWithTimestamp(
                                    string.Format(Resources.DuplicateNamesFoundInBothVMAndOSImages, this.ImageName));
                            }
                        }
                        else if (isVMImage)
                        {
                            if (this.DeleteVHD.IsPresent)
                            {
                                op = this.ComputeClient.VirtualMachineVMImages.Delete(this.ImageName, true);
                            }
                            else
                            {
                                WriteErrorWithTimestamp(Resources.VMImageDeletionMustSpecifyDeleteVhdParameter);
                            }
                        }
                        else
                        {
                            // Remove the image from the image repository
                            op = this.ComputeClient.VirtualMachineOSImages.Delete(this.ImageName, this.DeleteVHD.IsPresent);
                        }

                        return op;
                    });
        }

        protected override void OnProcessRecord()
        {
            this.RemoveVMImageProcess();
        }
    }
}

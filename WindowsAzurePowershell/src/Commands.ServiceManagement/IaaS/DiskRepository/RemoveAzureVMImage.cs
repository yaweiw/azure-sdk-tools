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
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Compute;
    using Utilities.Common;
    using Properties;

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

        public void RemoveVMImageProcess()
        {
            ServiceManagementProfile.Initialize(this);
            
            this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () =>
                    {
                        OperationResponse op = null;

                        bool isOSImage = GetAzureVMImage.ExistsImageInType(this.ComputeClient, this.ImageName, ImageType.OSImage);
                        bool isVMImage = GetAzureVMImage.ExistsImageInType(this.ComputeClient, this.ImageName, ImageType.VMImage);

                        if (isOSImage && isVMImage)
                        {
                            var errorMsg = string.Format(Resources.DuplicateNamesFoundInBothVMAndOSImages, this.ImageName);
                            WriteError(new ErrorRecord(new Exception(errorMsg), string.Empty, ErrorCategory.CloseError, null));
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

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
    using Management.Compute;
    using Management.Compute.Models;
    using Model;
    using Utilities.Common;

    internal enum ImageType { VMImage, OSImage };

    [Cmdlet(
        VerbsCommon.Get,
        AzureVMImageNoun),
    OutputType(
        typeof(OSImageContext))]
    public class GetAzureVMImage : ServiceManagementBaseCmdlet
    {
        protected const string AzureVMImageNoun = "AzureVMImage";

        [Parameter(
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            Mandatory = false,
            HelpMessage = "Name of the image in the image library.")]
        [ValidateNotNullOrEmpty]
        public string ImageName { get; set; }

        internal static bool CheckImageType(ComputeManagementClient computeClient, string imageName, ImageType imageType)
        {
            if (imageType == ImageType.OSImage)
            {
                return computeClient == null ? false : computeClient.VirtualMachineOSImages.List().Images.Any(
                    e => string.Equals(e.Name, imageName, StringComparison.OrdinalIgnoreCase));
            }
            else if (imageType == ImageType.VMImage)
            {
                return computeClient == null ? false : computeClient.VirtualMachineVMImages.List().VMImages.Any(
                    e => string.Equals(e.Name, imageName, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        protected void GetAzureVMImageProcess()
        {
            ServiceManagementProfile.Initialize(this);

            this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () => this.ComputeClient.VirtualMachineOSImages.List(),
                    (s, response) => response.Images.Where(t => string.Equals(t.Name, this.ImageName, StringComparison.OrdinalIgnoreCase)).Select(
                        image => this.ContextFactory<VirtualMachineOSImageListResponse.VirtualMachineOSImage, OSImageContext>(image, s)));

            this.ExecuteClientActionNewSM(
                null,
                this.CommandRuntime.ToString(),
                () => this.ComputeClient.VirtualMachineVMImages.List(),
                (s, response) => response.VMImages.Where(t => string.Equals(t.Name, this.ImageName, StringComparison.OrdinalIgnoreCase)).Select(
                    image => this.ContextFactory<VirtualMachineVMImageListResponse.VirtualMachineVMImage, VMImageContext>(image, s)));
        }

        protected override void OnProcessRecord()
        {
            GetAzureVMImageProcess();
        }
    }
}

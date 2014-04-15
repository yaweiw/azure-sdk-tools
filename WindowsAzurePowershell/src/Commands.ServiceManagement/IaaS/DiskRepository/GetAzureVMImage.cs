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
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using Management.Compute;
    using Management.Compute.Models;
    using Model;
    using Utilities.Common;

    internal enum ImageType { VMImage, OSImage };

    [Cmdlet(
        VerbsCommon.Get,
        AzureVMImageNoun),
    OutputType(
        typeof(OSImageContext),
        typeof(VMImageContext))]
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

        internal static bool ExistsImageInType(ComputeManagementClient computeClient, string imageName, ImageType imageType)
        {
            try
            {
                if (computeClient == null)
                {
                    return false;
                }
                else if (imageType == ImageType.OSImage)
                {
                    return string.Equals(
                        computeClient.VirtualMachineOSImages.Get(imageName).Name,
                        imageName,
                        StringComparison.OrdinalIgnoreCase);
                }
                else if (imageType == ImageType.VMImage)
                {
                    return computeClient.VirtualMachineVMImages.List().VMImages.Any(
                        e => string.Equals(
                            e.Name,
                            imageName,
                            StringComparison.OrdinalIgnoreCase));
                }
            }
            catch (CloudException e)
            {
                if (e.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return false;
        }

        protected void GetAzureVMImageProcess()
        {
            ServiceManagementProfile.Initialize(this);

            if (string.IsNullOrEmpty(this.ImageName))
            {
                this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () => this.ComputeClient.VirtualMachineOSImages.List(),
                    (s, response) => response.Images.Select(
                        t => this.ContextFactory<VirtualMachineOSImageListResponse.VirtualMachineOSImage, OSImageContext>(t, s)));

                this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () => this.ComputeClient.VirtualMachineVMImages.List(),
                    (s, response) => response.VMImages.Select(
                        t => this.ContextFactory<VirtualMachineVMImageListResponse.VirtualMachineVMImage, VMImageContext>(t, s)));
            }
            else
            {
                bool isOSImage = ExistsImageInType(this.ComputeClient, this.ImageName, ImageType.OSImage);
                bool isVMImage = ExistsImageInType(this.ComputeClient, this.ImageName, ImageType.VMImage);

                if (!isVMImage)
                {
                    this.ExecuteClientActionNewSM(
                        null,
                        this.CommandRuntime.ToString(),
                        () => this.ComputeClient.VirtualMachineOSImages.Get(this.ImageName),
                        (s, t) => this.ContextFactory<VirtualMachineOSImageGetResponse, OSImageContext>(t, s));
                }
                else
                {
                    if (isOSImage)
                    {
                        this.ExecuteClientActionNewSM(
                            null,
                            this.CommandRuntime.ToString(),
                            () => this.ComputeClient.VirtualMachineOSImages.Get(this.ImageName),
                            (s, t) => this.ContextFactory<VirtualMachineOSImageGetResponse, OSImageContext>(t, s));
                    }

                    this.ExecuteClientActionNewSM(
                        null,
                        this.CommandRuntime.ToString(),
                        () => this.ComputeClient.VirtualMachineVMImages.List(),
                        (s, response) =>
                        {
                            var imgs = response.VMImages.Where(
                                t => string.Equals(
                                    t.Name,
                                    this.ImageName,
                                    StringComparison.OrdinalIgnoreCase));

                            return imgs.Select(
                                    t => this.ContextFactory<VirtualMachineVMImageListResponse.VirtualMachineVMImage, VMImageContext>(t, s));
                        });
                }
            }
        }

        protected override void OnProcessRecord()
        {
            GetAzureVMImageProcess();
        }
    }
}

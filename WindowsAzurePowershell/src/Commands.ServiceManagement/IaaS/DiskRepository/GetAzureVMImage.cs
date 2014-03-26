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
            bool result = false;

            if (computeClient == null)
            {
                result = false;
            }
            else if (imageType == ImageType.OSImage)
            {
                try
                {
                    result = string.Equals(
                        computeClient.VirtualMachineOSImages.Get(imageName).Name,
                        imageName,
                        StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    result = false;
                }
            }
            else if (imageType == ImageType.VMImage)
            {
                try
                {
                    result = computeClient.VirtualMachineVMImages.List().VMImages.Any(
                        e => string.Equals(
                            e.Name,
                            imageName,
                            StringComparison.OrdinalIgnoreCase));
                }
                catch
                {
                    result = false;
                }
            }

            return result;
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
                bool found = false;
                var exceptions = new List<Exception>();

                try
                {
                    this.ExecuteClientActionNewSM(
                        null,
                        this.CommandRuntime.ToString(),
                        () => this.ComputeClient.VirtualMachineOSImages.Get(this.ImageName),
                        (s, t) =>
                        {
                            found = true;
                            return this.ContextFactory<VirtualMachineOSImageGetResponse, OSImageContext>(t, s);
                        });
                }
                catch (Exception ex)
                {
                    found = false;
                    exceptions.Add(ex);
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

                        if (imgs.Any())
                        {
                            found = true;
                            return imgs.Select(
                                t => this.ContextFactory<VirtualMachineVMImageListResponse.VirtualMachineVMImage, VMImageContext>(t, s));
                        }
                        else
                        {
                            found = found || false;
                            return null;
                        }
                    });

                if (!found && exceptions.Any())
                {
                    if (exceptions.Count() == 1)
                    {
                        throw exceptions.FirstOrDefault();
                    }
                    else
                    {
                        throw new AggregateException(exceptions);
                    }
                }
            }
        }

        protected override void OnProcessRecord()
        {
            GetAzureVMImageProcess();
        }
    }
}

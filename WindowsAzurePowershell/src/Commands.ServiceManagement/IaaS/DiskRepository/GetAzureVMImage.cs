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
    using System.Linq;
    using System.Management.Automation;
    using Management.Compute.Models;
    using Model;
    using Utilities.Common;

    [Cmdlet(
        VerbsCommon.Get,
        AzureVMImageNoun,
        DefaultParameterSetName = OSImageParamSet),
    OutputType(
        typeof(OSImageContext))]
    public class GetAzureVMImage : ServiceManagementBaseCmdlet
    {
        protected const string AzureVMImageNoun = "AzureVMImage";
        protected const string OSImageParamSet = "ListOSImages";
        protected const string VMImageParamSet = "ListVMImages";

        [Parameter(
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            Mandatory = false,
            HelpMessage = "Name of the image in the image library.")]
        [ValidateNotNullOrEmpty]
        public string ImageName { get; set; }

        [Parameter(
            ParameterSetName = OSImageParamSet,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            Mandatory = false,
            HelpMessage = "List OS Images only.")]
        public SwitchParameter ListOSImages { get; set; }

        [Parameter(
            ParameterSetName = VMImageParamSet,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            Mandatory = false,
            HelpMessage = "List VM Images only.")]
        public SwitchParameter ListVMImages { get; set; }

        protected void GetAzureVMImageProcess()
        {
            ServiceManagementProfile.Initialize(this);

            if (string.Equals(this.ParameterSetName, OSImageParamSet, System.StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(this.ImageName))
                {
                    this.ExecuteClientActionNewSM(
                        null,
                        this.CommandRuntime.ToString(),
                        () => this.ComputeClient.VirtualMachineImages.Get(this.ImageName),
                        (s, response) => this.ContextFactory<VirtualMachineImageGetResponse, OSImageContext>(response, s));
                }
                else
                {
                    this.ExecuteClientActionNewSM(
                        null,
                        this.CommandRuntime.ToString(),
                        () => this.ComputeClient.VirtualMachineImages.List(),
                        (s, response) => response.Images.Select(
                            image => this.ContextFactory<VirtualMachineImageListResponse.VirtualMachineImage, OSImageContext>(image, s)));
                }
            }
            else
            {
                this.ExecuteClientActionNewSM(
                    null,
                    this.CommandRuntime.ToString(),
                    () => this.ComputeClient.VirtualMachineVMImages.List(),
                    (s, response) =>
                    {
                        var results = response.VMImages.Select(
                            image => this.ContextFactory<VirtualMachineVMImageListResponse.VirtualMachineVMImage, VMImageContext>(image, s));

                        return string.IsNullOrEmpty(this.ImageName) ? results
                             : results.Where(t => string.Equals(t.VMImageName, this.ImageName, System.StringComparison.OrdinalIgnoreCase));
                    });
            }
        }

        protected override void OnProcessRecord()
        {
            GetAzureVMImageProcess();
        }
    }
}

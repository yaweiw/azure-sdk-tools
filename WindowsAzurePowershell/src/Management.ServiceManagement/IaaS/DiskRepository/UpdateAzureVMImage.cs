// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.DiskRepository
{
    using System;
    using System.Management.Automation;
    using Samples.WindowsAzure.ServiceManagement;
    using Management.Model;
    using Model;
    using Cmdlets.Common;
    using Extensions;

    [Cmdlet(VerbsData.Update, "AzureVMImage")]
    public class UpdateAzureVMImage : CloudBaseCmdlet<IServiceManagement>
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the image in the image library.")]
        [ValidateNotNullOrEmpty]
        public string ImageName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Label of the image.")]
        [ValidateNotNullOrEmpty]
        public string Label
        {
            get;
            set;
        }

        public void UpdateVMImageProcess()
        {
            var image = new OSImage
            {
                Name = this.ImageName,
                Label = this.Label
            };

            ExecuteClientActionInOCS(
                image,
                CommandRuntime.ToString(),
                s => this.Channel.UpdateOSImage(s, this.ImageName, image),
                WaitForOperation,
                (op, responseImage) => new OSImageContext
                {
                    AffinityGroup = responseImage.AffinityGroup,
                    Category = responseImage.Category,
                    Label = responseImage.Label,
                    Location = responseImage.Location,
                    MediaLink = responseImage.MediaLink,
                    ImageName = responseImage.Name,
                    OS = responseImage.OS,
                    OperationDescription = CommandRuntime.ToString(),
                    OperationId = op.OperationTrackingId,
                    OperationStatus = op.Status
                });
        }

        protected override void OnProcessRecord()
        {
            this.UpdateVMImageProcess();
        }
    }
}
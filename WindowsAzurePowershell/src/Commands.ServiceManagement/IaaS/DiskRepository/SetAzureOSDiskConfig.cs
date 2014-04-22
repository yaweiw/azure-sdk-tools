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
    using System.Management.Automation;
    using Management.Compute;
    using Management.Compute.Models;
    using Model;
    using Model.PersistentVMModel;
    using Utilities.Common;
    using PVM = Model.PersistentVMModel;

    [Cmdlet(
        VerbsCommon.Set,
        AzureOSDiskConfigurationNoun),
    OutputType(
        typeof(VirtualMachineDiskConfigurationSet))]
    public class SetAzureOSDiskConfiguration : PSCmdlet
    {
        protected const string AzureOSDiskConfigurationNoun = "AzureOSDiskConfiguration";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "DiskConfigurationSet")]
        [ValidateNotNullOrEmpty]
        public VirtualMachineDiskConfigurationSet DiskConfig { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the image in the image library to remove.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "HostCaching")]
        [ValidateNotNullOrEmpty]
        public string HostCaching { get; set; }

        [Parameter(
            Position = 3,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "OSState")]
        [ValidateNotNullOrEmpty]
        public string OSState { get; set; }

        [Parameter(
            Position = 4,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "OS")]
        public string OS { get; set; }

        [Parameter(
            Position = 5,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "MediaLocation")]
        public Uri MediaLocation { get; set; }

        [Parameter(
            Position = 6,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "LogicalDiskSizeInGB")]
        public int LogicalDiskSizeInGB { get; set; }

        protected override void ProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            if (DiskConfig.OSDiskConfiguration == null)
            {
                DiskConfig.OSDiskConfiguration = new PVM.OSDiskConfiguration();
            }

            DiskConfig.OSDiskConfiguration.Name                = this.Name;
            DiskConfig.OSDiskConfiguration.HostCaching         = this.HostCaching;
            DiskConfig.OSDiskConfiguration.OSState             = this.OSState;
            DiskConfig.OSDiskConfiguration.OS                  = this.OS;
            DiskConfig.OSDiskConfiguration.MediaLink           = this.MediaLocation;
            DiskConfig.OSDiskConfiguration.LogicalDiskSizeInGB = this.LogicalDiskSizeInGB;

            WriteObject(DiskConfig);
        }
    }
}

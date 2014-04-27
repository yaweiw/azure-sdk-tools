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
    using System.Linq;
    using System.Management.Automation;
    using Model;
    using Model.PersistentVMModel;

    [Cmdlet(
        VerbsCommon.Set,
        AzureDataDiskConfigurationNoun),
    OutputType(
        typeof(VirtualMachineDiskConfigSet))]
    public class SetAzureDataDiskConfig : PSCmdlet
    {
        protected const string AzureDataDiskConfigurationNoun = "AzureDataDiskConfig";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Disk Configuration Set")]
        [ValidateNotNullOrEmpty]
        public VirtualMachineDiskConfigSet DiskConfig { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            Position = 2,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Lun")]
        [ValidateNotNullOrEmpty]
        public int Lun { get; set; }

        [Parameter(
            Position = 3,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "HostCaching")]
        [ValidateNotNullOrEmpty]
        public string HostCaching { get; set; }

        protected override void ProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            if (DiskConfig.DataDiskConfigurations == null)
            {
                DiskConfig.DataDiskConfigurations = new DataDiskConfigurationList();
            }

            var diskConfig = DiskConfig.DataDiskConfigurations.FirstOrDefault(
                d => string.Equals(d.Name, this.Name));

            if (diskConfig == null)
            {
                diskConfig = new DataDiskConfiguration();
                DiskConfig.DataDiskConfigurations.Add(diskConfig);
            }

            diskConfig.Name        = this.Name;
            diskConfig.HostCaching = this.HostCaching;
            diskConfig.Lun         = this.Lun;

            WriteObject(DiskConfig);
        }
    }
}

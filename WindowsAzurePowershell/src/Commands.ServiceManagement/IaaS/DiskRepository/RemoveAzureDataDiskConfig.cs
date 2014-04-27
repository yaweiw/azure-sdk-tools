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
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Model;
    using Model.PersistentVMModel;
    using Utilities.Common;

    [Cmdlet(
        VerbsCommon.Remove,
        AzureDataDiskConfigurationNoun,
        DefaultParameterSetName = RemoveByDiskNameParamSet),
    OutputType(
        typeof(VirtualMachineDiskConfigSet))]
    public class RemoveAzureDataDiskConfig : PSCmdlet
    {
        protected const string AzureDataDiskConfigurationNoun = "AzureDataDiskConfig";
        protected const string RemoveByDiskNameParamSet = "RemoveByDiskName";
        protected const string RemoveByDiskLunParamSet = "RemoveByDiskLun";

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Disk Configuration Set")]
        [ValidateNotNullOrEmpty]
        public VirtualMachineDiskConfigSet DiskConfig { get; set; }

        [Parameter(
            ParameterSetName = RemoveByDiskNameParamSet,
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Disk Name")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(
            ParameterSetName = RemoveByDiskLunParamSet,
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Disk Lun")]
        [ValidateNotNullOrEmpty]
        public int Lun { get; set; }

        protected override void ProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            if (DiskConfig.DataDiskConfigurations != null)
            {
                IEnumerable<DataDiskConfiguration> diskConfigs = null;

                if (string.Equals(this.ParameterSetName, RemoveByDiskNameParamSet))
                {
                    diskConfigs = DiskConfig.DataDiskConfigurations.Where(
                        d => string.Equals(d.Name, this.Name));
                }
                else
                {
                    diskConfigs = DiskConfig.DataDiskConfigurations.Where(
                        d => string.Equals(d.Lun, this.Lun)
                    );
                }

                if (diskConfigs != null)
                {
                    diskConfigs.ForEach(
                        d => DiskConfig.DataDiskConfigurations.Remove(d));
                }
            }

            WriteObject(DiskConfig);
        }
    }
}

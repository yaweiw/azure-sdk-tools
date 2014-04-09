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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Model
{
    using System;
    using Model.PersistentVMModel;
    using Utilities.Common;

    public class VMImageContext : OSImageContext
    {
        public override string ImageName
        {
            get
            {
                return this.VMImageName;
            }

            set
            {
                this.VMImageName = value;
            }
        }

        public override string Label { get; set; }
        public override string Category { get; set; }
        public override string Description { get; set; }
        public override string Location { get; set; }
        public override string AffinityGroup { get; set; }

        public string VMImageName { get; set; }
        public OSDiskConfiguration OSDiskConfiguration { get; set; }
        public DataDiskConfigurationList DataDiskConfigurations { get; set; }
        public string ServiceName { get; set; }
        public string DeploymentName { get; set; }
        public string RoleName { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
}
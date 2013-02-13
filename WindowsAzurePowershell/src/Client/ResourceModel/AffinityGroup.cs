/**
* Copyright Microsoft Corporation 2012
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Microsoft.WindowsAzure.ServiceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [CollectionDataContract(Name = "AffinityGroups", ItemName = "AffinityGroup", Namespace = Constants.ServiceManagementNS)]
    public class AffinityGroupList : List<AffinityGroup>
    {
        public AffinityGroupList()
        {
        }

        public AffinityGroupList(IEnumerable<AffinityGroup> affinityGroups)
            : base(affinityGroups)
        {
        }
    }

    [CollectionDataContract(Name = "Capabilities", ItemName = "Capability", Namespace = Constants.ServiceManagementNS)]
    public class CapabilitiesList : List<String>, IExtensibleDataObject
    {
        public CapabilitiesList()
        {
        }

        public CapabilitiesList(IEnumerable<String> capabilities)
            : base(capabilities)
        {
        }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class AffinityGroup : IExtensibleDataObject
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Name { get; set;}

        [DataMember(Order = 2)]
        public string Label { get; set; }

        [DataMember(Order = 3)]
        public string Description { get; set; }

        [DataMember(Order = 4)]
        public string Location { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public HostedServiceList HostedServices { get; set; }

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public StorageServiceList StorageServices { get; set; }

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public CapabilitiesList Capabilities { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "CreateAffinityGroup", Namespace = Constants.ServiceManagementNS)]
    public class CreateAffinityGroupInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public string Label { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Order = 4)]
        public string Location { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "UpdateAffinityGroup", Namespace = Constants.ServiceManagementNS)]
    public class UpdateAffinityGroupInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Label { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string LocationConstraint { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }   
}

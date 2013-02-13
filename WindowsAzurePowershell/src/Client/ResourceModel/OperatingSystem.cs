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
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [CollectionDataContract(Name = "OperatingSystemFamilies", ItemName = "OperatingSystemFamily", Namespace = Constants.ServiceManagementNS)]
    public class OperatingSystemFamilyList : List<OperatingSystemFamily>
    {
        public OperatingSystemFamilyList()
        {
        }

        public OperatingSystemFamilyList(IEnumerable<OperatingSystemFamily> operatingSystemFamilies)
            : base(operatingSystemFamilies)
        {
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class OperatingSystemFamily : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Label { get; set; }

        [DataMember(Order = 3)]
        public OperatingSystemList OperatingSystems { get; set; } 

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "OperatingSystems", ItemName = "OperatingSystem", Namespace = Constants.ServiceManagementNS)]
    public class OperatingSystemList : List<OperatingSystem>
    {
        public OperatingSystemList()
        {
        }

        public OperatingSystemList(IEnumerable<OperatingSystem> operatingSystems)
            : base(operatingSystems)
        {
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class OperatingSystem : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Version { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Label { get; set; }

        [DataMember(Order = 3)]
        public bool IsDefault { get; set; }

        [DataMember(Order = 4)]
        public bool IsActive { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public string Family { get; set; }

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public string FamilyLabel { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }    
}

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

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum ImageSharedAccessSignaturePermission
    {
        [EnumMember]
        Read,
        [EnumMember]
        ReadWrite
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum ImageStatus
    {
        [EnumMember]
        Pending,
        [EnumMember]
        Committed
    }

    [DataContract(Name = "PrepareMachineImage", Namespace = Constants.ServiceManagementNS)]
    public class  PrepareImageUploadInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Label { get; set; }

        [DataMember(Order = 2)]
        public string Description { get; set; }

        [DataMember(Order = 3)]
        public string Uuid { get; set; }

        [DataMember(Order = 4)]
        public string Timestamp { get; set; }

        [DataMember(Order = 5)]
        public long CompressedSizeInBytes { get; set; }

        [DataMember(Order = 6)]
        public long MountedSizeInBytes { get; set; }

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public string Location { get; set; }

        [DataMember(Order = 8, EmitDefaultValue = false)]
        public string AffinityGroup { get; set; }

        [DataMember(Order = 9, EmitDefaultValue = false)]
        public string ParentUuid { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string ParentTimestamp { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name="SetMachineImageProperties", Namespace = Constants.ServiceManagementNS)]
    public class SetMachineImagePropertiesInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Label { get; set; }

        [DataMember(Order = 2)]
        public string Description { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name="ParentMachineImage", Namespace = Constants.ServiceManagementNS)]
    public class SetParentImageInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string ParentImageName { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }


    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class MachineImageReference : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string SharedAccessSignatureUrl { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name="MachineImages",ItemName = "MachineImage",Namespace = Constants.ServiceManagementNS)]
    public class MachineImageList : List<MachineImage>
    {
    }
    
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class MachineImage : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public string Label { get; set; }

        [DataMember(Order = 3)]
        public string Description { get; set; }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string Location { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public string AffinityGroup { get; set; }

        [DataMember(Order = 6)]
        public string Status { get; set; }

        [DataMember(Order = 7)]
        public string ParentImageName { get; set; }

        [DataMember(Order = 8)]
        public string Uuid { get; set; }

        [DataMember(Order = 9)]
        public string Timestamp { get; set; }

        [DataMember(Order = 10)]
        public long MountedSizeInBytes { get; set; }

        [DataMember(Order = 11)]
        public long CompressedSizeInBytes { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        public string ParentUuid { get; set; }

        [DataMember(Order = 13, EmitDefaultValue = false)]
        public string ParentTimestamp { get; set; }

        [DataMember(Order = 15)]
        public bool InUse { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }    
}

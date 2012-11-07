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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Disk : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 0)]
        public string AffinityGroup
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public RoleReference AttachedTo
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string OS
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public bool IsCorrupted ////Indicates whether the lease backing this Disk has been violated.
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public string Label
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        public string Location
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 6)]
        public int LogicalDiskSizeInGB
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 7)]
        public Uri MediaLink
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 8)]
        public string Name
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 9)]
        public string SourceImageName
        {
            get;
            set;
        }

        #region IExtensibleDataObject Members
        ExtensionDataObject IExtensibleDataObject.ExtensionData
        {
            get;
            set;
        }
        #endregion
    }

    [CollectionDataContract(Name = "Disks", ItemName = "Disk", Namespace = Constants.ServiceManagementNS)]
    public class DiskList : Collection<Disk>
    {
    }
}
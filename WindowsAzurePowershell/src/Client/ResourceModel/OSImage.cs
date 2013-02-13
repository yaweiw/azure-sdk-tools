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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

[assembly: CLSCompliant(true)]
namespace Microsoft.WindowsAzure.ServiceManagement
{
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class ReplicationInput : IExtensibleDataObject
    {
        [DataMember(Order = 0, EmitDefaultValue = false)]
        public RegionList TargetLocations { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "Regions", ItemName = "Region", Namespace = Constants.ServiceManagementNS)]
    public class RegionList : List<String>
    {
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class OSImage : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 0)]
        public string AffinityGroup
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Category
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string Label
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public string Location
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public int LogicalSizeInGB
        {
            get;
            set;
        }


        [DataMember(EmitDefaultValue = false, Order = 5)]
        public Uri MediaLink
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 6)]
        public string Name
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 7)]
        public string OS
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 8)]
        public string Eula
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 9)]
        public string Description
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public string ImageFamily
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 11)]
        public bool? ShowInGui
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 12)]
        public DateTime? PublishedDate
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 13)]
        public bool? IsPremium
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 14)]
        public Uri IconUri
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 15)]
        public Uri PrivacyUri
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 16)]
        public string RecommendedVMSize
        {
            get;
            set;
        }

        [DataMember ( EmitDefaultValue = false, Order = 17 )]
        public string PublisherName
        {
            get;
            set;
        }

        #region IExtensibleDataObject Members
        public ExtensionDataObject ExtensionData { get; set; }
        #endregion
    }

    [CollectionDataContract(Name = "Images", ItemName = "OSImage", Namespace = Constants.ServiceManagementNS)]
    public class OSImageList : Collection<OSImage>
    {

    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class RoleReference : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 0)]
        public string DeploymentName
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string HostedServiceName
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order= 2)]
        public string RoleName
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            RoleReference other = obj as RoleReference;

            if (other != null)
            {
                return string.Equals(other.HostedServiceName, this.HostedServiceName, StringComparison.OrdinalIgnoreCase) && string.Equals(other.DeploymentName, this.DeploymentName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(other.RoleName, this.RoleName, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            string hashString = string.Format("{0}-{1}-{2}", this.HostedServiceName, this.DeploymentName, this.RoleName);
            return hashString.GetHashCode();
        }

        #region IExtensibleDataObject Members
        public ExtensionDataObject ExtensionData { get; set; }
        #endregion
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum OSType
    {
        [EnumMember]
        Linux = 0,
        [EnumMember]
        Windows = 1
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class OSImageDetails : OSImage
    {
        [DataMember(EmitDefaultValue = false, Order = 90)]
        public bool IsCorrupted { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 100)]
        public ReplicationProgressList ReplicationProgress { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class ReplicationProgressElement : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Location { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string Progress { get; set; }

        #region IExtensibleDataObject Members
        public ExtensionDataObject ExtensionData { get; set; }
        #endregion

    }

    [CollectionDataContract(Name = "ReplicationProgressList", ItemName = "ReplicationProgressElement", Namespace = Constants.ServiceManagementNS)]
    public class ReplicationProgressList : Collection<ReplicationProgressElement>
    {

    }
}

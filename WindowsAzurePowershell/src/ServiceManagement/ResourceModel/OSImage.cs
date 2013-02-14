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

using System;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

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

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public string OS
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 11)]
        public string Eula
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 12)]
        public string Description
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

        [DataMember(EmitDefaultValue = false, Order = 2)]
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
        ExtensionDataObject IExtensibleDataObject.ExtensionData
        {
            get;
            set;
        }
        #endregion
    }

    /// <summary>
    /// The operating system type, e.g. Linux, Windows.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum OSType
    {
        [EnumMember]
        Linux = 0,
        [EnumMember]
        Windows = 1
    }
}
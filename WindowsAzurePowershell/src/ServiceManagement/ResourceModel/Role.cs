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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.Serialization;
#if SERVER
    using Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement;
#endif

    #region Role
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    [KnownType(typeof(PersistentVMRole))]
    public class Role : Mergable<PersistentVMRole>, IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public virtual string RoleName { get; set; }

        [DataMember(Order = 2)]
        public string OsVersion { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public virtual string RoleType { get; set; }

        [DataMember(Name = "ConfigurationSets", EmitDefaultValue = false, Order = 4)]
        public Collection<ConfigurationSet> ConfigurationSets
        {
            get
            {
                return this.GetValue<Collection<ConfigurationSet>>("ConfigurationSets");
            }

            set
            {
                base.SetValue("ConfigurationSets", value);
            }
        }

        public NetworkConfigurationSet NetworkConfigurationSet
        {
            get
            {
                if (this.ConfigurationSets == null)
                {
                    return null;
                }

                return this.ConfigurationSets.FirstOrDefault(
                   cset => cset is NetworkConfigurationSet) as NetworkConfigurationSet;
            }

            set
            {
                if (this.ConfigurationSets == null)
                {
                    this.ConfigurationSets = new Collection<ConfigurationSet>();
                }

                NetworkConfigurationSet networkConfigurationSet = this.ConfigurationSets.FirstOrDefault(
                        cset => cset is NetworkConfigurationSet) as NetworkConfigurationSet;

                if (networkConfigurationSet != null)
                {
                    this.ConfigurationSets.Remove(networkConfigurationSet);
                }

                this.ConfigurationSets.Add(value);
            }
        }

        public override object ResolveType()
        {
            if (this.GetType() != typeof(Role))
            {
                return this;
            }

            if (this.RoleType == typeof(PersistentVMRole).Name)
            {
                return base.Convert<PersistentVMRole>();
            }

            return this;
        }

        public ExtensionDataObject ExtensionData { get; set; }
    }
    #endregion

    #region PersistentVMRole
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class PersistentVMRole : Role
    {
        public override string RoleName
        {
            get
            {
                return base.GetValue<string>("RoleName");
            }

            set
            {
                base.SetValue("RoleName", value);
            }
        }

        [DataMember(Name = "AvailabilitySetName", EmitDefaultValue = false, Order = 0)]
        public string AvailabilitySetName
        {
            get
            {
                return this.GetValue<string>("AvailabilitySetName");
            }

            set
            {
                this.SetValue("AvailabilitySetName", value);
            }
        }

        [DataMember(Name = "DataVirtualHardDisks", EmitDefaultValue = false, Order = 1)]
        public Collection<DataVirtualHardDisk> DataVirtualHardDisks
        {
            get
            {
                return base.GetValue<Collection<DataVirtualHardDisk>>("DataVirtualHardDisks");
            }

            set
            {
                base.SetValue("DataVirtualHardDisks", value);
            }
        }

        [DataMember(Name = "Label", EmitDefaultValue = false, Order = 2)]
        public string Label
        {
            get
            {
                return base.GetValue<string>("Label");
            }

            set
            {
                base.SetValue("Label", value);
            }
        }

        [DataMember(Name = "OSVirtualHardDisk", EmitDefaultValue = false, Order = 3)]
        public OSVirtualHardDisk OSVirtualHardDisk
        {
            get
            {
                return base.GetValue<OSVirtualHardDisk>("OSVirtualHardDisk");
            }

            set
            {
                base.SetValue("OSVirtualHardDisk", value);
            }
        }

        [DataMember(Name = "RoleSize", EmitDefaultValue = false, Order = 4)]
        public string RoleSize
        {
            get
            {
                return this.GetValue<string>("RoleSize");
            }

            set
            {
                this.SetValue("RoleSize", value);
            }
        }

        public override string RoleType
        {
            get
            {
                return typeof(PersistentVMRole).Name;
            }

            set
            {
                base.RoleType = value;
            }
        }
    }
    #endregion
}
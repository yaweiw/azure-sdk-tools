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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    #region Configuration Set

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    [KnownType(typeof(ProvisioningConfigurationSet))]
    [KnownType(typeof(LinuxProvisioningConfigurationSet))]
    [KnownType(typeof(WindowsProvisioningConfigurationSet))]
    [KnownType(typeof(NetworkConfigurationSet))]
    public class ConfigurationSet : Mergable<ConfigurationSet>
    {
        [DataMember(EmitDefaultValue = false, Order = 0)]
        public virtual string ConfigurationSetType
        {
            get;
            set;
        }

        protected ConfigurationSet()
        {
        }

        public override object ResolveType()
        {
            if (this.GetType() != typeof(ConfigurationSet))
            {
                return this;
            }

            if (!string.IsNullOrEmpty(this.ConfigurationSetType))
            {
                if (string.Equals(this.ConfigurationSetType, "WindowsProvisioningConfiguration"))
                {
                    return base.Convert<WindowsProvisioningConfigurationSet>();
                }

                if (string.Equals(this.ConfigurationSetType, "LinuxProvisioningConfiguration"))
                {
                    return base.Convert<LinuxProvisioningConfigurationSet>();
                }

                if (string.Equals(this.ConfigurationSetType, "NetworkConfiguration"))
                {
                    return base.Convert<NetworkConfigurationSet>();
                }
            }

            return this;
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public abstract class ProvisioningConfigurationSet : ConfigurationSet
    {
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class WindowsProvisioningConfigurationSet : ProvisioningConfigurationSet
    {
        [DataMember(Name = "ComputerName", EmitDefaultValue = false, Order = 1)]
        public string ComputerName
        {
            get
            {
                return this.GetValue<string>("ComputerName");
            }

            set
            {
                this.SetValue("ComputerName", value);
            }
        }

        [DataMember(Name = "AdminPassword", EmitDefaultValue = false, Order = 2)]
        public string AdminPassword
        {
            get
            {
                return this.GetValue<string>("AdminPassword");
            }

            set
            {
                this.SetValue("AdminPassword", value);
            }
        }

        [DataMember(Name = "ResetPasswordOnFirstLogon", EmitDefaultValue = false, Order = 4)]
        private bool? resetPasswordOnFirstLogon
        {
            get
            {
                return this.GetField<bool>("ResetPasswordOnFirstLogon");
            }

            set
            {
                this.SetField("ResetPasswordOnFirstLogon", value);
            }
        }

        public bool ResetPasswordOnFirstLogon
        {
            get
            {
                return base.GetValue<bool>("ResetPasswordOnFirstLogon");
            }

            set
            {
                base.SetValue("ResetPasswordOnFirstLogon", value);
            }
        }

        [DataMember(Name = "EnableAutomaticUpdates", EmitDefaultValue = false, Order = 4)]
        public bool? EnableAutomaticUpdates
        {
            get
            {
                return base.GetValue<bool?>("EnableAutomaticUpdates");
            }

            set
            {
                base.SetValue("EnableAutomaticUpdates", value);
            }
        }

        [DataMember(Name = "TimeZone", EmitDefaultValue = false, Order = 5)]
        public string TimeZone
        {
            get
            {
                return base.GetValue<string>("TimeZone");
            }

            set
            {
                base.SetValue("TimeZone", value);
            }
        }

        [DataMember(Name = "DomainJoin", EmitDefaultValue = false, Order = 6)]
        public DomainJoinSettings DomainJoin
        {
            get
            {
                return base.GetValue<DomainJoinSettings>("DomainJoin");
            }

            set
            {
                base.SetValue("DomainJoin", value);
            }
        }

        [DataMember(Name = "StoredCertificateSettings", EmitDefaultValue = false, Order = 7)]
        public CertificateSettingList StoredCertificateSettings
        {
            get
            {
                return base.GetValue<CertificateSettingList>("StoredCertificateSettings");
            }

            set
            {
                base.SetValue("StoredCertificateSettings", value);
            }
        }

        public override string ConfigurationSetType
        {
            get
            {
                return "WindowsProvisioningConfiguration";
            }

            set
            {
                base.ConfigurationSetType = value;
            }
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class DomainJoinCredentials
        {
            [DataMember(Name = "Domain", EmitDefaultValue = false, Order = 1)]
            public string Domain { get; set; }

            [DataMember(Name = "Username", EmitDefaultValue = false, Order = 2)]
            public string Username { get; set; }

            [DataMember(Name = "Password", EmitDefaultValue = false, Order = 3)]
            public string Password { get; set; }
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class DomainJoinProvisioning
        {
            [DataMember(Name = "AccountData", EmitDefaultValue = false, Order = 1)]
            public string AccountData { get; set; }
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class DomainJoinSettings
        {
            [DataMember(Name = "Credentials", EmitDefaultValue = false, Order = 1)]
            public DomainJoinCredentials Credentials { get; set; }

            [DataMember(Name = "Provisioning", EmitDefaultValue = false, Order = 2)]
            public DomainJoinProvisioning Provisioning { get; set; }

            [DataMember(Name = "JoinDomain", EmitDefaultValue = false, Order = 3)]
            public string JoinDomain { get; set; }

            [DataMember(Name = "MachineObjectOU", EmitDefaultValue = false, Order = 4)]
            public string MachineObjectOU { get; set; }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class LinuxProvisioningConfigurationSet : ProvisioningConfigurationSet
    {
        [DataMember(Name = "HostName", EmitDefaultValue = false, Order = 1)]
        public string HostName
        {
            get
            {
                return this.GetValue<string>("HostName");
            }

            set
            {
                this.SetValue("HostName", value);
            }
        }

        [DataMember(Name = "UserName", EmitDefaultValue = false, Order = 2)]
        public string UserName
        {
            get
            {
                return this.GetValue<string>("UserName");
            }

            set
            {
                this.SetValue("UserName", value);
            }
        }

        [DataMember(Name = "UserPassword", EmitDefaultValue = false, Order = 3)]
        public string UserPassword
        {
            get
            {
                return this.GetValue<string>("UserPassword");
            }

            set
            {
                this.SetValue("UserPassword", value);
            }
        }

        [DataMember(Name = "DisableSshPasswordAuthentication", EmitDefaultValue = false, Order = 4)]
        public bool? DisableSshPasswordAuthentication
        {
            get
            {
                return base.GetValue<bool?>("DisableSshPasswordAuthentication");
            }

            set
            {
                base.SetValue("DisableSshPasswordAuthentication", value);
            }
        }

        [DataMember(Name = "SSH", EmitDefaultValue = false, Order = 5)]
        public SSHSettings SSH
        {
            get
            {
                return base.GetValue<SSHSettings>("SSH");
            }

            set
            {
                base.SetValue("SSH", value);
            }
        }

        public override string ConfigurationSetType
        {
            get
            {
                return "LinuxProvisioningConfiguration";
            }

            set
            {
                base.ConfigurationSetType = value;
            }
        }

        [DataContract(Name = "SSHSettings", Namespace = Constants.ServiceManagementNS)]
        public class SSHSettings
        {
            [DataMember(Name = "PublicKeys", EmitDefaultValue = false, Order = 1)]
            public SSHPublicKeyList PublicKeys { get; set; }

            [DataMember(Name = "KeyPairs", EmitDefaultValue = false, Order = 2)]
            public SSHKeyPairList KeyPairs { get; set; }
        }

        [CollectionDataContract(Name = "SSHPublicKeyList", ItemName = "PublicKey", Namespace = Constants.ServiceManagementNS)]
        public class SSHPublicKeyList : List<SSHPublicKey>
        {
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class SSHPublicKey
        {
            [DataMember(Name = "Fingerprint", EmitDefaultValue = false, Order = 1)]
            public string Fingerprint { get; set; }

            [DataMember(Name = "Path", EmitDefaultValue = false, Order = 2)]
            public string Path { get; set; }
        }

        [CollectionDataContract(Name = "SSHKeyPairList", ItemName = "KeyPair", Namespace = Constants.ServiceManagementNS)]
        public class SSHKeyPairList : List<SSHKeyPair>
        {
        }

        [DataContract(Namespace = Constants.ServiceManagementNS)]
        public class SSHKeyPair
        {
            [DataMember(Name = "Fingerprint", EmitDefaultValue = false, Order = 1)]
            public string Fingerprint { get; set; }

            [DataMember(Name = "Path", EmitDefaultValue = false, Order = 2)]
            public string Path { get; set; }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class NetworkConfigurationSet : ConfigurationSet
    {
        public override string ConfigurationSetType
        {
            get
            {
                return "NetworkConfiguration";
            }

            set
            {
                base.ConfigurationSetType = value;
            }
        }

        [DataMember(Name = "InputEndpoints", EmitDefaultValue = false, Order = 0)]
        public Collection<InputEndpoint> InputEndpoints
        {
            get
            {
                return base.GetValue<Collection<InputEndpoint>>("InputEndpoints");
            }

            set
            {
                base.SetValue("InputEndpoints", value);
            }
        }

        [DataMember(Name = "SubnetNames", EmitDefaultValue = false, Order = 1)]
        public SubnetNamesCollection SubnetNames
        {
            get
            {
                return this.GetValue<SubnetNamesCollection>("SubnetNames");
            }

            set
            {
                this.SetValue("SubnetNames", value);
            }
        }
    }

    [DataContract(Name = "InputEndpoint", Namespace = Constants.ServiceManagementNS)]
    public class InputEndpoint : Mergable<InputEndpoint>
    {
        ////[DataMember(Name = "EnableDirectServerReturn", EmitDefaultValue = false, Order = 0)]
        ////private bool? enableDirectServerReturn
        ////{
        ////    get
        ////    {
        ////        return this.GetField<bool>("EnableDirectServerReturn");
        ////    }
        ////    set
        ////    {
        ////        this.SetField("EnableDirectServerReturn", value);
        ////    }
        ////}
        ////public bool EnableDirectServerReturn
        ////{
        ////    get
        ////    {
        ////        return base.GetValue<bool>("EnableDirectServerReturn");
        ////    }
        ////    set
        ////    {
        ////        base.SetValue("EnableDirectServerReturn", value);
        ////    }
        ////}

        [DataMember(Name = "LoadBalancedEndpointSetName", EmitDefaultValue = false, Order = 1)]
        public string LoadBalancedEndpointSetName
        {
            get
            {
                return base.GetValue<string>("LoadBalancedEndpointSetName");
            }

            set
            {
                base.SetValue("LoadBalancedEndpointSetName", value);
            }
        }

        [DataMember(Name = "LocalPort", EmitDefaultValue = false, Order = 2)]
        private int? localPort
        {
            get
            {
                return base.GetField<int>("LocalPort");
            }

            set
            {
                base.SetField("LocalPort", value);
            }
        }

        public int LocalPort
        {
            get
            {
                return base.GetValue<int>("LocalPort");
            }

            set
            {
                base.SetValue("LocalPort", value);
            }
        }

        [DataMember(Name = "Name", EmitDefaultValue = false, Order = 3)]
        public string Name
        {
            get
            {
                return base.GetValue<string>("Name");
            }

            set
            {
                base.SetValue("Name", value);
            }
        }

        [DataMember(Name = "Port", EmitDefaultValue = false, Order = 4)]
        public int? Port
        {
            get
            {
                return base.GetValue<int?>("Port");
            }

            set
            {
                base.SetValue("Port", value);
            }
        }

        [DataMember(Name = "LoadBalancerProbe", EmitDefaultValue = false, Order = 5)]
        public LoadBalancerProbe LoadBalancerProbe
        {
            get
            {
                return base.GetValue<LoadBalancerProbe>("LoadBalancerProbe");
            }

            set
            {
                base.SetValue("LoadBalancerProbe", value);
            }
        }

        [DataMember(Name = "Protocol", EmitDefaultValue = false, Order = 6)]
        public string Protocol
        {
            get
            {
                return base.GetValue<string>("Protocol");
            }

            set
            {
                base.SetValue("Protocol", value);
            }
        }

        [DataMember(Name = "Vip", EmitDefaultValue = false, Order = 7)]
        public string Vip
        {
            get
            {
                return base.GetValue<string>("Vip");
            }

            set
            {
                base.SetValue("Vip", value);
            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class LoadBalancerProbe : Mergable<LoadBalancerProbe>
    {
        [DataMember(Name = "Path", EmitDefaultValue = false, Order = 0)]
        public string Path
        {
            get
            {
                return base.GetValue<string>("Path");
            }

            set
            {
                base.SetValue("Path", value);
            }
        }

        [DataMember(Name = "Port", EmitDefaultValue = false, Order = 1)]
        private int? port
        {
            get
            {
                return base.GetField<int>("Port");
            }

            set
            {
                base.SetField("Port", value);
            }
        }

        public int Port
        {
            get
            {
                return base.GetValue<int>("Port");
            }

            set
            {
                base.SetValue("Port", value);
            }
        }

        [DataMember(Name = "Protocol", EmitDefaultValue = false, Order = 2)]
        public string Protocol
        {
            get
            {
                return base.GetValue<string>("Protocol");
            }

            set
            {
                base.SetValue("Protocol", value);
            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class CertificateSetting : Mergable<CertificateSetting>
    {
        [DataMember(Name = "StoreLocation", EmitDefaultValue = false, Order = 0)]
        public string StoreLocation
        {
            get
            {
                return base.GetValue<string>("StoreLocation");
            }

            set
            {
                base.SetValue<string>("StoreLocation", value);
            }
        }

        [DataMember(Name = "StoreName", EmitDefaultValue = false, Order = 1)]
        public string StoreName
        {
            get
            {
                return base.GetValue<string>("StoreName");
            }

            set
            {
                base.SetValue<string>("StoreName", value);
            }
        }

        [DataMember(Name = "Thumbprint", EmitDefaultValue = false, Order = 2)]
        public string Thumbprint
        {
            get
            {
                return base.GetValue<string>("Thumbprint");
            }

            set
            {
                base.SetValue<string>("Thumbprint", value);
            }
        }
    }

    [CollectionDataContract(Name = "CertificateSettings", Namespace = Constants.ServiceManagementNS)]
    public class CertificateSettingList : List<CertificateSetting>
    {
    }

    [CollectionDataContract(Name = "SubnetNames", ItemName = "SubnetName", Namespace = Constants.ServiceManagementNS)]
    public class SubnetNamesCollection : Collection<string>
    {
    }

    #endregion

    #region DataDisk
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class DataVirtualHardDisk : Mergable<DataVirtualHardDisk>
    {
        [DataMember(Name = "HostCaching", EmitDefaultValue = false, Order = 0)]
        public string HostCaching
        {
            get
            {
                return this.GetValue<string>("HostCaching");
            }

            set
            {
                this.SetValue("HostCaching", value);
            }
        }

        [DataMember(Name = "DiskLabel", EmitDefaultValue = false, Order = 1)]
        public string DiskLabel
        {
            get
            {
                return this.GetValue<string>("DiskLabel");
            }

            set
            {
                this.SetValue("DiskLabel", value);
            }
        }

        [DataMember(Name = "DiskName", EmitDefaultValue = false, Order = 2)]
        public string DiskName
        {
            get
            {
                return base.GetValue<string>("DiskName");
            }

            set
            {
                base.SetValue("DiskName", value);
            }
        }

        [DataMember(Name = "Lun", EmitDefaultValue = false, Order = 3)]
        public int Lun ////Even though we are changing this to INT now; because it is persisted as XML it could just work fine fro deserialize.
        {
            get
            {
                return this.GetValue<int>("Lun");
            }

            set
            {
                this.SetValue("Lun", value);
            }
        }

        [DataMember(Name = "LogicalDiskSizeInGB", EmitDefaultValue = false, Order = 4)]
        private int? logicalDiskSizeInGB
        {
            get
            {
                return this.GetField<int>("LogicalDiskSizeInGB");
            }

            set
            {
                this.SetField("LogicalDiskSizeInGB", value);
            }
        }

        public int LogicalDiskSizeInGB
        {
            get
            {
                return this.GetValue<int>("LogicalDiskSizeInGB");
            }

            set
            {
                this.SetValue("LogicalDiskSizeInGB", value);
            }
        }

        [DataMember(Name = "MediaLink", EmitDefaultValue = false, Order = 5)]
        public Uri MediaLink
        {
            get
            {
                return this.GetValue<Uri>("MediaLink");
            }

            set
            {
                this.SetValue("MediaLink", value);
            }
        }

        [DataMember(Name = "SourceMediaLink", EmitDefaultValue = false, Order = 6)]
        public Uri SourceMediaLink
        {
            get
            {
                return this.GetValue<Uri>("SourceMediaLink");
            }

            set
            {
                this.SetValue("SourceMediaLink", value);
            }
        }
    }
    #endregion

    #region OSDisk
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class OSVirtualHardDisk : Mergable<OSVirtualHardDisk>
    {
        [DataMember(Name = "HostCaching", EmitDefaultValue = false, Order = 0)]
        public string HostCaching
        {
            get
            {
                return this.GetValue<string>("HostCaching");
            }

            set
            {
                this.SetValue("HostCaching", value);
            }
        }

        [DataMember(Name = "DiskLabel", EmitDefaultValue = false, Order = 1)]
        public string DiskLabel
        {
            get
            {
                return this.GetValue<string>("DiskLabel");
            }

            set
            {
                this.SetValue("DiskLabel", value);
            }
        }

        [DataMember(Name = "DiskName", EmitDefaultValue = false, Order = 2)]
        public string DiskName
        {
            get
            {
                return this.GetValue<string>("DiskName");
            }

            set
            {
                this.SetValue("DiskName", value);
            }
        }

        [DataMember(Name = "MediaLink", EmitDefaultValue = false, Order = 3)]
        public Uri MediaLink
        {
            get
            {
                return this.GetValue<Uri>("MediaLink");
            }

            set
            {
                this.SetValue("MediaLink", value);
            }
        }

        [DataMember(Name = "SourceImageName", EmitDefaultValue = false, Order = 4)]
        public string SourceImageName
        {
            get
            {
                return this.GetValue<string>("SourceImageName");
            }

            set
            {
                this.SetValue("SourceImageName", value);
            }
        }

        [DataMember(Name = "OS", EmitDefaultValue = false, Order = 5)]
        public string OS
        {
            get
            {
                return this.GetValue<string>("OS");
            }

            set
            {
                this.SetValue("OS", value);
            }
        }
    }
    #endregion

    #region RoleOperation
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class RoleOperation : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 0)]
        public virtual string OperationType
        {
            get;
            set;
        }

        protected RoleOperation()
        {
        }

        #region IExtensibleDataObject Members

        public ExtensionDataObject ExtensionData
        {
            get;
            set;
        }

        #endregion
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class ShutdownRoleOperation : RoleOperation
    {
        public override string OperationType
        {
            get
            {
                return "ShutdownRoleOperation";
            }

            set
            {
            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class StartRoleOperation : RoleOperation
    {
        public override string OperationType
        {
            get
            {
                return "StartRoleOperation";
            }

            set
            {
            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class RestartRoleOperation : RoleOperation
    {
        public override string OperationType
        {
            get
            {
                return "RestartRoleOperation";
            }

            set
            {
            }
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class CaptureRoleOperation : RoleOperation
    {
        public override string OperationType
        {
            get
            {
                return "CaptureRoleOperation";
            }

            set
            {
            }
        }

        [DataMember(EmitDefaultValue = false, Order = 0)]
        public string PostCaptureAction { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public ProvisioningConfigurationSet ProvisioningConfiguration
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string TargetImageLabel { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public string TargetImageName { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum PostCaptureAction
    {
        [EnumMember]
        Delete,
        [EnumMember]
        Reprovision
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum PowerState
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        Starting,
        [EnumMember]
        Started,
        [EnumMember]
        Stopping,
        [EnumMember]
        Stopped,
    }
    #endregion
}
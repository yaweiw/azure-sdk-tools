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
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a virtual network site belonging to a 
    /// subscription.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class VirtualNetworkSite : IExtensibleDataObject
    {
        public VirtualNetworkSite(string name)
        {
            this.Name = name;
        }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; private set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string Id { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public string Label { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public string AffinityGroup { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        public string State { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 6)]
        public bool InUse { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 7)]
        public AddressSpace AddressSpace { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 8)]
        public SubnetList Subnets { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 9)]
        public DnsSettings Dns { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public Gateway Gateway { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "VirtualNetworkSites", ItemName = "VirtualNetworkSite", Namespace = Constants.ServiceManagementNS)]
    public class VirtualNetworkSiteList : List<VirtualNetworkSite>
    {
        public VirtualNetworkSiteList()
        {
        }

        public VirtualNetworkSiteList(IEnumerable<VirtualNetworkSite> virtualNetworkSites)
            : base(virtualNetworkSites)
        {
        }
    }

    /// <summary>
    /// Represents a <see cref="DnsServer"/>
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class DnsServer : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string Address { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "DnsServers", ItemName = "DnsServer", Namespace = Constants.ServiceManagementNS)]
    public class DnsServerList : List<DnsServer>
    {
    }

    /// <summary>
    /// Represents the <see cref="DnsSettings"/> that can be assigned
    /// to a <see cref="VirtualNetworkSite"/> or a <see cref="Deployment"/>
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class DnsSettings : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public DnsServerList DnsServers { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Represents a Gateway used to access to a <see cref="LocalNetworkSite"/>
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Gateway : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Profile { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public LocalNetworkSiteList Sites { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Represents a <see cref="LocalNetworkSite"/> that provides access to 
    /// onpremise networks.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class LocalNetworkSite : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public AddressSpace AddressSpace { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public string VpnGatewayAddress { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "LocalNetworkSites", ItemName = "LocalNetworkSite", Namespace = Constants.ServiceManagementNS)]
    public class LocalNetworkSiteList : List<LocalNetworkSite>
    {
    }

    [CollectionDataContract(Name = "AddressPrefixes", ItemName = "AddressPrefix", Namespace = Constants.ServiceManagementNS)]
    public class AddressPrefixList : List<string>
    {
    }

    /// <summary>
    /// Represents a network address space.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class AddressSpace : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public AddressPrefixList AddressPrefixes { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Represensts a network subnet.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Subnet : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string AddressPrefix { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "Subnets", ItemName = "Subnet", Namespace = Constants.ServiceManagementNS)]
    public class SubnetList : List<Subnet>
    {
    }

    /// <summary>
    /// Represents the Gateway Configuration a Virtual Network.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class VirtualNetworkGatewayConfiguration : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string GatewayIPAddress { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string GatewayMacAddress { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Represents the tenant size for a Gateway.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum GatewaySize
    {
        [EnumMember]
        Small = 0,

        [EnumMember]
        Medium = 1,

        [EnumMember]
        Large = 2,

        [EnumMember]
        ExtraLarge = 3
    }

    /// <summary>
    /// Represents the state of a network.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public enum NetworkState
    {
        [EnumMember]
        Created = 0,

        [EnumMember]
        Creating = 1,

        [EnumMember]
        Updating = 2,

        [EnumMember]
        Deleting = 3,

        [EnumMember]
        Unavailable = 4
    }
}

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
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Name = "Swap", Namespace = Constants.ServiceManagementNS)]
    public class SwapDeploymentInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Production { get; set; }

        [DataMember(Order = 2)]
        public string SourceDeployment { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// This class represents a deployment in our deployment-related operations.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    ////[ODataType(Key = "Name")]
    public class Deployment : IExtensibleDataObject
    {
        public Deployment(string name, string slot, string status)
        {
            Name = name;
            Status = status;
            DeploymentSlot = slot;
        }

        public Deployment()
        {

        }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string DeploymentSlot { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string PrivateID { get; set; }

        /// <summary>
        /// The class DeploymentStatus defines its possible values. 
        /// </summary>
        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string Status { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public string Label { get; set; }

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public Uri Url { get; set; }

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public string Configuration { get; set; }

        [DataMember(Order = 8, EmitDefaultValue = false)]
        public RoleInstanceList RoleInstanceList { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public UpgradeStatus UpgradeStatus { get; set; }

        [DataMember(Order = 11, EmitDefaultValue = false)]
        public int UpgradeDomainCount { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        public RoleList RoleList { get; set; }

        [DataMember(Order = 13, EmitDefaultValue = false)]
        public string SdkVersion { get; set; }

        [DataMember(Order = 14, EmitDefaultValue = false)]
        public bool? Locked { get; set; }

        [DataMember(Order = 15, EmitDefaultValue = false)]
        public bool? RollbackAllowed { get; set; }

        [DataMember(Order = 16, EmitDefaultValue = false)]
        public string VirtualNetworkName { get; set; }

        [DataMember(Order = 17, EmitDefaultValue = false)]
        public string CreatedTime { get; set; }

        [DataMember(Order = 18, EmitDefaultValue = false)]
        public string LastModifiedTime { get; set; }

        [DataMember(Order = 19, EmitDefaultValue = false)]
        public ExtendedPropertiesList ExtendedProperties { get; set; }

        [DataMember(Order = 20, EmitDefaultValue = false)]
        public DnsSettings Dns { get; set; }

        [DataMember(Order = 21, EmitDefaultValue = false)]
        public PersistentVMDowntimeInfo PersistentVMDowntime { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class PersistentVMDowntimeInfo : IExtensibleDataObject
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public DateTime? StartTime { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public DateTime? EndTime { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Status { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Name = "RoleList", ItemName = "Role", Namespace = Constants.ServiceManagementNS)]
    public class RoleList : List<Role>
    {
        public RoleList()
        {
        }

        public RoleList(IEnumerable<Role> roles)
            : base(roles)
        {
        }
    }

    [CollectionDataContract(Name = "RoleInstanceList", ItemName = "RoleInstance", Namespace = Constants.ServiceManagementNS)]
    public class RoleInstanceList : List<RoleInstance>
    {
        public RoleInstanceList()
        {
        }

        public RoleInstanceList(IEnumerable<RoleInstance> roles)
            : base(roles)
        {
        }
    }

    // @todo: this should implement IExtensibleDataObject. Can we do this without destroying backwards compatibility???
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class RoleInstance : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string RoleName { get; set; }

        [DataMember(Order = 2)]
        public string InstanceName { get; set; }

        [DataMember(Order = 3)]
        public string InstanceStatus { get; set; }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        public int? InstanceUpgradeDomain { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public int? InstanceFaultDomain { get; set; }

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public string InstanceSize { get; set; }

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public string InstanceStateDetails { get; set; }

        [DataMember(Order = 8, EmitDefaultValue = false)]
        public string InstanceErrorCode { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public string IpAddress { get; set; }

        [DataMember(Order = 11, EmitDefaultValue = false)]
        public InstanceEndpointList InstanceEndpoints { get; set; }

        [DataMember(Order = 12, EmitDefaultValue = false)]
        public string PowerState { get; set; }

        [DataMember(Order = 13, EmitDefaultValue = false)]
        public string HostName { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "CreateDeployment", Namespace = Constants.ServiceManagementNS)]
    public class CreateDeploymentInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public Uri PackageUrl { get; set; }

        [DataMember(Order = 3)]
        public string Label { get; set; }

        [DataMember(Order = 4)]
        public string Configuration { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public bool? StartDeployment { get; set; }

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public bool? TreatWarningsAsError { get; set; }

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public ExtendedPropertiesList ExtendedProperties { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "ChangeConfiguration", Namespace = Constants.ServiceManagementNS)]
    public class ChangeConfigurationInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Configuration { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public bool? TreatWarningsAsError { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Mode { get; set; }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        public ExtendedPropertiesList ExtendedProperties { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "UpdateDeploymentStatus", Namespace = Constants.ServiceManagementNS)]
    public class UpdateDeploymentStatusInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Status { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "UpgradeDeployment", Namespace = Constants.ServiceManagementNS)]
    public class UpgradeDeploymentInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Mode { get; set; }

        [DataMember(Order = 2)]
        public Uri PackageUrl { get; set; }

        [DataMember(Order = 3)]
        public string Configuration { get; set; }

        [DataMember(Order = 4)]
        public string Label { get; set; }

        [DataMember(Order = 5)]
        public string RoleToUpgrade { get; set; }

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public bool? TreatWarningsAsError { get; set; }

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public bool? Force { get; set; }

        [DataMember(Order = 8, EmitDefaultValue = false)]
        public ExtendedPropertiesList ExtendedProperties { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "WalkUpgradeDomain", Namespace = Constants.ServiceManagementNS)]
    public class WalkUpgradeDomainInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public int UpgradeDomain { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class UpgradeStatus : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string UpgradeType { get; set; }

        [DataMember(Order = 2)]
        public string CurrentUpgradeDomainState { get; set; }

        [DataMember(Order = 3)]
        public int CurrentUpgradeDomain { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "RollbackUpdateOrUpgrade", Namespace = Constants.ServiceManagementNS)]
    public class RollbackUpdateOrUpgradeInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Mode { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public bool? Force { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [CollectionDataContract(Namespace = Constants.ServiceManagementNS)]
    public class InstanceEndpointList : List<InstanceEndpoint>
    {
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class InstanceEndpoint : IExtensibleDataObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string Vip { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public int PublicPort { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public int LocalPort { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        public string Protocol { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}

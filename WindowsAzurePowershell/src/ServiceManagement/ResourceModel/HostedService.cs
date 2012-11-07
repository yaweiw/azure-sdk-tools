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

    /// <summary>
    /// A list of hosted services
    /// </summary>
    [CollectionDataContract(Name = "HostedServices", ItemName = "HostedService", Namespace = Constants.ServiceManagementNS)]
    ////[ODataCollection(ODataName = "HostedServices", Link = "services/hostedservices")]
    public class HostedServiceList : List<HostedService>
    {
        public HostedServiceList()
        {
        }

        public HostedServiceList(IEnumerable<HostedService> hostedServices)
            : base(hostedServices)
        {
        }
    }

    /// <summary>
    /// A hosted service
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    ////[ODataType(Key = "ServiceName")]
    public class HostedService : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public Uri Url { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string ServiceName { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public HostedServiceProperties HostedServiceProperties { get; set; }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        ////[ODataCollection]
        public DeploymentList Deployments { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public bool? IsComplete { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// A list of deployments contained in the hosted service
    /// </summary>
    [CollectionDataContract(Name = "Deployments", ItemName = "Deployment", Namespace = Constants.ServiceManagementNS)]
    public class DeploymentList : List<Deployment>
    {
        public DeploymentList()
        {
        }

        public DeploymentList(IEnumerable<Deployment> deployments)
            : base(deployments)
        {
        }
    }

    /// <summary>
    /// A hosted service
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class HostedServiceProperties : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Description { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string AffinityGroup { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Location { get; set; }

        [DataMember(Order = 4)]
        public string Label { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        public string Status { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 6)]
        public string DateCreated { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 7)]
        public string DateLastModified { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 8)]
        public ExtendedPropertiesList ExtendedProperties { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// List of locations
    /// </summary>
    [CollectionDataContract(Name = "Locations", ItemName = "Location", Namespace = Constants.ServiceManagementNS)]
    public class LocationList : List<Location>
    {
        public LocationList()
        {
        }

        public LocationList(IEnumerable<Location> locations)
            : base(locations)
        {
        }
    }

    /// <summary>
    /// A location constraint
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Location : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string DisplayName { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public AvailableServicesList AvailableServices { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// List of AvailableServices at a LocationConstraint.
    /// </summary>
    [CollectionDataContract(Namespace = Constants.ServiceManagementNS, Name = "AvailableServices", ItemName = "AvailableService")]
    public class AvailableServicesList : List<string>, IExtensibleDataObject
    {
        public AvailableServicesList()
        {
        }

        public AvailableServicesList(IEnumerable<string> availableServices)
            : base(availableServices)
        {
        }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// CreateHostedService contract
    /// </summary>
    [DataContract(Name = "CreateHostedService", Namespace = Constants.ServiceManagementNS)]
    public class CreateHostedServiceInput : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string ServiceName { get; set; }

        [DataMember(Order = 2)]
        public string Label { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string Location { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public string AffinityGroup { get; set; }

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public ExtendedPropertiesList ExtendedProperties { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// UpdateHostedService contract
    /// </summary>
    [DataContract(Name = "UpdateHostedService", Namespace = Constants.ServiceManagementNS)]
    public class UpdateHostedServiceInput : IExtensibleDataObject
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Label { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Location { get; set; }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string AffinityGroup { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public ExtendedPropertiesList ExtendedProperties { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "AvailabilityResponse", Namespace = Constants.ServiceManagementNS)]
    public class AvailabilityResponse : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public bool Result { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}

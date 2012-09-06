//------------------------------------------------------------------------------
// <copyright file="CapacityDefinition.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Web.Hosting.Administration;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class StampCapacity
    {

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public long AvailableCapacity { get; set; }

        [DataMember]
        public long TotalCapacity { get; set; }

        [DataMember]
        public string Unit { get; set; }

        [DataMember(IsRequired = false)]
        public ComputeModeOptions? ComputeMode { get; set; }

        [DataMember(IsRequired = false)]
        public WorkerSizeOptions? WorkerSize { get; set; }

        [DataMember(IsRequired = false)]
        public bool ExcludeFromCapacityAllocation { get; set; }


        [DataMember]
        public bool IsApplicableForAllComputeModes { get; set; }

        [DataMember(IsRequired = false)]
        public string SiteMode { get; set; }

        public StampCapacity()
        {
        }

        public StampCapacity(string name, string unit, long available, long total)
            : this()
        {
            Name = name;
            AvailableCapacity = available;
            TotalCapacity = total;
            Unit = unit;
        }
    }

    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class StampCapacities : List<StampCapacity>
    {

        public StampCapacities()
        {
        }

        public StampCapacities(List<StampCapacity> capacities)
            : base(capacities)
        {
        }
    }

}

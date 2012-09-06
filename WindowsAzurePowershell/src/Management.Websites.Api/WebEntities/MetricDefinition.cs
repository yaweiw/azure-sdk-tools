//------------------------------------------------------------------------------
// <copyright file="MetricDefinition.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class MetricDefinition
    {

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Unit { get; set; }

        [DataMember]
        public string PrimaryAggregationType { get; set; }

        [DataMember]
        public List<MetricAvailabilily> MetricAvailabilities { get; set; }

        public MetricDefinition()
        {
            MetricAvailabilities = new List<MetricAvailabilily>();
        }

        public MetricDefinition(string name, string unit, string primaryAggregationType)
            : this()
        {
            Name = name;
            Unit = unit;
            PrimaryAggregationType = primaryAggregationType;
        }
    }

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class MetricAvailabilily
    {

        [DataMember]
        public TimeSpan TimeGrain { get; set; }

        [DataMember]
        public TimeSpan Retention { get; set; }

        public MetricAvailabilily()
        {
        }

        public MetricAvailabilily(TimeSpan timeGrain, TimeSpan retention)
        {
            TimeGrain = timeGrain;
            Retention = retention;
        }
    }

    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class MetricDefinitions : List<MetricDefinition>
    {

        public MetricDefinitions()
        {
        }

        public MetricDefinitions(List<MetricDefinition> metricDefinitions)
            : base(metricDefinitions)
        {
        }
    }

}

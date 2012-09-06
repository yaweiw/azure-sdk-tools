//------------------------------------------------------------------------------
// <copyright file="MetricSet.cs" company="Microsoft">
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
    public class MetricSet
    {

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Unit { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public TimeSpan TimeGrain { get; set; }

        [DataMember]
        public string PrimaryAggregationType { get; set; }

        [DataMember]
        public List<MetricSample> Values { get; set; }

        public MetricSet()
        {
            Values = new List<MetricSample>();
        }

        public MetricSet(string name, string units, DateTime startTime, DateTime endTime, TimeSpan timeGrain, string primaryAggregationType)
            : this()
        {
            Name = name;
            Unit = units;
            StartTime = startTime;
            EndTime = endTime;
            TimeGrain = timeGrain;
            PrimaryAggregationType = primaryAggregationType;
        }

    }

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class MetricSample
    {

        [DataMember]
        public DateTime TimeCreated { get; set; }

        [DataMember]
        public long? Total { get; set; }

        [DataMember]
        public long? Minimum { get; set; }

        [DataMember]
        public long? Maximum { get; set; }

        [DataMember]
        public long? Count { get; set; }

        public MetricSample()
        {
        }

        public MetricSample(long total, DateTime timeCreated)
            : base()
        {
            Total = total;
            TimeCreated = timeCreated;
            Count = 1;
        }
    }

    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class MetricSets : List<MetricSet>
    {
        public MetricSets()
        {
        }

        public MetricSets(List<MetricSet> metricSets)
            : base(metricSets)
        {
        }
    }
}

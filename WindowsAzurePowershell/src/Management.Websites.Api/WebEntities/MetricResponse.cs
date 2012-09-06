//------------------------------------------------------------------------------
// <copyright file="MetricResponse.cs" company="Microsoft">
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
    public class MetricResponse
    {
        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        [DataMember]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        [DataMember]
        public MetricSet Data { get; set; }
    }

    /// <summary>
    /// Collection of ResourceMetricResponses
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class MetricResponses : List<MetricResponse>
    {
        public MetricResponses()
            : base()
        {
        }

        public MetricResponses(IList<MetricResponse> list)
            : base(list)
        {
        }
    }

}

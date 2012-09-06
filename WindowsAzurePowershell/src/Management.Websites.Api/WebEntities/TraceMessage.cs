//------------------------------------------------------------------------------
// <copyright file="TraceMessage.cs" company="Microsoft">
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
    public class TraceMessage
    {
        [DataMember(IsRequired = false)]
        public string Message { get; set; }

        [DataMember(IsRequired = false)]
        public int MessageId { get; set; }

        [DataMember(IsRequired = false)]
        public string ServerName { get; set; }

        [DataMember(IsRequired = false)]
        public DateTime TimeStamp { get; set; }

        [DataMember(IsRequired = false)]
        public int TraceLevel { get; set; }

    }

    /// <summary>
    /// Collection of trace messages
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class TraceMessages : List<TraceMessage>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public TraceMessages() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="users"></param>
        public TraceMessages(List<TraceMessage> traceMessages) : base(traceMessages) { }
    }
}

//------------------------------------------------------------------------------
// <copyright file="AuditLog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Web.Hosting.Administration;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class AuditLog
    {
        [DataMember(IsRequired = false)]
        public int Id { get; set; }

        [DataMember(IsRequired = false)]
        public string User { get; set; }

        [DataMember(IsRequired = false)]
        [PIIValue]
        public string UserAddress { get; set; }

        [DataMember(IsRequired = false)]
        public DateTime Logged { get; set; }

        [DataMember(IsRequired = false)]
        public string Protocol { get; set; }

        [DataMember(IsRequired = false)]
        public int Level { get; set; }

        [DataMember(IsRequired = false)]
        public string Source { get; set; }

        [DataMember(IsRequired = false)]
        public string MessageIdentifier { get; set; }

        [DataMember(IsRequired = false)]
        public string Message { get; set; }

        [DataMember(IsRequired = false)]
        public string Details { get; set; }
    }

    /// <summary>
    /// Collection of audit logs
    /// </summary>
    [CollectionDataContract]
    public class AuditLogs : List<AuditLog>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public AuditLogs() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="sites"></param>
        public AuditLogs(List<AuditLog> auditLogs) : base(auditLogs) { }
    }
}

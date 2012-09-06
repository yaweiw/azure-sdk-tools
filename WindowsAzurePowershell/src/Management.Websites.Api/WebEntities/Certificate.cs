//------------------------------------------------------------------------------
// <copyright file="Certificate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using System;
    using Microsoft.Management.Websites.Api;
    using Web.Hosting.Administration;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Certificate
    {
        [DataMember(IsRequired = false)]
        public string FriendlyName { get; set; }

        [DataMember(IsRequired = false)]
        public string SubjectName { get; set; }

        [DataMember(IsRequired = false)]
        public string HostName { get; set; }

        [DataMember(IsRequired = false)]
        [PIIValue]
        public byte[] PfxBlob { get; set; }

        [DataMember(IsRequired = false)]
        public string SiteName { get; set; }

        [DataMember(IsRequired = false)]
        public Uri SelfLink { get; set; }

        [DataMember(IsRequired = false)]
        public string Issuer { get; set; }

        [DataMember(IsRequired = false)]
        public DateTime IssueDate { get; set; }

        [DataMember(IsRequired = false)]
        public DateTime ExpirationDate { get; set; }

        [DataMember(IsRequired = false)]
        public string Password { get; set; }

        [DataMember(IsRequired = false)]
        public string Thumbprint { get; set; }

        [DataMember(IsRequired = false)]
        public bool? Valid { get; set; }
    }
}
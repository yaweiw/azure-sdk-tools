//------------------------------------------------------------------------------
// <copyright file="SiteConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using Api;
    using Web.Hosting.Administration;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class SiteConfig
    {
        [DataMember(IsRequired = false)]
        public int? NumberOfWorkers { get; set; }

        [DataMember(IsRequired = false)]
        public string[] DefaultDocuments { get; set; }

        [DataMember(IsRequired = false)]
        public string NetFrameworkVersion { get; set; }

        [DataMember(IsRequired = false)]
        public string PhpVersion { get; set; }

        [DataMember(IsRequired = false)]
        public bool? RequestTracingEnabled { get; set; }

        [DataMember(IsRequired = false)]
        public bool? HttpLoggingEnabled { get; set; }

        [DataMember(IsRequired = false)]
        public bool? DetailedErrorLoggingEnabled { get; set; }

        [DataMember(IsRequired = false)]
        public string PublishingUsername { get; set; }

        [DataMember(IsRequired = false)]
        [PIIValue]
        public string PublishingPassword { get; set; }

        [DataMember(IsRequired = false)]
        public NameValuePropertyBag AppSettings { get; set; }

        [DataMember(IsRequired = false)]
        public NameValuePropertyBag Metadata { get; set; }

        [DataMember(IsRequired = false)]
        public ConnStringPropertyBag ConnectionStrings { get; set; }

        [DataMember(IsRequired = false)]
        public HandlerMapping[] HandlerMappings { get; set; }
    }
}

//------------------------------------------------------------------------------
// <copyright file="ConfigurationSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class ConfigurationSettings
    {
        [DataMember(IsRequired = false)]
        public int? ChangeNotificationPollingTime { get; set; }

        [DataMember(IsRequired = false)]
        [PIIValue]
        public string AuthenticationConnectionString { get; set; }

        [DataMember(IsRequired = false)]
        public bool? PublishingAuditLogEnabled { get; set; }

        [DataMember(IsRequired = false)]
        public Certificate DefaultDomainCertificate { get; set; }

        [DataMember(IsRequired = false)]
        public Certificate WebDeployCertificate { get; set; }

        [DataMember(IsRequired = false)]
        public Certificate FtpCertificate { get; set; }
    }
}
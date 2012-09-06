//------------------------------------------------------------------------------
// <copyright file="SiteRepositoryDev.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class SiteRepositoryDev
    {

        [DataMember(IsRequired = false)]
        public string WebRootSubPath { get; set; }

        [DataMember(IsRequired = false)]
        public Uri SiteUri { get; set; }
    }
}

//------------------------------------------------------------------------------
// <copyright file="SiteProperties.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class SiteProperties
    {

        [DataMember(IsRequired = true)]
        public NameValuePropertyBag Properties { get; set; }

        [DataMember(IsRequired = true)]
        public NameValuePropertyBag Metadata { get; set; }
    }
}

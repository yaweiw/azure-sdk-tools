//------------------------------------------------------------------------------
// <copyright file="PublishingCredential.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class PublishingCredential
    {
        [DataMember(IsRequired = true)]
        public string UserName { get; set; }

        [DataMember(IsRequired = true)]
        [PIIValue]
        public string Password { get; set; }
    }
}

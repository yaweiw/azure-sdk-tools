//------------------------------------------------------------------------------
// <copyright file="PermissionScopePair.cs" company="Microsoft">
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
    public class PermissionScopePair
    {

        [DataMember(IsRequired = true)]
        public Permission Permission { get; set; }

        [DataMember(IsRequired = true)]
        [PIIValue]
        public PermissionScope Scope { get; set; }
    }

    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class PermissionScopePairs : List<PermissionScopePair>
    {

        public PermissionScopePairs()
        {
        }

        public PermissionScopePairs(List<PermissionScopePair> nameValuePairs)
            : base(nameValuePairs)
        {
        }
    }
}

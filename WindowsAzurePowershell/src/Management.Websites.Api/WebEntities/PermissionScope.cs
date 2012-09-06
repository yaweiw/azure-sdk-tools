//------------------------------------------------------------------------------
// <copyright file="PermissionScope.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Name = "PermissionScope", Namespace = UriElements.ServiceNamespace)]
    public enum PermissionScope
    {
        [EnumMember]
        Current = 0, // Permission applies to Current entity only and not to the sub-trees.

        [EnumMember]
        InheritOnly = 1, // Permission applies to sub-trees only and Not to the current entity.

        [EnumMember]
        All = 3 // Current and Inherit Only
    }
}

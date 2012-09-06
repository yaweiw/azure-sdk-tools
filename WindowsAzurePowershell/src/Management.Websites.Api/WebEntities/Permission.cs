//------------------------------------------------------------------------------
// <copyright file="Permission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Name = "Permission", Namespace = UriElements.ServiceNamespace)]
    public enum Permission
    {
        [EnumMember]
        Create = 0, // Can create an entity. Usually used in conjunction with All/InheritOnly scope to create children.
        
        [EnumMember]
        Read = 1, // Can read an entity

        [EnumMember]
        Update = 2, // Can update an entity

        [EnumMember]
        Delete = 3, // Can delete an entity

        [EnumMember]
        Publish = 4, // Can publish content an entity. Applicable to websites only

        [EnumMember]
        Admin = 5 // Create, Read, Update, Delete and Publish
    }
}

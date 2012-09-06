//------------------------------------------------------------------------------
// <copyright file="User.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class User
    {

        [DataMember(IsRequired = false)]
        [PIIValue]
        public string Name { get; set; }

        [DataMember(IsRequired = false)]
        public string PublishingUserName { get; set; }

        [DataMember(IsRequired = false)]
        [PIIValue]
        public string PublishingPassword { get; set; }
    }

    /// <summary>
    /// Collection of users
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Users : List<User>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Users() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="users"></param>
        public Users(List<User> users) : base(users) { }
    }
}

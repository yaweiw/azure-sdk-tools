//------------------------------------------------------------------------------
// <copyright file="ConnectionString.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Api;
    using Utilities;

    /// <summary>
    /// Class that represents a connection string.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class ConnectionString
    {
        /// <summary>
        /// Name for the database
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Connection string
        /// </summary>
        [DataMember]
        [PIIValue]
        public string Value { get; set; }

    }

    /// <summary>
    /// Collection of connection strings
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class ConnectionStrings : List<ConnectionString>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public ConnectionStrings() { }

        /// <summary>
        /// Initialize from list
        /// </summary>
        /// <param name="plans"></param>
        public ConnectionStrings(List<ConnectionString> list) : base(list) { }
    }
}

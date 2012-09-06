//------------------------------------------------------------------------------
// <copyright file="Database.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Web.Hosting.Administration;

    /// <summary>
    /// Database resource
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Database
    {
        /// <summary>
        /// The name of the database
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Database password
        /// </summary>
        [DataMember]
        [PIIValue]
        public string Password { get; set; }

        /// <summary>
        /// Database connection string
        /// </summary>
        [DataMember]
        [PIIValue]
        public string ConnectionString { get; set; }
    }

    /// <summary>
    /// Collection of MySqlDatabases
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Databases : List<Database>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Databases() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="databases"></param>
        public Databases(List<Database> databases) : base(databases) { }
    }
}

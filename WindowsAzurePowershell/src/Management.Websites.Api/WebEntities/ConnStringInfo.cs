//------------------------------------------------------------------------------
// <copyright file="NameValuePair.cs" company="Microsoft">
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
    public class ConnStringInfo
    {

        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        [DataMember(IsRequired = true)]
        [PIIValue]
        public string ConnectionString { get; set; }

        [DataMember(IsRequired = true)]
        public DatabaseType Type { get; set; }
    }

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public enum DatabaseType
    {
        [EnumMember]
        MySql,
        [EnumMember]
        SQLServer,
        [EnumMember]
        SQLAzure,
        [EnumMember]
        Custom
    }

    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class ConnStringPropertyBag : List<ConnStringInfo>
    {

        public ConnStringPropertyBag()
        {
        }

        public ConnStringPropertyBag(List<ConnStringInfo> connections)
            : base(connections)
        {
        }
    }
}

//------------------------------------------------------------------------------
// <copyright file="PagedSet.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class PagedSet<T>
    {

        [DataMember]
        public string Filter { get; set; }

        [DataMember]
        public int PageNumber { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int TotalRecords { get; set; }

        [DataMember]
        public List<T> Values { get; set; }

        public PagedSet()
        {
            Values = new List<T>();
        }
    }
}

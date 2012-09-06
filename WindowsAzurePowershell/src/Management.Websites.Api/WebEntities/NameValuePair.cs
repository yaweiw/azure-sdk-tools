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
    public class NameValuePair
    {

        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        [DataMember(IsRequired = true)]
        [PIIValue]
        public string Value { get; set; }
    }

    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class NameValuePropertyBag : List<NameValuePair>
    {

        public NameValuePropertyBag()
        {
        }

        public NameValuePropertyBag(List<NameValuePair> nameValuePairs)
            : base(nameValuePairs)
        {
        }
    }
}

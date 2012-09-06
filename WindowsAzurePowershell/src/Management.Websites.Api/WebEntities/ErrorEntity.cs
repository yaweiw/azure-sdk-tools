//------------------------------------------------------------------------------
// <copyright file="ErrorEntity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;

    /// <summary>
    /// Body of the error response returned from the API.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace, Name = "Error")]
    public class ErrorEntity
    {
        /// <summary>
        /// Basic error code
        /// </summary>
        [DataMember(Order = 1)]
        public string Code { get; set; }

        /// <summary>
        /// Any details of the error
        /// </summary>
        [DataMember(Order = 2)]
        public string Message { get; set; }

        /// <summary>
        /// Type of error
        /// </summary>
        [DataMember(Order = 3)]
        public string ExtendedCode { get; set; }

        /// <summary>
        /// Message template
        /// </summary>
        [DataMember(Order = 4)]
        public string MessageTemplate { get; set; }

        /// <summary>
        /// Parameters for the template
        /// </summary>
        [DataMember(Order = 5)]
        public List<string> Parameters { get; set; }
    }
}

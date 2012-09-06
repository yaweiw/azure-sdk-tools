//------------------------------------------------------------------------------
// <copyright file="HandlerMapping.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class HandlerMapping
    {
        /// <summary>
        /// Requests with this extension will be handled using the specified FastCGI application.
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Extension { get; set; }

        /// <summary>
        /// The path to the FastCGI application.
        /// </summary>
        // TODO: Relative or Absolute?
        [DataMember(IsRequired = true)]
        public string ScriptProcessor { get; set; }

        /// <summary>
        /// Command-line arguments to be passed to the script processor.
        /// </summary>
        [DataMember(IsRequired = false)]
        public string Arguments { get; set; }
    }
}

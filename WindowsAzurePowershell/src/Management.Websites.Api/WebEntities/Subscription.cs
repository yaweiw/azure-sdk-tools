//------------------------------------------------------------------------------
// <copyright file="Subscription.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Web.Hosting.Administration;

    /// <summary>
    /// Class that represents a subscription.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Subscription
    {
        /// <summary>
        /// Name of the subscription
        /// </summary>
        [DataMember]
        [Description("Subscription Name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of the subscription
        /// </summary>
        [DataMember]
        [Description("Subscription Description")]
        public string Description { get; set; }

        /// <summary>
        /// Suspended
        /// </summary>
        [DataMember]
        public bool? Suspended { get; set; }

        /// <summary>
        /// Name of the user who is owner of the Subscription
        /// </summary>
        [DataMember]
        [PIIValue]
        public string OwnerUserName { get; set; }
    }

    /// <summary>
    /// Collection of subscriptions
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Subscriptions : List<Subscription>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Subscriptions() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="subscriptions"></param>
        public Subscriptions(List<Subscription> subscriptions) : base(subscriptions) { }
    }
}

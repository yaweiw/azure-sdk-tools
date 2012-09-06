//------------------------------------------------------------------------------
// <copyright file="Offer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    /// <summary>
    /// Offer Contract (has to be implemented by all resource providers called by the RDFE)
    /// </summary>
    /// Note: Need to keep this data contract in sync with RDFE
    [DataContract(Name = "ServiceOffer", Namespace = UriElements.ServiceNamespace)]
    public class Offer
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string OfferName { get; set; }

        /// <summary>
        /// Gets or sets the offer settings.
        /// </summary>
        /// <value>
        /// The offer settings.
        /// </value>
        [DataMember]
        public IList<OfferSetting> ServiceOfferSettings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Offer"/> class.
        /// </summary>
        public Offer()
        {
            this.ServiceOfferSettings = new List<OfferSetting>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Offer"/> class.
        /// </summary>
        /// <param name="offerName">Name of the offer.</param>
        /// <param name="quotaSettings">Existing quota settings.</param>
        public Offer(string offerName, IList<OfferSetting> serviceOfferSettings)
        {
            this.OfferName = offerName;
            this.ServiceOfferSettings = serviceOfferSettings;
        }
    }

    [DataContract(Name = "ServiceOfferSetting", Namespace = UriElements.ServiceNamespace)]
    public class OfferSetting
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Value { get; set; }
    }

    /// <summary>
    /// Collection of offers
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Offers : List<Offer>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Offers() { }

        /// <summary>
        /// Initialize from list
        /// </summary>
        /// <param name="offers"></param>
        public Offers(List<Offer> offers) : base(offers) { }
    }
}

//------------------------------------------------------------------------------
// <copyright file="WebSystem.cs" company="Microsoft">
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
    /// Class that represents a Web System.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class WebSystem
    {
        /// <summary>
        /// Name for the web system
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Administrator account
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// PAssword for the administrator account
        /// </summary>
        [DataMember]
        [PIIValue]
        public string Password { get; set; }

        /// <summary>
        /// File share for content storage
        /// </summary>
        [DataMember]
        public string FileShare { get; set; }

        /// <summary>
        /// Publishing DNS
        /// </summary>
        [DataMember]
        public string PublishingDns { get; set; }

        /// <summary>
        /// Ftp DNS
        /// </summary>
        [DataMember]
        public string FtpDns { get; set; }


        /// <summary>
        /// Parking page name
        /// </summary>
        [DataMember]
        public string ParkingPage { get; set; }

        /// <summary>
        /// Parking page content
        /// </summary>
        [DataMember]
        public string ParkingPageContent { get; set; }

        /// <summary>
        /// Storage domain
        /// </summary>
        [DataMember]
        public string StorageDomain { get; set; }

        /// <summary>
        /// Databases
        /// </summary>
        [DataMember]
        public ConnectionStrings ConnectionStrings { get; set; }

        /// <summary>
        /// Link to a page that displays the Control Panel for the Web System.
        /// </summary>
        [DataMember]
        public string ControlPanelUrl { get; set; }
    }

    /// <summary>
    /// Collection of web systems
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class WebSystems : List<WebSystem>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public WebSystems() { }

        /// <summary>
        /// Initialize from list
        /// </summary>
        /// <param name="plans"></param>
        public WebSystems(List<WebSystem> list) : base(list) { }
    }
}

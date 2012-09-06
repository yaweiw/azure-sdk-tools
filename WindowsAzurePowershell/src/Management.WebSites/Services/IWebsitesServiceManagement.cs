// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Websites.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ServiceModel.Web;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Serialization;
    using Utilities;
    using WebEntities;

    [XmlRootAttribute(ElementName = "Error", Namespace = UriElements.ServiceNamespace)]
    public class ServiceError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string ExtendedCode { get; set; }
        public string MessageTemplate { get; set; }

        [XmlArray("Parameters")]
        [XmlArrayItem(typeof(string), Namespace="http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
        public List<string> Parameters { get; set; }
    }

    /// <summary>
    /// Provides the Windows Azure Service Management Api for Windows Azure Websites. 
    /// </summary>
    [ServiceContract(Namespace = UriElements.ServiceNamespace)]
    public interface IWebsitesServiceManagement
    {
        [Description("Returns all subscriptions")]
        [WebGet(UriTemplate = UriElements.Root + UriElements.ContinuationParameters)]
        Subscriptions GetSubscriptions(string marker, int recordCount);

        [Description("Returns the subscription details")]
        [WebGet(UriTemplate = UriElements.NameTemplateParameter)]
        Subscription GetSubscription(string name);

        [Description("Creates a new subscription")]
        [WebInvoke(UriTemplate = UriElements.Root, Method = "POST")]
        Subscription CreateSubscription(Subscription subscription);

        [Description("Updates an existing subscription")]
        [WebInvoke(UriTemplate = UriElements.NameTemplateParameter, Method = "PUT")]
        void UpdateSubscription(string name, Subscription subscription);

        [Description("Migrates an existing subscription")]
        [WebInvoke(UriTemplate = UriElements.NameTemplateParameter, Method = "POST")]
        void MigrateSubscription(string name, Subscription targetSubscription);

        [Description("Delete a subscription")]
        [WebInvoke(UriTemplate = UriElements.NameTemplateParameter, Method = "DELETE")]
        void DeleteSubscription(string name);

        [Description("Gets all webspaces for subscription")]
        [WebGet(UriTemplate = UriElements.WebSpacesRoot)]
        WebSpaces GetWebSpaces(string subscriptionName);

        [Description("Gets all webspaces for subscription")]
        [WebGet(UriTemplate = UriElements.WebSpacesRoot + UriElements.NameTemplateParameter)]
        WebSpace GetWebSpace(string subscriptionName, string name);

        [Description("Creates a new webspace")]
        [WebInvoke(UriTemplate = UriElements.WebSpacesRoot + UriElements.AllowPendingStateParameter, Method = "POST")]
        WebSpace CreateWebSpace(string subscriptionName, bool allowPendingState, WebSpace webSpace);

        [Description("Updates an existing webspace")]
        [WebInvoke(UriTemplate = UriElements.WebSpacesRoot + UriElements.NameTemplateParameter + UriElements.AllowPendingStateParameter, Method = "PUT")]
        WebSpace UpdateWebSpace(string subscriptionName, string name, bool allowPendingState, WebSpace webSpace);

        [Description("Deletes a webspace")]
        [WebInvoke(UriTemplate = UriElements.WebSpacesRoot + UriElements.NameTemplateParameter, Method = "DELETE")]
        void DeleteWebSpace(string subscriptionName, string name);

        [Description("Gets quota usages")]
        [WebGet(UriTemplate = UriElements.WebSpaceUsagesRoot)]
        Usages GetUsages(string subscriptionName, string webspaceName, string usages, string computeMode, string siteMode);

        [Description("Gets all publishing users for subscription")]
        [WebGet(UriTemplate = UriElements.SubscriptionPublishingUsers)]
        string[] GetSubscriptionPublishingUsers(string subscriptionName);

        #region Site CRUD

        [Description("Returns all the sites for a given subscription and webspace.")]
        [WebGet(UriTemplate = UriElements.WebSitesRoot + UriElements.PropertiesToIncludeParameter)]
        Sites GetSites(string subscriptionName, string webspaceName, string propertiesToInclude);

        [Description("Returns the details of a particular site.")]
        [WebGet(UriTemplate = UriElements.WebSitesRoot + UriElements.NameTemplateParameter + UriElements.PropertiesToIncludeParameter)]
        Site GetSite(string subscriptionName, string webspaceName, string name, string propertiesToInclude);

        [Description("Adds a new site")]
        [WebInvoke(UriTemplate = UriElements.WebSitesRoot, Method = "POST")]
        Site CreateSite(string subscriptionName, string webspaceName, SiteWithWebSpace site);

        [Description("Updates an existing site")]
        [WebInvoke(UriTemplate = UriElements.WebSitesRoot + UriElements.NameTemplateParameter, Method = "PUT")]
        void UpdateSite(string subscriptionName, string webspaceName, string name, Site site);

        [Description("Deletes an existing site.")]
        [WebInvoke(UriTemplate = UriElements.WebSitesRoot + UriElements.NameTemplateParameter + UriElements.DeleteMetricsParameter, Method = "DELETE")]
        void DeleteSite(string subscriptionName, string webspaceName, string name, string deleteMetrics);

        #endregion

        #region Site configuration settings

        [Description("Gets site's configuration settings")]
        [WebGet(UriTemplate = UriElements.WebSiteConfig)]
        SiteConfig GetSiteConfig(string subscriptionName, string webspaceName, string name);

        [Description("Updates site's configuration settings")]
        [WebInvoke(UriTemplate = UriElements.WebSiteConfig, Method = "PUT")]
        void UpdateSiteConfig(string subscriptionName, string webspaceName, string name, SiteConfig siteConfig);

        #endregion

        #region Repository methods

        [Description("Creates a repository for a site")]
        [WebInvoke(UriTemplate = UriElements.WebSiteRepository, Method = "POST")]
        void CreateSiteRepository(string subscriptionName, string webspaceName, string name);

        [Description("Gets a site's repository URI")]
        [WebGet(UriTemplate = UriElements.WebSiteRepository)]
        Uri GetSiteRepositoryUri(string subscriptionName, string webspaceName, string name);

        [Description("Deletes a site's repository")]
        [WebInvoke(UriTemplate = UriElements.WebSiteRepository, Method = "DELETE")]
        void DeleteSiteRepository(string subscriptionName, string webspaceName, string name);

        [Description("Creates a development site in a site's repository")]
        [WebInvoke(UriTemplate = UriElements.WebSiteRepositoryDev, Method = "POST")]
        void CreateDevSite(string subscriptionName, string webspaceName, string name);

        [Description("Gets a development site in a site's repository")]
        [WebGet(UriTemplate = UriElements.WebSiteRepositoryDev)]
        SiteRepositoryDev GetDevSite(string subscriptionName, string webspaceName, string name);

        [Description("Updates a development site in a site's repository")]
        [WebInvoke(UriTemplate = UriElements.WebSiteRepositoryDev, Method = "PUT")]
        void UpdateDevSite(string subscriptionName, string webspaceName, string name, SiteRepositoryDev repositoryDevSite);

        [Description("Deletes a development site in a site's repository")]
        [WebInvoke(UriTemplate = UriElements.WebSiteRepositoryDev, Method = "DELETE")]
        void DeleteDevSite(string subscriptionName, string webspaceName, string name);

        #endregion

        #region Site usages and metrics

        [Description("Returns the quota usage for a particular site.")]
        [WebGet(UriTemplate = UriElements.WebSiteUsagesRoot)]
        Usages GetUsages(string subscriptionName, string webspaceName, string name, string usages, string computeMode, string siteMode);

        [Description("Returns the usage metrics for a particular site.")]
        [WebGet(UriTemplate = UriElements.WebSiteMetricsRoot + UriElements.MetricsParameters)]
        MetricResponses GetMetrics(string subscriptionName, string webspaceName, string name, string metrics, string startTime, string endTime);

        [Description("Returns the metric definitions for a particular site.")]
        [WebGet(UriTemplate = UriElements.WebSiteMetricDefinitions)]
        MetricDefinitions GetSiteMetricDefinitions(string subscriptionName, string webspaceName, string name);

        #endregion

        #region Misc operations
        [Description("Returns the audit logs for a particular site.")]
        [WebGet(UriTemplate = UriElements.WebSiteAuditLogs)]
        AuditLogs GetAuditLogs(string subscriptionName, string webspaceName, string name, string startTime, string endTime);

        [Description("Returns the last audit log for a particular site, if exists.")]
        [WebGet(UriTemplate = UriElements.WebSiteGetLastAuditLog)]
        AuditLog GetLastAuditLog(string subscriptionName, string webspaceName, string name);

        [Description("Returns all the sites for a given subscription.")]
        [WebGet(UriTemplate = UriElements.WebSitesPerSubscription + UriElements.PropertiesToIncludeParameter)]
        Sites GetSitesPerSubscription(string subscriptionName, string propertiesToInclude);

        [Description("Returns the publishing profile xml for a particular site.")]
        [WebGet(UriTemplate = UriElements.WebSitePublishingProfile)]
        XmlElement GetPublishingProfileXml(string subscriptionName, string webspaceName, string name);

        [Description("Swaps traffic of an existing site with another site.")]
        [WebInvoke(UriTemplate = UriElements.WebSiteSwap, Method = "POST")]
        void SwapSite(string subscriptionName, string webspaceName, string name, string command, string otherSiteName);

        [Description("Checks whether the hostname is available.")]
        [WebGet(UriTemplate = UriElements.HostNameAvailability)]
        bool IsHostNameAvailable(string subDomain);

        [Description("Checks whether the hostname is reserved or not allowed.")]
        [WebGet(UriTemplate = UriElements.HostNameReservedOrNotAllowed)]
        bool IsHostNameReservedOrNotAllowed(string subDomain);

        [Description("Checks site exists.")]
        [WebGet(UriTemplate = UriElements.WebSitesRoot + UriElements.NameTemplateParameter + UriElements.ExistsParameter)]
        bool CheckSiteExists(string subscriptionName, string webspaceName, string name);

        [Description("Restarts the site.")]
        [WebInvoke(UriTemplate = UriElements.WebSiteRestart, Method = "POST")]
        void RestartSite(string subscriptionName, string webspaceName, string name);

        [Description("Checks whether the custom domain is valid for this site.")]
        [WebGet(UriTemplate = UriElements.WebSiteIsValidCustomDomain)]
        void IsValidCustomDomain(string subscriptionName, string webspaceName, string name, string hostName, string recordType);

        #endregion
    }
}

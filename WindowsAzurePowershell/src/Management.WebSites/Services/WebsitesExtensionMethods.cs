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
    using System.Linq;
    using GeoEntities;
    using WebEntities;

    public static class WebsitesExtensionMethods
    {
        public static WebSpaces GetWebSpaces(this IWebsitesServiceManagement proxy, string subscriptionName)
        {
            return proxy.EndGetWebSpaces(proxy.BeginGetWebSpaces(subscriptionName, null, null));
        }

        public static WebSpaces GetWebSpacesWithCache(this IWebsitesServiceManagement proxy, string subscriptionName)
        {
            WebSpaces webSpaces = Cache.GetWebSpaces(subscriptionName);
            if (webSpaces != null)
            {
                return webSpaces;
            }

            webSpaces = GetWebSpaces(proxy, subscriptionName);
            Cache.SaveSpaces(subscriptionName, webSpaces);

            return webSpaces;
        }

        public static WebSpace GetWebSpace(this IWebsitesServiceManagement proxy, string subscriptionName, string name)
        {
            return proxy.EndGetWebSpace(proxy.BeginGetWebSpace(subscriptionName, name, null, null));
        }

        public static WebSpace CreateWebSpace(this IWebsitesServiceManagement proxy, string subscriptionName, bool allowPendingState, WebSpace webSpace)
        {
            return proxy.EndCreateWebSpace(proxy.BeginCreateWebSpace(subscriptionName, allowPendingState, webSpace, null, null));
        }

        public static WebSpace UpdateWebSpace(this IWebsitesServiceManagement proxy, string subscriptionName, string name, bool allowPendingState, WebSpace webSpace)
        {
            return proxy.EndUpdateWebSpace(proxy.BeginUpdateWebSpace(subscriptionName, name, allowPendingState, webSpace, null, null));
        }

        public static void DeleteWebSpace(this IWebsitesServiceManagement proxy, string subscriptionName, string name)
        {
            proxy.EndDeleteWebSpace(proxy.BeginDeleteWebSpace(subscriptionName, name, null, null));
        }

        public static string[] GetSubscriptionPublishingUsers(this IWebsitesServiceManagement proxy, string subscriptionName)
        {
            return proxy.EndGetSubscriptionPublishingUsers(proxy.BeginGetSubscriptionPublishingUsers(subscriptionName, null, null));
        }

        public static Sites GetSites(this IWebsitesServiceManagement proxy, string subscriptionName, string webspaceName, string propertiesToInclude)
        {
            return proxy.EndGetSites(proxy.BeginGetSites(subscriptionName, webspaceName, propertiesToInclude, null, null));
        }

        public static Sites GetSitesWithCache(this IWebsitesServiceManagement proxy, string subscriptionName, string webspaceName, string propertiesToInclude)
        {
            Sites sites = Cache.GetSites(subscriptionName, webspaceName);
            if (sites != null)
            {
                return sites;
            }

            sites = GetSites(proxy, subscriptionName, webspaceName, propertiesToInclude);
            Cache.SaveSites(subscriptionName, sites);
            return sites;
        }

        public static Site GetSite(this IWebsitesServiceManagement proxy, string subscriptionName, string webspaceName, string name, string propertiesToInclude)
        {
            return proxy.EndGetSite(proxy.BeginGetSite(subscriptionName, webspaceName, name, propertiesToInclude, null, null));
        }

        public static Site CreateSite(this IWebsitesServiceManagement proxy, string subscriptionName, string webspaceName, SiteWithWebSpace site)
        {
            return proxy.EndCreateSite(proxy.BeginCreateSite(subscriptionName, webspaceName, site, null, null));
        }

        public static void UpdateSite(this IWebsitesServiceManagement proxy, string subscriptionName, string webspaceName, string name, Site site)
        {
            proxy.EndUpdateSite(proxy.BeginUpdateSite(subscriptionName, webspaceName, name, site, null, null));
        }

        public static void DeleteSite(this IWebsitesServiceManagement proxy, string subscriptionName, string webspaceName, string name, string deleteMetrics)
        {
            proxy.EndDeleteSite(proxy.BeginDeleteSite(subscriptionName, webspaceName, name, deleteMetrics, null, null));
        }

        public static SiteConfig GetSiteConfig(this IWebsitesServiceManagement proxy, string subscriptionName, string webspaceName, string name)
        {
            return proxy.EndGetSiteConfig(proxy.BeginGetSiteConfig(subscriptionName, webspaceName, name, null, null));
        }

        public static void UpdateSiteConfig(this IWebsitesServiceManagement proxy, string subscriptionName, string webspaceName, string name, SiteConfig siteConfig)
        {
            proxy.EndUpdateSiteConfig(proxy.BeginUpdateSiteConfig(subscriptionName, webspaceName, name, siteConfig, null, null));
        }

        public static void CreateSiteRepository(this IWebsitesServiceManagement proxy, string subscriptionName, string webspaceName, string name)
        {
            proxy.EndCreateSiteRepository(proxy.BeginCreateSiteRepository(subscriptionName, webspaceName, name, null, null));
        }

        public static GeoRegions GetRegions(this IWebsitesServiceManagement proxy, bool listOnlyOnline)
        {
            return proxy.EndGetRegions(proxy.BeginGetRegions(listOnlyOnline, null, null));
        }

        public static GeoLocations GetLocations(this IWebsitesServiceManagement proxy, string regionName)
        {
            return proxy.EndGetLocations(proxy.BeginGetLocations(regionName, null, null));
        }

        public static Site GetSite(this IWebsitesServiceManagement proxy, string subscriptionId, string website, string propertiesToInclude)
        {
            var webspaces = proxy.GetWebSpacesWithCache(subscriptionId);
            foreach (var webspace in webspaces)
            {
                var websites = proxy.GetSitesWithCache(subscriptionId, webspace.Name, propertiesToInclude);
                var matchWebsite = websites.FirstOrDefault(w => w.Name.Equals(website));
                if (matchWebsite != null)
                {
                    return matchWebsite;
                }
            }

            return null;
        }
    }
}
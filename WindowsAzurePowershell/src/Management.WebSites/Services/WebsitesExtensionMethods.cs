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

namespace Microsoft.WindowsAzure.Management.WebSites.Services
{
    using System.Collections.Generic;
    using System.Linq;

    public static class WebsitesExtensionMethods
    {
        public static WebspaceList GetWebspaces(this IWebsitesServiceManagement proxy, string subscriptionId)
        {
            return proxy.EndGetWebspaces(proxy.BeginGetWebspaces(subscriptionId, null, null));
        }

        public static WebsiteList GetWebsites(this IWebsitesServiceManagement proxy, string subscriptionId, string webspace, IList<string> propertiesToInclude)
        {
            var properties = string.Empty;
            if (propertiesToInclude != null && propertiesToInclude.Count > 0)
            {
                properties = string.Join(",", propertiesToInclude.ToArray());
            }

            return proxy.EndGetWebsites(proxy.BeginGetWebsites(subscriptionId, webspace, properties, null, null));
        }

        public static WebsiteConfig GetWebsiteConfiguration(this IWebsitesServiceManagement proxy, string subscriptionId, string webspace, string website)
        {
            return proxy.EndGetWebsiteConfiguration(proxy.BeginGetWebsiteConfiguration(subscriptionId, webspace, website, null, null));
        }

        public static void DeleteWebsite(this IWebsitesServiceManagement proxy, string subscriptionId, string webspace, string website)
        {
            proxy.EndDeleteWebsite(proxy.BeginDeleteWebsite(subscriptionId, webspace, website, null, null));
        }

        public static void GetPublishingUsers(this IWebsitesServiceManagement proxy, string subscriptionId)
        {
            proxy.EndGetPublishingUsers(proxy.BeginGetPublishingUsers(subscriptionId, null, null));
        }

        public static Website GetWebsite(this IWebsitesServiceManagement proxy, string subscriptionId, string website)
        {
            var webspaces = proxy.GetWebspaces(subscriptionId);
            foreach (var webspace in webspaces)
            {
                var websites = proxy.GetWebsites(subscriptionId, webspace.Name, null);
                var matchWebsite = websites.FirstOrDefault(w => w.Name.Equals(website));
                if (matchWebsite != null)
                {
                    return matchWebsite;
                }
            }

            return null;
        }

        public static void NewWebsite(this IWebsitesServiceManagement proxy, string subscriptionId, string webspace, Website website)
        {
            proxy.EndNewWebsite(proxy.BeginNewWebsite(subscriptionId, webspace, website, null, null));
        }

        public static void UpdateWebsite(this IWebsitesServiceManagement proxy, string subscriptionId, string webspace, string websiteName, Website website)
        {
            proxy.EndUpdateWebsite(proxy.BeginUpdateWebsite(subscriptionId, webspace, websiteName, website, null, null));
        }

        public static void CreateWebsiteRepository(this IWebsitesServiceManagement proxy, string subscriptionId, string webspace, string websiteName)
        {
            proxy.EndCreateWebsiteRepository(proxy.BeginCreateWebsiteRepository(subscriptionId, webspace, websiteName, null, null));
        }
    }
}
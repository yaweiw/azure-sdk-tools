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

        public static void GetWebsites(this IWebsitesServiceManagement proxy, string webspace, IList<string> propertiesToInclude, string subscriptionId)
        {
            proxy.EndGetWebsites(proxy.BeginGetWebsites(subscriptionId, webspace, string.Join(",", propertiesToInclude.ToArray()), null, null));
        }

        public static void GetWebsiteConfiguration(this IWebsitesServiceManagement proxy, string webspace, string website, string subscriptionId)
        {
            proxy.EndGetWebsiteConfiguration(proxy.BeginGetWebsiteConfiguration(subscriptionId, webspace, website, null, null));
        }

        public static void GetPublishingUsers(this IWebsitesServiceManagement proxy, string webspace, IList<string> propertiesToInclude, string subscriptionId)
        {
            proxy.EndGetPublishingUsers(proxy.BeginGetPublishingUsers(subscriptionId, null, null));
        }
    }
}
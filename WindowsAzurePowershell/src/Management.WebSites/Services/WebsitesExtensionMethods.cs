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
    using System.Collections.Generic;
    using System.Linq;
    using WebEntities;

    public static class WebsitesExtensionMethods
    {
        public static Site GetWebsite(this IWebsitesServiceManagement proxy, string subscriptionId, string website)
        {
            var webspaces = proxy.GetWebSpaces(subscriptionId);
            foreach (var webspace in webspaces)
            {
                var websites = proxy.GetSites(subscriptionId, webspace.Name, null);
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
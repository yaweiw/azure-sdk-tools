// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.Utilities.Websites.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Script.Serialization;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using WebEntities;

    public static class Cache
    {
        public static void AddWebSpace(string subscriptionId, WebSpace webSpace)
        {
            WebSpaces webSpaces = GetWebSpaces(subscriptionId);
            if (webSpaces == null)
            {
                webSpaces = new WebSpaces();
            }

            webSpaces.Add(webSpace);
            SaveSpaces(subscriptionId, webSpaces);
        }

        public static void RemoveWebSpace(string subscriptionId, WebSpace webSpace)
        {
            WebSpaces webSpaces = GetWebSpaces(subscriptionId);
            if (webSpaces == null)
            {
                return;
            }

            webSpaces.RemoveAll(ws => ws.Name.Equals(webSpace.Name));
            SaveSpaces(subscriptionId, webSpaces);
        }

        public static WebSpaces GetWebSpaces(string subscriptionId)
        {
            try
            {
                string webspacesFile = Path.Combine(GlobalPathInfo.GlobalSettingsDirectory,
                                                    string.Format("spaces.{0}.json", subscriptionId));
                if (!File.Exists(webspacesFile))
                {
                    return new WebSpaces();
                }

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                List<WebSpace> webSpaces =
                    javaScriptSerializer.Deserialize<List<WebSpace>>(File.ReadAllText(webspacesFile));
                return new WebSpaces(webSpaces);
            }
            catch
            {
                return new WebSpaces();
            }
        }

        public static void AddSite(string subscriptionId, Site site)
        {
            Sites sites = GetSites(subscriptionId);
            if (sites == null)
            {
                sites = new Sites();
            }

            sites.Add(site);
            SaveSites(subscriptionId, sites);
        }

        public static void RemoveSite(string subscriptionId, Site site)
        {
            Sites sites = GetSites(subscriptionId);
            if (sites == null)
            {
                return;
            }

            sites.RemoveAll(s => s.Name.Equals(site.Name));
            SaveSites(subscriptionId, sites);
        }

        public static Site GetSite(string subscriptionId, string website, string propertiesToInclude)
        {
            Sites sites = GetSites(subscriptionId);
            if (sites != null)
            {
                return sites.FirstOrDefault(s => s.Name.Equals(website, System.StringComparison.InvariantCultureIgnoreCase));
            }

            return null;
        }

        public static Sites GetSites(string subscriptionId)
        {
            try
            {
                string sitesFile = Path.Combine(GlobalPathInfo.GlobalSettingsDirectory,
                                                string.Format("sites.{0}.json", subscriptionId));
                if (!File.Exists(sitesFile))
                {
                    return null;
                }

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                List<Site> sites = javaScriptSerializer.Deserialize<List<Site>>(File.ReadAllText(sitesFile));
                return new Sites(sites);
            }
            catch
            {
                return null;
            }
        }

        public static void SaveSpaces(string subscriptionId, WebSpaces webSpaces)
        {
            try
            {
                string webspacesFile = Path.Combine(GlobalPathInfo.GlobalSettingsDirectory,
                                                    string.Format("spaces.{0}.json", subscriptionId));
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

                // Make sure path exists
                Directory.CreateDirectory(GlobalPathInfo.GlobalSettingsDirectory);
                File.WriteAllText(webspacesFile, javaScriptSerializer.Serialize(webSpaces));
            }
            catch
            {
                // Do nothing. Caching is optional.
            }
        }

        public static void SaveSites(string subscriptionId, Sites sites)
        {
            try
            {
                string sitesFile = Path.Combine(GlobalPathInfo.GlobalSettingsDirectory,
                                                string.Format("sites.{0}.json", subscriptionId));
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

                // Make sure path exists
                Directory.CreateDirectory(GlobalPathInfo.GlobalSettingsDirectory);
                File.WriteAllText(sitesFile, javaScriptSerializer.Serialize(sites));
            }
            catch
            {
                // Do nothing. Caching is optional.
            }
        }
    }
}
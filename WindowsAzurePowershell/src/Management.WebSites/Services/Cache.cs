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
    using System.IO;
    using System.Web.Script.Serialization;
    using Management.Services;
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
            string webspacesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("spaces.{0}.json", subscriptionId));
            if (!File.Exists(webspacesFile))
            {
                return null;
            }

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            List<WebSpace> webSpaces = javaScriptSerializer.Deserialize<List<WebSpace>>(File.ReadAllText(webspacesFile));
            return new WebSpaces(webSpaces);
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

        public static Sites GetSites(string subscriptionId)
        {
            string sitesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("sites.{0}.json", subscriptionId));
            if (!File.Exists(sitesFile))
            {
                return null;
            }
            
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            List<Site> sites = javaScriptSerializer.Deserialize<List<Site>>(File.ReadAllText(sitesFile));
            return new Sites(sites);
        }

        public static void SaveSpaces(string subscriptionId, WebSpaces webSpaces)
        {
            string webspacesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("spaces.{0}.json", subscriptionId));
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            
            // Make sure path exists
            Directory.CreateDirectory(GlobalPathInfo.AzureAppDir);
            File.WriteAllText(webspacesFile, javaScriptSerializer.Serialize(webSpaces));
        }

        public static void SaveSites(string subscriptionId, Sites sites)
        {
            string sitesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("sites.{0}.json", subscriptionId));
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            // Make sure path exists
            Directory.CreateDirectory(GlobalPathInfo.AzureAppDir);
            File.WriteAllText(sitesFile, javaScriptSerializer.Serialize(sites));
        }

        public static void Clear(string subscriptionId)
        {
            string webspacesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("spaces.{0}.json", subscriptionId));
            string sitesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("sites.{0}.json", subscriptionId));
            
            if (File.Exists(webspacesFile))
            {
                File.Delete(webspacesFile);
            }

            if (File.Exists(sitesFile))
            {
                File.Delete(sitesFile);
            }
        }
    }
}
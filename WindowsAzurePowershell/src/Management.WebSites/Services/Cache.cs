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
    using System.IO;
    using System.Web.Script.Serialization;
    using Management.Services;
    using WebEntities;

    public static class Cache
    {
        public static WebSpaces GetWebSpaces(string subscriptionName)
        {
            string webspacesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("spaces.{0}.json", subscriptionName));
            if (!File.Exists(webspacesFile))
            {
                return null;
            }

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            List<WebSpace> webSpaces = javaScriptSerializer.Deserialize<List<WebSpace>>(File.ReadAllText(webspacesFile));
            return new WebSpaces(webSpaces);
        }

        public static Sites GetSites(string subscriptionName)
        {
            string sitesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("sites.{0}.json", subscriptionName));
            if (!File.Exists(sitesFile))
            {
                return null;
            }
            
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            List<Site> sites = javaScriptSerializer.Deserialize<List<Site>>(File.ReadAllText(sitesFile));
            return new Sites(sites);
        }

        public static void SaveSpaces(string subscriptionName, WebSpaces webSpaces)
        {
            string webspacesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("spaces.{0}.json", subscriptionName));
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            
            // Make sure path exists
            Directory.CreateDirectory(GlobalPathInfo.AzureAppDir);
            File.WriteAllText(webspacesFile, javaScriptSerializer.Serialize(webSpaces));
        }

        public static void SaveSites(string subscriptionName, Sites sites)
        {
            string sitesFile = Path.Combine(GlobalPathInfo.AzureAppDir, string.Format("sites.{0}.json", subscriptionName));
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            // Make sure path exists
            Directory.CreateDirectory(GlobalPathInfo.AzureAppDir);
            File.WriteAllText(sitesFile, javaScriptSerializer.Serialize(sites));
        }
    }
}

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

namespace Microsoft.WindowsAzure.Management.Utilities.Websites
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.DeploymentEntities;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.WebEntities;
    using Newtonsoft.Json.Linq;

    public class WebsitesClient
    {
        public IWebsitesServiceManagement WebsiteChannel { get; internal set; }

        public string SubscriptionId { get; set; }

        public Action<string> Logger { get; set; }

        /// <summary>
        /// Parameterless constructor for mocking.
        /// </summary>
        public WebsitesClient()
        {

        }

        /// <summary>
        /// Creates new WebsitesClient.
        /// </summary>
        /// <param name="subscription">The Windows Azure subscription data object</param>
        /// <param name="logger">The logger action</param>
        public WebsitesClient(SubscriptionData subscription, Action<string> logger)
        {
            SubscriptionId = subscription.SubscriptionId;
            Logger = logger;
            WebsiteChannel = ServiceManagementHelper.CreateServiceManagementChannel<IWebsitesServiceManagement>(
                ConfigurationConstants.WebHttpBinding(),
                new Uri(subscription.ServiceEndpoint),
                subscription.Certificate,
                new HttpRestMessageInspector(logger));
        }

        /// <summary>
        /// Gets website name in the current directory.
        /// </summary>
        /// <returns></returns>
        private string GetWebsiteFromCurrentDirectory()
        {
            return GitWebsite.ReadConfiguration().Name;
        }

        private Repository GetRepository(string websiteName)
        {
            Site site = WebsiteChannel.GetSite(
                SubscriptionId,
                websiteName,
                "repositoryuri,publishingpassword,publishingusername");
            if (site != null)
            {
                return new Repository(site);
            }

            throw new Exception(Resources.RepositoryNotSetup);
        }

        private Repository TryGetRepository(string websiteName)
        {
            Site site = WebsiteChannel.GetSite(
                SubscriptionId,
                websiteName,
                "repositoryuri,publishingpassword,publishingusername");
            if (site != null)
            {
                return new Repository(site);
            }

            return null;
        }

        private HttpClient CreateHttpClient(string websiteName)
        {
            Repository repository;
            ICredentials credentials;
            websiteName = GetWebsiteDeploymentHttpConfiguration(websiteName, out repository, out credentials);
            return HttpClientHelper.CreateClient(repository.RepositoryUri, credentials);
        }

        private string GetWebsiteDeploymentHttpConfiguration(
            string name,
            out Repository repository,
            out ICredentials credentials)
        {
            name = string.IsNullOrEmpty(name) ? GetWebsiteFromCurrentDirectory() : name;
            repository = GetRepository(name);
            credentials = new NetworkCredential(
                repository.PublishingUsername,
                repository.PublishingPassword);
            return name;
        }

        /// <summary>
        /// Starts log streaming for the given website.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="path">The log path, by default root</param>
        /// <param name="message">The substring message</param>
        /// <param name="endStreaming">Predicate to end streaming</param>
        /// <param name="waitInternal">The fetch wait interval</param>
        /// <returns>The log line</returns>
        public virtual IEnumerable<string> StartLogStreaming(
            string name,
            string path,
            string message,
            Predicate<string> endStreaming = null,
            int waitInternal = 10000)
        {
            Repository repository;
            ICredentials credentials;
            name = GetWebsiteDeploymentHttpConfiguration(name, out repository, out credentials);
            path = HttpUtility.UrlEncode(path);
            message = HttpUtility.UrlEncode(message);

            RemoteLogStreamManager manager = new RemoteLogStreamManager(
                repository.RepositoryUri,
                path,
                message,
                credentials,
                Logger);

            using (LogStreamWaitHandle logHandler = new LogStreamWaitHandle(manager.GetStream().Result))
            {
                bool doStreaming = true;
                
                while (doStreaming)
                {
                    string line = logHandler.WaitNextLine(waitInternal);

                    if (line != null)
                    {
                        yield return line;
                    }

                    doStreaming = endStreaming == null ? true : endStreaming(line);
                }
            }
        }

        /// <summary>
        /// List log paths for a given website.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual List<LogPath> ListLogPaths(string name)
        {
            List<LogPath> logPaths = new List<LogPath>();
            using (HttpClient client = CreateHttpClient(name))
            {
                logPaths = client.GetJson<List<LogPath>>(UriElements.LogPaths, Logger);
            }

            return logPaths;
        }

        public virtual void SetDiagnosticsSettings(string name, bool? drive, LogEntryType driveLevel, bool? table, LogEntryType tableLevel)
        {
            DiagnosticsSettings diagnosticsSettings;

            using (HttpClient client = CreateHttpClient(name))
            {
                diagnosticsSettings = client.GetJson<DiagnosticsSettings>(UriElements.DiagnosticsSettings, Logger);

                JObject json = null;

                IncludeIfChanged<bool?>(
                    drive,
                    diagnosticsSettings.AzureDriveTraceEnabled,
                    UriElements.AzureDriveTraceEnabled,
                    ref json);

                IncludeIfChanged<LogEntryType>(
                    driveLevel,
                    diagnosticsSettings.AzureDriveTraceLevel,
                    UriElements.AzureDriveTraceLevel,
                    ref json);

                IncludeIfChanged<bool?>(
                    table,
                    diagnosticsSettings.AzureTableTraceEnabled,
                    UriElements.AzureTableTraceEnabled,
                    ref json);

                IncludeIfChanged<LogEntryType>(
                    tableLevel,
                    diagnosticsSettings.AzureTableTraceLevel,
                    UriElements.AzureTableTraceLevel,
                    ref json);

                if (json != null)
                {
                    client.PostAsJsonAsync(UriElements.DiagnosticsSettings, json, Logger);
                }
            }
        }

        private void IncludeIfChanged<T>(T current, T original, string key, ref JObject json)
        {
            if (IsChanged<T>(original, current))
            {
                json = json ?? new JObject();
                json[key] = JToken.FromObject(current);
            }
        }

        private bool IsChanged<T>(T original, T current)
        {
            return current != null && !current.Equals(original);
        }
    }
}

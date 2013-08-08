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

namespace Microsoft.WindowsAzure.Commands.Utilities.Websites
{
    using CloudService;
    using Newtonsoft.Json.Linq;
    using Properties;
    using ServiceManagement;
    using Services;
    using Services.DeploymentEntities;
    using Services.GeoEntities;
    using Services.WebEntities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web;
    using System.Xml.Linq;
    using Utilities.Common;

    public class WebsitesClient : IWebsitesClient
    {
        private string subscriptionId;

        private CloudServiceClient cloudServiceClient;

        public const string WebsitesServiceVersion = "2012-12-01";

        public IWebsitesServiceManagement WebsiteChannel { get; internal set; }

        public IServiceManagement ServiceManagementChannel { get; internal set; }

        public SubscriptionData Subscription { get; set; }

        public Action<string> Logger { get; set; }

        public HeadersInspector HeadersInspector { get; set; }

        /// <summary>
        /// Creates new WebsitesClient.
        /// </summary>
        /// <param name="subscription">The Windows Azure subscription data object</param>
        /// <param name="logger">The logger action</param>
        public WebsitesClient(SubscriptionData subscription, Action<string> logger)
        {
            subscriptionId = subscription.SubscriptionId;
            Subscription = subscription;
            Logger = logger;
            HeadersInspector = new HeadersInspector();
            HeadersInspector.RequestHeaders.Add(ServiceManagement.Constants.VersionHeaderName, WebsitesServiceVersion);
            HeadersInspector.RequestHeaders.Add(ApiConstants.UserAgentHeaderName, ApiConstants.UserAgentHeaderValue);
            HeadersInspector.RemoveHeaders.Add(ApiConstants.VSDebuggerCausalityDataHeaderName);
            WebsiteChannel = ChannelHelper.CreateChannel<IWebsitesServiceManagement>(
                ConfigurationConstants.WebHttpBinding(),
                new Uri(subscription.ServiceEndpoint),
                subscription.Certificate,
                HeadersInspector,
                new HttpRestMessageInspector(logger));

            ServiceManagementChannel = ChannelHelper.CreateServiceManagementChannel<IServiceManagement>(
                ConfigurationConstants.WebHttpBinding(),
                new Uri(subscription.ServiceEndpoint),
                subscription.Certificate,
                new HttpRestMessageInspector(logger));

            cloudServiceClient = new CloudServiceClient(subscription, debugStream: logger);
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
            Site site = WebsiteChannel.GetSiteWithCache(
                subscriptionId,
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
            Site site = WebsiteChannel.GetSiteWithCache(
                subscriptionId,
                websiteName,
                "repositoryuri,publishingpassword,publishingusername");
            if (site != null)
            {
                return new Repository(site);
            }

            return null;
        }

        private HttpClient CreateDeploymentHttpClient(string websiteName)
        {
            Repository repository;
            ICredentials credentials;
            websiteName = GetWebsiteDeploymentHttpConfiguration(websiteName, out repository, out credentials);
            return HttpClientHelper.CreateClient(repository.RepositoryUri, credentials);
        }

        private HttpClient CreateWebsitesHttpClient()
        {
            WebRequestHandler requestHandler = new WebRequestHandler();
            requestHandler.ClientCertificates.Add(Subscription.Certificate);
            StringBuilder endpoint = new StringBuilder(General.EnsureTrailingSlash(Subscription.ServiceEndpoint));
            endpoint.Append(subscriptionId);
            endpoint.Append("/services/");
            HttpClient client = HttpClientHelper.CreateClient(endpoint.ToString(), handler: requestHandler);
            client.DefaultRequestHeaders.Add(ServiceManagement.Constants.VersionHeaderName, WebsitesServiceVersion);

            return client;
        }

        private string GetWebsiteDeploymentHttpConfiguration(
            string name,
            out Repository repository,
            out ICredentials credentials)
        {
            name = GetWebsiteName(name);
            repository = GetRepository(name);
            credentials = new NetworkCredential(
                repository.PublishingUsername,
                repository.PublishingPassword);
            return name;
        }

        private string GetWebsiteName(string name)
        {
            return string.IsNullOrEmpty(name) ? GetWebsiteFromCurrentDirectory() : name;
        }

        private void ChangeWebsiteState(string name, string webspace, WebsiteState state)
        {
            Site siteUpdate = new Site { Name = name, State = state.ToString() };
            WebsiteChannel.UpdateSite(subscriptionId, webspace, name, siteUpdate);
        }

        private void SetApplicationDiagnosticsSettings(
            string name,
            WebsiteDiagnosticOutput output,
            bool setFlag,
            Dictionary<DiagnosticProperties, object> properties = null)
        {
            Site website = GetWebsite(name);

            using (HttpClient client = CreateDeploymentHttpClient(website.Name))
            {
                DiagnosticsSettings diagnosticsSettings = GetApplicationDiagnosticsSettings(website.Name);
                switch (output)
                {
                    case WebsiteDiagnosticOutput.FileSystem:
                        diagnosticsSettings.AzureDriveTraceEnabled = setFlag;
                        diagnosticsSettings.AzureDriveTraceLevel = setFlag ?
                        (LogEntryType)properties[DiagnosticProperties.LogLevel] : 
                        diagnosticsSettings.AzureDriveTraceLevel;
                        break;

                    case WebsiteDiagnosticOutput.StorageTable:
                        diagnosticsSettings.AzureTableTraceEnabled = setFlag;
                        if (setFlag)
                        {
                            const string storageTableName = "CLOUD_STORAGE_ACCOUNT";
                            string storageAccountName = (string)properties[DiagnosticProperties.StorageAccountName];
                            string connectionString = cloudServiceClient.GetStorageServiceConnectionString(
                                storageAccountName);
                            SetConnectionString(website.Name, storageTableName, connectionString, DatabaseType.Custom);
                            
                            diagnosticsSettings.AzureTableTraceLevel = setFlag ?
                                (LogEntryType)properties[DiagnosticProperties.LogLevel] : 
                                diagnosticsSettings.AzureTableTraceLevel;
                        }
                        break;

                    default:
                        throw new ArgumentException();
                }

                JObject json = new JObject(
                    new JProperty(UriElements.AzureDriveTraceEnabled, diagnosticsSettings.AzureDriveTraceEnabled),
                    new JProperty(UriElements.AzureDriveTraceLevel, diagnosticsSettings.AzureDriveTraceLevel.ToString()),
                    new JProperty(UriElements.AzureTableTraceEnabled, diagnosticsSettings.AzureTableTraceEnabled),
                    new JProperty(UriElements.AzureTableTraceLevel, diagnosticsSettings.AzureTableTraceLevel.ToString()));
                client.PostJson(UriElements.DiagnosticsSettings, json, Logger);
            }
        }

        private void SetSiteDiagnosticsSettings(
            string name,
            bool webServerLogging,
            bool detailedErrorMessages,
            bool failedRequestTracing,
            bool setFlag)
        {
            Site website = GetWebsite(name);
            SiteConfig configuration = WebsiteChannel.GetSiteConfig(
                subscriptionId,
                website.WebSpace,
                website.Name);

            configuration.HttpLoggingEnabled = webServerLogging ? setFlag : configuration.HttpLoggingEnabled;
            configuration.DetailedErrorLoggingEnabled = detailedErrorMessages ? setFlag : 
                configuration.DetailedErrorLoggingEnabled;
            configuration.RequestTracingEnabled = failedRequestTracing ? setFlag : configuration.RequestTracingEnabled;
            
            WebsiteChannel.UpdateSiteConfig(subscriptionId, website.WebSpace, website.Name, configuration);
        }

        /// <summary>
        /// Gets the index of an application setting. Returns -1 if not found.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="key">the applicationsetting key</param>
        /// <returns>The application setting index</returns>
        private int GetAppSettingIndex(string name, string key)
        {
            Site website = GetWebsite(name);
            SiteConfig configuration = WebsiteChannel.GetSiteConfig(
                subscriptionId,
                website.WebSpace,
                website.Name);

            for (int i = 0; i < configuration.AppSettings.Count; i++)
            {
                if (configuration.AppSettings[i].Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Starts log streaming for the given website.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="path">The log path, by default root</param>
        /// <param name="message">The substring message</param>
        /// <param name="endStreaming">Predicate to end streaming</param>
        /// <param name="waitInterval">The fetch wait interval</param>
        /// <returns>The log line</returns>
        public IEnumerable<string> StartLogStreaming(
            string name,
            string path,
            string message,
            Predicate<string> endStreaming,
            int waitInterval)
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
                    string line = logHandler.WaitNextLine(waitInterval);

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
        public List<LogPath> ListLogPaths(string name)
        {
            List<LogPath> logPaths = new List<LogPath>();
            using (HttpClient client = CreateDeploymentHttpClient(name))
            {
                logPaths = client.GetJson<List<LogPath>>(UriElements.LogPaths, Logger);
            }

            return logPaths;
        }

        /// <summary>
        /// Gets the application diagnostics settings
        /// </summary>
        /// <param name="name">The website name</param>
        /// <returns>The website application diagnostics settings</returns>
        public DiagnosticsSettings GetApplicationDiagnosticsSettings(string name)
        {
            DiagnosticsSettings diagnosticsSettings = null;

            using (HttpClient client = CreateDeploymentHttpClient(name))
            {
                diagnosticsSettings = client.GetJson<DiagnosticsSettings>(UriElements.DiagnosticsSettings, Logger);
            }

            return diagnosticsSettings;
        }

        /// <summary>
        /// Restarts a website.
        /// </summary>
        /// <param name="name">The website name</param>
        public void RestartAzureWebsite(string name)
        {
            Site website = GetWebsite(name);
            ChangeWebsiteState(website.Name, website.WebSpace, WebsiteState.Stopped);
            ChangeWebsiteState(website.Name, website.WebSpace, WebsiteState.Running);
        }

        /// <summary>
        /// Starts a website.
        /// </summary>
        /// <param name="name">The website name</param>
        public void StartAzureWebsite(string name)
        {
            Site website = GetWebsite(name);
            ChangeWebsiteState(website.Name, website.WebSpace, WebsiteState.Running);
        }

        /// <summary>
        /// Stops a website.
        /// </summary>
        /// <param name="name">The website name</param>
        public void StopAzureWebsite(string name)
        {
            Site website = GetWebsite(name);
            ChangeWebsiteState(website.Name, website.WebSpace, WebsiteState.Stopped);
        }

        /// <summary>
        /// Gets a website instance.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <returns>The website instance</returns>
        public Site GetWebsite(string name)
        {
            name = GetWebsiteName(name);
            Site website = WebsiteChannel.GetSiteWithCache(subscriptionId, name, null);

            if (website == null)
            {
                throw new Exception(string.Format(Resources.InvalidWebsite, name));
            }

            return website;
        }

        /// <summary>
        /// Gets the website configuration.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <returns>The website configuration object</returns>
        public SiteWithConfig GetWebsiteConfiguration(string name)
        {
            Site website = GetWebsite(name);
            SiteConfig configuration = WebsiteChannel.GetSiteConfig(subscriptionId, website.WebSpace, website.Name);
            DiagnosticsSettings diagnosticsSettings = GetApplicationDiagnosticsSettings(website.Name);
            SiteWithConfig siteWithConfig = new SiteWithConfig(website, configuration, diagnosticsSettings);

            return siteWithConfig;
        }

        /// <summary>
        /// Sets an AppSetting of a website.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="key">The app setting name</param>
        /// <param name="value">The app setting value</param>
        public void SetAppSetting(string name, string key, string value)
        {
            Site website = GetWebsite(name);
            SiteConfig configuration = WebsiteChannel.GetSiteConfig(
                subscriptionId,
                website.WebSpace,
                website.Name);

            int index = GetAppSettingIndex(name, key);

            if (index != -1)
            {
                configuration.AppSettings[index].Value = value;
            }
            else
            {
                configuration.AppSettings.Add(new NameValuePair() { Name = key, Value = value });
            }
            
            WebsiteChannel.UpdateSiteConfig(subscriptionId, website.WebSpace, website.Name, configuration);
        }

        /// <summary>
        /// Sets a connection string for a website.
        /// </summary>
        /// <param name="name">Name of the website.</param>
        /// <param name="key">Connection string key.</param>
        /// <param name="value">Value for the connection string.</param>
        /// <param name="connectionStringType">Type of connection string.</param>
        public void SetConnectionString(string name, string key, string value, DatabaseType connectionStringType)
        {
            Site website = GetWebsite(name);
            SiteConfig configuration = WebsiteChannel.GetSiteConfig(
                subscriptionId, website.WebSpace, website.Name);

            var index = configuration.ConnectionStrings.FindIndex(cs => cs.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (index == -1)
            {
                configuration.ConnectionStrings.Add(new ConnStringInfo()
                {
                    Name = key,
                    ConnectionString =  value,
                    Type = connectionStringType
                });
            }
            else
            {
                configuration.ConnectionStrings[index].ConnectionString = value;
                configuration.ConnectionStrings[index].Type = connectionStringType;
            }

            WebsiteChannel.UpdateSiteConfig(subscriptionId, website.WebSpace, website.Name, configuration);
        }

        /// <summary>
        /// Enables website diagnostic.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="webServerLogging">Flag for webServerLogging</param>
        /// <param name="detailedErrorMessages">Flag for detailedErrorMessages</param>
        /// <param name="failedRequestTracing">Flag for failedRequestTracing</param>
        public void EnableSiteDiagnostic(
            string name,
            bool webServerLogging,
            bool detailedErrorMessages,
            bool failedRequestTracing)
        {
            SetSiteDiagnosticsSettings(name, webServerLogging, detailedErrorMessages, failedRequestTracing, true);
        }

        /// <summary>
        /// Disables site diagnostic.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="webServerLogging">Flag for webServerLogging</param>
        /// <param name="detailedErrorMessages">Flag for detailedErrorMessages</param>
        /// <param name="failedRequestTracing">Flag for failedRequestTracing</param>
        public void DisableSiteDiagnostic(
            string name,
            bool webServerLogging,
            bool detailedErrorMessages,
            bool failedRequestTracing)
        {
            SetSiteDiagnosticsSettings(name, webServerLogging, detailedErrorMessages, failedRequestTracing, false);
        }

        public void EnableApplicationDiagnostic(
            string name,
            WebsiteDiagnosticOutput output,
            Dictionary<DiagnosticProperties, object> properties)
        {
            SetApplicationDiagnosticsSettings(name, output, true, properties);
        }

        public void DisableApplicationDiagnostic(string name, WebsiteDiagnosticOutput output)
        {
            SetApplicationDiagnosticsSettings(name, output, false);
        }

        /// <summary>
        /// Lists available website locations.
        /// </summary>
        /// <returns>List of location names</returns>
        public List<string> ListAvailableLocations()
        {
            List<string> locations = new List<string>();
            WebSpaces webspaces = WebsiteChannel.GetWebSpaces(subscriptionId);
            List<string> webspacesGeoRegions = new List<string>();
            webspaces.ForEach(w => webspacesGeoRegions.Add(w.GeoRegion));
            GeoRegions regions = new GeoRegions();

            using (HttpClient client = CreateWebsitesHttpClient())
            {
                regions = client.GetXml<GeoRegions>(UriElements.WebSpacesGeoRegionsRoot, Logger);
            }

            regions.ForEach(r => locations.Add(r.Name));
            locations = locations.Union(webspacesGeoRegions).ToList();

            return locations;
        }

        /// <summary>
        /// Gets the default website DNS suffix for the current environment.
        /// </summary>
        /// <returns>The website DNS suffix</returns>
        public string GetWebsiteDnsSuffix()
        {
            string suffix;

            using (HttpClient client = CreateWebsitesHttpClient())
            {
                string body = client.GetXml(UriElements.DnsSuffix, Logger);
                suffix = General.IsXml(body) ? XDocument.Parse(body).Root.Value : body.Replace("\"", "");
            }

            return suffix;
        }

        /// <summary>
        /// Gets the default location for websites.
        /// </summary>
        /// <returns>The default location name.</returns>
        public string GetDefaultLocation()
        {
            return ListAvailableLocations().First();
        }
    }
}

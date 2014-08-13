// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using Authentication;
    using Commands.Common.Properties;
    using Subscriptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Net.Http;

    [Serializable]
    public class WindowsAzureEnvironment
    {
        /// <summary>
        /// The Microsoft Azure environment name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The management portal endpoint.
        /// </summary>
        public string PublishSettingsFileUrl { get; set; }

        /// <summary>
        /// The service management RDFE endpoint.
        /// </summary>
        public string ServiceEndpoint { get; set; }

        /// <summary>
        /// The service management CSM endpoint.
        /// </summary>
        public string ResourceManagerEndpoint { get; set; }

        /// <summary>
        /// Url to the Microsoft Azure management portal.
        /// </summary>
        public string ManagementPortalUrl { get; set; }

        /// <summary>
        /// Url for the Active Directory tenant for this environment
        /// </summary>
        /// <remarks>If null, this environment does not support AD authentication</remarks>
        public string ActiveDirectoryEndpoint { get; set; }

        /// <summary>
        /// Name for the common tenant used as the first step
        /// in the AD authentication process for this environment.
        /// </summary>
        /// <remarks>If null, this environment does not support AD authentication</remarks>
        public string ActiveDirectoryCommonTenantId { get; set; }

        public string ActiveDirectoryServiceEndpointResourceId { get; set; }

        private string storageEndpointSuffix;

        /// <summary>
        /// The storage endpoint suffix for this environment.
        /// </summary>
        public string StorageEndpointSuffix
        {
            get { return storageEndpointSuffix; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Validate.ValidateDnsName(value, "value");
                }
                storageEndpointSuffix = value;
            }
        }

        private const string storageFormatTemplate = "{{0}}://{{1}}.{0}.{1}/";

        private string EndpointFormatFor(string service)
        {
            if (string.IsNullOrEmpty(storageEndpointSuffix)) return null;
            return string.Format(storageFormatTemplate, service, storageEndpointSuffix);
        }

        /// <summary>
        /// The storage service blob endpoint format.
        /// </summary>
        public string StorageBlobEndpointFormat
        { 
            get { return EndpointFormatFor("blob"); }
        }

        /// <summary>
        /// The storage service queue endpoint format.
        /// </summary>
        public string StorageQueueEndpointFormat
        {
            get { return EndpointFormatFor("queue"); }
        }

        /// <summary>
        /// The storage service table endpoint format.
        /// </summary>
        public string StorageTableEndpointFormat
        {
            get { return EndpointFormatFor("table"); }
        }

        /// <summary>
        /// The storage service file endpoint format.
        /// </summary>
        public string StorageFileEndpointFormat
        {
            get { return EndpointFormatFor("file"); }
        }

        public string GalleryEndpoint { get; set; }

        /// <summary>
        /// Gets the endpoint for storage blob.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <param name="useHttps">Use Https when creating the URI. Defaults to true.</param>
        /// <returns>The fully qualified uri to the blob service</returns>
        public Uri GetStorageBlobEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageBlobEndpointFormat, useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Gets the endpoint for storage queue.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <param name="useHttps">Use Https when creating the URI. Defaults to true.</param>
        /// <returns>The fully qualified uri to the queue service</returns>
        public Uri GetStorageQueueEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageQueueEndpointFormat, useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Gets the endpoint for storage table.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <param name="useHttps">Use Https when creating the URI. Defaults to true.</param>
        /// <returns>The fully qualified uri to the table service</returns>
        public Uri GetStorageTableEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageTableEndpointFormat, useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Gets the endpoint for storage file.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <param name="useHttps">Use Https when creating the URI. Defaults to true.</param>
        /// <returns>The fully qualified uri to the file service</returns>
        public Uri GetStorageFileEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageFileEndpointFormat, useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Gets or sets the DNS suffix for Azure SQL Database servers.
        /// </summary>
        public string SqlDatabaseDnsSuffix { get; set; }

        /// <summary>
        /// Gets the management portal URI with a particular realm suffix if supplied
        /// </summary>
        /// <param name="realm">Realm for user's account</param>
        /// <returns>Url to management portal.</returns>
        public string ManagementPortalUrlWithRealm(string realm = null)
        {
            return AddRealm(ManagementPortalUrl, realm);
        }

        /// <summary>
        /// Get the publish settings file download url with a realm suffix if needed.
        /// </summary>
        /// <param name="realm">Realm for user's account</param>
        /// <returns>Url to publish settings file</returns>
        public string PublishSettingsFileUrlWithRealm(string realm = null)
        {
            return AddRealm(PublishSettingsFileUrl, realm);
        }

        private string AddRealm(string baseUrl, string realm)
        {
            if (!string.IsNullOrEmpty(realm))
            {
                baseUrl += string.Format(Resources.PublishSettingsFileRealmFormat, realm);
            }
            return baseUrl;
        }

        public SubscriptionClient AddUserAgent(SubscriptionClient client)
        {
            if (!client.UserAgent.Contains(ApiConstants.UserAgentValue))
            {
                client.UserAgent.Add(ApiConstants.UserAgentValue);
            }
            return client;
        }

        public SubscriptionClient AddRestLogHandler(SubscriptionClient client)
        {
            var withHandlerMethod = typeof(SubscriptionClient).GetMethod("WithHandler", new[] { typeof(DelegatingHandler) });
            SubscriptionClient finalClient =
                (SubscriptionClient)withHandlerMethod.Invoke(client, new object[] { new HttpRestCallLogger() });
            client.Dispose();
            return finalClient;
        }

        public IEnumerable<WindowsAzureSubscription> AddAccount(ITokenProvider tokenProvider, PSCredential credential)
        {
            if (ActiveDirectoryEndpoint == null || ActiveDirectoryServiceEndpointResourceId == null)
            {
                throw new Exception(string.Format(Resources.EnvironmentDoesNotSupportActiveDirectory, Name));
            }

            IAccessToken mainToken;
            if (credential != null)
            {
                mainToken = tokenProvider.GetNewToken(this, credential.UserName, credential.Password);
            }
            else
            {
                mainToken = tokenProvider.GetNewToken(this);
            }
            var credentials = new TokenCloudCredentials(mainToken.AccessToken);

            using (var subscriptionClient = AddRestLogHandler(AddUserAgent(new SubscriptionClient(credentials, new Uri(ServiceEndpoint)))))
            {
                var result = subscriptionClient.Subscriptions.List();
                // Filter out subscriptions with no tenant, backfill's not done on them
                foreach (var subscription in result.Subscriptions.Where(s => !string.IsNullOrEmpty(s.ActiveDirectoryTenantId)))
                {
                    var azureSubscription = new WindowsAzureSubscription
                    {
                        ActiveDirectoryEndpoint = ActiveDirectoryEndpoint,
                        ActiveDirectoryTenantId = subscription.ActiveDirectoryTenantId,
                        ActiveDirectoryUserId = mainToken.UserId,
                        ActiveDirectoryServiceEndpointResourceId = ActiveDirectoryServiceEndpointResourceId,
                        SubscriptionId = subscription.SubscriptionId,
                        SubscriptionName = subscription.SubscriptionName,
                        ServiceEndpoint = !string.IsNullOrEmpty(ServiceEndpoint) ? new Uri(ServiceEndpoint) : null,
                        ResourceManagerEndpoint = !string.IsNullOrEmpty(ResourceManagerEndpoint) ? new Uri(ResourceManagerEndpoint) : null,
                        TokenProvider = tokenProvider,
                        GalleryEndpoint = !string.IsNullOrEmpty(GalleryEndpoint) ? new Uri(GalleryEndpoint) : null,
                        SqlDatabaseDnsSuffix = SqlDatabaseDnsSuffix ?? WindowsAzureEnvironmentConstants.AzureSqlDatabaseDnsSuffix,
                    };

                    if (mainToken.LoginType == LoginType.LiveId)
                    {
                        azureSubscription.SetAccessToken(tokenProvider.GetNewToken(azureSubscription, mainToken.UserId));
                    }
                    else
                    {
                        azureSubscription.SetAccessToken(mainToken);
                    }
                    yield return azureSubscription;
                }
            }
        }

        /// <summary>
        /// Predefined Microsoft Azure environments
        /// </summary>
        public static Dictionary<string, WindowsAzureEnvironment> PublicEnvironments
        {
            get { return environments; }
        }

        private static readonly Dictionary<string, WindowsAzureEnvironment> environments = 
            new Dictionary<string, WindowsAzureEnvironment>(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                EnvironmentName.AzureCloud,
                new WindowsAzureEnvironment
                {
                    Name = EnvironmentName.AzureCloud,
                    PublishSettingsFileUrl = WindowsAzureEnvironmentConstants.AzurePublishSettingsFileUrl,
                    ServiceEndpoint = WindowsAzureEnvironmentConstants.AzureServiceEndpoint,
                    ResourceManagerEndpoint = WindowsAzureEnvironmentConstants.AzureResourceManagerEndpoint,
                    ManagementPortalUrl = WindowsAzureEnvironmentConstants.AzureManagementPortalUrl,
                    ActiveDirectoryEndpoint = "https://login.windows.net/",
                    ActiveDirectoryCommonTenantId = "common",
                    ActiveDirectoryServiceEndpointResourceId = WindowsAzureEnvironmentConstants.AzureServiceEndpoint,
                    StorageEndpointSuffix = WindowsAzureEnvironmentConstants.AzureStorageEndpointSuffix,
                    GalleryEndpoint = WindowsAzureEnvironmentConstants.GalleryEndpoint,
                    SqlDatabaseDnsSuffix = WindowsAzureEnvironmentConstants.AzureSqlDatabaseDnsSuffix,
                }
            },
            {
                EnvironmentName.AzureChinaCloud,
                new WindowsAzureEnvironment
                {
                    Name = EnvironmentName.AzureChinaCloud,
                    PublishSettingsFileUrl = WindowsAzureEnvironmentConstants.ChinaPublishSettingsFileUrl,
                    ServiceEndpoint = WindowsAzureEnvironmentConstants.ChinaServiceEndpoint,
                    ResourceManagerEndpoint = string.Empty,
                    ActiveDirectoryEndpoint  = "https://login.chinacloudapi.cn/",
                    ActiveDirectoryCommonTenantId = "common",
                    ActiveDirectoryServiceEndpointResourceId = WindowsAzureEnvironmentConstants.ChinaServiceEndpoint, 
                    ManagementPortalUrl = WindowsAzureEnvironmentConstants.ChinaManagementPortalUrl,
                    StorageEndpointSuffix = WindowsAzureEnvironmentConstants.ChinaStorageEndpointSuffix,
                    GalleryEndpoint = string.Empty,
                    SqlDatabaseDnsSuffix = WindowsAzureEnvironmentConstants.ChinaSqlDatabaseDnsSuffix,
                }
            }
        };
    }
}

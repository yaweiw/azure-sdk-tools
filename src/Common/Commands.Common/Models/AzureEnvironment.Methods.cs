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

using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Commands.Common.Models
{
    public partial class AzureEnvironment
    {
        /// <summary>
        /// Predefined Microsoft Azure environments
        /// </summary>
        public static Dictionary<string, AzureEnvironment> PublicEnvironments
        {
            get { return environments; }
        }

        private const string storageFormatTemplate = "{{0}}://{{1}}.{0}.{1}/";

        private string EndpointFormatFor(string service)
        {
            string suffix = GetEndpoint(AzureEnvironment.Endpoint.StorageEndpointSuffix);
            string endpoint = null;

            if (!string.IsNullOrEmpty(endpoint))
            {
                endpoint = string.Format(storageFormatTemplate, service, suffix);
            }

            return endpoint;
        }

        /// <summary>
        /// The storage service blob endpoint format.
        /// </summary>
        private string StorageBlobEndpointFormat()
        {
            return EndpointFormatFor("blob");
        }

        /// <summary>
        /// The storage service queue endpoint format.
        /// </summary>
        private string StorageQueueEndpointFormat()
        {
            return EndpointFormatFor("queue");
        }

        /// <summary>
        /// The storage service table endpoint format.
        /// </summary>
        private string StorageTableEndpointFormat()
        {
            return EndpointFormatFor("table");
        }

        /// <summary>
        /// The storage service file endpoint format.
        /// </summary>
        private string StorageFileEndpointFormat()
        {
            return EndpointFormatFor("file");
        }

        private static readonly Dictionary<string, AzureEnvironment> environments =
            new Dictionary<string, AzureEnvironment>(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                EnvironmentName.AzureCloud,
                new AzureEnvironment
                {
                    Name = EnvironmentName.AzureCloud,
                    Endpoints = new Dictionary<AzureEnvironment.Endpoint, string>
                    {
                        { AzureEnvironment.Endpoint.PublishSettingsFileUrl, WindowsAzureEnvironmentConstants.AzurePublishSettingsFileUrl },
                        { AzureEnvironment.Endpoint.ServiceEndpoint, WindowsAzureEnvironmentConstants.AzureServiceEndpoint },
                        { AzureEnvironment.Endpoint.ResourceManagerEndpoint, WindowsAzureEnvironmentConstants.AzureResourceManagerEndpoint },
                        { AzureEnvironment.Endpoint.ManagementPortalUrl, WindowsAzureEnvironmentConstants.AzureManagementPortalUrl },
                        { AzureEnvironment.Endpoint.ActiveDirectoryEndpoint, WindowsAzureEnvironmentConstants.AzureActiveDirectoryEndpoint },
                        { AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId, WindowsAzureEnvironmentConstants.AzureServiceEndpoint },
                        { AzureEnvironment.Endpoint.StorageEndpointSuffix, WindowsAzureEnvironmentConstants.AzureStorageEndpointSuffix },
                        { AzureEnvironment.Endpoint.GalleryEndpoint, WindowsAzureEnvironmentConstants.GalleryEndpoint },
                        { AzureEnvironment.Endpoint.SqlDatabaseDnsSuffix, WindowsAzureEnvironmentConstants.AzureSqlDatabaseDnsSuffix },
                    }
                }
            },
            {
                EnvironmentName.AzureChinaCloud,
                new AzureEnvironment
                {
                    Name = EnvironmentName.AzureChinaCloud,
                    Endpoints = new Dictionary<AzureEnvironment.Endpoint, string>
                    {
                        { AzureEnvironment.Endpoint.PublishSettingsFileUrl, WindowsAzureEnvironmentConstants.ChinaPublishSettingsFileUrl },
                        { AzureEnvironment.Endpoint.ServiceEndpoint, WindowsAzureEnvironmentConstants.ChinaServiceEndpoint },
                        { AzureEnvironment.Endpoint.ResourceManagerEndpoint, null },
                        { AzureEnvironment.Endpoint.ManagementPortalUrl, WindowsAzureEnvironmentConstants.ChinaManagementPortalUrl },
                        { AzureEnvironment.Endpoint.ActiveDirectoryEndpoint, WindowsAzureEnvironmentConstants.ChinaActiveDirectoryEndpoint },
                        { AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId, WindowsAzureEnvironmentConstants.ChinaServiceEndpoint },
                        { AzureEnvironment.Endpoint.StorageEndpointSuffix, WindowsAzureEnvironmentConstants.ChinaStorageEndpointSuffix },
                        { AzureEnvironment.Endpoint.GalleryEndpoint, null },
                        { AzureEnvironment.Endpoint.SqlDatabaseDnsSuffix, WindowsAzureEnvironmentConstants.ChinaSqlDatabaseDnsSuffix },
                    }
                }
            }
        };

        public string GetEndpoint(AzureEnvironment.Endpoint endpoint)
        {
            if (Endpoints.ContainsKey(endpoint))
            {
                return Endpoints[endpoint];
            }

            return null;
        }

        /// <summary>
        /// Gets the endpoint for storage blob.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <param name="useHttps">Use Https when creating the URI. Defaults to true.</param>
        /// <returns>The fully qualified uri to the blob service</returns>
        public Uri GetStorageBlobEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageBlobEndpointFormat(), useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Gets the endpoint for storage queue.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <param name="useHttps">Use Https when creating the URI. Defaults to true.</param>
        /// <returns>The fully qualified uri to the queue service</returns>
        public Uri GetStorageQueueEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageQueueEndpointFormat(), useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Gets the endpoint for storage table.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <param name="useHttps">Use Https when creating the URI. Defaults to true.</param>
        /// <returns>The fully qualified uri to the table service</returns>
        public Uri GetStorageTableEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageTableEndpointFormat(), useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Gets the endpoint for storage file.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <param name="useHttps">Use Https when creating the URI. Defaults to true.</param>
        /// <returns>The fully qualified uri to the file service</returns>
        public Uri GetStorageFileEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageFileEndpointFormat(), useHttps ? "https" : "http", accountName));
        }

        public enum Endpoint
        {
            ActiveDirectoryServiceEndpointResourceId,

            AdTenantUrl,

            GalleryEndpoint,

            ManagementPortalUrl,

            ServiceEndpoint,

            PublishSettingsFileUrl,

            ResourceManagerEndpoint,

            SqlDatabaseDnsSuffix,

            StorageEndpointSuffix,

            ActiveDirectoryEndpoint
        }
    }
}

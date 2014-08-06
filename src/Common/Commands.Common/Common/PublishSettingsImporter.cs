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

using Microsoft.WindowsAzure.Commands.Common.Models;

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml.Serialization;
    using XmlSchema;

    /// <summary>
    /// Class that handles loading publishsettings files
    /// and turning them into WindowsAzureSubscription objects.
    /// </summary>
    public static class PublishSettingsImporter
    {
        public static IEnumerable<WindowsAzureSubscription> ImportWindowsAzureSubscription(string filename)
        {
            using (var s = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return ImportWindowsAzureSubscription(s);
            }
        }

        public static IEnumerable<WindowsAzureSubscription> ImportWindowsAzureSubscription(Stream stream)
        {
            var publishData = DeserializePublishData(stream);
            PublishDataPublishProfile profile = publishData.Items.Single();
            return profile.Subscription.Select(s => PublishSubscriptionToWindowsAzureSubscription(profile, s));
        }

        public static IEnumerable<AzureSubscription> ImportAzureSubscription(Stream stream, string environment)
        {
            var publishData = DeserializePublishData(stream);
            PublishDataPublishProfile profile = publishData.Items.Single();
            return profile.Subscription.Select(s => PublishSubscriptionToAzureSubscription(profile, s, environment));
        }

        private static PublishData DeserializePublishData(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(PublishData));
            return (PublishData)serializer.Deserialize(stream);
        }

        private static WindowsAzureSubscription PublishSubscriptionToWindowsAzureSubscription(
            PublishDataPublishProfile profile,
            PublishDataPublishProfileSubscription s)
        {
            return new WindowsAzureSubscription
            {
                Certificate = GetCertificate(profile, s),
                SubscriptionName = s.Name,
                ServiceEndpoint = GetManagementUri(profile, s),
                ResourceManagerEndpoint = null,
                SubscriptionId = s.Id,
                SqlDatabaseDnsSuffix = WindowsAzureEnvironmentConstants.AzureSqlDatabaseDnsSuffix,
            };
        }

        private static AzureSubscription PublishSubscriptionToAzureSubscription(
            PublishDataPublishProfile profile,
            PublishDataPublishProfileSubscription s,
            string environment)
        {
            var certificate = GetCertificate(profile, s);
            
            return new AzureSubscription
            {
                Id = new Guid(s.Id),
                Name = s.Name,
                Environment = environment,
                Properties = new Dictionary<AzureSubscription.Property,string>
                {
                    { AzureSubscription.Property.Thumbprint, certificate.Thumbprint },
                }
            };
        }

        private static X509Certificate2 GetCertificate(PublishDataPublishProfile profile,
            PublishDataPublishProfileSubscription s)
        {
            string certificateString;
            if (!string.IsNullOrEmpty(s.ManagementCertificate))
            {
                certificateString = s.ManagementCertificate;
            }
            else
            {
                certificateString = profile.ManagementCertificate;
            }
            return WindowsAzureCertificate.FromPublishSettingsString(certificateString);
        }

        private static Uri GetManagementUri(PublishDataPublishProfile profile, PublishDataPublishProfileSubscription s)
        {
            if (!string.IsNullOrEmpty(s.ServiceManagementUrl))
            {
                return new Uri(s.ServiceManagementUrl);
            }
            else if (!string.IsNullOrEmpty(profile.Url))
            {
                return new Uri(profile.Url);
            }
            return null;
        }
    }
}

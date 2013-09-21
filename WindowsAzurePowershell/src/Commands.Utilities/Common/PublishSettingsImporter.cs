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
        public static IEnumerable<WindowsAzureSubsciption> Import(string filename)
        {
            using (var s = new StreamReader(filename, true))
            {
                return Import(s);
            }
        }

        public static IEnumerable<WindowsAzureSubsciption> Import(StreamReader reader)
        {
            var publishData = DeserializePublishData(reader);
            PublishDataPublishProfile profile = publishData.Items.Single();
            return profile.Subscription.Select(s => PublishSubscriptionToAzureSubscription(profile, s));
        }

        private static PublishData DeserializePublishData(StreamReader reader)
        {
            var serializer = new XmlSerializer(typeof(PublishData));
            return (PublishData)serializer.Deserialize(reader);
        }

        private static WindowsAzureSubsciption PublishSubscriptionToAzureSubscription(
            PublishDataPublishProfile profile,
            PublishDataPublishProfileSubscription s)
        {
            return new WindowsAzureSubsciption
            {
                Certificate = GetCertificate(profile, s),
                Name = s.Name,
                ManagementEndpoint = GetManagementUri(profile, s),
                SubscriptionId = s.Id
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
            return new X509Certificate2(Convert.FromBase64String(certificateString), string.Empty);
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

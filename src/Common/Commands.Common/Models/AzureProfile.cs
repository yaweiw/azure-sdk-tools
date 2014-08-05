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

using Microsoft.WindowsAzure.Commands.Common.Interfaces;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WindowsAzure.Commands.Common.Models
{
    public class AzureProfile
    {
        private IDataStore store;

        private static string DefaultProfilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Resources.AzureDirectoryName);

        private void Copy(AzureProfile other)
        {
            // Copy environments

            // Copy subscriptions

            // Copy certificates

            // Ignore copying the cached tokens as we require users to login again
        }

        private void LoadProfile()
        {
            JsonProfileSerializer jsonSerializer = new JsonProfileSerializer();
            XmlProfileSerializer xmlSerializer = new XmlProfileSerializer();
            string xmlProfilePath = Path.Combine(DefaultProfilePath, xmlSerializer.ProfileFile);
            string jsonProfilePath = Path.Combine(DefaultProfilePath, jsonSerializer.ProfileFile);

            if (store.FileExists(xmlProfilePath))
            {
                AzureProfile profile = xmlSerializer.Deserialize(store.ReadFile(xmlProfilePath));
                Copy(profile);
                SaveProfile();
            }
            else if (store.FileExists(jsonProfilePath))
            {
                // Do JSON parsing
            }
        }

        private void SaveProfile()
        {
            throw new NotImplementedException();
        }

        public AzureProfile(IDataStore store)
        {
            this.store = store;

            LoadProfile();

            foreach (AzureEnvironment env in Environments)
            {
                if (env.DefaultSubscriptionId.HasValue)
                {
                    CurrentSubscription = GetSubscription(env.DefaultSubscriptionId.Value);
                    break;
                }
            }
        }

        public AzureProfile() : this(new DiskDataStore())
        {
            
        }

        public List<AzureEnvironment> Environments
        {
            // Parse whatever environments saved on the disk and append to AzureEnvironment.PublicEnvironments
            get { throw new NotImplementedException(); }
        }

        public List<AzureSubscription> Subscriptions
        {
            get { throw new NotImplementedException(); }
        }

        public AzureSubscription CurrentSubscription { get; set; }

        public AzureEnvironment CurrentEnvironment
        {
            get
            {
                if (CurrentSubscription == null)
                {
                    return GetEnvironment(EnvironmentName.AzureCloud);
                }
                else
                {
                    return GetEnvironment(CurrentSubscription.Environment);
                }
            }
        }

        #region General Methods

        public X509Certificate2 GetCertificate(string thumbprint)
        {
            throw new NotImplementedException();
        }

        public void AddCertificate(X509Certificate2 cert)
        {
            throw new NotImplementedException();
        }

        public void SaveTokenCache(byte[] data)
        {

        }

        public byte[] LoadTokenCache()
        {
            throw new NotImplementedException();
        }

        public void UpdateDefaultSubscritpion(Guid subscriptionId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Environment Methods

        public void AddEnvironment(AzureEnvironment environment)
        {
            throw new NotImplementedException();
        }

        public AzureEnvironment GetEnvironment(string name)
        {
            return Environments.FirstOrDefault(e => e.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }

        public void RemoveEnvironment(string name)
        {
            throw new NotImplementedException();
        }

        public void UpdateEnvironment(AzureEnvironment environment)
        {
            throw new NotImplementedException();
        }

        public Uri GetEndpoint(string enviornment, AzureEnvironment.Endpoint endpoint)
        {
            AzureEnvironment env = GetEnvironment(enviornment);
            Uri endpointUri = null;

            if (env != null)
            {
                string endpointString = env.GetEndpoint(endpoint);
                if (!string.IsNullOrEmpty(endpointString))
                {
                    endpointUri = new Uri(endpointString);
                }
            }

            return endpointUri;
        }

        #endregion

        #region Subscription Methods

        public void AddSubscription(AzureSubscription subscription)
        {
            /*
             * This method will do epic work merging between given subscription and existing
             * subscriptions on the disk
             */
            throw new NotImplementedException();
        }

        public AzureSubscription GetSubscription(Guid? id)
        {
            if (id.HasValue)
            {
                return GetSubscription(id.Value);
            }

            return null;
        }

        public AzureSubscription GetSubscription(Guid id)
        {
            return Subscriptions.FirstOrDefault(s => Guid.Equals(id, s.Id));
        }

        public void RemoveSubscription(string name)
        {
            throw new NotImplementedException();
        }

        public void UpdateSubscription(AzureSubscription subscription)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

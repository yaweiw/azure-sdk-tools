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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WindowsAzure.Commands.Common.Models
{
    public sealed class AzureProfile : IDisposable
    {
        private IDataStore store;

        private AzureSubscription defaultSubscription;

        private void LoadProfile()
        {
            JsonProfileSerializer jsonSerializer = new JsonProfileSerializer();
            string jsonProfilePath = Path.Combine(store.ProfileDirectory, jsonSerializer.ProfileFile);

            if (store.FileExists(jsonProfilePath))
            {
                jsonSerializer.Deserialize(store.ReadAllText(jsonProfilePath), this);
            }

            // Adding predefined environments
            foreach (AzureEnvironment env in AzureEnvironment.PublicEnvironments.Values)
            {
                Environments.Add(env.Name, env);
            }
        }

        private void SaveProfile()
        {
            // Removing predefined environments
            foreach (string env in AzureEnvironment.PublicEnvironments.Keys)
            {
                Environments.Remove(env);
            }

            JsonProfileSerializer jsonSerializer = new JsonProfileSerializer();
            string path = Path.Combine(store.ProfileDirectory, jsonSerializer.ProfileFile);
            string contents = jsonSerializer.Serialize(this);
            store.WriteAllText(path, contents);
        }

        private void UpgradeProfileFormat()
        {
            XmlProfileSerializer xmlSerializer = new XmlProfileSerializer();
            string xmlProfilePath = Path.Combine(store.ProfileDirectory, xmlSerializer.ProfileFile);

            if (store.FileExists(xmlProfilePath))
            {
                // Deserialize the old profile format and delete it.
                xmlSerializer.Deserialize(store.ReadAllText(xmlProfilePath), this);
                store.DeleteFile(xmlProfilePath);

                // Save the profile in the new format
                JsonProfileSerializer jsonSerializer = new JsonProfileSerializer();
                string jsonProfilePath = Path.Combine(store.ProfileDirectory, jsonSerializer.ProfileFile);
                string contents = jsonSerializer.Serialize(this);
                store.WriteAllText(jsonProfilePath, contents);
            }
        }

        public AzureProfile(IDataStore store)
        {
            this.store = store;
            UpgradeProfileFormat();
            LoadProfile();
            defaultSubscription = Subscriptions.FirstOrDefault(
                s => s.Value.Properties.ContainsKey(AzureSubscription.Property.Default)).Value;
        }

        public Dictionary<string, AzureEnvironment> Environments { get; set; }

        public Dictionary<Guid, AzureSubscription> Subscriptions { get; set; }

        public AzureSubscription DefaultSubscription
        {
            get { return defaultSubscription; }

            set
            {
                defaultSubscription.Properties.Remove(AzureSubscription.Property.Default);
                defaultSubscription = value;
                defaultSubscription.Properties.Add(AzureSubscription.Property.Default, null);
            }
        }

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

        public void Dispose()
        {
            SaveProfile();
        }

        public Uri GetEndpoint(string enviornment, AzureEnvironment.Endpoint endpoint)
        {
            AzureEnvironment env = Environments[enviornment];
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
    }
}

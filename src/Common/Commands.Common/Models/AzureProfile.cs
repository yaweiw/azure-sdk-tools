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
using Microsoft.WindowsAzure.Common.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WindowsAzure.Commands.Common.Models
{
    public sealed class AzureProfile
    {
        private IDataStore store;

        private AzureSubscription defaultSubscription;

        private void Load()
        {
            IProfileSerializer serializer;
            string contents = store.ReadProfile();

            if (ParserHelper.IsXml(contents))
            {
                serializer = new XmlProfileSerializer();
                serializer.Deserialize(contents, this);
            }
            else if (ParserHelper.IsJson(contents))
            {
                serializer = new JsonProfileSerializer();
                serializer.Deserialize(contents, this);
            }

            // Adding predefined environments
            foreach (AzureEnvironment env in AzureEnvironment.PublicEnvironments.Values)
            {
                Environments[env.Name] = env;
            }
        }

        public AzureProfile(IDataStore store)
        {
            this.store = store;
            Load();
            defaultSubscription = Subscriptions.FirstOrDefault(
                s => s.Value.Properties.ContainsKey(AzureSubscription.Property.Default)).Value;
        }
        public void Save()
        {
            // Removing predefined environments
            foreach (string env in AzureEnvironment.PublicEnvironments.Keys)
            {
                Environments.Remove(env);
            }

            JsonProfileSerializer jsonSerializer = new JsonProfileSerializer();
            string diskContents = store.ReadProfile();
            string contents = jsonSerializer.Serialize(this);

            if (diskContents != contents)
            {
                store.WriteProfile(contents);
            }
        }

        public IDataStore DataStore { get { return store; } }

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
            return store.GetCertificate(thumbprint);
        }

        public void AddCertificate(X509Certificate2 cert)
        {
            store.AddCertificate(cert);
        }

        public void SaveTokenCache(byte[] data)
        {
            store.WriteTokenCache(data);
        }

        public byte[] LoadTokenCache()
        {
            return store.ReadTokenCache();
        }
    }
}

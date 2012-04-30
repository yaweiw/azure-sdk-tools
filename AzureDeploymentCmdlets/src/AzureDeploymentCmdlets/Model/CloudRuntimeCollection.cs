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

namespace AzureDeploymentCmdlets.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Xml;
    using AzureDeploymentCmdlets.Properties;

    public class CloudRuntimeCollection : Collection<CloudRuntimePackage>, IDisposable
    {
        Dictionary<Runtime, List<CloudRuntimePackage>> packages = new Dictionary<Runtime, List<CloudRuntimePackage>>();
        Dictionary<Runtime, CloudRuntimePackage> defaults = new Dictionary<Runtime, CloudRuntimePackage>();
        private XmlReader documentReader;
        private bool disposed;

        private CloudRuntimeCollection()
        {
            foreach (Runtime runtime in Enum.GetValues(typeof(Runtime)))
            {
                packages[runtime] = new List<CloudRuntimePackage>();
            }
        }

        public static bool CreateCloudRuntimeCollection(Location location, out CloudRuntimeCollection runtimes, string manifestFile = null)
        {
            runtimes = new CloudRuntimeCollection();
            XmlDocument manifest = runtimes.GetManifest(manifestFile);
            string baseUri;
            Collection<CloudRuntimePackage> runtimePackages;
            bool success = TryGetBlobUriFromManifest(manifest, location, out baseUri);
            success &= TryGetRuntimePackages(manifest, baseUri, out runtimePackages);
            foreach (CloudRuntimePackage package in runtimePackages)
            {
                runtimes.AddPackage(package);
            }

            return success;
        }

        public bool TryFindMatch(CloudRuntime runtime, out CloudRuntimePackage matchingPackage)
        {
            matchingPackage = defaults[runtime.Runtime];
            foreach (CloudRuntimePackage package in packages[runtime.Runtime])
            {
                if (runtime.Match(package))
                {
                    matchingPackage = package;
                    return true;
                }
            }

            return false;
        }

        protected override void ClearItems()
        {
            foreach (Runtime runtime in this.packages.Keys)
            {
                this.packages[runtime].Clear();
            }
        }

        protected override void InsertItem(int index, CloudRuntimePackage item)
        {
            Debug.Assert(index < this.packages.Count, string.Format(
                "Attempt to insert a runtime package at position {0} when there are {1} packages total", index, this.packages.Count));
            this.packages[item.Runtime].Insert(index, item);
        }

        protected override void RemoveItem(int index)
        {
            Debug.Assert(index < this.packages.Count, string.Format(
                "Attempt to remove a runtime package at position {0} when there are {1} packages total", index, this.packages.Count));
        }

        protected override void SetItem(int index, CloudRuntimePackage item)
        {
            Debug.Assert(index < this.packages[item.Runtime].Count, string.Format(
                "Attempt to set a runtime package at position {0} when there are {1} packages total", index, this.packages.Count));
            this.packages[item.Runtime][index] = item;
        }

        private static bool TryGetBlobUriFromManifest(XmlDocument manifest, Location location, out string baseUri)
        {
            Debug.Assert(manifest != null);
            bool found = false;
            baseUri = null;
            string query = string.Format(Resources.DatacenterBlobQuery, ArgumentConstants.Locations[location].ToUpperInvariant());
            XmlNode node = manifest.SelectSingleNode(query);
            if (node != null)
            {
                found = true;
                XmlAttribute blobUriAttribute = node.Attributes[Resources.ManifestBlobUriKey];
                if (null == blobUriAttribute || null == blobUriAttribute.Value)
                {
                    throw new ArgumentException(Resources.InvalidManifestError);
                }

                baseUri = blobUriAttribute.Value;
            }

            return found;
        }

        private XmlDocument GetManifest(string filePath = null)
        {
            if (filePath != null)
            {
                byte[] buffer = File.ReadAllBytes(filePath);
                MemoryStream myStream = new MemoryStream(buffer);
                this.documentReader = XmlReader.Create(myStream);
            }
            else
            {
                this.documentReader = XmlReader.Create(Resources.ManifestUri);
            }

            XmlDocument document = new XmlDocument();
            document.Load(documentReader);
            return document;
        }

        private static bool TryGetRuntimePackages(XmlDocument manifest, string baseUri, out Collection<CloudRuntimePackage> packages)
        {
            bool retrieved = false;
            packages = new Collection<CloudRuntimePackage>();
            XmlNodeList nodes = manifest.SelectNodes(Resources.RuntimeQuery);
            if (nodes != null)
            {
                retrieved = true;
                foreach (XmlNode node in nodes)
                {
                    packages.Add(new CloudRuntimePackage(node, baseUri));
                }
            }

            return retrieved;
        }

        private void AddPackage(CloudRuntimePackage package)
        {
            if (package.IsDefaultRuntimePackage)
            {
                this.defaults[package.Runtime] = package;
            }
            else
            {
                this.packages[package.Runtime].Add(package);
            }
        }


        public void Dispose()
        {
            this.Dispose(!this.disposed);
            this.disposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.documentReader != null)
                {
                    this.documentReader.Close();
                }
            }
        }
    }
}

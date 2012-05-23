// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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
    using System.IO;
    using System.Net;
    using System.Xml;
    using System.Diagnostics;
    using AzureDeploymentCmdlets.Properties;

    public class CloudRuntimePackage
    {
        public const string PrimaryKey = "primaryversionkey";
        public const string SecondaryKey = "secondaryversionkey";
        public const string FileKey = "filepath";
        public const string DefaultKey = "default";
        public const string RuntimeKey = "type";

        public CloudRuntimePackage(XmlNode versionNode, string baseUri)
        {
            this.PrimaryVersionKey = versionNode.Attributes[CloudRuntimePackage.PrimaryKey].Value;
            string filePath = versionNode.Attributes[CloudRuntimePackage.FileKey].Value;
            this.PackageUri = GetUri(baseUri, filePath);
            XmlAttribute secondary = versionNode.Attributes[CloudRuntimePackage.SecondaryKey];
            if (secondary != null)
            {
                this.SecondaryVersionKey = secondary.Value;
            }

            this.Runtime = GetRuntimeType(versionNode.Attributes[CloudRuntimePackage.RuntimeKey].Value);
            this.IsDefaultRuntimePackage = bool.Parse(versionNode.Attributes[CloudRuntimePackage.DefaultKey].Value);
        }

        public string PrimaryVersionKey
        {
            get;
            private set;
        }

        public string SecondaryVersionKey
        {
            get;
            private set;
        }

        public Runtime Runtime
        {
            get;
            private set;
        }

        public Uri PackageUri
        {
            get;
            private set;
        }

        public bool IsDefaultRuntimePackage
        {
            get;
            private set;
        }

        private static Uri GetUri(string baseUri, string filePath)
        {
            UriBuilder baseBuilder = new UriBuilder(baseUri);
            baseBuilder.Path = filePath;
            return baseBuilder.Uri;
        }

        private static Runtime GetRuntimeType(string typeValue)
        {
            Debug.Assert(typeValue != null);
            foreach (Runtime runtime in Enum.GetValues(typeof(Runtime)))
            {
                string comparisonValue = Enum.GetName(typeof(Runtime), runtime);
                if (string.Equals(typeValue, comparisonValue, StringComparison.OrdinalIgnoreCase))
                {
                    return runtime;
                }
            }

            throw new ArgumentException(string.Format(Resources.InvalidRuntimeError, typeValue));
        }
    }
}

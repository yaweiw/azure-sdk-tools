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

namespace Microsoft.WindowsAzure.Management.Websites.Services.WebEntities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Utilities;

    public interface ISiteConfig
    {
        int? NumberOfWorkers { get; set; }

        string[] DefaultDocuments { get; set; }

        string NetFrameworkVersion { get; set; }

        string PhpVersion { get; set; }

        bool? RequestTracingEnabled { get; set; }

        bool? HttpLoggingEnabled { get; set; }

        bool? DetailedErrorLoggingEnabled { get; set; }

        Hashtable AppSettings { get; set; }

        List<NameValuePair> Metadata { get; set; }

        ConnStringPropertyBag ConnectionStrings { get; set; }

        HandlerMapping[] HandlerMappings { get; set; }
    }

    public class FriendlySiteConfig : ISiteConfig
    {
        public SiteConfig SiteConfig { private set; get; }

        public FriendlySiteConfig()
        {
            SiteConfig = new SiteConfig();
        }

        public FriendlySiteConfig(SiteConfig siteConfig)
        {
            SiteConfig = siteConfig;
        }

        public int? NumberOfWorkers
        {
            get { return SiteConfig.NumberOfWorkers; }
            set { SiteConfig.NumberOfWorkers = value; }
        }

        public string[] DefaultDocuments
        {
            get { return SiteConfig.DefaultDocuments; }
            set { SiteConfig.DefaultDocuments = value; }
        }

        public string NetFrameworkVersion
        {
            get { return SiteConfig.NetFrameworkVersion; }
            set { SiteConfig.NetFrameworkVersion = value; }
        }

        public string PhpVersion
        {
            get { return SiteConfig.PhpVersion; }
            set { SiteConfig.PhpVersion = value; }
        }

        public bool? RequestTracingEnabled
        {
            get { return SiteConfig.RequestTracingEnabled; }
            set { SiteConfig.RequestTracingEnabled = value; }
        }
        public bool? HttpLoggingEnabled
        {
            get { return SiteConfig.HttpLoggingEnabled; }
            set { SiteConfig.HttpLoggingEnabled = value; }
        }

        public bool? DetailedErrorLoggingEnabled
        {
            get { return SiteConfig.DetailedErrorLoggingEnabled; }
            set { SiteConfig.DetailedErrorLoggingEnabled = value; }
        }

        public string PublishingUsername
        {
            get { return SiteConfig.PublishingUsername; }
            set { SiteConfig.PublishingUsername = value; }
        }

        public string PublishingPassword
        {
            get { return SiteConfig.PublishingPassword; }
            set { SiteConfig.PublishingPassword = value; }
        }

        public Hashtable AppSettings
        {
            get
            {
                if (SiteConfig.AppSettings != null)
                {
                    Hashtable appSettings = new Hashtable();
                    foreach (var setting in SiteConfig.AppSettings)
                    {
                        appSettings[setting.Name] = setting.Value;
                    }

                    return appSettings;
                }

                return null;
            }

            set
            {
                if (value != null)
                {
                    SiteConfig.AppSettings = new List<NameValuePair>();
                    foreach (var setting in value.Keys)
                    {
                        SiteConfig.AppSettings.Add(new NameValuePair
                        {
                            Name = (string)setting,
                            Value = (string)value[setting]
                        });
                    }
                }
                else
                {
                    SiteConfig.AppSettings = null;
                }
            }
        }
        
        public List<NameValuePair> Metadata
        {
            get { return SiteConfig.Metadata; }
            set { SiteConfig.Metadata = value; }
        }

        public ConnStringPropertyBag ConnectionStrings
        {
            get { return SiteConfig.ConnectionStrings; }
            set { SiteConfig.ConnectionStrings = value; }
        }

        public HandlerMapping[] HandlerMappings
        {
            get { return SiteConfig.HandlerMappings; }
            set { SiteConfig.HandlerMappings = value; }
        }
    }

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class SiteConfig
    {
        [DataMember(IsRequired = false)]
        public int? NumberOfWorkers { get; set; }

        [DataMember(IsRequired = false)]
        public string[] DefaultDocuments { get; set; }

        [DataMember(IsRequired = false)]
        public string NetFrameworkVersion { get; set; }

        [DataMember(IsRequired = false)]
        public string PhpVersion { get; set; }

        [DataMember(IsRequired = false)]
        public bool? RequestTracingEnabled { get; set; }

        [DataMember(IsRequired = false)]
        public bool? HttpLoggingEnabled { get; set; }

        [DataMember(IsRequired = false)]
        public bool? DetailedErrorLoggingEnabled { get; set; }

        [DataMember(IsRequired = false)]
        public string PublishingUsername { get; set; }

        [DataMember(IsRequired = false)]
        [PIIValue]
        public string PublishingPassword { get; set; }

        [DataMember(IsRequired = false)]
        public List<NameValuePair> AppSettings { get; set; }

        [DataMember(IsRequired = false)]
        public List<NameValuePair> Metadata { get; set; }

        [DataMember(IsRequired = false)]
        public ConnStringPropertyBag ConnectionStrings { get; set; }

        [DataMember(IsRequired = false)]
        public HandlerMapping[] HandlerMappings { get; set; }
    }
}

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

        List<NameValuePair> AppSettings { get; set; }

        List<NameValuePair> Metadata { get; set; }

        ConnStringPropertyBag ConnectionStrings { get; set; }

        HandlerMapping[] HandlerMappings { get; set; }
    }

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class SiteConfig : ISiteConfig
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

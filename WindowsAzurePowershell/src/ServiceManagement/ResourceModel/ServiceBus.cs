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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel;

    /// <summary>
    /// Represents service bus namespace
    /// </summary>
    [XmlRoot("NamespaceDescription", Namespace = ServiceBusConstants.ServiceBusXNamespace)]
    public class ServiceBusNamespace
    {
        [XmlElement(Namespace=ServiceBusConstants.ServiceBusXNamespace)]
        public string Name { get; set; }

        [XmlElement(Namespace = ServiceBusConstants.ServiceBusXNamespace)]
        public string Region { get; set; }

        [XmlElement(Namespace = ServiceBusConstants.ServiceBusXNamespace)]
        public string DefaultKey { get; set; }

        [XmlElement(Namespace = ServiceBusConstants.ServiceBusXNamespace)]
        public string Status { get; set; }

        [XmlElement(Namespace = ServiceBusConstants.ServiceBusXNamespace)]
        public string CreatedAt { get; set; }

        [XmlElement(Namespace = ServiceBusConstants.ServiceBusXNamespace)]
        public string AcsManagementEndpoint { get; set; }

        [XmlElement(Namespace = ServiceBusConstants.ServiceBusXNamespace)]
        public string ServiceBusEndpoint { get; set; }

        [XmlElement(Namespace = ServiceBusConstants.ServiceBusXNamespace)]
        public string ConnectionString { get; set; }

        public override bool Equals(object obj)
        {
            ServiceBusNamespace lhs = obj as ServiceBusNamespace;

            if (string.IsNullOrEmpty(Name))
            {
                return false;
            }

            return this.Name.Equals(lhs.Name) && this.Region.Equals(lhs.Region);
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return base.GetHashCode();
            }
            else
            {
                return this.Name.GetHashCode() ^ this.Region.GetHashCode();
            }
        }
    }

    /// <summary>
    /// Represents service bus region entry.
    /// </summary>
    [XmlRoot("RegionCodeDescription", Namespace = ServiceBusConstants.ServiceBusXNamespace)]
    public class ServiceBusRegion
    {
        public static ServiceBusRegion Create(XElement namespaceDescription)
        {
            ServiceBusRegion regions = new ServiceBusRegion();

            regions.Code = namespaceDescription.Element(XName.Get("Code", ServiceBusConstants.ServiceBusXNamespace)).Value;
            regions.FullName = namespaceDescription.Element(XName.Get("FullName", ServiceBusConstants.ServiceBusXNamespace)).Value;

            return regions;
        }

        [XmlElement(Namespace = ServiceBusConstants.ServiceBusXNamespace)]
        public string Code { get; set; }

        [XmlElement(Namespace = ServiceBusConstants.ServiceBusXNamespace)]
        public string FullName { get; set; }
    }
}

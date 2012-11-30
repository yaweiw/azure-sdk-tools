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
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel;

    /// <summary>
    /// List of service bus namespaces.
    /// </summary>
    public class NamespaceList : List<Namespace>
    {
        
    }

    /// <summary>
    /// Represents single service bus namespace
    /// </summary>
    public class Namespace
    {
        public static Namespace Create(XElement namespaceDescription)
        {
            Namespace serviceBusNamespace = new Namespace();

            serviceBusNamespace.Name = namespaceDescription.Element(XName.Get("Name", ServiceBusConstants.ServiceBusXNamespace)).Value;
            serviceBusNamespace.Region = namespaceDescription.Element(XName.Get("Region", ServiceBusConstants.ServiceBusXNamespace)).Value;
            serviceBusNamespace.DefaultKey = namespaceDescription.Element(XName.Get("DefaultKey", ServiceBusConstants.ServiceBusXNamespace)).Value;
            serviceBusNamespace.Status = namespaceDescription.Element(XName.Get("Status", ServiceBusConstants.ServiceBusXNamespace)).Value;
            serviceBusNamespace.CreatedAt = namespaceDescription.Element(XName.Get("CreatedAt", ServiceBusConstants.ServiceBusXNamespace)).Value;
            serviceBusNamespace.AcsManagementEndpoint = new Uri(namespaceDescription.Element(XName.Get("AcsManagementEndpoint", ServiceBusConstants.ServiceBusXNamespace)).Value);
            serviceBusNamespace.ServiceBusEndpoint = new Uri(namespaceDescription.Element(XName.Get("ServiceBusEndpoint", ServiceBusConstants.ServiceBusXNamespace)).Value);
            serviceBusNamespace.ConnectionString = namespaceDescription.Element(XName.Get("ConnectionString", ServiceBusConstants.ServiceBusXNamespace)).Value;

            return serviceBusNamespace;
        }

        public string Name { get; set; }

        public string Region { get; set; }

        public string DefaultKey { get; set; }

        public string Status { get; set; }

        public string CreatedAt { get; set; }

        public Uri AcsManagementEndpoint { get; set; }

        public Uri ServiceBusEndpoint { get; set; }

        public string ConnectionString { get; set; }
    }

    /// <summary>
    /// List of service bus regions.
    /// </summary>
    public class ServiceBusRegionList : List<ServiceBusRegion>
    {

    }

    /// <summary>
    /// Represents service bus region entry.
    /// </summary>
    public class ServiceBusRegion
    {
        public static ServiceBusRegion Create(XElement namespaceDescription)
        {
            ServiceBusRegion regions = new ServiceBusRegion();

            regions.Code = namespaceDescription.Element(XName.Get("Code", ServiceBusConstants.ServiceBusXNamespace)).Value;
            regions.FullName = namespaceDescription.Element(XName.Get("FullName", ServiceBusConstants.ServiceBusXNamespace)).Value;

            return regions;
        }

        public string Code { get; set; }

        public string FullName { get; set; }
    }
}

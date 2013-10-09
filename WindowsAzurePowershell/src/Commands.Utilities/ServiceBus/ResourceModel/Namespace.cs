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

namespace Microsoft.WindowsAzure.Commands.Utilities.ServiceBus.ResourceModel
{
    using System.Xml.Serialization;

    /// <summary>
    /// Represents service bus namespace
    /// </summary>
    [XmlRoot("NamespaceDescription", Namespace = ServiceBusConstants.ServiceBusXNamespace)]
    public class ServiceBusNamespace
    {
        public string Name { get; set; }

        public string Region { get; set; }

        public string DefaultKey { get; set; }

        public string Status { get; set; }

        public string CreatedAt { get; set; }

        public string AcsManagementEndpoint { get; set; }

        public string ServiceBusEndpoint { get; set; }

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
        public string Code { get; set; }

        public string FullName { get; set; }

        public override bool Equals(object obj)
        {
            ServiceBusRegion rhs = obj as ServiceBusRegion;

            return this.Code.Equals(rhs.Code);
        }

        public override int GetHashCode()
        {
            return this.Code.GetHashCode();
        }
    }

    [XmlRoot("NamespaceAvailability", Namespace = ServiceBusConstants.ServiceBusXNamespace)]
    public class ServiceBusNamespaceAvailabilityResponse
    {
        public bool Result { get; set; }

        public override bool Equals(object obj)
        {
            ServiceBusNamespaceAvailabilityResponse rhs = obj as ServiceBusNamespaceAvailabilityResponse;

            return this.Result.Equals(rhs.Result);
        }

        public override int GetHashCode()
        {
            return this.Result.GetHashCode();
        }
    }
}

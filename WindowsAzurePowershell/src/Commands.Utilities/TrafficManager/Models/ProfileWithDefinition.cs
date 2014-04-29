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

namespace Microsoft.WindowsAzure.Commands.Utilities.TrafficManager.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.WindowsAzure.Management.TrafficManager.Models;

    /// <summary>
    /// Class that will be exposed to PowerShell to interact with profiles
    /// This class will be piped between cmdlets.
    /// Note that some definition properties are missing because they are not configurable yet
    /// and would be as read-only. These properties are not exposed in the portal either:
    /// - MonitorExpectedStatusCode
    /// - MonitorVerb
    /// - MonitorIntervalInSeconds
    /// - MonitorTimeOutInSeconds
    /// - MonitorToleratedNumberOfFailures
    /// </summary>
    public class ProfileWithDefinition : IProfileWithDefinition
    {
        private Profile profile { get; set; }
        private Definition definition { get; set; }

        public string Name
        {
            get { return profile.Name; }
            set { profile.Name = value; }
        }

        public string DomainName
        {
            get { return profile.DomainName; }
            set { profile.DomainName = value; }
        }

        public int TimeToLiveInSeconds
        {
            get { return definition.DnsOptions.TimeToLiveInSeconds; }
            set { definition.DnsOptions.TimeToLiveInSeconds = value; }
        }

        public string MonitorRelativePath
        {
            get { return definition.Monitors[0].HttpOptions.RelativePath; }
            set { definition.Monitors[0].HttpOptions.RelativePath = value; }
        }

        public int MonitorPort
        {
            get { return definition.Monitors[0].Port; }
            set { definition.Monitors[0].Port = value; }
        }

        public DefinitionMonitorProtocol MonitorProtocol
        {
            get { return definition.Monitors[0].Protocol; }
            set { definition.Monitors[0].Protocol = value; }
        }

        public LoadBalancingMethod LoadBalancingMethod
        {
            get { return definition.Policy.LoadBalancingMethod; }
            set { definition.Policy.LoadBalancingMethod = value; }
        }

        public IList<TrafficManagerEndpoint> Endpoints
        {
            get
            {
                List<TrafficManagerEndpoint> endpoints = new List<TrafficManagerEndpoint>();

                foreach (DefinitionEndpointResponse endpointReponse in definition.Policy.Endpoints)
                {
                    TrafficManagerEndpoint endpoint = new TrafficManagerEndpoint();
                    endpoint.DomainName = endpointReponse.DomainName;
                    endpoint.Location = endpointReponse.Location;
                    endpoint.Type = endpointReponse.Type;
                    endpoint.Status = endpointReponse.Status;
                    endpoint.Weight = endpointReponse.Weight;

                    endpoints.Add(endpoint);
                }
                return endpoints;
            }
        }

        public ProfileDefinitionStatus Status
        {
            get { return profile.Status; }
            set { profile.Status = value; }
        }



        public ProfileWithDefinition GetInstance()
        {
            return this;
        }

        public ProfileWithDefinition(Profile profile, Definition definition)
        {
            this.profile = profile;
            this.definition = definition;
        }
    }

    public class TrafficManagerEndpoint
    {
        [DataMember(IsRequired = true)]
        public string DomainName { get; set; }

        [DataMember(IsRequired = true)]
        public string Location { get; set; }

        [DataMember(IsRequired = true)]
        public EndpointType Type { get; set; }

        [DataMember(IsRequired = true)]
        public EndpointStatus Status { get; set; }

        [DataMember(IsRequired = true)]
        public int Weight { get; set; }
    }
}

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

namespace Microsoft.WindowsAzure.Commands.Utilities.TrafficManager
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager.Models;
    using Microsoft.WindowsAzure.Management.TrafficManager;
    using Microsoft.WindowsAzure.Management.TrafficManager.Models;

    public class TrafficManagerClient : ITrafficManagerClient
    {
        public TrafficManagerManagementClient Client { get; internal set; }

        public TrafficManagerClient(WindowsAzureSubscription subscription)
        {
            Client = subscription.CreateClient<TrafficManagerManagementClient>();
        }

        public TrafficManagerClient(TrafficManagerManagementClient client)
        {
            Client = client;
        }

        public ProfileWithDefinition NewAzureTrafficManagerProfile(
            string profileName,
            string domainName,
            string loadBalancingMethod,
            int monitorPort,
            string monitorProtocol,
            string monitorRelativePath,
            int ttl)
        {
            // Create the profile
            CreateTrafficManagerProfile(profileName, domainName);

            // Create the definition
            DefinitionCreateParameters definitionParameter = InstantiateTrafficManagerDefinition(
                loadBalancingMethod,
                monitorPort,
                monitorProtocol,
                monitorRelativePath,
                ttl,
                null);

            CreateTrafficManagerDefinition(profileName, definitionParameter);

            return GetTrafficManagerProfileWithDefinition(profileName);
        }

        public ProfileWithDefinition AssignDefinitionToProfile(string profileName, DefinitionCreateParameters definitionParameter)
        {
            Client.Definitions.Create(profileName, definitionParameter);
            return GetTrafficManagerProfileWithDefinition(profileName);
        }

        public bool RemoveTrafficManagerProfile(string profileName)
        {
            OperationResponse resp = Client.Profiles.Delete(profileName);
            return resp.StatusCode == HttpStatusCode.OK;
        }

        public ProfileWithDefinition GetTrafficManagerProfileWithDefinition(string profileName)
        {
            Profile profile = GetProfile(profileName).Profile;
            Definition definition = null;
            try
            {
                definition = GetDefinition(profileName).Definition;

            }
            catch (CloudException)
            {
                
            }
            return new ProfileWithDefinition(profile, definition);
        }

        public DefinitionCreateParameters InstantiateTrafficManagerDefinition(
            string loadBalancingMethod,
            int monitorPort,
            string monitorProtocol,
            string monitorRelativePath,
            int ttl,
            IList<TrafficManagerEndpoint> endpoints)
        {
            // Create the definition
            DefinitionCreateParameters definitionParameter = new DefinitionCreateParameters();
            DefinitionDnsOptions dnsOptions = new DefinitionDnsOptions();
            DefinitionMonitor monitor = new DefinitionMonitor();
            DefinitionPolicyCreateParameters policyParameter = new DefinitionPolicyCreateParameters();
            DefinitionMonitorHTTPOptions monitorHttpOption = new DefinitionMonitorHTTPOptions();

            dnsOptions.TimeToLiveInSeconds = ttl;

            monitorHttpOption.RelativePath = monitorRelativePath;
            monitorHttpOption.Verb = Constants.monitorHttpOptionVerb;
            monitorHttpOption.ExpectedStatusCode = Constants.monitorHttpOptionExpectedStatusCode;

            monitor.Protocol =
                (DefinitionMonitorProtocol)Enum.Parse(typeof(DefinitionMonitorProtocol), monitorProtocol);
            monitor.IntervalInSeconds = Constants.monitorIntervalInSeconds;
            monitor.TimeoutInSeconds = Constants.monitorTimeoutInSeconds;
            monitor.ToleratedNumberOfFailures = Constants.monitorToleratedNumberOfFailures;
            monitor.Port = monitorPort;

            policyParameter.LoadBalancingMethod =
                (LoadBalancingMethod)Enum.Parse(typeof(LoadBalancingMethod), loadBalancingMethod);

            policyParameter.Endpoints = new List<DefinitionEndpointCreateParameters>();
            foreach (TrafficManagerEndpoint endpoint in endpoints)
            {
                DefinitionEndpointCreateParameters endpointParam = new DefinitionEndpointCreateParameters();
                endpointParam.DomainName = endpoint.DomainName;
                endpointParam.Location = endpoint.Location;
                endpointParam.Status = endpoint.Status;
                endpointParam.Type = endpoint.Type;
                endpointParam.Weight = endpoint.Weight;

                policyParameter.Endpoints.Add(endpointParam);
            }


            definitionParameter.DnsOptions = dnsOptions;
            definitionParameter.Policy = policyParameter;
            definitionParameter.Monitors.Add(monitor);
            monitor.HttpOptions = monitorHttpOption;

            return definitionParameter;
        }

        /// <summary>
        /// Transforms a Definition instance to a DefinitionCreateParameters
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public DefinitionCreateParameters InstantiateTrafficManagerDefinition(Definition definition)
        {
            DefinitionCreateParameters definitionCreateParams = new DefinitionCreateParameters();

            List<DefinitionEndpointCreateParameters> endpoints = new List<DefinitionEndpointCreateParameters>();
            foreach (DefinitionEndpointResponse endpointReponse in definition.Policy.Endpoints)
            {
                DefinitionEndpointCreateParameters endpoint = new DefinitionEndpointCreateParameters();
                endpoint.DomainName = endpointReponse.DomainName;
                endpoint.Location = endpointReponse.Location;
                endpoint.Type = endpointReponse.Type;
                endpoint.Status = endpointReponse.Status;
                endpoint.Weight = endpointReponse.Weight;

                endpoints.Add(endpoint);
            }

            definitionCreateParams.Policy = new DefinitionPolicyCreateParameters()
            {
                Endpoints = endpoints,
                LoadBalancingMethod = definition.Policy.LoadBalancingMethod
            };

            definitionCreateParams.DnsOptions = definition.DnsOptions;
            definitionCreateParams.Monitors = definition.Monitors;

            return definitionCreateParams;
        }

        public void UpdateProfileStatus(string profileName, ProfileDefinitionStatus targetStatus)
        {
            ProfileDefinitionStatus currentStatus = GetStatus(profileName);
            if (currentStatus != targetStatus)
            {
                Client.Profiles.Update(profileName, targetStatus, 1);
            }
        }

        public ProfileDefinitionStatus GetStatus(string profileName)
        {
            return Client.Profiles.Get(profileName).Profile.Status;
        }

        public void CreateTrafficManagerProfile(string profileName, string domainName)
        {
            Client.Profiles.Create(profileName, domainName);
        }

        public void CreateTrafficManagerDefinition(string profileName, DefinitionCreateParameters parameters)
        {
            Client.Definitions.Create(profileName, parameters);
        }

        public ProfileGetResponse GetProfile(string profileName)
        {
            return Client.Profiles.Get(profileName);
        }

        public DefinitionGetResponse GetDefinition(string profileName)
        {
            return Client.Definitions.Get(profileName);
        }


        public IEnumerable<SimpleProfile> ListProfiles()
        {
            IList<Profile> respProfiles = Client.Profiles.List().Profiles;


            IEnumerable<SimpleProfile> resultProfiles =
                respProfiles.Select(respProfile => new SimpleProfile(respProfile));

            return resultProfiles;
        }


        public bool TestDomainAvailability(string domainName)
        {
            return Client.Profiles.CheckDnsPrefixAvailability(domainName).Result;
        }
    }
}

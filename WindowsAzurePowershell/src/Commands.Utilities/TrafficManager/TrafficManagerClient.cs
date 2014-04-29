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
    using System.Collections.Generic;
    using System.Net;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager.Models;
    using Microsoft.WindowsAzure.Management.TrafficManager;
    using Microsoft.WindowsAzure.Management.TrafficManager.Models;

    public class TrafficManagerClient
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
            System.Int32 monitorPort,
            string monitorProtocol,
            string monitorRelativePath,
            System.Int32 ttl)
        {
            // Create the profile
            Client.Profiles.Create(profileName, domainName);

            // Create the definition
            DefinitionCreateParameters definitionParameter = InstantiateTrafficManagerDefinition(
                loadBalancingMethod,
                monitorPort,
                monitorProtocol,
                monitorRelativePath,
                ttl);

            Client.Definitions.Create(profileName, definitionParameter);

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
            Profile profile = Client.Profiles.Get(profileName).Profile;
            Definition definition = Client.Definitions.Get(profileName).Definition;
            return new ProfileWithDefinition(profile, definition);
        }

        public DefinitionCreateParameters InstantiateTrafficManagerDefinition(
            string loadBalancingMethod,
            System.Int32 monitorPort,
            string monitorProtocol,
            string monitorRelativePath,
            System.Int32 ttl)
        {
            // Create the definition
            DefinitionCreateParameters definitionParameter = new DefinitionCreateParameters();
            DefinitionDnsOptions dnsOptions = new DefinitionDnsOptions();
            DefinitionMonitor monitor = new DefinitionMonitor();
            DefinitionPolicyCreateParameters policyParameter = new DefinitionPolicyCreateParameters();
            DefinitionMonitorHTTPOptions monitorHttpOption = new DefinitionMonitorHTTPOptions();

            dnsOptions.TimeToLiveInSeconds = ttl;

            monitorHttpOption.RelativePath = monitorRelativePath;
            monitorHttpOption.Verb = "GET";
            monitorHttpOption.ExpectedStatusCode = (int)HttpStatusCode.OK;
            // TODO: Use the one supplied and add validation
            monitor.Protocol = DefinitionMonitorProtocol.Http;
            monitor.IntervalInSeconds = 30;
            monitor.TimeoutInSeconds = 10;
            monitor.ToleratedNumberOfFailures = 3;
            monitor.Port = 80;

            // TODO: Use the one supplied and add validation
            policyParameter.LoadBalancingMethod = LoadBalancingMethod.Performance;
            policyParameter.Endpoints = new List<DefinitionEndpointCreateParameters>();

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
    }
}

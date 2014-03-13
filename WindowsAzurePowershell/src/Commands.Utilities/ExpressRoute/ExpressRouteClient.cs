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

namespace Microsoft.WindowsAzure.Commands.Utilities.ExpressRoute
{
    using System.Linq;
    using System.Net;
    using Microsoft.WindowsAzure.Management.ExpressRoute;
    using Microsoft.WindowsAzure.Management.ExpressRoute.Models;
    using CloudService;
    using System;
    using System.Collections.Generic;
    using Utilities.Common;
   
    public class ExpressRouteClient
    {
        public ExpressRouteManagementClient Client { get; internal set; }

        /// <summary>
        /// Creates new ExpressRouteClient
        /// </summary>
        /// <param name="subscription">Subscription containing websites to manipulate</param>
        /// <param name="logger">The logger action</param>
        public ExpressRouteClient(WindowsAzureSubscription subscription)
        {
            Client = subscription.CreateClient<ExpressRouteManagementClient>();
        }

        public ExpressRouteClient(ExpressRouteManagementClient client)
        {
            Client = client;
        }

        public AzureBgpPeering GetAzureBGPPeering(string serviceKey, BgpPeeringAccessType accessType)
        {
            return Client.BgpPeering.Get(serviceKey, accessType).BgpPeering;
        }

        public AzureBgpPeering NewAzureBGPPeering(string serviceKey, UInt32 peerAsn, string primaryPeerSubnet,
            string secondaryPeerSubnet, UInt32 vlanId, BgpPeeringAccessType accessType, string sharedKey = null)
        {
            return Client.BgpPeering.New(serviceKey, accessType, new BgpPeeringNewParameters()
            {
                PeerAsn = peerAsn,
                PrimaryPeerSubnet = primaryPeerSubnet,
                SecondaryPeerSubnet = secondaryPeerSubnet,
                SharedKey = sharedKey,
                VlanId = vlanId
            }).BgpPeering;
        }

        public bool RemoveAzureBGPPeering(string serviceKey, BgpPeeringAccessType accessType)
        {
            var result = Client.BgpPeering.Remove(serviceKey, accessType);
            return result.HttpStatusCode.Equals(HttpStatusCode.OK);
        }

        public AzureBgpPeering UpdateAzureBGPPeering(string serviceKey, 
            BgpPeeringAccessType accessType, UInt32 peerAsn, string primaryPeerSubnet,
            string secondaryPeerSubnet, UInt32 vlanId, string sharedKey)
        {
            return
               (Client.BgpPeering.Update(serviceKey, accessType, new BgpPeeringUpdateParameters()
                {
                    PeerAsn = peerAsn,
                    PrimaryPeerSubnet = primaryPeerSubnet,
                    SecondaryPeerSubnet = secondaryPeerSubnet,
                    SharedKey = sharedKey,
                    VlanId = vlanId,
                })).BgpPeering;
        }
        
        public AzureDedicatedCircuit GetAzureDedicatedCircuit(string serviceKey)
        {
            return (Client.DedicatedCircuit.Get(serviceKey)).DedicatedCircuit;
        }

		public AzureDedicatedCircuit NewAzureDedicatedCircuit(string circuitName, 
            UInt32 bandwidth, string location, string serviceProviderName)
        {
            return (Client.DedicatedCircuit.New(new DedicatedCircuitNewParameters()
            {
                Bandwidth = bandwidth,
                CircuitName = circuitName,
                Location = location,
                ServiceProviderName = serviceProviderName
            })).DedicatedCircuit;
        }

        public IEnumerable<AzureDedicatedCircuit> ListAzureDedicatedCircuit()
        {
            return (Client.DedicatedCircuit.List().DedicatedCircuits);
        }

        public bool RemoveAzureDedicatedCircuit(string serviceKey)
        {
            var result = Client.DedicatedCircuit.Remove(serviceKey);
            return result.HttpStatusCode.Equals(HttpStatusCode.OK);
        }

        public AzureDedicatedCircuitLink GetAzureDedicatedCircuitLink(string serviceKey, string vNetName)
        {
            return (Client.DedicatedCircuitLink.Get(serviceKey, vNetName)).DedicatedCircuitLink;
        }

        public AzureDedicatedCircuitLink NewAzureDedicatedCircuitLink(string serviceKey, string vNetName)
        {
            return (Client.DedicatedCircuitLink.New(serviceKey, vNetName)).DedicatedCircuitLink;
        }

        public IEnumerable<AzureDedicatedCircuitLink> ListAzureDedicatedCircuitLink(string serviceKey)
        {
            return (Client.DedicatedCircuitLink.List(serviceKey).DedicatedCircuitLinks);
        }

        public bool RemoveAzureDedicatedCircuitLink(string serviceKey, string vNetName)
        {
            var result = Client.DedicatedCircuitLink.Remove(serviceKey, vNetName);
            return result.HttpStatusCode.Equals(HttpStatusCode.OK);
        }

        public IEnumerable<AzureDedicatedCircuitServiceProvider> ListAzureDedicatedCircuitServiceProviders()
        {
            return (Client.DedicatedCircuitServiceProvider.List().DedicatedCircuitServiceProviders);
        }
    }    
}

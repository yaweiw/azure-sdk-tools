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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Xml.Serialization;
    using Management.VirtualNetworks;
    using Management.VirtualNetworks.Models;
    using Properties;
    using Utilities.Common;
    // TODO: Need to wait for the fix for this.NetworkClient.Networks.SetConfiguration(netParams))
    using WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsCommon.Set, "AzureVNetConfig"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureVNetConfigCommand : ServiceManagementBaseCmdlet
    {

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Path to the Network Configuration file (.xml).")]
        [ValidateNotNullOrEmpty]
        public string ConfigurationPath
        {
            get;
            set;
        }

        internal void ExecuteCommandNewSM()
        {
            ValidateParameters();
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(this.ConfigurationPath);
                XmlSerializer ser = new XmlSerializer(typeof(NetworkConfiguration));
                NetworkConfiguration netConfig = (NetworkConfiguration)ser.Deserialize(sr);
                if (netConfig == null)
                {
                    throw new ArgumentException(Resources.NetworkConfigurationCannotBeDeserialized);
                }

                var netParams = new NetworkSetConfigurationParameters();

                if (netConfig.VirtualNetworkConfiguration != null)
                {
                    if (netConfig.VirtualNetworkConfiguration.Dns != null &&
                        netConfig.VirtualNetworkConfiguration.Dns.DnsServers != null)
                    {
                        foreach (var ds in netConfig.VirtualNetworkConfiguration.Dns.DnsServers)
                        {
                            netParams.DnsServers.Add(
                                new NetworkSetConfigurationParameters.DnsServer
                                {
                                    IPAddress = IPAddress.Parse(ds.IPAddress),
                                    Name = ds.name
                                });
                        }
                    }

                    if (netConfig.VirtualNetworkConfiguration.LocalNetworkSites != null)
                    {
                        foreach (var lns in netConfig.VirtualNetworkConfiguration.LocalNetworkSites)
                        {
                            var newItem = new NetworkSetConfigurationParameters.LocalNetworkSite();
                            if (lns.AddressSpace != null)
                            {
                                foreach (var aa in lns.AddressSpace)
                                {
                                    newItem.AddressSpace.Add(aa);
                                }
                            }

                            newItem.Name = lns.name;
                            newItem.VpnGatewayAddress = IPAddress.Parse(lns.VPNGatewayAddress);
                            netParams.LocalNetworkSites.Add(newItem);
                        }
                    }

                    if (netConfig.VirtualNetworkConfiguration.VirtualNetworkSites != null)
                    {
                        foreach (var vns in netConfig.VirtualNetworkConfiguration.VirtualNetworkSites)
                        {
                            var newItem = new NetworkSetConfigurationParameters.VirtualNetworkSite();
                            newItem.AffinityGroup = vns.AffinityGroup;
                            newItem.Name = vns.name;
                            newItem.Label = vns.InternetGatewayNetwork == null ? vns.name : vns.InternetGatewayNetwork.name;

                            if (vns.AddressSpace != null)
                            {
                                foreach (var aa in vns.AddressSpace)
                                {
                                    newItem.AddressSpace.Add(aa);
                                }
                            }

                            if (vns.DnsServersRef != null)
                            {
                                foreach (var dsr in vns.DnsServersRef)
                                {
                                    newItem.DnsServersReference.Add(
                                        new NetworkSetConfigurationParameters.DnsServerReference
                                        {
                                            Name = dsr.name
                                        });
                                }
                            }

                            newItem.Gateway = new NetworkSetConfigurationParameters.Gateway();
                            if (vns.Gateway != null)
                            {
                                newItem.Gateway.Profile = vns.Gateway.profile.ToString();

                                if (vns.Gateway.VPNClientAddressPool != null)
                                {
                                    foreach (var ca in vns.Gateway.VPNClientAddressPool)
                                    {
                                        newItem.Gateway.VpnClientAddressPool.Add(ca);
                                    }
                                }

                                if (vns.Gateway.ConnectionsToLocalNetwork != null)
                                {
                                    foreach (var lnsr in vns.Gateway.ConnectionsToLocalNetwork)
                                    {
                                        if (lnsr.Connection != null)
                                        {
                                            foreach (var conn in lnsr.Connection)
                                            {

                                                newItem.Gateway.ConnectionsToLocalNetwork.Add(
                                                    new NetworkSetConfigurationParameters.LocalNetworkSiteReference
                                                    {
                                                        ConnectionType = (LocalNetworkConnectionType)Enum.Parse(typeof(LocalNetworkConnectionType), conn.type.ToString(), true),
                                                        Name = lnsr.name
                                                    });
                                            }
                                        }
                                    }
                                }
                            }

                            if (vns.Subnets != null)
                            {
                                foreach (var sn in vns.Subnets)
                                {
                                    newItem.Subnets.Add(new NetworkSetConfigurationParameters.Subnet
                                    {
                                        AddressPrefix = sn.AddressPrefix,
                                        Name = sn.name
                                    });
                                }
                            }
                        }
                    }
                }

                ExecuteClientActionNewSM(
                    null,
                    CommandRuntime.ToString(),
                    () => this.NetworkClient.Networks.SetConfiguration(netParams));
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }
            }
        }

        internal void ExecuteCommand()
        {
            ValidateParameters();

            FileStream netConfigFS = null;

            try
            {
                netConfigFS = new FileStream(this.ConfigurationPath, FileMode.Open);

                ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.SetNetworkConfiguration(s, netConfigFS));
            }
            finally
            {
                if (netConfigFS != null)
                {
                    netConfigFS.Close();
                }
            }
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }

        private void ValidateParameters()
        {
            if (!File.Exists(ConfigurationPath))
            {
                throw new ArgumentException(Resources.NetworkConfigurationFilePathDoesNotExist);
            }
        }
    }
}

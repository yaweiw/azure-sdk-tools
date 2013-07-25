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


namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using WindowsAzure.ServiceManagement;

    public class ConfigurationSetsBuilder
    {
        public Collection<ConfigurationSet> ConfigurationSets { get; set; }

        private WindowsProvisioningConfigurationSetBuilder windowsBuilder;
        public WindowsProvisioningConfigurationSetBuilder WindowsConfigurationBuilder
        {
            get { return windowsBuilder ?? (windowsBuilder = new WindowsProvisioningConfigurationSetBuilder(ConfigurationSets)); }
        }

        private LinuxProvisioningConfigurationSetBuilder linuxBuilder;
        public LinuxProvisioningConfigurationSetBuilder LinuxConfigurationBuilder
        {
            get { return linuxBuilder ?? (linuxBuilder = new LinuxProvisioningConfigurationSetBuilder(ConfigurationSets)); }
        }

        private NetworkConfigurationSetBuilder networkConfigurationBuilder;
        public NetworkConfigurationSetBuilder NetworkConfigurationBuilder
        {
            get { return networkConfigurationBuilder ?? (networkConfigurationBuilder = new NetworkConfigurationSetBuilder(ConfigurationSets)); }
        }

        public ConfigurationSetsBuilder(Collection<ConfigurationSet> configurationSets)
        {
            ConfigurationSets = configurationSets;
        }
    }

    public class WindowsProvisioningConfigurationSetBuilder
    {
        protected Collection<ConfigurationSet> ConfigurationSets { get; set; }

        public WindowsProvisioningConfigurationSet WindowsProvisioning
        {
            get;
            private set;
        }

        public WindowsProvisioningConfigurationSetBuilder(Collection<ConfigurationSet> configurationSets)
        {
            this.ConfigurationSets = configurationSets;
            Initialize();
        }

        private void Initialize()
        {
            var provisioningConfigurationSet = ConfigurationSets.OfType<WindowsProvisioningConfigurationSet>().SingleOrDefault();
            if (provisioningConfigurationSet == null)
            {
                WindowsProvisioning = new WindowsProvisioningConfigurationSet();
                ConfigurationSets.Add(WindowsProvisioning);
            }
            else
            {
                WindowsProvisioning = provisioningConfigurationSet;
            }
        }

        public static bool WindowsConfigurationExists(Collection<ConfigurationSet> configurationSets)
        {
            var provisioningConfigurationSet = configurationSets.OfType<WindowsProvisioningConfigurationSet>().SingleOrDefault();
            return provisioningConfigurationSet != null;
        }
    }

    public class LinuxProvisioningConfigurationSetBuilder
    {
        protected Collection<ConfigurationSet> ConfigurationSets { get; set; }

        public LinuxProvisioningConfigurationSet LinuxProvisioning
        {
            get;
            private set;
        }

        public LinuxProvisioningConfigurationSetBuilder(Collection<ConfigurationSet> configurationSets)
        {
            this.ConfigurationSets = configurationSets;
            Initialize();
        }

        private void Initialize()
        {
            var provisioningConfigurationSet = ConfigurationSets.OfType<LinuxProvisioningConfigurationSet>().SingleOrDefault();
            if (provisioningConfigurationSet == null)
            {
                LinuxProvisioning = new LinuxProvisioningConfigurationSet();
                ConfigurationSets.Add(LinuxProvisioning);
            }
            else
            {
                LinuxProvisioning = provisioningConfigurationSet;
            }
        }

        public static bool LinuxConfigurationExists(Collection<ConfigurationSet> configurationSets)
        {
            var provisioningConfigurationSet = configurationSets.OfType<LinuxProvisioningConfigurationSet>().SingleOrDefault();
            return provisioningConfigurationSet != null;
        }
    }

    public class NetworkConfigurationSetBuilder
    {
        private const int RDPPortNumber = 3389;
        private const int WinRMPortNumber = 5986;
        private const int SSHPortNumber = 22;

        protected Collection<ConfigurationSet> ConfigurationSets { get; set; }

        public static bool HasNetworkConfigurationSet(Collection<ConfigurationSet> configurationSets)
        {
            var networkConfigurationSet = configurationSets.OfType<NetworkConfigurationSet>().SingleOrDefault();
            return networkConfigurationSet != null;
        }

        public NetworkConfigurationSet NetworkConfigurationSet
        {
            get;
            private set;
        }

        public NetworkConfigurationSetBuilder(Collection<ConfigurationSet> configurationSets)
        {
            this.ConfigurationSets = configurationSets;
            Initialize();
        }

        private void Initialize()
        {
            var networkConfigurationSet = ConfigurationSets.OfType<NetworkConfigurationSet>().SingleOrDefault();
            if (networkConfigurationSet == null)
            {
                NetworkConfigurationSet = new NetworkConfigurationSet();
                ConfigurationSets.Add(NetworkConfigurationSet);
            }
            else
            {
                NetworkConfigurationSet = networkConfigurationSet;
            }

            if (NetworkConfigurationSet.InputEndpoints == null)
            {
                NetworkConfigurationSet.InputEndpoints = new Collection<InputEndpoint>();
            }
        }

        public void AddWinRmEndpoint()
        {
            var winRmEndpoint = new InputEndpoint { LocalPort = WinRMPortNumber, Protocol = "tcp", Name = WinRMConstants.EndpointName };
            NetworkConfigurationSet.InputEndpoints.Add(winRmEndpoint);
        }

        public void AddRdpEndpoint()
        {
            var endPoint = GetRdpEndpoint(NetworkConfigurationSet);
            if (endPoint != null)
            {
                endPoint.Port = null; // null out to avoid conflicts
            }
            else
            {
                var rdpEndpoint = new InputEndpoint { LocalPort = RDPPortNumber, Protocol = "tcp", Name = "RDP" };
                NetworkConfigurationSet.InputEndpoints.Add(rdpEndpoint);
            }
        }

        private static InputEndpoint GetRdpEndpoint(NetworkConfigurationSet networkConfigurationSet)
        {
            return networkConfigurationSet.InputEndpoints.FirstOrDefault(ep => string.Compare(ep.Name, "RDP", StringComparison.OrdinalIgnoreCase) == 0 || ep.LocalPort == RDPPortNumber);
        }

        public void AddSshEndpoint()
        {
            var endpoint = GetSSHEndpoint(NetworkConfigurationSet);
            if (endpoint != null)
            {
                endpoint.Port = null;  // null out to avoid conflicts
            }
            else
            {
                var sshEndpoint = new InputEndpoint { LocalPort = SSHPortNumber, Protocol = "tcp", Name = "SSH" };
                NetworkConfigurationSet.InputEndpoints.Add(sshEndpoint);
            }
        }

        private static InputEndpoint GetSSHEndpoint(NetworkConfigurationSet networkConfigurationSet)
        {
            return networkConfigurationSet.InputEndpoints.FirstOrDefault(ep => string.Compare(ep.Name, "SSH", StringComparison.OrdinalIgnoreCase) == 0 || ep.LocalPort == SSHPortNumber);
        }
    }
}
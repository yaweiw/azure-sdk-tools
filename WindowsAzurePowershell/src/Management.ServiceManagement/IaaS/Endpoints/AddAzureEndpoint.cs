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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.Endpoints
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using Samples.WindowsAzure.ServiceManagement;
    using IaaS;
    using Model;

    [Cmdlet(VerbsCommon.Add, "AzureEndpoint", DefaultParameterSetName = "NoLB"), OutputType(typeof(IPersistentVM))]
    public class AddAzureEndpoint : VirtualMachineConfigurationCmdletBase 
    {
        [Parameter(Position = 0, ParameterSetName = "NoLB", Mandatory = true, HelpMessage = "Endpoint name")]
        [Parameter(Position = 0, ParameterSetName = "LoadBalanced", Mandatory = true, HelpMessage = "Endpoint name")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "NoLB", HelpMessage = "Endpoint protocol.")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "LoadBalanced", HelpMessage = "Endpoint protocol.")]
        [ValidateSet("tcp", "udp", IgnoreCase = true)]
        [ValidateNotNullOrEmpty]
        public string Protocol
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "NoLB", HelpMessage = "Local port.")]
        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "LoadBalanced", HelpMessage = "Local port.")]
        [ValidateNotNullOrEmpty]
        public int LocalPort
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "NoLB", HelpMessage = "Public port.")]
        [Parameter(Mandatory = false, ParameterSetName = "LoadBalanced", HelpMessage = "Public port.")]
        [ValidateNotNullOrEmpty]
        public int? PublicPort
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "LoadBalanced", HelpMessage = "Load Balanced Endpoint Set Name")]
        [Alias("LoadBalancedEndpointSetName")]
        public string LBSetName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "LoadBalanced", HelpMessage = "Probe Port")]
        public int ProbePort
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "LoadBalanced", HelpMessage = "Probe Protocol (http/tcp)")]
        [ValidateSet("tcp", "http", IgnoreCase = true)]
        public string ProbeProtocol
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "LoadBalanced", HelpMessage = "Probe Relative Path")]
        public string ProbePath
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            ValidateParameters();

            var endpoints = GetInputEndpoints();
            var endpoint = endpoints.SingleOrDefault(p => p.Name == Name);

            if (endpoint != null)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                            new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "An endpoint named '{0}' has already been defined for this VM. Specify a different endpoint name or use Set-Endpoint to change the configuration settings of the existing endpoint.", this.Name)),
                            string.Empty,
                            ErrorCategory.InvalidData,
                            null));
            }

            endpoint = new InputEndpoint
            {
                Name = Name,
                Port = PublicPort.HasValue ? PublicPort : null,
                LocalPort = LocalPort,
                Protocol = Protocol,
            };

            if (!string.IsNullOrEmpty(LBSetName))
            {
                endpoint.LoadBalancedEndpointSetName = LBSetName;
                endpoint.LoadBalancerProbe = new LoadBalancerProbe { Protocol = ProbeProtocol };

                if (endpoint.LoadBalancerProbe.Protocol == "http")
                {
                    if (string.IsNullOrEmpty(ProbePath) == false)
                    {
                        endpoint.LoadBalancerProbe.Path = ProbePath;
                    }
                    else
                    {
                        endpoint.LoadBalancerProbe.Path = "/";
                    }
                }

                endpoint.LoadBalancerProbe.Port = ProbePort;
            }

            endpoints.Add(endpoint);

            WriteObject(VM, true);
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        protected Collection<InputEndpoint> GetInputEndpoints()
        {
            var role = VM.GetInstance();

            var networkConfiguration = role.ConfigurationSets
                                        .OfType<NetworkConfigurationSet>()
                                        .SingleOrDefault();

            if (networkConfiguration == null)
            {
                networkConfiguration = new NetworkConfigurationSet();
                role.ConfigurationSets.Add(networkConfiguration);
            }

            if (networkConfiguration.InputEndpoints == null)
            {
                networkConfiguration.InputEndpoints = new Collection<InputEndpoint>();
            }

            var inputEndpoints = networkConfiguration.InputEndpoints;

            return inputEndpoints;
        }

        private void ValidateParameters()
        {
            if (string.Compare(ParameterSetName, "LoadBalanced", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (string.Compare(ProbeProtocol, "tcp", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!string.IsNullOrEmpty(ProbePath))
                    {
                        throw new ArgumentException("ProbePath not valid with tcp");
                    }
                }

                if (string.Compare(ProbeProtocol, "http", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(ProbePath))
                    {
                        throw new ArgumentException("ProbePath is required for http");
                    }
                }
            }

            if (LocalPort < 0 || LocalPort > 65535)
            {
                throw new ArgumentException("Ports must be in the range of 0 - 65535");
            }

            if (PublicPort != null && (PublicPort < 0 || PublicPort > 65535))
            {
                throw new ArgumentException("Ports must be in the range of 0 - 65535");
            }

            if (ProbePort < 0 || ProbePort > 65535)
            {
                throw new ArgumentException("Ports must be in the range of 0 - 65535");
            }
        }       
    }
}
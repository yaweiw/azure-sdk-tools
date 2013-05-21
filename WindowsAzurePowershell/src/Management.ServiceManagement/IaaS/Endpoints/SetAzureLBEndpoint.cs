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
    using System.Linq;
    using System.Management.Automation;
    using IaaS;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsCommon.Set, "AzureLBEndpoint", DefaultParameterSetName = SetAzureLBEndpoint.DefaultProbeParameterSet), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureLBEndpoint : IaaSDeploymentManagementCmdletBase
    {
        public const string DefaultProbeParameterSet = "DefaultProbe";
        public const string TCPProbeParameterSet = "TCPProbe";
        public const string HTTPProbeParameterSet = "HTTPProbe";

        [Parameter(Mandatory = false, ParameterSetName = SetAzureLBEndpoint.DefaultProbeParameterSet, HelpMessage = "Should a default probe be used.")]
        public SwitchParameter NoDefaultProbe { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = SetAzureLBEndpoint.TCPProbeParameterSet, HelpMessage = "Should a TCP probe should be used.")]
        public SwitchParameter ProbeProtocolTCP { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = SetAzureLBEndpoint.HTTPProbeParameterSet, HelpMessage = "Should a HTTP probe should be used.")]
        public SwitchParameter ProbeProtocolHTTP { get; set; }
                
        [Parameter(Mandatory = true, HelpMessage = "Load balancer set name.")]
        [ValidateNotNullOrEmpty]
        public string LBSetName { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Endpoint protocol.")]
        [ValidateSet("TCP", "UDP", IgnoreCase = true)]
        public string Protocol { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Private port.")]
        public int LocalPort { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Public port.")]
        public int? PublicPort { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Enable Direct Server Return")]
        public bool DirectServerReturn { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "ACLs to specify with the endpoint.")]
        public NetworkAclObject ACL { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = SetAzureLBEndpoint.HTTPProbeParameterSet, HelpMessage = "Relative path to the HTTP probe.")]
        [ValidateNotNullOrEmpty]
        public string ProbePath { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Probe interval in seconds.")]
        public int? ProbeIntervalInSeconds { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Probe timeout in seconds.")]
        public int? ProbeTimeoutInSeconds { get; set; }

        internal override void ExecuteCommand()
        {
            base.ExecuteCommand();
            if (string.IsNullOrEmpty(this.ServiceName) || this.CurrentDeployment == null)
            {
                return;
            }

            var endpoint = this.GetEndpoint();

            this.UpdateEndpointProperties(endpoint);

            this.ExecuteClientActionInOCS(
                null,
                this.CommandRuntime.ToString(),
                s => this.Channel.UpdateLoadBalancedEndpointSet(
                    this.CurrentSubscription.SubscriptionId,
                    this.ServiceName,
                    this.CurrentDeployment.Name,
                    new LoadBalancedEndpointList{endpoint}));
        }

        private InputEndpoint GetEndpoint()
        {
            var role = this.CurrentDeployment.RoleList.SingleOrDefault();

            if (role.NetworkConfigurationSet == null)
            {
                return null;
            }

            if (role.NetworkConfigurationSet.InputEndpoints == null)
            {
                return null;
            }

            return role.NetworkConfigurationSet.InputEndpoints.SingleOrDefault(p => p.LoadBalancedEndpointSetName == this.LBSetName);
        }

        private void UpdateEndpointProperties(InputEndpoint endpoint)
        {
            if (this.ParameterSpecified("Protocol"))
            {
                endpoint.Protocol = this.Protocol;
            }

            if (this.ParameterSpecified("LocalPort"))
            {
                endpoint.LocalPort = this.LocalPort;
            }

            if (this.ParameterSpecified("PublicPort"))
            {
                endpoint.Port = this.PublicPort;
            }

            if (this.ParameterSpecified("DirectServerReturn"))
            {
                endpoint.EnableDirectServerReturn = this.DirectServerReturn;
            }

            if (this.ParameterSpecified("ACL"))
            {
                endpoint.EndpointAccessControlList = this.ACL;
            }

            if (this.ParameterSpecified("ProbeIntervalInSeconds"))
            {
                endpoint.LoadBalancerProbe.IntervalInSeconds = this.ProbeIntervalInSeconds;
            }

            if (this.ParameterSpecified("ProbeTimeoutInSeconds"))
            {
                endpoint.LoadBalancerProbe.TimeoutInSeconds = this.ProbeTimeoutInSeconds;
            }

            if (this.ParameterSetName.Equals(SetAzureLBEndpoint.DefaultProbeParameterSet)
                && !this.NoDefaultProbe)
            {
                endpoint.LoadBalancerProbe = new LoadBalancerProbe()
                {
                    Protocol = "TCP",
                    Port = endpoint.LocalPort
                };
            }

            if (this.ParameterSetName.Equals(SetAzureLBEndpoint.HTTPProbeParameterSet))
            {
                endpoint.LoadBalancerProbe.Protocol = "http";
                endpoint.LoadBalancerProbe.Path = this.ProbePath;
            }

            if (this.ParameterSetName.Equals(SetAzureLBEndpoint.TCPProbeParameterSet))
            {
                endpoint.LoadBalancerProbe.Protocol = "tcp";
                endpoint.LoadBalancerProbe.Path = null;
            }   
        }

        private bool ParameterSpecified(string parameterName)
        {
            // Check for parameters by name so we can tell the difference between 
            // the user not specifying them, and the user specifying null/empty.
            return this.MyInvocation.BoundParameters.ContainsKey(parameterName);
        }
    }
}

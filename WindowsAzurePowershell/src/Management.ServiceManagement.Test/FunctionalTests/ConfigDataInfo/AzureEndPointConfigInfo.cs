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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.ConfigDataInfo
{
    using System;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;

    public class AzureEndPointConfigInfo
    {
        public enum ParameterSet { NoLB, LoadBalanced, LoadBalancedProbe };

        public ProtocolInfo EndpointProtocol { get; set; }
        public int EndpointLocalPort { get; set; }
        public int? EndpointPublicPort { get; set; }
        public string EndpointName { get; set; }
        public string LBSetName { get; set; }
        public int ProbePort { get; set; }
        public ProtocolInfo ProbeProtocol { get; set; }
        public string ProbePath { get; set; }
        public int? ProbeInterval { get; set; }
        public int? ProbeTimeout { get; set; }
        public PersistentVM Vm { get; set; }
        public ParameterSet ParamSet { get; set; }

        public AzureEndPointConfigInfo(ProtocolInfo endpointProtocol, int endpointLocalPort,
            int endpointPublicPort, string endpointName)
        {
            this.Initialize(
                endpointProtocol, 
                endpointLocalPort, 
                endpointPublicPort, 
                endpointName, 
                string.Empty, 
                0, 
                ProtocolInfo.tcp, 
                string.Empty, 
                null, 
                null,
                ParameterSet.NoLB);
        }

        public AzureEndPointConfigInfo(ProtocolInfo endpointProtocol, int endpointLocalPort,
            int endpointPublicPort, string endpointName, string lBSetName)
        {
            this.Initialize(
                endpointProtocol,
                endpointLocalPort,
                endpointPublicPort,
                endpointName,
                lBSetName,
                0,
                ProtocolInfo.tcp,
                string.Empty,
                null,
                null,
                ParameterSet.LoadBalanced);
        }

        public AzureEndPointConfigInfo(ProtocolInfo endpointProtocol, int endpointLocalPort, 
            int endpointPublicPort, string endpointName, string lBSetName, int probePort,
            ProtocolInfo probeProtocol, string probePath, int? probeInterval, int? probeTimeout)
        {
            this.Initialize(
                endpointProtocol,
                endpointLocalPort,
                endpointPublicPort,
                endpointName,
                lBSetName,
                probePort,
                probeProtocol,
                probePath,
                probeInterval,
                probeTimeout,
                ParameterSet.LoadBalancedProbe);
        }

        public AzureEndPointConfigInfo(AzureEndPointConfigInfo other)
        {
            this.Initialize(
                other.EndpointProtocol,
                other.EndpointLocalPort,
                other.EndpointPublicPort,
                other.EndpointName,
                other.LBSetName,
                other.ProbePort,
                other.ProbeProtocol,
                other.ProbePath,
                other.ProbeInterval,
                other.ProbeTimeout,
                other.ParamSet);
        }

        private void Initialize(ProtocolInfo protocol, int internalPort,
            int? externalPort, string endpointName, string lBSetName, int probePort,
            ProtocolInfo probeProtocol, string probePath, 
            int? probeInterval, int? probeTimeout, ParameterSet paramSet)
        {
            this.EndpointLocalPort = internalPort;
            this.EndpointProtocol = protocol;
            this.EndpointPublicPort = externalPort;
            this.EndpointName = endpointName;
            this.LBSetName = lBSetName;
            this.ProbePort = probePort;
            this.ProbeProtocol = probeProtocol;
            this.ProbePath = probePath;
            this.ProbeInterval = probeInterval;
            this.ProbeTimeout = probeTimeout;
            this.ParamSet = paramSet;
        }

        public bool CheckInputEndpointContext(InputEndpointContext context)
        {
            bool ret = context.Protocol == this.EndpointProtocol.ToString()
                && context.LocalPort == this.EndpointLocalPort
                && context.Port == this.EndpointPublicPort
                && context.Name == this.EndpointName;

            if (ParamSet == ParameterSet.LoadBalanced)
            {
                ret = ret && context.LBSetName == this.LBSetName;
            }

            if (ParamSet == ParameterSet.LoadBalancedProbe)
            {
                ret = ret && context.LBSetName == this.LBSetName
                    && context.ProbePort == this.ProbePort
                    && context.ProbeProtocol == this.ProbeProtocol.ToString();

                ret = ret && ( this.ProbeInterval.HasValue 
                                ? context.ProbeIntervalInSeconds == this.ProbeInterval 
                                : context.ProbeIntervalInSeconds == 15 );

                ret = ret && ( this.ProbeTimeout.HasValue
                                ? context.ProbeTimeoutInSeconds == this.ProbeTimeout
                                : context.ProbeTimeoutInSeconds == 31 );
            }

            return ret;
        }
    }
}

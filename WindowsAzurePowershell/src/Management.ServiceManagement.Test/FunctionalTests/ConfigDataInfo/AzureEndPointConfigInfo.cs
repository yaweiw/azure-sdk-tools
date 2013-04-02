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
    public class AzureEndPointConfigInfo
    {
        public readonly ProtocolInfo Protocol;
        public readonly int ExternalPort;
        public readonly int InternalPort;
        public readonly string EndpointName;
        public readonly string LBSetName;
        public readonly int ProbePort;
        public readonly ProtocolInfo ProbeProtocol;
        public readonly string ProbePath;

        public AzureEndPointConfigInfo(ProtocolInfo protocol, int internalPort,
            int externalPort, string endpointName)
        {
            this.InternalPort = internalPort;
            this.Protocol = protocol;
            this.ExternalPort = externalPort;
            this.EndpointName = endpointName;
            this.ProbeProtocol = protocol;
        }

        public AzureEndPointConfigInfo(ProtocolInfo protocol, int internalPort, 
            int externalPort, string endpointName, string lBSetName, int probePort,
            ProtocolInfo probeProtocol, string probePath)
        {
            this.InternalPort = internalPort;
            this.Protocol = protocol;
            this.ExternalPort = externalPort;
            this.EndpointName = endpointName;
            this.LBSetName = lBSetName;
            this.ProbePort = probePort;
            this.ProbeProtocol = probeProtocol;
            this.ProbePath = probePath; 
        }

        public Model.PersistentVM Vm { get; set; }
    }
}

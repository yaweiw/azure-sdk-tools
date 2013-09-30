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
    using System.IO;
    using System.Management.Automation;
    using System.Xml;
    using System.Xml.Linq;
    using Management.VirtualNetworks;
    using Management.VirtualNetworks.Models;
    using Utilities.Common;
    // TODO: Need to wait for the fix for this.NetworkClient.Networks.SetConfiguration(netParams))
    using WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsCommon.Remove, "AzureVNetConfig"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureVNetConfigCommand : ServiceManagementBaseCmdlet
    {
        private static readonly XNamespace NetconfigNamespace = "http://schemas.microsoft.com/ServiceHosting/2011/07/NetworkConfiguration";
        private static readonly XNamespace InstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        protected void OnProcessRecordNewSM()
        {
            NetworkSetConfigurationParameters networkConfigParams = new NetworkSetConfigurationParameters();

            this.ExecuteClientActionNewSM(
                networkConfigParams,
                this.CommandRuntime.ToString(),
                () => this.NetworkClient.Networks.SetConfiguration(networkConfigParams));
        }

        protected override void OnProcessRecord()
        {
            var netConfig = new XElement(
                NetconfigNamespace + "NetworkConfiguration",
                new XAttribute("xmlns", NetconfigNamespace.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "xsi", InstanceNamespace.NamespaceName),
                new XElement(NetconfigNamespace + "VirtualNetworkConfiguration"));

            var stream = new MemoryStream();
            var writer1 = XmlWriter.Create(stream);
            netConfig.WriteTo(writer1);
            writer1.Flush();
            stream.Seek(0L, SeekOrigin.Begin);

            this.ExecuteClientActionInOCS(null, this.CommandRuntime.ToString(), s => this.Channel.SetNetworkConfiguration(s, stream));
        }
    }
}

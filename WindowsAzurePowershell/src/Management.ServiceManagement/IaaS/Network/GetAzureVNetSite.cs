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
    using System.ServiceModel;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using Samples.WindowsAzure.ServiceManagement;
    using Model;
    using Cmdlets.Common;

    [Cmdlet(VerbsCommon.Get, "AzureVNetSite"), OutputType(typeof(IEnumerable<VirtualNetworkSiteContext>))]
    public class GetAzureVNetSiteCommand : CloudBaseCmdlet<IServiceManagement>
    {
        public GetAzureVNetSiteCommand()
        {
        }

        public GetAzureVNetSiteCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, HelpMessage = "The virtual network name.")]
        [ValidateNotNullOrEmpty]
        public string VNetName
        {
            get;
            set;
        }

        public IEnumerable<VirtualNetworkSiteContext> GetVirtualNetworkSiteProcess()
        {            
            using (new OperationContextScope((IContextChannel)Channel))
            {
                try
                {
                    var sites = this.RetryCall(s => this.Channel.ListVirtualNetworkSites(s)).ToList();

                    if (!string.IsNullOrEmpty(this.VNetName))
                    {
                        sites = sites.Where(s => string.Equals(s.Name, this.VNetName, StringComparison.InvariantCultureIgnoreCase)).ToList();

                        if (sites.Count() == 0)
                        {
                            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The specified virtual network name was not found: {0}", this.VNetName), "VirtualNetworkName");
                        }
                    }

                    Operation operation = WaitForOperation(CommandRuntime.ToString());
                    return sites.Select(s => new VirtualNetworkSiteContext
                    {
                        OperationId = operation.OperationTrackingId,
                        OperationDescription = CommandRuntime.ToString(),
                        OperationStatus = operation.Status,
                        AddressSpacePrefixes = s.AddressSpace != null ? s.AddressSpace.AddressPrefixes : null,
                        AffinityGroup = s.AffinityGroup,
                        DnsServers = s.Dns != null ? s.Dns.DnsServers : null,
                        GatewayProfile = s.Gateway != null ? s.Gateway.Profile : null,
                        GatewaySites = s.Gateway != null ? s.Gateway.Sites : null,
                        Id = s.Id,
                        InUse = s.InUse,
                        Label = s.Label,
                        Name = s.Name,
                        State = s.State,
                        Subnets = s.Subnets
                    }).ToList();
                }
                catch (CommunicationException ex)
                {
                    if (ex is EndpointNotFoundException && !IsVerbose())
                    {
                        return null;
                    }
                    else
                    {
                        this.WriteErrorDetails(ex);
                    }
                }

                return null;
            }
        }

        protected override void OnProcessRecord()
        {
            var virtualNetworkSites = this.GetVirtualNetworkSiteProcess();

            if (virtualNetworkSites != null)
            {
                WriteObject(virtualNetworkSites, true);
            }
        }
    }
}
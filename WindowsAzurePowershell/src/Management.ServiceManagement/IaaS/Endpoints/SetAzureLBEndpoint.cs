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
    using Properties;
    using WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsCommon.Set, "AzureLBEndpoint"), OutputType(typeof(IPersistentVM))]
    public class SetAzureLBEndpoint : VirtualMachineConfigurationCmdletBase
    {
        public const string TCPProbeParameterSet = "TCPProbe";
        public const string HTTPProbeParameterSet = "HTTPProbe";

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = SetAzureLBEndpoint.TCPProbeParameterSet, HelpMessage = "Indicates that a TCP probe should be used.")]
        public SwitchParameter ProbeProtocolTCP { get; set; }

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = SetAzureLBEndpoint.HTTPProbeParameterSet, HelpMessage = "Indicates that a HTTP probe should be used.")]
        public SwitchParameter ProbeProtocolHTTP { get; set; }

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "Cloud service name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName { get; set; }

        [Parameter(Position = 2, Mandatory = true, HelpMessage = "Load balancer set name.")]
        [ValidateNotNullOrEmpty]
        public string LBSetName { get; set; }

        [Parameter(Position = 3, Mandatory = true, HelpMessage = "Endpoint protocol.")]
        [ValidateSet("TCP", "UDP", IgnoreCase = true)]
        public string Protocol { get; set; }

        [Parameter(Position = 4, Mandatory = true, HelpMessage = "Private port.")]
        public int LocalPort { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Public port.")]
        public int? PublicPort { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = SetAzureLBEndpoint.HTTPProbeParameterSet, HelpMessage = "Relative path to the HTTP probe.")]
        [ValidateNotNullOrEmpty]
        public string ProbePath { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Probe interval in seconds.")]
        public int? ProbeIntervalInSeconds { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Probe timeout in seconds.")]
        public int? ProbeTimeoutInSeconds { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "ACLs to specify with the endpoint.")]
        public NetworkAclObject[] ACL { get; set; }

        internal void ExecuteCommand()
        {
            /*
            ValidateParameters();
            var endpoints = GetInputEndpoints();
            var endpoint = endpoints.SingleOrDefault(p => p.Name == Name);

            if (endpoint == null)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                            new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.EndpointCanNotBeFoundInVMConfigurationInSetAzureEndpoint, this.Name)),
                            string.Empty,
                            ErrorCategory.InvalidData,
                            null));
            }

            endpoint.Port = PublicPort.HasValue ? PublicPort : null;
            endpoint.LocalPort = LocalPort;
            endpoint.Protocol = Protocol;

            WriteObject(VM, true);
             * */
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
            /*
            base.ExecuteCommand();

            SubscriptionData currentSubscription = this.GetCurrentSubscription();
            if (CurrentDeployment == null)
            {
                return;
            }

            // Auto generate disk names based off of default storage account 
            foreach (DataVirtualHardDisk datadisk in this.VM.DataVirtualHardDisks)
            {
                if (datadisk.MediaLink == null && string.IsNullOrEmpty(datadisk.DiskName))
                {
                    CloudStorageAccount currentStorage = CloudStorageAccountFactory.GetCurrentCloudStorageAccount(Channel, currentSubscription);
                    if (currentStorage == null)
                    {
                        throw new ArgumentException(Resources.CurrentStorageAccountIsNotAccessible);
                    }

                    DateTime dateTimeCreated = DateTime.Now;
                    string diskPartName = VM.RoleName;

                    if (datadisk.DiskLabel != null)
                    {
                        diskPartName += "-" + datadisk.DiskLabel;
                    }

                    string vhdname = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", ServiceName, diskPartName, dateTimeCreated.Year, dateTimeCreated.Month, dateTimeCreated.Day, dateTimeCreated.Millisecond);
                    string blobEndpoint = currentStorage.BlobEndpoint.AbsoluteUri;

                    if (blobEndpoint.EndsWith("/") == false)
                    {
                        blobEndpoint += "/";
                    }

                    datadisk.MediaLink = new Uri(blobEndpoint + "vhds/" + vhdname);
                }

                if (VM.DataVirtualHardDisks.Count > 1)
                {
                    // To avoid duplicate disk names
                    System.Threading.Thread.Sleep(1);
                }
            }

            var role = new PersistentVMRole
            {
                AvailabilitySetName = VM.AvailabilitySetName,
                ConfigurationSets = VM.ConfigurationSets,
                DataVirtualHardDisks = VM.DataVirtualHardDisks,
                Label = VM.Label,
                OSVirtualHardDisk = VM.OSVirtualHardDisk,
                RoleName = VM.RoleName,
                RoleSize = VM.RoleSize,
                RoleType = VM.RoleType
            };

            ExecuteClientActionInOCS(role, CommandRuntime.ToString(), s => this.Channel.UpdateRole(s, this.ServiceName, CurrentDeployment.Name, this.Name, role));


            /*
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
            return inputEndpoints;*/
            return null;
        }

        private void ValidateParameters()
        {
            if (LocalPort < 0 || LocalPort > 65535)
            {
                throw new ArgumentException(Resources.PortsNotInRangeInSetAzureEndpoint);
            }

            if (PublicPort != null && (PublicPort < 0 || PublicPort > 65535))
            {
                throw new ArgumentException(Resources.PortsNotInRangeInSetAzureEndpoint);
            }
        }
    }
}

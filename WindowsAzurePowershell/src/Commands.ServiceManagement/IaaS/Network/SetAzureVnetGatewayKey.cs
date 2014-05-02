using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.Service.Gateway;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Management.Network.Models;
using Microsoft.WindowsAzure.ServiceManagement;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    [Cmdlet(VerbsCommon.Set, "AzureVNetGatewayKey"), OutputType(typeof(SharedKeyContext))]
    public class SetAzureVNetGatewayKey : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "The virtual network name.")]
        [ValidateNotNullOrEmpty]
        public string VNetName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "The local network site name.")]
        [ValidateNotNullOrEmpty]
        public string LocalNetworkSiteName
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, HelpMessage = "The shared key that will be used on the Azure Gateway and the customer's VPN device.")]
        [ValidateNotNullOrEmpty]
        public string SharedKey
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            this.ExecuteClientActionNewSM(
                null,
                this.CommandRuntime.ToString(),
                () => this.NetworkClient.Gateways.SetSharedKey(
                            this.VNetName,
                            this.LocalNetworkSiteName,
                            new GatewaySetSharedKeyParameters()
                            {
                                Value = SharedKey,
                            }));
        }
    }
}

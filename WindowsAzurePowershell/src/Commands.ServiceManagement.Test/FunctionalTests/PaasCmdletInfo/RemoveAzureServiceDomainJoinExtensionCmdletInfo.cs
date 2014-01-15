
namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PaasCmdletInfo
{
    using PowershellCore;

    public class RemoveAzureServiceDomainJoinExtensionCmdletInfo:CmdletsInfo
    {
        public RemoveAzureServiceDomainJoinExtensionCmdletInfo(string serviceName, string slot, string[] role, bool uninstallConfiguration)
        {
            this.cmdletName = Utilities.RemoveAzureServiceDomainJoinExtension;
            if(!string.IsNullOrEmpty(serviceName))
            {
                this.cmdletParams.Add(new CmdletParam("ServiceName", serviceName));
            }
            if(!string.IsNullOrEmpty(slot))
            {
                this.cmdletParams.Add(new CmdletParam("Slot", slot));
            }
            if(role != null)
            {
                this.cmdletParams.Add(new CmdletParam("Role", role));
            }
            if(uninstallConfiguration)
            {
                this.cmdletParams.Add(new CmdletParam("UninstallConfiguration"));
            }
        }
    }
}

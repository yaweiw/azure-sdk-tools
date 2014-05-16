

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PaasCmdletInfo
{
    using PowershellCore;
    public class ResetAzureRoleInstanceCmdletInfo : CmdletsInfo
    {
        public ResetAzureRoleInstanceCmdletInfo(string serviceName, string instanceName, string slot, bool reboot = false, bool reimage = false)
        {
            cmdletName = Utilities.ResetAzureRoleInstanceCmdletName;
            cmdletParams.Add(new CmdletParam("ServiceName", serviceName));
            cmdletParams.Add(new CmdletParam("InstanceName", instanceName));
            cmdletParams.Add(new CmdletParam("Slot", slot));
            if (reboot)
                cmdletParams.Add(new CmdletParam("Reboot"));
            if (reimage)
                cmdletParams.Add(new CmdletParam("Reimage"));
        }
    }
}

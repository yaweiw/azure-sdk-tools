
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PaasCmdletInfo
{
    using PowershellCore;
    public class ReSetAzureRoleInstanceCmdletInfo : CmdletsInfo
    {
        public ReSetAzureRoleInstanceCmdletInfo(string ServiceName, string InstanceName, string Slot, bool Reboot = false, bool Reimage = false)
        {
            cmdletName = Utilities.ReSetAzureRoleInstanceCmdletName;
            cmdletParams.Add(new CmdletParam("ServiceName", ServiceName));
            cmdletParams.Add(new CmdletParam("InstanceName", InstanceName));
            cmdletParams.Add(new CmdletParam("Slot", Slot));
            if (Reboot)
                cmdletParams.Add(new CmdletParam("Reboot"));
            if (Reimage)
                cmdletParams.Add(new CmdletParam("Reimage"));
        }
    }
}

using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo.Extensions.BGInfo
{
    public class SetAzureVMBGInfoExtensionCmdletInfo : CmdletsInfo
    {
        public SetAzureVMBGInfoExtensionCmdletInfo(IPersistentVM vm, string version = null, string referenceName = null, bool disable = false)
        {
            cmdletName = Utilities.SetAzureVMBGInfoExtensionCmdletName;
            cmdletParams.Add(new CmdletParam("VM", vm));
            if (!string.IsNullOrEmpty(version))
            {
                cmdletParams.Add(new CmdletParam("Version", version));
            }
            if (!string.IsNullOrEmpty(referenceName))
            {
                cmdletParams.Add(new CmdletParam("ReferenceName", referenceName));
            }
            if (disable)
            {
                cmdletParams.Add(new CmdletParam("Disable"));
            }
        }
    }
}

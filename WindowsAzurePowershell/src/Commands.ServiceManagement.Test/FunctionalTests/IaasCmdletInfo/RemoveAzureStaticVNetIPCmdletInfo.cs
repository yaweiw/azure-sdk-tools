using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{
    public class RemoveAzureStaticVNetIPCmdletInfo : CmdletsInfo
    {
        public RemoveAzureStaticVNetIPCmdletInfo(IPersistentVM vM)
        {
            cmdletName = Utilities.RemoveAzureStaticVNetIPCmdletName;
            cmdletParams.Add(new CmdletParam("VM", vM));
        }

    }
}

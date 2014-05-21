using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{
    public class SetAzureStaticVNetIPCmdletInfo : CmdletsInfo
    {
        public SetAzureStaticVNetIPCmdletInfo(string iPAddress, IPersistentVM vM)
        {
            cmdletName = Utilities.SetAzureStaticVNetIPCmdletName;
            cmdletParams.Add(new CmdletParam("IPAddress", iPAddress));
            cmdletParams.Add(new CmdletParam("VM", vM));
        }

    }
}

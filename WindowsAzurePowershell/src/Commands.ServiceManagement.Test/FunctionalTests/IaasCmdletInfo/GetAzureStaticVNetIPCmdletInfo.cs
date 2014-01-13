using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{
    
    public class GetAzureStaticVNetIPCmdletInfo : CmdletsInfo
    {
        public GetAzureStaticVNetIPCmdletInfo(IPersistentVM vM)
        {
            cmdletName = Utilities.GetAzureStaticVNetIPCmdletName;
            cmdletParams.Add(new CmdletParam("VM", vM));
        }

    }
}

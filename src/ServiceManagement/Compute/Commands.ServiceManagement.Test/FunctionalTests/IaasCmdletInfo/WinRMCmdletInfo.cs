using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{

    using ConfigDataInfo;
    using Microsoft.WindowsAzure.ServiceManagement;
    using PowershellCore;

    class WinRMCmdletInfo : CmdletsInfo
    {

        public WinRMCmdletInfo(string servicename, string name)
        {
            cmdletName = Utilities.GetAzureWinRMUriCmdletName;

            cmdletParams.Add(new CmdletParam("ServiceName", servicename));

            cmdletParams.Add(new CmdletParam("Name", name));
            
        }
    }
}

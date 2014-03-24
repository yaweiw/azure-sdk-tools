
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{
    public class TestAzureStaticVNetIPCmdletInfo: CmdletsInfo
    {
        public TestAzureStaticVNetIPCmdletInfo(string vNetName, string iPAddress)
        {
            cmdletName = Utilities.TestAzureStaticVNetIPCmdletName;
            cmdletParams.Add(new CmdletParam("VNetName", vNetName));
            cmdletParams.Add(new CmdletParam("IPAddress", iPAddress));
        }

    }
}

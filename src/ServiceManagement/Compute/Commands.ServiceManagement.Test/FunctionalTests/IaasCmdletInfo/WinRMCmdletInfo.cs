
namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{

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

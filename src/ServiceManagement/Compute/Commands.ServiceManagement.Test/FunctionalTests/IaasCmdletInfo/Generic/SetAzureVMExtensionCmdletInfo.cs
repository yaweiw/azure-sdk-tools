using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo.Extensions.Common
{
    public class SetAzureVMExtensionCmdletInfo: CmdletsInfo
    {
        public SetAzureVMExtensionCmdletInfo(IPersistentVM vm, string extensionName, string publisher, string version = null, string referenceName = null,
            string publicConfiguration = null, string privateConfiguration = null,string publicConfigPath = null,string privateConfigPath =  null, bool disable = false)
        {
            cmdletName = Utilities.SetAzureVMExtensionCmdletName;
            cmdletParams.Add(new CmdletParam("VM", vm));
            cmdletParams.Add(new CmdletParam("ExtensionName", extensionName));
            cmdletParams.Add(new CmdletParam("Publisher", publisher));
            if (!string.IsNullOrEmpty(version))
            {
                cmdletParams.Add(new CmdletParam("Version", version));
            }
            if (!string.IsNullOrEmpty(referenceName))
            {
                cmdletParams.Add(new CmdletParam("ReferenceName", referenceName));
            }
            if (!string.IsNullOrEmpty(publicConfiguration))
            {
                cmdletParams.Add(new CmdletParam("PublicConfiguration", publicConfiguration));
            }
            if (!string.IsNullOrEmpty(privateConfiguration))
            {
                cmdletParams.Add(new CmdletParam("PrivateConfiguration", privateConfiguration));
            }
            if (disable)
            {
                cmdletParams.Add(new CmdletParam("Disable"));
            }
            if (!string.IsNullOrEmpty(publicConfigPath))
            {
                cmdletParams.Add(new CmdletParam("PublicConfigPath", publicConfigPath));
            }
            if (!string.IsNullOrEmpty(privateConfigPath))
            {
                cmdletParams.Add(new CmdletParam("PrivateConfigPath", privateConfigPath));
            }
        }
    }
}

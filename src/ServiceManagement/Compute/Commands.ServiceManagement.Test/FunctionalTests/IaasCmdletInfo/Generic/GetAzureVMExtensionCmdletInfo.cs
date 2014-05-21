using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo.Extensions.Common
{
    public class GetAzureVMExtensionCmdletInfo : CmdletsInfo
    {
        public GetAzureVMExtensionCmdletInfo(IPersistentVM vm,string extensionName=null, string publisher=null, string version = null, string referenceName = null,
            string publicConfiguration = null, string privateConfiguration = null, bool disable= false)
        {
            cmdletName = Utilities.GetAzureVMExtensionCmdletName;
            cmdletParams.Add(new CmdletParam("VM", vm));
            if (!string.IsNullOrEmpty(extensionName))
            {
                cmdletParams.Add(new CmdletParam("ExtensionName", extensionName));
            }
            if (!string.IsNullOrEmpty(publisher))
            {
                cmdletParams.Add(new CmdletParam("Publisher", publisher));
            }
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
        }
    }
}

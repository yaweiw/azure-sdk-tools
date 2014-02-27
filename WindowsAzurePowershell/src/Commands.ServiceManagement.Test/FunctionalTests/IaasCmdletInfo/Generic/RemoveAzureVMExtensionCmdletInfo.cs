using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo.Extensions.Common
{
    public class RemoveAzureVMExtensionCmdletInfo : CmdletsInfo
    {

        public RemoveAzureVMExtensionCmdletInfo(IPersistentVM vm, string extensionName, string publisher, string referenceName, bool removeAll)
        {
            cmdletName = Utilities.RemoveAzureVMExtensionCmdletName;
            cmdletParams.Add(new CmdletParam("VM", vm));
            if (!string.IsNullOrEmpty(extensionName))
            {
                cmdletParams.Add(new CmdletParam("ExtensionName", extensionName));
            }
            if (!string.IsNullOrEmpty(publisher))
            {
                cmdletParams.Add(new CmdletParam("Publisher", publisher));
            }
            if (!string.IsNullOrEmpty(referenceName))
            {
                cmdletParams.Add(new CmdletParam("ReferenceName", referenceName));
            } 
            if (removeAll)
            {
                cmdletParams.Add(new CmdletParam("RemoveAll", removeAll));
            } 
        }
    }
}

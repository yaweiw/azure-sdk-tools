using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo.Extensions.Common
{
    public class GetAzureVMAvailableExtensionCmdletInfo: CmdletsInfo
    {
        
        private GetAzureVMAvailableExtensionCmdletInfo()
        {
            cmdletName = Utilities.GetAzureVMAvailableExtensionCmdletName;
        }

        //ListLatestExtensionsParamSet -> ExtensionName,Publisher,
        public GetAzureVMAvailableExtensionCmdletInfo(string extensionName = null, string publisher = null)
            :this()
        {
            
            if (!string.IsNullOrEmpty(extensionName))
            {
                cmdletParams.Add(new CmdletParam("ExtensionName", extensionName));
            }
            if (!string.IsNullOrEmpty(publisher))
            {
                cmdletParams.Add(new CmdletParam("Publisher", publisher));
            } 
        }

        //ListAllVersionsParamSetName -> ExtensionName,Publisher,AllVersions
        public GetAzureVMAvailableExtensionCmdletInfo(string extensionName, string publisher, bool allVersions)
            : this(extensionName, publisher)
        {
            if (allVersions)
            {
                cmdletParams.Add(new CmdletParam("AllVersions"));
            }
        }

        //ListSingleVersionParamSetName -> ExtensionName,Publisher,Version
        public GetAzureVMAvailableExtensionCmdletInfo(string extensionName, string publisher, string version)
            : this(extensionName, publisher)
        {
            if (!string.IsNullOrEmpty(publisher))
            {
                cmdletParams.Add(new CmdletParam("Version", version));
            }
        }
    }
}

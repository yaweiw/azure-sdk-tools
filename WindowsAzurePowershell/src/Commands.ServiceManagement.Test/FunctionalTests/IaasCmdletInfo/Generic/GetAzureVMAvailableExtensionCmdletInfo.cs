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
        
        private GetAzureVMAvailableExtensionCmdletInfo(IPersistentVM vm = null)
        {
            cmdletName = Utilities.GetAzureVMAvailableExtensionCmdletName;

            if (vm != null)
            {
                cmdletParams.Add(new CmdletParam("VM", vm));
            }
        }

        //ListLatestExtensionsParamSet -> ExtensionName,Publisher,
        public GetAzureVMAvailableExtensionCmdletInfo(IPersistentVM vm = null, string extensionName = null, string publisher = null)
            :this(vm)
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
        public GetAzureVMAvailableExtensionCmdletInfo(IPersistentVM vm,string extensionName, string publisher, bool allVersions)
            : this(vm,extensionName, publisher)
        {
            if (allVersions)
            {
                cmdletParams.Add(new CmdletParam("AllVersions", allVersions));
            }
        }

        //ListSingleVersionParamSetName -> ExtensionName,Publisher,Version
        public GetAzureVMAvailableExtensionCmdletInfo(IPersistentVM vm,string extensionName, string publisher, string version)
            : this(vm,extensionName, publisher)
        {
            if (!string.IsNullOrEmpty(publisher))
            {
                cmdletParams.Add(new CmdletParam("Version", version));
            }
        }
    }
}

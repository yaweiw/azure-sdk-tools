using Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PowershellCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo.Extensions.Common
{
    public class GetAzureVMExtensionConfigTemplateCmdletInfo:CmdletsInfo
    {

        public GetAzureVMExtensionConfigTemplateCmdletInfo(string extensionName, string publisher,string sampleConfigPath, string version = null)
        {
            cmdletName = Utilities.GetAzureVMExtensionConfigTemplateCmdletName;

            cmdletParams.Add(new CmdletParam("ExtensionName", extensionName));
            cmdletParams.Add(new CmdletParam("Publisher", publisher));
            cmdletParams.Add(new CmdletParam("SampleConfigPath", sampleConfigPath));

            if (!string.IsNullOrEmpty(version))
            {
                cmdletParams.Add(new CmdletParam("Version", version));
            }
                
        }
    }
}

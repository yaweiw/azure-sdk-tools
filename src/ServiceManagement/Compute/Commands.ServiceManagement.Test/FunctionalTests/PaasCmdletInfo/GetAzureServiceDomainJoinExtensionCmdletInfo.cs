using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PaasCmdletInfo
{
    using PowershellCore;
    public class GetAzureServiceDomainJoinExtensionCmdletInfo: CmdletsInfo
    {
        public GetAzureServiceDomainJoinExtensionCmdletInfo(string serviceName = null, string slot = null)
        {
            this.cmdletName = Utilities.GetAzureServiceDomainJoinExtension;
            if (!string.IsNullOrEmpty(serviceName))
            {
                this.cmdletParams.Add(new CmdletParam("ServiceName", serviceName));
            }
            if (!string.IsNullOrEmpty(slot))
            {
                this.cmdletParams.Add(new CmdletParam("Slot", slot));
            }
        }
    }
}

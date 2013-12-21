
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Test.FunctionalTests.PaasCmdletInfo
{
    using PowershellCore;
using System.Management.Automation;
    public class RemoveAzureServiceDomainJoinExtensionCmdletInfo:CmdletsInfo
    {
        public RemoveAzureServiceDomainJoinExtensionCmdletInfo(string serviceName = null, string slot = null, string[] role = null, SwitchParameter? uninstallConfiguration = null)
        {
            this.cmdletName = Utilities.RemoveAzureServiceDomainJoinExtension;
            if(!string.IsNullOrEmpty(serviceName))
            {
                this.cmdletParams.Add(new CmdletParam("ServiceName",serviceName));
            }
            if(!string.IsNullOrEmpty(slot))
            {
                this.cmdletParams.Add(new CmdletParam("Slot",slot));
            }
            if(role != null)
            {
                this.cmdletParams.Add(new CmdletParam("Role",role));
            }
            if(uninstallConfiguration.HasValue)
            {
                this.cmdletParams.Add(new CmdletParam("UninstallConfiguration",uninstallConfiguration.Value));
            }
        }
    }
}

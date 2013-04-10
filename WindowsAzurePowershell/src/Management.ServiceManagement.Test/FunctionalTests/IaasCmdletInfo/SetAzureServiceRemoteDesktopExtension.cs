// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{    
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.PowershellCore;
    using System;

    public class SetAzureServiceRemoteDesktopExtensionCmdletInfo : CmdletsInfo
    {
        public SetAzureServiceRemoteDesktopExtensionCmdletInfo(string serviceName)
        {
            this.cmdletName = Utilities.SetAzureServiceRemoteDesktopExtensionCmdletName;
            this.cmdletParams.Add(new CmdletParam("Remove"));
            this.cmdletParams.Add(new CmdletParam("ServiceName", serviceName));
        }

        public SetAzureServiceRemoteDesktopExtensionCmdletInfo(string serviceName, string slot)
            : this(serviceName)
        {
            this.cmdletParams.Add(new CmdletParam("Slot", slot));
        }

        public SetAzureServiceRemoteDesktopExtensionCmdletInfo(string serviceName, string userName, string password)
        {
            this.cmdletName = Utilities.SetAzureServiceRemoteDesktopExtensionCmdletName;            
            this.cmdletParams.Add(new CmdletParam("ServiceName", serviceName));
            this.cmdletParams.Add(new CmdletParam("UserName", userName));
            this.cmdletParams.Add(new CmdletParam("Password", password));
        }

        public SetAzureServiceRemoteDesktopExtensionCmdletInfo(
            string serviceName, string userName, string password, string slot)
            : this(serviceName, userName, password)
        {
            if (string.IsNullOrEmpty(slot))
            {
                this.cmdletParams.Add(new CmdletParam("Slot", slot));
            }
        }

        public SetAzureServiceRemoteDesktopExtensionCmdletInfo(
            string serviceName, string userName, string password, string slot, string extensionID)
            : this(serviceName, userName, password, slot)
        {
            if (string.IsNullOrEmpty(extensionID))
            {
                this.cmdletParams.Add(new CmdletParam("ExtensionID", extensionID));
            }
        }

        public SetAzureServiceRemoteDesktopExtensionCmdletInfo(
            string serviceName, string userName, string password, string slot, string extensionID, DateTime expirationDate)
            : this(serviceName, userName, password, slot, extensionID)
        {
            this.cmdletParams.Add(new CmdletParam("Expiration", expirationDate));
        }
    }
}

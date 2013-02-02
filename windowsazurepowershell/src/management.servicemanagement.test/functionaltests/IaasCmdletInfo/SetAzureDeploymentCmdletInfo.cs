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
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;

    public class SetAzureDeploymentCmdletInfo : CmdletsInfo
    {
        public SetAzureDeploymentCmdletInfo(string option, string serviceName, string packagePath, string newStatus, string configName, string slot, string mode, string label, string roleName, bool force)
        {
            cmdletName = Utilities.SetAzureDeploymentCmdletName;

            switch (option)
            {
                case "config":
                    cmdletParams.Add(new CmdletParam("Config"));
                    break;
                case "status":
                    cmdletParams.Add(new CmdletParam("Status"));
                    break;
                case "upgrade":
                    cmdletParams.Add(new CmdletParam("Upgrade"));
                    break;
            }

            cmdletParams.Add(new CmdletParam("ServiceName", serviceName));
            if (packagePath != null)
            {
                cmdletParams.Add(new CmdletParam("Package", packagePath));
            }
            if (newStatus != null)
            {
                cmdletParams.Add(new CmdletParam("NewStatus", newStatus));
            }
            if (configName != null)
            {
                cmdletParams.Add(new CmdletParam("Configuration", configName));
            }
            if (slot != null)
            {
                cmdletParams.Add(new CmdletParam("Slot", slot));
            }
            if (mode != null)
            {
                cmdletParams.Add(new CmdletParam("Mode", mode));
            }
            if (label != null)
            {
                cmdletParams.Add(new CmdletParam("Label", label));
            }
            if (roleName != null)
            {
                cmdletParams.Add(new CmdletParam("RoleName", roleName));
            }
            if (force)
            {
                cmdletParams.Add(new CmdletParam("Force"));
            }  
        }
    }
}

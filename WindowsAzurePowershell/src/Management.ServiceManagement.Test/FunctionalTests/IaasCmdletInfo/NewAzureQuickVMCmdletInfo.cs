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

    public class NewAzureQuickVMCmdletInfo : CmdletsInfo
    {
        public NewAzureQuickVMCmdletInfo(OS os, string name, string serviceName, string imageName, string password, string locationName)
        {
            cmdletName = Utilities.NewAzureQuickVMCmdletName;
             
            if (os == OS.Windows)
                cmdletParams.Add(new CmdletParam("Windows", null));
            else
                cmdletParams.Add(new CmdletParam("Linux", null));
            cmdletParams.Add(new CmdletParam("ImageName", imageName));
            cmdletParams.Add(new CmdletParam("Name", name));
            cmdletParams.Add(new CmdletParam("ServiceName", serviceName));
            cmdletParams.Add(new CmdletParam("AdminUsername", "DanAdmin"));
            cmdletParams.Add(new CmdletParam("Password", password));
            cmdletParams.Add(new CmdletParam("Location", locationName));
        }


        public NewAzureQuickVMCmdletInfo(OS os, string name, string serviceName, string imageName, string userName, string password, string locationName)
        {
            cmdletName = Utilities.NewAzureQuickVMCmdletName;

            if (os == OS.Windows)
                cmdletParams.Add(new CmdletParam("Windows", null));
            else
                cmdletParams.Add(new CmdletParam("Linux", null));
            cmdletParams.Add(new CmdletParam("ImageName", imageName));
            cmdletParams.Add(new CmdletParam("Name", name));
            cmdletParams.Add(new CmdletParam("ServiceName", serviceName));
            if (os == OS.Windows)
                cmdletParams.Add(new CmdletParam("AdminUsername", userName));
            else
                cmdletParams.Add(new CmdletParam("LinuxUser", userName));
            cmdletParams.Add(new CmdletParam("Password", password));
            cmdletParams.Add(new CmdletParam("Location", locationName));
        }
        
    }
}

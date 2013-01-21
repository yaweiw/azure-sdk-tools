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
//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.PowershellCore;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{
    internal class AddAzureVhdCmdletInfo : CmdletsInfo
    {
        public AddAzureVhdCmdletInfo(string destination, string locaFilePath)
        {
            cmdletName = Utilities.AddAzureVhdCmdletName;
            cmdletParams.Add(new CmdletParam("Destination", destination));
            cmdletParams.Add(new CmdletParam("LocalFilePath", locaFilePath));
        }

        public AddAzureVhdCmdletInfo(string destination, string locaFilePath, string baseImage)
        {
            cmdletName = Utilities.AddAzureVhdCmdletName;
            cmdletParams.Add(new CmdletParam("Destination", destination));
            cmdletParams.Add(new CmdletParam("LocalFilePath", locaFilePath));
            cmdletParams.Add(new CmdletParam("BaseImageUriToPatch", baseImage));
        }

        public AddAzureVhdCmdletInfo(string destination, string locaFilePath, bool overWrite)
        {
            cmdletName = Utilities.AddAzureVhdCmdletName;
            cmdletParams.Add(new CmdletParam("Destination", destination));
            cmdletParams.Add(new CmdletParam("LocalFilePath", locaFilePath));
            if(overWrite)
            {
                cmdletParams.Add(new CmdletParam("OverWrite", null));
            }
        }

        public AddAzureVhdCmdletInfo(string destination, string locaFilePath, int numberOfUploaderThreads, bool overWrite)
        {
            cmdletName = Utilities.AddAzureVhdCmdletName;
            cmdletParams.Add(new CmdletParam("Destination", destination));
            cmdletParams.Add(new CmdletParam("LocalFilePath", locaFilePath));
            cmdletParams.Add(new CmdletParam("NumberOfUploaderThreads", numberOfUploaderThreads));
            if(overWrite)
            {
                cmdletParams.Add(new CmdletParam("OverWrite", null));
            }
        }
    }
}
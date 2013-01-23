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
    public class CopyAzureBlobCmdletInfo : CmdletsInfo
    {
        public CopyAzureBlobCmdletInfo(string source, string destination, bool overwrite)
        {
            this.cmdletName = Utilities.CopyAzureBlobCmdletName;
            this.cmdletParams.Add(new CmdletParam("Source", source));
            this.cmdletParams.Add(new CmdletParam("Destination", destination));

            if (overwrite)
            {
                this.cmdletParams.Add(new CmdletParam("Overwrite", null));
            }
        }
    }
}

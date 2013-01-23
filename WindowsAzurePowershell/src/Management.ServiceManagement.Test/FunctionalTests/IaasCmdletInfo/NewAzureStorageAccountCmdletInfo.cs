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
using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.PowershellCore;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo
{
    public class NewAzureStorageAccountCmdletInfo : CmdletsInfo
    {
        public NewAzureStorageAccountCmdletInfo(string storageAccountName, string location)
        {
            this.cmdletName = Utilities.NewAzureStorageAccountCmdletName;

            this.cmdletParams.Add(new CmdletParam("StorageAccountName", storageAccountName));
            this.cmdletParams.Add(new CmdletParam("Location", location));
        }
    }
}

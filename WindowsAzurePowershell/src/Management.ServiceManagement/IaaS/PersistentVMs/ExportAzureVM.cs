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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
    using System.Management.Automation;
    using Samples.WindowsAzure.ServiceManagement;
    using Helpers;
    using Model;

    [Cmdlet(VerbsData.Export, "AzureVM")]
    public class ExportAzureVMCommand : GetAzureVMCommand
    {
        public ExportAzureVMCommand()
        {
        }

        public ExportAzureVMCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 2, Mandatory = true, HelpMessage = "The file path in which serialize the persistent VM role state.")]
        [ValidateNotNullOrEmpty]
        public string Path
        {
            get;
            set;
        }

        protected override void SaveRoleState(PersistentVM role)
        {
            PersistentVMHelper.SaveStateToFile(role, Path);
        }
    }
}

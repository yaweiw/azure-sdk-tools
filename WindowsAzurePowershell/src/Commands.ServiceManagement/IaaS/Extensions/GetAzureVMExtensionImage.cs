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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Management.Compute;
    using Management.Compute.Models;
    using Utilities.Common;

    /// <summary>
    /// Get Windows Azure VM Extension Image.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, AzureVMExtensionImageCommandNoun), OutputType(typeof(VMExtensionImageContext))]
    public class GetAzureVMExtensionImageCommand : ServiceManagementBaseCmdlet
    {
        protected const string AzureVMExtensionImageCommandNoun = "AzureVMExtensionImage";

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, HelpMessage = "The Extension Image Type.")]
        [ValidateNotNullOrEmpty]
        public string ExtensionImageType
        {
            get;
            set;
        }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, HelpMessage = "The Extension Publisher.")]
        [ValidateNotNullOrEmpty]
        public string Publisher
        {
            get;
            set;
        }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2, HelpMessage = "The Extension Version.")]
        [ValidateNotNullOrEmpty]
        public string Version
        {
            get;
            set;
        }

        public void ExecuteCommand()
        {
            ServiceManagementProfile.Initialize(this);

            var truePred = (Func<VirtualMachineExtensionListResponse.ResourceExtension, bool>)(s => true);

            Func<string, Func<VirtualMachineExtensionListResponse.ResourceExtension, string>,
                 Func<VirtualMachineExtensionListResponse.ResourceExtension, bool>> predFunc =
                 (x, f) => string.IsNullOrEmpty(x) ? truePred : s => string.Equals(x, f(s), StringComparison.OrdinalIgnoreCase);

            var typePred = predFunc(this.ExtensionImageType, s => s.Name);
            var publisherPred = predFunc(this.Publisher, s => s.Publisher);
            var versionPred = predFunc(this.Version, s => s.Version);

            ExecuteClientActionNewSM(null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.VirtualMachineExtensions.List(),
                (op, response) => response.Where(typePred).Where(publisherPred).Where(versionPred).Select(
                     extension => ContextFactory<VirtualMachineExtensionListResponse.ResourceExtension, VMExtensionImageContext>(extension, op)));
        }

        protected override void OnProcessRecord()
        {
            try
            {
                this.ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}

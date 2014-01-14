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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Extensions
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Management.Compute;
    using Management.Compute.Models;
    using Utilities.Common;

    /// <summary>
    /// Get Windows Azure Service Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureServiceExtension"), OutputType(typeof(ExtensionImageContext))]
    public class GetAzureServiceExtensionCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, HelpMessage = "The Extension Image Type.")]
        [ValidateNotNullOrEmpty]
        public string ExtensionImageType
        {
            get;
            set;
        }

        public void ExecuteCommand()
        {
            ServiceManagementProfile.Initialize(this);

            Func<HostedServiceListAvailableExtensionsResponse.ExtensionImage, bool> pred =
                string.IsNullOrEmpty(this.ExtensionImageType) ? (Func<HostedServiceListAvailableExtensionsResponse.ExtensionImage, bool>)(s => true)
                                                              : s => string.Equals(this.ExtensionImageType, s.Type, StringComparison.OrdinalIgnoreCase);

            ExecuteClientActionNewSM(null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.HostedServices.ListAvailableExtensions(),
                (op, response) => response.Where(pred).Select(
                     extension => ContextFactory<HostedServiceListAvailableExtensionsResponse.ExtensionImage, ExtensionImageContext>(extension, op)));
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

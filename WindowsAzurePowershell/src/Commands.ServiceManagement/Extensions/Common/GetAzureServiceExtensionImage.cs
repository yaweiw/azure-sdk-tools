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
    [Cmdlet(VerbsCommon.Get, AzureServiceExtensionImageCommandNoun), OutputType(typeof(ExtensionImageContext))]
    public class GetAzureServiceExtensionImageCommand : ServiceManagementBaseCmdlet
    {
        protected const string AzureServiceExtensionImageCommandNoun = "AzureServiceExtensionImage";

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, HelpMessage = "The Extension Image Type.")]
        [ValidateNotNullOrEmpty]
        public string ExtensionImageType
        {
            get;
            set;
        }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, HelpMessage = "The Extension Provider Namespace.")]
        [ValidateNotNullOrEmpty]
        public string ProviderNameSpace
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

            var truePred = (Func<HostedServiceListAvailableExtensionsResponse.ExtensionImage, bool>)(s => true);

            Func<string, Func<HostedServiceListAvailableExtensionsResponse.ExtensionImage, string>,
                 Func<HostedServiceListAvailableExtensionsResponse.ExtensionImage, bool>> predFunc =
                 (x, f) => string.IsNullOrEmpty(x) ? truePred : s => string.Equals(x, f(s), StringComparison.OrdinalIgnoreCase);

            var typePred = predFunc(this.ExtensionImageType, s => s.Type);
            var nameSpacePred = predFunc(this.ProviderNameSpace, s => s.ProviderNamespace);
            var versionPred = predFunc(this.Version, s => s.Version);

            ExecuteClientActionNewSM(null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.HostedServices.ListAvailableExtensions(),
                (op, response) => response.Where(typePred).Where(nameSpacePred).Where(versionPred).Select(
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

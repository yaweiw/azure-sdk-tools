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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.PlatformImageRepository.ExtensionPublishing
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using IaaS.Extensions;
    using Management.Compute.Models;
    using Utilities.Common;

    /// <summary>
    /// Get Windows Azure VM Platform Extension Image.
    /// </summary>
    [Cmdlet(
        VerbsCommon.Get,
        AzureVMPlatformExtensionCommandNoun,
        DefaultParameterSetName = ListLatestExtensionsParamSetName),
    OutputType(
        typeof(VirtualMachineExtensionImageContext))]
    public class GetAzureVMPlatformExtensionCommand : ServiceManagementBaseCmdlet
    {
        protected const string AzureVMPlatformExtensionCommandNoun = "AzureVMPlatformExtension";
        protected const string ListLatestExtensionsParamSetName = "ListLatestExtensions";
        protected const string ListSingleVersionParamSetName = "ListSingleVersion";

        [Parameter(
            ParameterSetName = ListLatestExtensionsParamSetName,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Image Name.")]
        [Parameter(
            ParameterSetName = ListSingleVersionParamSetName,
            Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Image Name.")]
        [ValidateNotNullOrEmpty]
        public string ExtensionName { get; set; }

        [Parameter(
            ParameterSetName = ListLatestExtensionsParamSetName,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Publisher.")]
        [Parameter(
            ParameterSetName = ListSingleVersionParamSetName,
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Publisher.")]
        [ValidateNotNullOrEmpty]
        public string Publisher { get; set; }

        [Parameter(
            ParameterSetName = ListSingleVersionParamSetName,
            Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Version.")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            Func<ExtensionImageListResponse.ResourceExtension, bool> truePred = s => true;

            Func<string, Func<ExtensionImageListResponse.ResourceExtension, string>,
                 Func<ExtensionImageListResponse.ResourceExtension, bool>> predFunc =
                 (x, f) => string.IsNullOrEmpty(x) ? truePred : s => string.Equals(x, f(s), StringComparison.OrdinalIgnoreCase);

            var typePred = predFunc(this.ExtensionName, s => s.Name);
            var publisherPred = predFunc(this.Publisher, s => s.Publisher);
            var versionPred = predFunc(this.Version, s => s.Version);

            ExecuteClientActionNewSM(null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.ExtensionImages.List(),
                (op, response) => response.Where(typePred).Where(publisherPred).Where(versionPred).Select(
                     extension => ContextFactory<ExtensionImageListResponse.ResourceExtension, VirtualMachineExtensionImageContext>(extension, op)));
        }
    }
}

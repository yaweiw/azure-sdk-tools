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
        VerbsData.Publish,
        AzureVMPlatformExtensionCommandNoun),
    OutputType(
        typeof(ManagementOperationContext))]
    public class PublishAzurePlatformExtensionCommand : ServiceManagementBaseCmdlet
    {
        protected const string AzureVMPlatformExtensionCommandNoun = "AzurePlatformExtension";

        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Image Name.")]
        [ValidateNotNullOrEmpty]
        public string ExtensionName { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Publisher.")]
        [ValidateNotNullOrEmpty]
        public string Publisher { get; set; }

        [Parameter(
            Mandatory = true,
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Version.")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; }
        
        [Parameter(
            Mandatory = true,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Hosting Resources.")]
        [ValidateNotNullOrEmpty]
        public string HostingResources { get; set; }
        
        [Parameter(
            Mandatory = true,
            Position = 4,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Description.")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }
        
        [Parameter(
            Mandatory = true,
            Position = 5,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Media Link.")]
        [ValidateNotNullOrEmpty]
        public Uri MediaLink { get; set; }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            ExecuteClientActionNewSM(null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.ExtensionImages.Register(
                    new ExtensionImageRegisterParameters
                    {
                        ProviderNameSpace = this.Publisher,
                        Type = this.ExtensionName,
                        Version = this.Version,
                        HostingResources = this.HostingResources,
                        Description = this.Description,
                        MediaLink = this.MediaLink,
                        IsInternalExtension = true
                    }));
        }
    }
}

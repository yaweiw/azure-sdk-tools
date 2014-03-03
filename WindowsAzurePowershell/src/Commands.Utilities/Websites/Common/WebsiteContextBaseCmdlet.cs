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

namespace Microsoft.WindowsAzure.Commands.Utilities.Websites.Common
{
    using System;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Services;

    public abstract class WebsiteContextBaseCmdlet : WebsiteBaseCmdlet
    {
        protected bool websiteNameDiscovery;

        public WebsiteContextBaseCmdlet()
        {
            websiteNameDiscovery = true;
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site slot name.")]
        [ValidateNotNullOrEmpty]
        public string Slot { get; set; }

        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public override void ExecuteCmdlet()
        {
            try
            {
                if (string.IsNullOrEmpty(Name) && websiteNameDiscovery)
                {
                    // If the website name was not specified as a parameter try to infer it
                    Name = GitWebsite.ReadConfiguration().Name;
                }
                Slot = string.IsNullOrEmpty(Slot) ? WebsitesClient.GetSlotName(Name) : Slot;
            }
            catch (Exception ex)
            {
                WriteExceptionError(ex);
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            Slot = null;
        }
    }
}

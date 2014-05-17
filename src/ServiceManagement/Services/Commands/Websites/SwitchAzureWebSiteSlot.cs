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

namespace Microsoft.WindowsAzure.Commands.Websites
{
    using Microsoft.WindowsAzure.Commands.Utilities.Websites;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Utilities.Properties;
    using Utilities.Websites.Common;
    using Utilities.Websites.Services;
    using Utilities.Websites.Services.WebEntities;

    /// <summary>
    /// Switches the existing slot with the production one.
    /// </summary>
    [Cmdlet(VerbsCommon.Switch, "AzureWebsiteSlot", SupportsShouldProcess = true)]
    public class SwitchAzureWebsiteSlotCommand : WebsiteBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not confirm web site swap")]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            if (string.IsNullOrEmpty(Name))
            {
                // If the website name was not specified as a parameter try to infer it
                Name = GitWebsite.ReadConfiguration().Name;
            }

            Name = WebsitesClient.GetWebsiteNameFromFullName(Name);
            List<Site> sites = WebsitesClient.GetWebsiteSlots(Name);
            string slotName = null;
            string webspace = null;

            if (sites.Count != 2)
            {
                throw new PSInvalidOperationException("The website must have exactly two slots to apply swap");
            }
            else
            {
                foreach (Site website in sites)
                {
                    string currentSlotName = WebsitesClient.GetSlotName(website.Name) ?? WebsiteSlotName.Production.ToString();
                    if (!currentSlotName.Equals(WebsiteSlotName.Production.ToString(), System.StringComparison.OrdinalIgnoreCase))
                    {
                        slotName = currentSlotName;
                        webspace = website.WebSpace;
                        break;
                    }
                }
            }

            ConfirmAction(
                Force.IsPresent,
                string.Format(Resources.SwapWebsiteSlotWarning, Name, slotName),
                Resources.SwappingWebsite,
                Name,
                () =>
                {
                    WebsitesClient.SwitchSlot(webspace, Name, slotName);
                }); 
        }
    }
}

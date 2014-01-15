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
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.Websites;
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
        public string Name
        {
            get;
            set;
        }

        public override void ExecuteCmdlet()
        {
            if (string.IsNullOrEmpty(Name))
            {
                // If the website name was not specified as a parameter try to infer it
                Name = GitWebsite.ReadConfiguration().Name;
            }

            base.ProcessRecord();

            Site websiteObject = WebsitesClient.GetWebsite(Name);
            WebsitesClient.SwitchSlot(websiteObject.WebSpace, Name, WebsiteSlotName.Staging.ToString());
        }
    }
}

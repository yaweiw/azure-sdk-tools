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
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Commands.Utilities.Websites.Common;
    using Commands.Utilities.Websites.Services;
    using Commands.Utilities.Websites.Services.WebEntities;

    /// <summary>
    /// Shows an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Show, "AzureWebsite")]
    public class ShowAzureWebsiteCommand : WebsiteContextBaseCmdlet
    {
        /// <summary>
        /// Initializes a new instance of the ShowAzureWebsiteCommand class.
        /// </summary>
        public ShowAzureWebsiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ShowAzureWebsiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public ShowAzureWebsiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        public override void ExecuteCmdlet()
        {
            InvokeInOperationContext(() =>
            {
                // Show website
                Site websiteObject = RetryCall(s => Channel.GetSiteWithCache(s, Name, null));
                if (websiteObject == null)
                {
                    throw new Exception(string.Format(Resources.InvalidWebsite, Name));
                }

                // Show website in the portal
                General.LaunchWebPage("http://" + websiteObject.HostNames.First());
            });
        }
    }
}
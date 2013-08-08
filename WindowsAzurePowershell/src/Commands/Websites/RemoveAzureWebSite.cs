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
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Commands.Utilities.Websites.Common;
    using Commands.Utilities.Websites.Services;
    using Commands.Utilities.Websites.Services.WebEntities;

    /// <summary>
    /// Removes an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureWebsite", SupportsShouldProcess = true), OutputType(typeof(Site))]
    public class RemoveAzureWebsiteCommand : WebsiteContextBaseCmdlet
    {
        [Parameter(HelpMessage = "Do not confirm web site deletion")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the RemoveAzureWebsiteCommand class.
        /// </summary>
        public RemoveAzureWebsiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RemoveAzureWebsiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public RemoveAzureWebsiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        protected virtual void WriteWebsite(Site website)
        {
            WriteObject(website, true);
        }

        public override void ExecuteCmdlet()
        {
            ConfirmAction(
                Force.IsPresent,
                string.Format(Resources.RemoveWebsiteWarning, Name),
                Resources.RemoveWebsiteMessage,
                Name,
                () =>
                {
                    InvokeInOperationContext(() =>
                    {
                        // Find out in which webspace is the website
                        Site websiteObject = RetryCall(s => Channel.GetSiteWithCache(s, Name, null));
                        if (websiteObject == null)
                        {
                            throw new Exception(string.Format(Resources.InvalidWebsite, Name));
                        }

                        RetryCall(s => Channel.DeleteSite(s, websiteObject.WebSpace, websiteObject.Name, string.Empty));
                        WaitForOperation(CommandRuntime.ToString());

                        Cache.RemoveSite(CurrentSubscription.SubscriptionId, websiteObject);
                    });
                });
        }
    }
}

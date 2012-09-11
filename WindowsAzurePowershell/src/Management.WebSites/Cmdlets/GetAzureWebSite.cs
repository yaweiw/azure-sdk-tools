// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.Websites.Cmdlets
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Common;
    using Properties;
    using Services;
    using Services.WebEntities;

    /// <summary>
    /// Gets an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebsite"), OutputType(typeof(Site))]
    public class GetAzureWebsiteCommand : WebsitesCmdletBase
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureWebsiteCommand class.
        /// </summary>
        public GetAzureWebsiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureWebsiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public GetAzureWebsiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        protected virtual void WriteWebsites(IEnumerable<Site> websites)
        {
            WriteObject(websites, true);
        }

        internal override void ExecuteCommand()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                // Show website
                Site websiteObject = RetryCall(s => Channel.GetSite(s, Name, "repositoryuri,publishingpassword,publishingusername"));
                if (websiteObject == null)
                {
                    throw new Exception(string.Format(Resources.InvalidWebsite, Name));
                }

                // if a name is passed, do the same as show-azurewebsite
                SiteConfig websiteConfiguration = null;
                InvokeInOperationContext(() => { websiteConfiguration = RetryCall(s => Channel.GetSiteConfig(s, websiteObject.WebSpace, websiteObject.Name)); });

                // Output results
                WriteObject(websiteObject, false);
                WriteObject(websiteConfiguration, false);
            }
            else
            {
                WebSpaces webspaces = null;
                InvokeInOperationContext(() => { webspaces = RetryCall(s => Channel.GetWebSpaces(s)); });

                WaitForOperation(CommandRuntime.ToString());

                List<Site> websites = new List<Site>();
                foreach (var webspace in webspaces)
                {
                    websites.AddRange(RetryCall(s => Channel.GetSites(s, webspace.Name, "repositoryuri,publishingpassword,publishingusername")));
                    WaitForOperation(CommandRuntime.ToString());
                }

                Cache.SaveSites(CurrentSubscription.SubscriptionId, new Sites(websites));
                WriteWebsites(websites);
            }
        }
    }
}

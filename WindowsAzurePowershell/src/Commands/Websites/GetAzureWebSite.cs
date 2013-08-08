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
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Commands.Utilities.Websites;
    using Commands.Utilities.Websites.Common;
    using Commands.Utilities.Websites.Services;
    using Commands.Utilities.Websites.Services.DeploymentEntities;
    using Commands.Utilities.Websites.Services.WebEntities;

    /// <summary>
    /// Gets an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebsite"), OutputType(typeof(SiteWithConfig), typeof(IEnumerable<Site>))]
    public class GetAzureWebsiteCommand : WebsitesBaseCmdlet
    {
        public IWebsitesClient WebsitesClient { get; set; }

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

        public override void ExecuteCmdlet()
        {
            if (CurrentSubscription == null)
            {
                throw new Exception(Resources.NoDefaultSubscriptionMessage);
            }

            if (!string.IsNullOrEmpty(Name))
            {
                // Show website
                Site websiteObject = RetryCall(s => Channel.GetSiteWithCache(s, Name, "repositoryuri,publishingpassword,publishingusername"));
                if (websiteObject == null)
                {
                    throw new Exception(string.Format(Resources.InvalidWebsite, Name));
                }

                SiteConfig websiteConfiguration = null;
                InvokeInOperationContext(() =>
                {
                    websiteConfiguration = RetryCall(s => Channel.GetSiteConfig(s, websiteObject.WebSpace, websiteObject.Name));
                    WaitForOperation(CommandRuntime.ToString());
                });

                // Add to cache
                Cache.AddSite(CurrentSubscription.SubscriptionId, websiteObject);

                DiagnosticsSettings diagnosticsSettings = new DiagnosticsSettings();
                if (websiteObject.State == "Running")
                {
                    WebsitesClient = WebsitesClient ?? new WebsitesClient(CurrentSubscription, WriteDebug);

                    try
                    {
                        diagnosticsSettings = WebsitesClient.GetApplicationDiagnosticsSettings(Name);
                    }
                    catch
                    {
                        // Ignore the exception and use default values.
                    }
                }

                // Output results
                WriteObject(new SiteWithConfig(websiteObject, websiteConfiguration, diagnosticsSettings), false);
            }
            else
            {
                // Show websites
                WebSpaces webspaces = null;
                InvokeInOperationContext(() =>
                {
                    webspaces = RetryCall(s => Channel.GetWebSpaces(s));
                    WaitForOperation(CommandRuntime.ToString());
                });

                List<Site> websites = new List<Site>();
                foreach (var webspace in webspaces)
                {
                    InvokeInOperationContext(() =>
                    {
                        websites.AddRange(RetryCall(s => Channel.GetSites(s, webspace.Name, "repositoryuri,publishingpassword,publishingusername")));
                        WaitForOperation(CommandRuntime.ToString());
                    });
                }

                // Add to cache
                Cache.SaveSites(CurrentSubscription.SubscriptionId, new Sites(websites));

                // Output results
                WriteWebsites(websites);
            }
        }
    }
}

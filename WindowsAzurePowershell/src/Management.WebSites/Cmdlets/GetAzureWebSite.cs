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
    using System.Management.Automation;
    using Common;
    using Properties;
    using Services;

    /// <summary>
    /// Gets an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebsite")]
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

        protected virtual void WriteWebsite(Website website)
        {
            WriteObject(website, true);
        }

        internal override bool ExecuteCommand()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                // if a name is passed, do the same as show-azurewebsite
                InvokeInOperationContext(() =>
                {
                    // Show website
                    Website websiteObject = RetryCall(s => Channel.GetWebsite(s, Name));
                    if (websiteObject == null)
                    {
                        throw new Exception(string.Format(Resources.InvalidWebsite, Name));
                    }

                    // Show configuration
                    WebsiteConfig websiteConfiguration = RetryCall(s => Channel.GetWebsiteConfiguration(s, websiteObject.WebSpace, websiteObject.Name));

                    // Output results
                    websiteConfiguration.Merge(websiteObject);
                    WriteObject(websiteConfiguration, true);
                });
            }
            else
            {
                InvokeInOperationContext(() =>
                {
                    WebspaceList webspaces = RetryCall(s => Channel.GetWebspaces(s));
                    WaitForOperation(CommandRuntime.ToString());

                    foreach (var webspace in webspaces)
                    {
                        WebsiteList currentWebsites = RetryCall(s => Channel.GetWebsites(s, webspace.Name,
                            new[] { "repositoryuri", "publishingpassword", "publishingusername" }));

                        WaitForOperation(CommandRuntime.ToString());

                        foreach (var website in currentWebsites)
                        {
                            WriteWebsite(website);
                        }
                    }
                });
            }

            return true;
        }
    }
}

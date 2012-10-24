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
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.Management.WebSites.Cmdlets.Common;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Websites.Services;
    using Microsoft.WindowsAzure.Management.Websites.Properties;
    using Microsoft.WindowsAzure.Management.Websites.Services.WebEntities;
    using Microsoft.WindowsAzure.Management.Utilities;
    using System.ServiceModel;

    
    [Cmdlet(VerbsLifecycle.Restart, "AzureWebsite")]
    public class RestartAzureWebsiteCommand : WebsiteContextBaseCmdlet
    {
        /// <summary>
        /// Initializes a new instance of the RestartAzureWebsiteCommand class.
        /// </summary>
        public RestartAzureWebsiteCommand()
            : this(null)
        {
        }

        public RestartAzureWebsiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        internal override void ExecuteCommand()
        {
            Site website = GetWebSite();
            Site siteUpdate = new Site
            {
                Name = Name,
                HostNames = new [] { Name + General.AzureWebsiteHostNameSuffix }
            };

            InvokeInContext(() =>
            {
                siteUpdate.State = "Stopped";
                RetryCall(s => Channel.UpdateSite(s, website.WebSpace, Name, siteUpdate));

                siteUpdate.State = "Running";
                RetryCall(s => Channel.UpdateSite(s, website.WebSpace, Name, siteUpdate));
            });
        }

        private Site GetWebSite()
        {
            Site website = null;

            InvokeInContext(() =>
            {
                website = RetryCall(s => Channel.GetSite(s, Name, null));
            });

            if (website == null)
            {
                throw new Exception(string.Format(Resources.InvalidWebsite, Name));
            }

            return website;
        }

        private void InvokeInContext(Action action)
        {
            InvokeInOperationContext(() =>
            {
                try
                {
                    action();
                }
                catch (CommunicationException ex)
                {
                    WriteErrorDetails(ex);
                    throw;
                }
            });
        }
    }
}

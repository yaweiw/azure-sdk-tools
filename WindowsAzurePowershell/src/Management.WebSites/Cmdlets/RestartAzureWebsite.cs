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

namespace Microsoft.WindowsAzure.Management.Websites.Cmdlets
{
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

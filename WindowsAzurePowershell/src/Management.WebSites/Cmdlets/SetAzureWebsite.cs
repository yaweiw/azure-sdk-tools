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
    using System.ServiceModel;
    using Properties;
    using Services;
    using Services.WebEntities;
    using WebSites.Cmdlets.Common;
    using System.Management.Automation;
    
    /// <summary>
    /// Sets an azure website properties.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureWebsite")]
    public class SetAzureWebsiteCommand : WebsiteContextBaseCmdlet, ISiteConfig
    {
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Number of workers.")]
        [ValidateNotNullOrEmpty]
        public int? NumberOfWorkers { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Default Documents.")]
        [ValidateNotNullOrEmpty]
        public string[] DefaultDocuments { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = ".NET framework version.")]
        [ValidateNotNullOrEmpty]
        public string NetFrameworkVersion { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "PHP Version.")]
        [ValidateNotNullOrEmpty]
        public string PhpVersion { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Request tracing enabled.")]
        [ValidateNotNullOrEmpty]
        public bool? RequestTracingEnabled { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "HTTP Logging enabled.")]
        [ValidateNotNullOrEmpty]
        public bool? HttpLoggingEnabled { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Detailed Error Logging enabled.")]
        [ValidateNotNullOrEmpty]
        public bool? DetailedErrorLoggingEnabled { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Hostnames.")]
        [ValidateNotNullOrEmpty]
        public string[] HostNames { get; set; }

        /// <summary>
        /// Initializes a new instance of the SetAzureWebsiteCommand class.
        /// </summary>
        public SetAzureWebsiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SetAzureWebsiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public SetAzureWebsiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        internal override void ExecuteCommand()
        {
            Site website = null;
            SiteConfig websiteConfig = null;
            InvokeInOperationContext(() =>
            {
                try
                {
                    website = RetryCall(s => Channel.GetSite(s, Name, null));
                    websiteConfig = RetryCall(s => Channel.GetSiteConfig(s, website.WebSpace, Name));
                }
                catch (CommunicationException ex)
                {
                    WriteErrorDetails(ex);
                }
            });

            if (website == null)
            {
                throw new Exception(string.Format(Resources.InvalidWebsite, Name));
            }

            SiteConfig websiteConfigUpdate = new SiteConfig();
            
            bool changes = false;
            if (NumberOfWorkers != null && !NumberOfWorkers.Equals(websiteConfig.NumberOfWorkers))
            {
                changes = true;
                websiteConfigUpdate.NumberOfWorkers = NumberOfWorkers;
            }

            if (DefaultDocuments != null && !DefaultDocuments.Equals(websiteConfig.DefaultDocuments))
            {
                changes = true;
                websiteConfigUpdate.DefaultDocuments = DefaultDocuments;
            }
       
            if (NetFrameworkVersion != null && !NetFrameworkVersion.Equals(websiteConfig.NetFrameworkVersion))
            {
                changes = true;
                websiteConfigUpdate.NetFrameworkVersion = NetFrameworkVersion;
            }

            if (PhpVersion != null && !PhpVersion.Equals(websiteConfig.PhpVersion))
            {
                changes = true;
                websiteConfigUpdate.PhpVersion = PhpVersion;
            }

            if (RequestTracingEnabled != null && !RequestTracingEnabled.Equals(websiteConfig.RequestTracingEnabled))
            {
                changes = true;
                websiteConfigUpdate.RequestTracingEnabled = RequestTracingEnabled;
            }

            if (HttpLoggingEnabled != null && !HttpLoggingEnabled.Equals(websiteConfig.HttpLoggingEnabled))
            {
                changes = true;
                websiteConfigUpdate.HttpLoggingEnabled = HttpLoggingEnabled;
            }

            if (DetailedErrorLoggingEnabled != null && !DetailedErrorLoggingEnabled.Equals(websiteConfig.DetailedErrorLoggingEnabled))
            {
                changes = true;
                websiteConfigUpdate.DetailedErrorLoggingEnabled = DetailedErrorLoggingEnabled;
            }

            bool siteChanges = false;
            Site websiteUpdate = new Site
            {
                Name = Name,
                HostNames = new[] { Name + ".azurewebsites.net" }
            };
            if (HostNames != null)
            {
                siteChanges = true;
                List<string> newHostNames = new List<string> { Name + ".azurewebsites.net" };
                newHostNames.AddRange(HostNames);
                websiteUpdate.HostNames = newHostNames.ToArray();
            }

            if (changes)
            {
                InvokeInOperationContext(() =>
                {
                    try
                    {
                        RetryCall(s => Channel.UpdateSiteConfig(s, website.WebSpace, Name, websiteConfigUpdate));
                    }
                    catch (CommunicationException ex)
                    {
                        WriteErrorDetails(ex);
                    }
                });
            }

            if (siteChanges)
            {
                InvokeInOperationContext(() =>
                {
                    try
                    {
                        RetryCall(s => Channel.UpdateSite(s, website.WebSpace, Name, websiteUpdate));
                    }
                    catch (CommunicationException ex)
                    {
                        WriteErrorDetails(ex);
                    }
                });
            }
        }
    }
}
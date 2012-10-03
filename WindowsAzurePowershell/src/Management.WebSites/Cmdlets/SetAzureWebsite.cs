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

using System;
using System.ServiceModel;
using Microsoft.WindowsAzure.Management.Websites.Properties;
using Microsoft.WindowsAzure.Management.Websites.Services;
using Microsoft.WindowsAzure.Management.Websites.Services.WebEntities;

namespace Microsoft.WindowsAzure.Management.Websites.Cmdlets
{
    using WebSites.Cmdlets.Common;
    using System.Management.Automation;
    
    /// <summary>
    /// Sets an azure website properties.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureWebsite")]
    public class SetAzureWebsite : WebsiteContextBaseCmdlet
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

        internal override void ExecuteCommand()
        {
            Site website = null;
            SiteConfig websiteConfig = null;
            InvokeInOperationContext(() =>
            {
                try
                {
                    website = RetryCall(s => Channel.GetSite(s, Name, null));
                    websiteConfig = RetryCall(s => Channel.GetSiteConfig(s, Name, null));
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

            SiteConfig websiteUpdate = new SiteConfig();
            
            bool changes = false;
            if (NumberOfWorkers != null && !NumberOfWorkers.Equals(websiteConfig.NumberOfWorkers))
            {
                changes = true;
                websiteUpdate.NumberOfWorkers = NumberOfWorkers;
            }

            if (DefaultDocuments != null && !DefaultDocuments.Equals(websiteConfig.DefaultDocuments))
            {
                changes = true;
                websiteUpdate.DefaultDocuments = DefaultDocuments;
            }
       
            if (NetFrameworkVersion != null && !NetFrameworkVersion.Equals(websiteConfig.NetFrameworkVersion))
            {
                changes = true;
                websiteUpdate.NetFrameworkVersion = NetFrameworkVersion;
            }

            if (PhpVersion != null && !PhpVersion.Equals(websiteConfig.PhpVersion))
            {
                changes = true;
                websiteUpdate.PhpVersion = PhpVersion;
            }

            if (RequestTracingEnabled != null && !RequestTracingEnabled.Equals(websiteConfig.RequestTracingEnabled))
            {
                changes = true;
                websiteUpdate.RequestTracingEnabled = RequestTracingEnabled;
            }

            if (HttpLoggingEnabled != null && !HttpLoggingEnabled.Equals(websiteConfig.HttpLoggingEnabled))
            {
                changes = true;
                websiteUpdate.HttpLoggingEnabled = HttpLoggingEnabled;
            }

            if (DetailedErrorLoggingEnabled != null && !DetailedErrorLoggingEnabled.Equals(websiteConfig.DetailedErrorLoggingEnabled))
            {
                changes = true;
                websiteUpdate.DetailedErrorLoggingEnabled = DetailedErrorLoggingEnabled;
            }

            if (changes)
            {
                InvokeInOperationContext(() =>
                {
                    try
                    {
                        RetryCall(s => Channel.UpdateSiteConfig(s, website.WebSpace, Name, websiteUpdate));
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
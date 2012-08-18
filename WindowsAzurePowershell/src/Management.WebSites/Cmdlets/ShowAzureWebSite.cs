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

namespace Microsoft.WindowsAzure.Management.WebSites.Cmdlets
{
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using Common;
    using Properties;
    using Services;

    /// <summary>
    /// Shows an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Show, "AzureWebsite")]
    public class ShowAzureWebSiteCommand : WebsitesCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site name.")]
        [ValidateNotNullOrEmpty]
        public string Website
        {
            get;
            set;
        }

        internal bool ShowWebsiteProcess(string website)
        {
            InvokeInOperationContext(() =>
            {
                try
                {
                    // Show website
                    var websiteObject = RetryCall(s => Channel.GetWebsite(s, website));
                    if (websiteObject == null)
                    {
                        throw new Exception(Resources.InvalidWebsite);
                    }

                    WriteObject(websiteObject, false);

                    // Show configuration
                    var websiteConfiguration = RetryCall(s => Channel.GetWebsiteConfiguration(s, websiteObject.WebSpace, websiteObject.Name));
                    WriteObject(websiteConfiguration, false);
                }
                catch (CommunicationException ex)
                {
                    WriteErrorDetails(ex);
                }
            });

            return true;
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ShowWebsiteProcess(Website);
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}

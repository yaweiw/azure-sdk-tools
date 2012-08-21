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
    /// Removes an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureWebsite")]
    public class RemoveAzureWebSiteCommand : WebsitesCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Do not confirm web site deletion")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the RemoveAzureWebSiteCommand class.
        /// </summary>
        public RemoveAzureWebSiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RemoveAzureWebSiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public RemoveAzureWebSiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        protected virtual void WriteWebsite(Website website)
        {
            WriteObject(website, true);
        }

        internal bool RemoveWebsiteProcess(string website)
        {
            if (!Force.IsPresent &&
                !ShouldProcess("", string.Format(Resources.RemoveWebsiteWarning, website),
                                Resources.ShouldProcessCaption))
            {
                return false;
            }

            InvokeInOperationContext(() =>
            {
                try
                {
                    // Find out in which webspace is the website
                    var websiteObject = RetryCall(s => Channel.GetWebsite(s, website));
                    if (websiteObject == null)
                    {
                        throw new Exception(Resources.InvalidWebsite);
                    }

                    RetryCall(s => Channel.DeleteWebsite(s, websiteObject.WebSpace, websiteObject.Name));
                    WaitForOperation(CommandRuntime.ToString());
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

                if (RemoveWebsiteProcess(Name))
                {
                    SafeWriteObjectWithTimestamp(Resources.CompleteMessage);
                }
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}

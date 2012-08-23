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
    using System.Management.Automation;
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using Common;
    using Properties;
    using Services;

    /// <summary>
    /// Starts an azure website.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "AzureWebSite")]
    public class StartAzureWebSiteCommand : WebsitesCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the StartAzureWebSiteCommand class.
        /// </summary>
        public StartAzureWebSiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the StartAzureWebSiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public StartAzureWebSiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        internal bool StartWebsiteProcess(string name)
        {
            Website website = null;

            InvokeInOperationContext(() =>
            {
                try
                {
                    website = RetryCall(s => Channel.GetWebsite(s, name));
                }
                catch (CommunicationException ex)
                {
                    WriteErrorDetails(ex);
                }
            });

            if (website == null)
            {
                throw new Exception(Resources.InvalidWebsite);
            }

            InvokeInOperationContext(() =>
            {
                try
                {
                    var websiteUpdate = new Website
                                            {
                                                Name = name,
                                                HostNames = new List<string>(new[] { name + ".azurewebsites.net" }),
                                                State = "Running"
                                            };

                    RetryCall(s => Channel.UpdateWebsite(s, website.WebSpace, name, websiteUpdate));
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

                if (StartWebsiteProcess(Name))
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

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

using System.Linq;

namespace Microsoft.WindowsAzure.Management.WebSites.Cmdlets
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.ServiceModel;
    using Common;
    using Properties;
    using Services;

    /// <summary>
    /// Creates a new azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureWebSite")]
    public class NewAzureWebSiteCommand : WebsitesCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The geographic region to create the website")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Custom host name to use.")]
        [ValidateNotNullOrEmpty]
        public string Hostname
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureWebSiteCommand class.
        /// </summary>
        public NewAzureWebSiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureWebSiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public NewAzureWebSiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        internal bool NewWebsiteProcess(string location, string name, string hostname)
        {
            InvokeInOperationContext(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(location))
                    {
                        // If no location was provided as a parameter, try to default it
                        var webspaces = RetryCall(s => Channel.GetWebspaces(s));
                        if (webspaces.Count > 0)
                        {
                            location = webspaces.First().Name;
                        } 
                    }

                    if (string.IsNullOrEmpty(location))
                    {
                        // If location is still empty or null, give portal instructions.
                        SafeWriteObjectWithTimestamp(string.Format(Resources.PortalInstructions, name));
                        return;
                    }

                    // New website
                    CreateWebsite website = new CreateWebsite
                                          {
                                              Name = name,
                                              HostNames = new List<string>(new [] { name + ".azurewebsites.net" })
                                          };

                    if (!string.IsNullOrEmpty(hostname))
                    {
                        website.HostNames.Add(hostname);
                    }

                    RetryCall(s => Channel.NewWebsite(s, location, website));
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

                if (NewWebsiteProcess(Location, Name, Hostname))
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

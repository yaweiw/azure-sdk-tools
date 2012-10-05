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
    using System.Security.Permissions;
    using Management.Utilities;
    using Properties;

    /// <summary>
    /// Opens the azure portal.
    /// </summary>
    [Cmdlet(VerbsCommon.Show, "AzurePortal")]
    public class ShowAzurePortalCommand : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the website.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Realm of the account.")]
        [ValidateNotNullOrEmpty]
        public string Realm { get; set; }

        [EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        internal void ProcessShowAzurePortal()
        {
            Validate.ValidateInternetConnection();

            UriBuilder uriBuilder = new UriBuilder(PortalUrl);
            if (!string.IsNullOrEmpty(Name))
            {
                uriBuilder.Fragment += string.Format(Resources.WebsiteSufixUrl, Name);
            }

            if (Realm != null)
            {
                string queryToAppend = string.Format("whr={0}", Realm);
                if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
                {
                    uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + queryToAppend;
                }
                else
                {
                    uriBuilder.Query = queryToAppend;
                }
            }

            General.LaunchWebPage(uriBuilder.ToString());
        }

        protected string PortalUrl
        {
            get { return Resources.AzurePortalUrl; }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                ProcessShowAzurePortal();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
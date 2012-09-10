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

        [EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        internal void ProcessShowAzurePortal(string url, string webSiteName)
        {
            Validate.ValidateStringIsNullOrEmpty(url, "Azure portal url");
            Validate.ValidateInternetConnection();

            if (!string.IsNullOrEmpty(webSiteName))
            {
                url += string.Format(Resources.WebsiteSufixUrl, webSiteName);
            }

            General.LaunchWebPage(url);
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ProcessShowAzurePortal(Resources.AzurePortalUrl, Name);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
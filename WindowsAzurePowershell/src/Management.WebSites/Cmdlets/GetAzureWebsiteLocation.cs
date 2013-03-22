// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
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
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.WebEntities;

    /// <summary>
    /// Gets an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebsiteLocation"), OutputType(typeof(ICollection<string>))]
    public class GetAzureWebsiteLocationCommand : WebsitesBaseCmdlet
    {
        /// <summary>
        /// Initializes a new instance of the GetAzureWebsiteLocationCommand class.
        /// </summary>
        public GetAzureWebsiteLocationCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureWebsiteLocationCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public GetAzureWebsiteLocationCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        public override void ExecuteCmdlet()
        {
            // For now geo regions will be hardcoded since the Get GeoRegions endpoint is still not exposed.
            WriteObject(AvailableWebspaces.Webspaces.Keys, true);
        }
    }
}

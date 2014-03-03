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

namespace Microsoft.WindowsAzure.Commands.Websites
{
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.Websites;
    using Utilities.Properties;
    using Utilities.Websites.Common;
    using Utilities.Websites.Services;
    using Utilities.Websites.Services.WebEntities;
    using System.Linq;
    using System;

    /// <summary>
    /// Updates a website git remote config to include slots
    /// </summary>
    [Cmdlet(VerbsData.Update, "AzureWebsiteRepository", SupportsShouldProcess = true)]
    public class UpdateAzureWebsiteRepositoryCommand : WebsiteBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The web site name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The publishing user name.")]
        [ValidateNotNullOrEmpty]
        public string PublishingUsername { get; set; }

        public override void ExecuteCmdlet()
        {
            if (string.IsNullOrEmpty(Name))
            {
                // If the website name was not specified as a parameter try to infer it
                Name = GitWebsite.ReadConfiguration().Name;
            }

            List<Site> sites = WebsitesClient.GetWebsiteSlots(Name);
            IList<string> remoteRepositories = Git.GetRemoteRepositories();

            // Clear all existing remotes that are created by us
            foreach (string remoteName in remoteRepositories)
            {
                if (remoteName.StartsWith("azure"))
                {
                    Git.RemoveRemoteRepository(remoteName);
                }
            }

            foreach (Site website in sites)
            {
                string repositoryUri = website.GetProperty("RepositoryUri");
                string publishingUsername = PublishingUsername;
                string uri = Git.GetUri(repositoryUri, website.RepositorySiteName, publishingUsername);
                string slot = WebsitesClient.GetSlotName(website.Name);
                string remoteName = string.Empty;

                if (!string.IsNullOrEmpty(slot) && !slot.Equals(WebsiteSlotName.Production.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    remoteName = "-" + slot;
                }

                Git.AddRemoteRepository(string.Format("azure{0}", remoteName), uri);
            }
        }
    }
}

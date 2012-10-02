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
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Text.RegularExpressions;
    using Properties;
    using Services;
    using Services.WebEntities;
    using WebSites.Cmdlets.Common;

    /// <summary>
    /// Creates a new azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureWebsite")]
    public class NewAzureWebsiteCommand : WebsiteContextBaseCmdlet
    {
        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The geographic region to create the website.")]
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

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The publishing user name.")]
        [ValidateNotNullOrEmpty]
        public string PublishingUsername
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Configure git on web site and local folder.")]
        public SwitchParameter Git
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureWebsiteCommand class.
        /// </summary>
        public NewAzureWebsiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureWebsiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public NewAzureWebsiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        internal bool IsGitWorkingTree()
        {
            return Services.Git.GetWorkingTree().Any(line => line.Equals(".git"));
        }

        internal void InitGitOnCurrentDirectory()
        {
            Services.Git.InitRepository();

            if (!File.Exists(".gitignore"))
            {
                // Scaffold gitignore
                string cmdletPath = Directory.GetParent(MyInvocation.MyCommand.Module.Path).FullName;
                File.Copy(Path.Combine(cmdletPath, "Resources/Scaffolding/Node/.gitignore"), ".gitignore");
            }
        }

        internal void CopyIisNodeWhenServerJsPresent()
        {
            if (!File.Exists("iisnode.yml") && (File.Exists("server.js") || File.Exists("app.js")))
            {
                string cmdletPath = Directory.GetParent(MyInvocation.MyCommand.Module.Path).FullName;
                File.Copy(Path.Combine(cmdletPath, "Resources/Scaffolding/Node/iisnode.yml"), "iisnode.yml");
            }
        }

        internal void UpdateLocalConfigWithSiteName(string websiteName, string webspace)
        {
            GitWebsite gitWebsite = new GitWebsite(websiteName, webspace);
            gitWebsite.WriteConfiguration();
        }

        internal string GetPublishingUser()
        {
            if (!string.IsNullOrEmpty(PublishingUsername))
            {
                return PublishingUsername;
            }

            // Get publishing users
            IList<string> users = null;
            // InvokeInOperationContext(() => { users = RetryCall(s => Channel.GetSubscriptionPublishingUsers(s)); });
            users = new List<string> { "andrerod" };

            IEnumerable<string> validUsers = users.Where(user => !string.IsNullOrEmpty(user)).ToList();
            if (!validUsers.Any())
            {
                throw new Exception(Resources.InvalidGitCredentials);
            } 
            
            if (!(validUsers.Count() == 1 && users.Count() == 1))
            {
                throw new Exception(Resources.MultiplePublishingUsernames);
            }

            return users.First();
        }

        internal string GetRepositoryUri(Site website)
        {
            if (website.SiteProperties.Properties.Any(kvp => kvp.Name.Equals("RepositoryUri")))
            {
                return website.SiteProperties.Properties.First(kvp => kvp.Name.Equals("RepositoryUri")).Value;
            }

            return null;
        }

        internal void CreateRepositoryAndAddRemote(string publishingUser, string webspace, string websiteName)
        {
            // Create website repository
            InvokeInOperationContext(() => RetryCall(s => Channel.CreateSiteRepository(s, webspace, websiteName)));

            // Get remote repos
            IList<string> remoteRepositories = Services.Git.GetRemoteRepositories();
            if (remoteRepositories.Any(repository => repository.Equals("azure")))
            {
                // Removing existing azure remote alias
                Services.Git.RemoveRemoteRepository("azure");
            }

            // Get website and from it the repository url
            Site website = RetryCall(s => Channel.GetSite(s, webspace, websiteName, "repositoryuri,publishingpassword,publishingusername"));
            string repositoryUri = GetRepositoryUri(website);

            string uri = Services.Git.GetUri(repositoryUri, Name, publishingUser);
            Services.Git.AddRemoteRepository("azure", uri);
        }

        [EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        internal override void ExecuteCommand()
        {
            string publishingUser = null;
            if (Git)
            {
                publishingUser = GetPublishingUser();
            }

            WebSpaces webspaceList = null;

            InvokeInOperationContext(() => { webspaceList = RetryCall(s => Channel.GetWebSpacesWithCache(s)); });
            if (webspaceList.Count == 0)
            {
                // If location is still empty or null, give portal instructions.
                string error = string.Format(Resources.PortalInstructions, Name);
                throw new Exception(!Git
                    ? error
                    : string.Format("{0}\n{1}", error, Resources.PortalInstructionsGit));
            }

            WebSpace webspace = null;
            if (string.IsNullOrEmpty(Location))
            {
                // If no location was provided as a parameter, try to default it
                webspace = webspaceList.FirstOrDefault();
                if (webspace == null)
                {
                    // Use east us
                    webspace = new WebSpace
                    {
                        Name = "eastuswebspace",
                        GeoRegion = "East US",
                        Subscription = CurrentSubscription.SubscriptionId,
                        Plan = "VirtualDedicatedPlan"
                    };
                }
            }
            else
            {
                // Find the webspace that corresponds to the georegion
                webspace = webspaceList.FirstOrDefault(w => w.GeoRegion.Equals(Location, StringComparison.OrdinalIgnoreCase));
                if (webspace == null)
                {
                    // If no webspace corresponding to the georegion was found, attempt to create it
                    webspace = new WebSpace
                    {
                        Name = Regex.Replace(Location.ToLower(), " ", "") + "webspace",
                        GeoRegion = Location,
                        Subscription = CurrentSubscription.SubscriptionId,
                        Plan = "VirtualDedicatedPlan"
                    };
                }
            }

            Site website = new Site
            {
                Name = Name,
                HostNames = new[] { Name + ".azurewebsites.net" },
                WebSpace = webspace.Name
            };

            if (!string.IsNullOrEmpty(Hostname))
            {
                List<string> newHostNames = new List<string>(website.HostNames);
                newHostNames.Add(Hostname);
                website.HostNames = newHostNames.ToArray();
            }

            try
            {
                InvokeInOperationContext(() => RetryCall(s => Channel.CreateSite(s, webspace.Name, website)));

                // If operation succeeded try to update cache with new webspace if that's the case
                if (webspaceList.FirstOrDefault(ws => ws.Name.Equals(webspace.Name)) == null)
                {
                    Cache.AddWebSpace(CurrentSubscription.SubscriptionId, webspace);
                }

                Cache.AddSite(CurrentSubscription.SubscriptionId, website);
            }
            catch (ProtocolException ex)
            {
                // Handle site creating indepently so that cmdlet is idempotent.
                ProcessException(ex);
            }

            if (Git)
            {
                try
                {
                    Directory.SetCurrentDirectory(SessionState.Path.CurrentFileSystemLocation.Path);
                }
                catch (Exception)
                {
                    // Do nothing if session state is not present
                }

                if (!IsGitWorkingTree())
                {
                    // Init git in current directory
                    InitGitOnCurrentDirectory();
                }

                CopyIisNodeWhenServerJsPresent();
                UpdateLocalConfigWithSiteName(Name, webspace.Name);
                CreateRepositoryAndAddRemote(publishingUser, webspace.Name, Name);
            }
        }
    }
}

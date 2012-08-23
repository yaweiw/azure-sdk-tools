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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
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
                File.WriteAllText(".gitignore", Resources.GitIgnoreFileContent);
            }
        }

        internal void CopyIisNodeWhenServerJsPresent()
        {
            if (!File.Exists("iisnode.yml") && (File.Exists("server.js") || File.Exists("app.js")))
            {
                File.Copy("Resources/Scaffolding/Node/iisnode.yml", "iisnode.yml");
            }
        }

        internal void UpdateLocalConfigWithSiteName(string websiteName, string webspace)
        {
            GitWebSite gitWebSite = new GitWebSite(websiteName, webspace);
            gitWebSite.WriteConfiguration();
        }

        internal string GetPublishingUser()
        {
            if (!string.IsNullOrEmpty(PublishingUsername))
            {
                return PublishingUsername;
            }

            // Get publishing users
            IList<string> users = null;
            InvokeInOperationContext(() => { users = RetryCall(s => Channel.GetPublishingUsers(s)); });

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

        internal string GetRepositoryUri(Website website)
        {
            if (website.SiteProperties.Properties.ContainsKey("RepositoryUri"))
            {
                return website.SiteProperties.Properties["RepositoryUri"];
            }

            return null;
        }

        internal void CreateRepositoryAndAddRemote(string publishingUser, string websiteName, string webspace)
        {
            // Create website repository
            InvokeInOperationContext(() => RetryCall(s => Channel.CreateWebsiteRepository(s, webspace, websiteName)));

            // Get remote repos
            IList<string> remoteRepositories = Services.Git.GetRemoteRepositories();
            if (remoteRepositories.Any(repository => repository.Equals("azure")))
            {
                // Removing existing azure remote alias
                Services.Git.RemoveRemoteRepository("azure");
            }

            // Get website and from it the repository url
            Website website = RetryCall(s => Channel.GetWebsite(s, Name));
            string repositoryUri = GetRepositoryUri(website);

            string uri = Services.Git.GetUri(repositoryUri, Name, publishingUser);
            Services.Git.AddRemoteRepository("azure", uri);
        }

        internal override bool ExecuteCommand()
        {
            string publishingUser = null;
            if (Git)
            {
                publishingUser = GetPublishingUser();
            }

            if (string.IsNullOrEmpty(Location))
            {
                InvokeInOperationContext(() =>
                {
                    // If no location was provided as a parameter, try to default it
                    Location = RetryCall(s => Channel.GetWebspaces(s).Select(webspace => webspace.Name).FirstOrDefault());
                });
            }

            if (string.IsNullOrEmpty(Location))
            {
                // If location is still empty or null, give portal instructions.
                SafeWriteObjectWithTimestamp(string.Format(Resources.PortalInstructions, Name));
                return false;
            }

            InvokeInOperationContext(() =>
            {
                Website website = new Website
                                        {
                                            Name = Name,
                                            HostNames = new List<string> { Name + ".azurewebsites.net" }
                                        };

                if (!string.IsNullOrEmpty(Hostname))
                {
                    website.HostNames.Add(Hostname);
                }

                RetryCall(s => Channel.NewWebsite(s, Location, website));
            });

            if (Git)
            {
                if(!IsGitWorkingTree())
                {
                    // Init git in current directory
                    InitGitOnCurrentDirectory();
                    CopyIisNodeWhenServerJsPresent();
                    UpdateLocalConfigWithSiteName(Name, Location);
                    CreateRepositoryAndAddRemote(publishingUser, Name, Location);
                }   
            }

            return true;
        }
    }
}

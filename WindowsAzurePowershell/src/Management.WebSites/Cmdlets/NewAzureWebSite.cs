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
    using System.Collections.Generic;
    using System.Diagnostics;
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
            bool gitWorkingTree = false;
            var lines = Services.Git.GetWorkingTree();
            gitWorkingTree = lines.Any(line => line.Equals(".git"));

            return gitWorkingTree;
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

        internal void CreateRepositoryAndAddRemote(string websiteName, string webspace)
        {
            // Create website repository
            InvokeInOperationContext(() => RetryCall(s => Channel.CreateWebsiteRepository(s, webspace, websiteName)));

            // Get publishing users
            IList<string> users = null;
            InvokeInOperationContext(() =>
            {
                users = RetryCall(s => Channel.GetPublishingUsers(s));
            });

            // Get remote repos
            IList<string> remoteRepos = Services.Git.GetRemoteRepos();
        }

        internal override bool ExecuteCommand()
        {
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
                    CreateRepositoryAndAddRemote(Name, Location);
                }   
            }

            return true;
        }
    }
}

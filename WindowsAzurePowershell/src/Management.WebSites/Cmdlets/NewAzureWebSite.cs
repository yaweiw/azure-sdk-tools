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
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = "rev-parse --git-dir";
                process.Start();

                // Read the output stream first and then wait.
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split('\n');
                gitWorkingTree = lines.Any(line => line.Equals(".git"));
            }

            return gitWorkingTree;
        }

        internal void InitGitOnCurrentDirectory()
        {
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "git";
                process.StartInfo.Arguments = "init";
                process.Start();
                process.WaitForExit();

                if (!File.Exists(".gitignore"))
                {
                    // Scaffold gitignore
                    File.WriteAllText(".gitignore", Resources.GitIgnoreFileContent);
                }
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
            var gitWebSite = new GitWebSite(websiteName, webspace);
            gitWebSite.WriteConfiguration();
        }

        internal void CreateRepositoryAndAddRemote(string websiteName, string webspace)
        {
            // Create website repository
            InvokeInOperationContext(() => RetryCall(s => Channel.CreateWebsiteRepository(s, webspace, websiteName)));
        }

        internal bool NewWebsiteProcess(string location, string name, string hostname)
        {
            if (string.IsNullOrEmpty(location))
            {
                InvokeInOperationContext(() =>
                {
                    // If no location was provided as a parameter, try to default it
                    var webspaces = RetryCall(s => Channel.GetWebspaces(s));
                    if (webspaces.Count > 0)
                    {
                        location = webspaces.First().Name;
                    } 
                });
            }

            if (string.IsNullOrEmpty(location))
            {
                // If location is still empty or null, give portal instructions.
                SafeWriteObjectWithTimestamp(string.Format(Resources.PortalInstructions, name));
                return false;
            }

            InvokeInOperationContext(() =>
            {
                var website = new Website
                                        {
                                            Name = name,
                                            HostNames = new List<string>(new [] { name + ".azurewebsites.net" })
                                        };

                if (!string.IsNullOrEmpty(hostname))
                {
                    website.HostNames.Add(hostname);
                }

                RetryCall(s => Channel.NewWebsite(s, location, website));
            });

            if (Git)
            {
                if(!IsGitWorkingTree())
                {
                    // Init git in current directory
                    InitGitOnCurrentDirectory();
                    CopyIisNodeWhenServerJsPresent();
                    UpdateLocalConfigWithSiteName(name, location);
                    CreateRepositoryAndAddRemote(name, location);
                }   
            }

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

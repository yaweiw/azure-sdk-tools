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

namespace Microsoft.WindowsAzure.Management.Websites.Services
{
    using Microsoft.WindowsAzure.Management.Websites.Services.Github;
    using Microsoft.WindowsAzure.Management.Websites.Services.Github.Entities;
    using Microsoft.WindowsAzure.Management.Websites.Services.WebEntities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;

    public class GithubClient : LinkedRevisionControl
    {
        protected IGithubServiceManagement GithubChannel { get; set; }
        private GithubRepository LinkedRepository { get; set; }
        private string username;
        private string password;
        private string repositoryFullName;

        public GithubClient(string invocationPath, IGithubServiceManagement channel, string githubUsername, string githubPassword, string githubRepository)
            : base(invocationPath)
        {
            GithubChannel = channel;
            this.username = githubUsername;
            this.password = githubPassword;
            this.repositoryFullName = githubRepository;
        }

        private void Authenticate()
        {
            EnsureCredentials();
        }

        private void EnsureCredentials()
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new Exception("Invalid github credentials");
            }
        }

        private IList<GithubRepository> GetRepositories()
        {
            List<GithubRepository> repositories = null;
            InvokeInGithubOperationContext(() => { repositories = GithubChannel.GetRepositories(); });

            List<GithubOrganization> organizations = null;
            InvokeInGithubOperationContext(() => { organizations = GithubChannel.GetOrganizations(); });

            List<GithubRepository> orgRepositories = new List<GithubRepository>();
            foreach (var organization in organizations)
            {
                // GetRepositoriesFromOrg
                List<GithubRepository> currentOrgRepositories = null;
                InvokeInGithubOperationContext(() => { currentOrgRepositories = GithubChannel.GetRepositoriesFromOrg(organization.Login); });
                orgRepositories.AddRange(currentOrgRepositories);
            }

            repositories.AddRange(orgRepositories);
            return repositories;
        }

        private void CreateOrUpdateHook(string owner, string repository, Site website)
        {
            string baseUri = website.GetProperty("repositoryuri");
            string publishingUsername = website.GetProperty("publishingusername");
            string publishingPassword = website.GetProperty("publishingpassword");
            UriBuilder newUri = new UriBuilder(baseUri);
            newUri.UserName = publishingUsername;
            newUri.Password = publishingPassword;
            newUri.Path = "/deploy";

            string deployUri = newUri.ToString();

            List<GithubRepositoryHook> repositoryHooks = new List<GithubRepositoryHook>();
            InvokeInGithubOperationContext(() => { repositoryHooks = GithubChannel.GetRepositoryHooks(owner, repository); });

            var existingHook = repositoryHooks.FirstOrDefault(h => h.Name.Equals("web") && new Uri(h.Config.Url).Host.Equals(new Uri(deployUri).Host));
            if (existingHook != null)
            {
                if (existingHook.Config.Url.Equals(newUri))
                {
                    existingHook.Config.Url = deployUri;
                    InvokeInGithubOperationContext(() => { GithubChannel.UpdateRepositoryHook(owner, repository, existingHook.Id, existingHook); });
                }
                else
                {
                    throw new Exception("Link already established");
                }
            }
            else
            {
                GithubRepositoryHook githubRepositoryHook = new GithubRepositoryHook()
                {
                    Name = "web",
                    Active = true,
                    Events = new List<string> { "push" },
                    Config = new GithubRepositoryHookConfig
                    {
                        Url = deployUri,
                        InsecureSsl = "1",
                        ContentType = "form"
                    }
                };

                InvokeInGithubOperationContext(() => { githubRepositoryHook = GithubChannel.CreateRepositoryHook(owner, repository, githubRepositoryHook); });
                InvokeInGithubOperationContext(() => { GithubChannel.TestRepositoryHook(owner, repository, githubRepositoryHook.Id); });
            }  
        }

        public override void Init()
        {
            Authenticate();

            if (!IsGitWorkingTree())
            {
                // Init git in current directory
                InitGitOnCurrentDirectory();
            }

            IList<GithubRepository> repositories = GetRepositories();
            if (!string.IsNullOrEmpty(repositoryFullName))
            {
                LinkedRepository = repositories.FirstOrDefault(r => r.FullName.Equals(repositoryFullName));
            }
        }

        public override void Deploy(Site website)
        {
            CreateOrUpdateHook(LinkedRepository.Owner.Login, LinkedRepository.Name, website);
        }

        /// <summary>
        /// Invoke the given operation within an OperationContextScope if the
        /// channel supports it.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        protected void InvokeInGithubOperationContext(Action action)
        {
            IContextChannel contextChannel = GithubChannel as IContextChannel;
            if (contextChannel != null)
            {
                using (new OperationContextScope(contextChannel))
                {
                    action();
                }
            }
            else
            {
                action();
            }
        }
    }
}
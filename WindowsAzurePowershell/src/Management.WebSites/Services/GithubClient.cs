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
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    public class GithubClient : LinkedRevisionControl
    {
        protected IGithubServiceManagement GithubChannel { get; set; }
        private GithubRepository LinkedRepository { get; set; }
        private string username;
        private string password;

        public GithubClient(string invocationPath, IGithubServiceManagement channel, string githubUsername, string githubPassword)
            : base(invocationPath)
        {
            GithubChannel = channel;
            this.username = githubUsername;
            this.password = githubPassword;
        }

        private void Authenticate()
        {
            EnsureCredentials();
            /*
            GithubAuthorization authorization;
            InvokeInGithubOperationContext(() => { authorization = GithubChannel.CreateAuthorizationToken(new GithubAuthorizationRequest() { Scopes = new List<string> { "public_repo" } }); });
        */
        }

        private void EnsureCredentials()
        {

        }

        private IList<GithubRepository> GetRepositories()
        {
            IList<GithubRepository> repositories = null;
            InvokeInGithubOperationContext(() => { repositories = GithubChannel.GetRepositories(); });

            IList<GithubOrganization> organizations = null;
            InvokeInGithubOperationContext(() => { organizations = GithubChannel.GetOrganizations(); });



            return repositories;
        }

        private void CreateOrUpdateHook()
        {
            throw new NotImplementedException();
        }

        private void CreateHook()
        {
            throw new NotImplementedException();
        }

        private void UpdateHook()
        {
            throw new NotImplementedException();
        }

        private void TestHook()
        {
            throw new NotImplementedException();
        }

        private void GetHooks()
        {
            throw new NotImplementedException();
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
        }

        public override void Deploy()
        {
            throw new NotImplementedException();
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
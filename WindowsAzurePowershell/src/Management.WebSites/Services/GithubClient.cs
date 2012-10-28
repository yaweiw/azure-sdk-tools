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
    using Microsoft.WindowsAzure.Management.Websites.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Websites.Services.Github;
    using Microsoft.WindowsAzure.Management.Websites.Services.Github.Entities;
    using Microsoft.WindowsAzure.Management.Websites.Services.WebEntities;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    public static class SecureStringExtensionMethods
    {
        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }

    public class GithubClient : LinkedRevisionControl
    {
        private GithubRepository LinkedRepository { get; set; }
        private string username;
        private string password;
        private string repositoryFullName;
        private IGithubCmdlet pscmdlet;

        public GithubClient(IGithubCmdlet pscmdlet, string githubUsername, string githubPassword, string githubRepository)
        {
            this.pscmdlet = pscmdlet;
            this.invocationPath = ((PSCmdlet)pscmdlet).MyInvocation.MyCommand.Module.Path;
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
            // Ensure credentials
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                PSCredential cred = ((PSCmdlet)pscmdlet).Host.UI.PromptForCredential("Enter username/password",
                                                     "", username, "");
                username = cred.UserName;
                password = cred.Password.ConvertToUnsecureString();
            }

            pscmdlet.GithubChannel = CreateGithubChannel();
        }

        private IList<GithubRepository> GetRepositories()
        {
            List<GithubRepository> repositories = null;
            InvokeInGithubOperationContext(() => { repositories = pscmdlet.GithubChannel.GetRepositories(); });

            List<GithubOrganization> organizations = null;
            InvokeInGithubOperationContext(() => { organizations = pscmdlet.GithubChannel.GetOrganizations(); });

            List<GithubRepository> orgRepositories = new List<GithubRepository>();
            foreach (var organization in organizations)
            {
                // GetRepositoriesFromOrg
                List<GithubRepository> currentOrgRepositories = null;
                InvokeInGithubOperationContext(() => { currentOrgRepositories = pscmdlet.GithubChannel.GetRepositoriesFromOrg(organization.Login); });
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
            InvokeInGithubOperationContext(() => { repositoryHooks = pscmdlet.GithubChannel.GetRepositoryHooks(owner, repository); });

            var existingHook = repositoryHooks.FirstOrDefault(h => h.Name.Equals("web") && new Uri(h.Config.Url).Host.Equals(new Uri(deployUri).Host));
            if (existingHook != null)
            {
                if (existingHook.Config.Url.Equals(newUri))
                {
                    existingHook.Config.Url = deployUri;
                    InvokeInGithubOperationContext(() => { pscmdlet.GithubChannel.UpdateRepositoryHook(owner, repository, existingHook.Id, existingHook); });
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

                InvokeInGithubOperationContext(() => { githubRepositoryHook = pscmdlet.GithubChannel.CreateRepositoryHook(owner, repository, githubRepositoryHook); });
                InvokeInGithubOperationContext(() => { pscmdlet.GithubChannel.TestRepositoryHook(owner, repository, githubRepositoryHook.Id); });
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
                LinkedRepository = repositories.FirstOrDefault(r => r.FullName.Equals(repositoryFullName, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                var remoteUris = Git.GetRemoteUris();
                if (remoteUris.Count == 1)
                {
                    LinkedRepository = repositories.FirstOrDefault(r => r.GitUrl.Equals(remoteUris.First(), StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    // filter repositories to reduce prompt options
                    repositories = repositories.Where(r => remoteUris.Any(u => u.Equals(r.GitUrl, StringComparison.InvariantCultureIgnoreCase))).ToList<GithubRepository>();
                }
            }

            if (LinkedRepository == null)
            {
                Collection<ChoiceDescription> choices = new Collection<ChoiceDescription>(repositories.Select(item => new ChoiceDescription(item.FullName)).ToList<ChoiceDescription>());
                var choice = ((PSCmdlet)pscmdlet).Host.UI.PromptForChoice(
                    "Choose a repository",
                    "",
                    choices,
                    0
                );

                LinkedRepository = repositories.FirstOrDefault(r => r.FullName.Equals(choices[choice].Label));
            }
        }

        public override void Deploy(Site website)
        {
            CreateOrUpdateHook(LinkedRepository.Owner.Login, LinkedRepository.Name, website);
        }

        protected IGithubServiceManagement CreateGithubChannel()
        {
            // If ShareChannel is set by a unit test, use the same channel that
            // was passed into out constructor.  This allows the test to submit
            // a mock that we use for all network calls.
            if (pscmdlet.ShareChannel)
            {
                return pscmdlet.GithubChannel;
            }

            return CreateServiceManagementChannel<IGithubServiceManagement>(new Uri("https://api.github.com"), username, password);
        }

        public static T CreateServiceManagementChannel<T>(Uri remoteUri, string username, string password)
            where T : class
        {
            WebChannelFactory<T> factory = new WebChannelFactory<T>(remoteUri);
            factory.Endpoint.Behaviors.Add(new GithubAutHeaderInserter() { Username = username, Password = password });

            WebHttpBinding wb = factory.Endpoint.Binding as WebHttpBinding;
            wb.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            wb.Security.Mode = WebHttpSecurityMode.Transport;

            if (!string.IsNullOrEmpty(username))
            {
                factory.Credentials.UserName.UserName = username;
            }
            if (!string.IsNullOrEmpty(password))
            {
                factory.Credentials.UserName.Password = password;
            }

            return factory.CreateChannel();
        }

        /// <summary>
        /// Invoke the given operation within an OperationContextScope if the
        /// channel supports it.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        protected void InvokeInGithubOperationContext(Action action)
        {
            IContextChannel contextChannel = pscmdlet.GithubChannel as IContextChannel;
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
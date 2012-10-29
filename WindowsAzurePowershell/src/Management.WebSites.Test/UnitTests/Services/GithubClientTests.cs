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

namespace Microsoft.WindowsAzure.Management.Websites.Test.UnitTests.Services
{
    using Microsoft.WindowsAzure.Management.Websites.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Websites.Services.Github;
    using Microsoft.WindowsAzure.Management.Websites.Services.Github.Entities;
    using Microsoft.WindowsAzure.Management.Websites.Test.UnitTests.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Services;

    [TestClass]
    public class GithubClientTests
    {
        [TestMethod]
        public void TestGetRepositories()
        {
            // Setup
            SimpleGithubManagement channel = new SimpleGithubManagement();

            channel.GetRepositoriesThunk = ar => new List<GithubRepository> { new GithubRepository { Name = "userrepo1" } };
            channel.GetOrganizationsThunk = ar => new List<GithubOrganization> { new GithubOrganization { Login = "org1" }, new GithubOrganization { Login = "org2" } };
            channel.GetRepositoriesFromOrgThunk = ar => 
            {
                if (ar.Values["organization"] == "org1")
                {
                    return new List<GithubRepository> { new GithubRepository { Name = "org1repo1" } };
                }
                else if (ar.Values["organization"] == "org2")
                {
                    return new List<GithubRepository> { new GithubRepository { Name = "org2repo1" } };
                }

                return new List<GithubRepository> { new GithubRepository { Name = "other" } };
            };


            // Test
            CmdletAccessor cmdletAccessor = new CmdletAccessor();
            cmdletAccessor.GithubChannel = channel;
            
            GithubClientAccessor githubClientAccessor = new GithubClientAccessor(cmdletAccessor, null, null, null);
            var repositories = githubClientAccessor.GetRepositoriesAccessor();

            Assert.AreEqual(3, repositories.Count);
            Assert.IsNotNull(repositories.FirstOrDefault(r => r.Name.Equals("userrepo1")));
            Assert.IsNotNull(repositories.FirstOrDefault(r => r.Name.Equals("org1repo1")));
            Assert.IsNotNull(repositories.FirstOrDefault(r => r.Name.Equals("org2repo1")));
        }
    }

    internal class GithubClientAccessor : GithubClient
    {
        public GithubClientAccessor(IGithubCmdlet pscmdlet, string githubUsername, string githubPassword, string githubRepository)
            : base (pscmdlet, githubUsername, githubPassword, githubRepository)
        {
        }

        public IList<GithubRepository> GetRepositoriesAccessor()
        {
            return GetRepositories();
        }
    }

    internal class CmdletAccessor : IGithubCmdlet
    {
        public IGithubServiceManagement GithubChannel { get; set; }

        public bool ShareChannel
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public InvocationInfo MyInvocation
        {
            get { return null; }
        }

        public PSHost Host
        {
            get { throw new NotImplementedException(); }
        }
    }
}
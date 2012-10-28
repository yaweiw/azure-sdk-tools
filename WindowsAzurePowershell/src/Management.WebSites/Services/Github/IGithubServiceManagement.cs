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

namespace Microsoft.WindowsAzure.Management.Websites.Services.Github
{
    using Microsoft.WindowsAzure.Management.Websites.Services.Github.Entities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using WebEntities;

    /// <summary>
    /// Provides the Github Api. 
    /// </summary>
    [ServiceContract]
    public interface IGithubServiceManagement
    {
        [Description("Gets the organizations for the authenticated user")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = "/user/orgs")]
        IAsyncResult BeginGetOrganizations(AsyncCallback callback, object state);
        IList<GithubOrganization> EndGetOrganizations(IAsyncResult asyncResult);
        
        [Description("Gets the organizations for an user")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = "/users/{user}/orgs")]
        IAsyncResult BeginGetOrganizationsFromUser(string user, AsyncCallback callback, object state);
        IList<GithubOrganization> EndGetOrganizationsFromUser(IAsyncResult asyncResult);

        [Description("Gets the repositories for the authenticated user")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = "/user/repos")]
        IAsyncResult BeginGetRepositories(AsyncCallback callback, object state);
        IList<GithubRepository> EndGetRepositories(IAsyncResult asyncResult);

        [Description("Gets the repositories for an user")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = "/users/{user}/repos")]
        IAsyncResult BeginGetRepositoriesFromUser(string user, AsyncCallback callback, object state);
        IList<GithubRepository> EndGetRepositoriesFromUser(IAsyncResult asyncResult);

        [Description("Gets the repositories for an organization")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = "/orgs/{organization}/repos")]
        IAsyncResult BeginGetRepositoriesFromOrg(string organization, AsyncCallback callback, object state);
        IList<GithubRepository> EndGetRepositoriesFromOrg(IAsyncResult asyncResult);

        [Description("Gets the repository hooks")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = "/repos/{owner}/{repository}/hooks")]
        IAsyncResult BeginGetRepositoryHooks(string owner, string repository, AsyncCallback callback, object state);
        IList<GithubRepositoryHook> EndGetRepositoryHooks(IAsyncResult asyncResult);

        [Description("Creates a repository hook")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = "/repos/{owner}/{repository}/hooks")]
        IAsyncResult BeginCreateRepositoryHook(string owner, string repository, GithubRepositoryHook hook, AsyncCallback callback, object state);
        void EndCreateRepositoryHook(IAsyncResult asyncResult);

        [Description("Updates a repository hook")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PATCH", UriTemplate = "/repos/{owner}/{repository}/hooks/{id}")]
        IAsyncResult BeginUpdateRepositoryHook(string owner, string repository, string id, GithubRepositoryHook hook, AsyncCallback callback, object state);
        void EndUpdateRepositoryHook(IAsyncResult asyncResult);

        [Description("Tests a repository hook")]
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = "/repos/{owner}/{repository}/hooks/{id}/test")]
        IAsyncResult BeginTestRepositoryHook(string owner, string repository, string id, AsyncCallback callback, object state);
        void EndTestRepositoryHook(IAsyncResult asyncResult);
    }
}

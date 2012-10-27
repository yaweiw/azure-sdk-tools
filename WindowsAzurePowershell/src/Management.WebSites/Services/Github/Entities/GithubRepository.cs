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

namespace Microsoft.WindowsAzure.Management.Websites.Services.Github.Entities
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class GithubRepositoryOwner
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Login { get; set; }

        [DataMember(Name = "avatar_url")]
        public string AvatarUrl { get; set; }

        [DataMember(Name = "gravatar_id")]
        public string GravatarId { get; set; }

        [DataMember]
        public string Id { get; set; }
    }

    [DataContract]
    public class GithubRepository
    {
        [DataMember(Name = "clone_url")]
        public string CloneUrl { get; set; }

        [DataMember(Name = "forks_count")]
        public int ForksCount { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "watchers")]
        public int Watchers { get; set; }

        [DataMember(Name = "has_issues")]
        public bool HasIssues { get; set; }

        [DataMember(Name = "open_issues_count")]
        public int OpenIssuesCount { get; set; }

        [DataMember(Name = "owner")]
        public GithubRepositoryOwner Owner { get; set; }

        [DataMember(Name = "full_name")]
        public string FullName { get; set; }

        [DataMember(Name = "has_wiki")]
        public bool HasWiki { get; set; }

        [DataMember(Name = "mirror_url")]
        public string MirrorUrl { get; set; }

        [DataMember(Name = "permissions")]
        public string Permissions { get; set; }

        [DataMember(Name = "created_at")]
        public DateTime CreatedAt { get; set; }

        [DataMember(Name = "homepage")]
        public string Homepage { get; set; }

        [DataMember(Name = "svn_url")]
        public string SvnUrl { get; set; }

        [DataMember(Name = "open_issues")]
        public string OpenIssues { get; set; }

        [DataMember(Name = "pushed_at")]
        public DateTime PushedAt { get; set; }

        [DataMember(Name = "forks")]
        public int Forks { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "ssh_url")]
        public string SshUrl { get; set; }

        [DataMember(Name = "size")]
        public int Size { get; set; }

        [DataMember(Name = "fork")]
        public bool Fork { get; set; }

        [DataMember(Name = "updated_at")]
        public DateTime UpdatedAt { get; set; }

        [DataMember(Name = "git_url")]
        public string GitUrl { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "has_downloads")]
        public string HasDownloads { get; set; }

        [DataMember(Name = "private")]
        public bool Private { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "watchers_count")]
        public string WatchersCount { get; set; }

        [DataMember(Name = "language")]
        public string Language { get; set; }

        [DataMember(Name = "html_url")]
        public string HtmlUrl { get; set; }
    }
}

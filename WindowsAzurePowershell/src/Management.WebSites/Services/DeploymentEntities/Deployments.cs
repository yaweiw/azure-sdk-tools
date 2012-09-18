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

namespace Microsoft.WindowsAzure.Management.Websites.Services.DeploymentEntities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Utilities;

    /// <summary>
    /// Deployment.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Deployment
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "status")]
        public int Status { get; set; }

        [DataMember(Name = "status_text")]
        public string StatusText { get; set; }

        [DataMember(Name = "author_email")]
        public string AuthorEmail { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }

        [DataMember(Name = "deployer")]
        public string Deployer { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "received_time")]
        public DateTime ReceivedTime { get; set; }

        [DataMember(Name = "start_time")]
        public DateTime StartTime { get; set; }

        [DataMember(Name = "end_time")]
        public DateTime EndTime { get; set; }

        [DataMember(Name = "last_success_end_time")]
        public DateTime LastSuccessEndTime { get; set; }

        [DataMember(Name = "complete")]
        public bool Complete { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "log_url")]
        public string LogUrl { get; set; }
    }

    /// <summary>
    /// Collection of deployments.
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Deployments : List<Deployment>
    {

        /// <summary>
        /// Empty collection.
        /// </summary>
        public Deployments() { }

        /// <summary>
        /// Initialize collection.
        /// </summary>
        /// <param name="deployments"></param>
        public Deployments(List<Deployment> deployments) : base(deployments) { }
    }
}

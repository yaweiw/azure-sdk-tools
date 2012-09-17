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

namespace Microsoft.WindowsAzure.Management.Websites.Cmdlets.Common
{
    using System;
    using System.Security.Permissions;
    using Management.Services;
    using Management.Utilities;
    using Services;
    using Services.WebEntities;
    using WebSites.Cmdlets.Common;

    public abstract class DeploymentBaseCmdlet : WebsiteContextBaseCmdlet
    {
        protected IDeploymentServiceManagement DeploymentChannel { get; set; }

        private Repository GetRepository(string websiteName)
        {
            Site site = null;
            InvokeInOperationContext(() => { site = RetryCall(s => Channel.GetSite(s, websiteName, "repositoryuri,publishingpassword,publishingusername")); });
            if (site != null)
            {
                return new Repository(site);
            }

            return null;
        }

        internal override void ExecuteCommand()
        {
            Repository repository = GetRepository(Name);
            DeploymentChannel = CreateDeploymentChannel(repository);
        }        

        protected IDeploymentServiceManagement CreateDeploymentChannel(Repository repository)
        {
            // If ShareChannel is set by a unit test, use the same channel that
            // was passed into out constructor.  This allows the test to submit
            // a mock that we use for all network calls.
            if (ShareChannel)
            {
                return DeploymentChannel;
            }

            UriBuilder uriBuilder = new UriBuilder(repository.RepositoryUri)
            {
                UserName = repository.PublishingUsername,
                Password = repository.PublishingPassword
            };

            return ServiceManagementHelper.CreateServiceManagementChannel<IDeploymentServiceManagement>(uriBuilder.Uri, null);
        }
    }
}
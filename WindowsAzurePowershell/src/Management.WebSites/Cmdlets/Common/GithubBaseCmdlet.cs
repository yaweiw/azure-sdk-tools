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

namespace Microsoft.WindowsAzure.Management.WebSites.Cmdlets.Common
{
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Websites.Services.Github;
    using Microsoft.WindowsAzure.Management.WebSites.Cmdlets.Common;
    using System;
    using System.Management.Automation;
    using System.ServiceModel;

    public class GithubBaseCmdlet : WebsiteContextBaseCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The github username.")]
        [ValidateNotNullOrEmpty]
        public string GithubUsername
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The github password.")]
        [ValidateNotNullOrEmpty]
        public string GithubPassword
        {
            get;
            set;
        }

        protected IGithubServiceManagement GithubChannel { get; set; }

        internal override void ExecuteCommand()
        {
            GithubChannel = CreateGithubChannel();
        }

        protected IGithubServiceManagement CreateGithubChannel()
        {
            // If ShareChannel is set by a unit test, use the same channel that
            // was passed into out constructor.  This allows the test to submit
            // a mock that we use for all network calls.
            if (ShareChannel)
            {
                return GithubChannel;
            }

            return ServiceManagementHelper2.CreateServiceManagementChannel<IGithubServiceManagement>(new Uri("https://api.github.com"), GithubUsername, GithubPassword);
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

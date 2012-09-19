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

namespace Microsoft.WindowsAzure.Management.Websites.Cmdlets
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Common;
    using Properties;
    using Services;
    using Services.DeploymentEntities;

    /// <summary>
    /// Gets the git deployments.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebsiteDeployment")]
    public class GetAzureWebsiteDeploymentCommand : DeploymentBaseCmdlet
    {
        private const int DefaultMaxResults = 20;

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The maximum number of results to display.")]
        [ValidateNotNullOrEmpty]
        public string CommitId
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The maximum number of results to display.")]
        [ValidateNotNullOrEmpty]
        public int? MaxResults
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureWebsiteDeploymentCommand class.
        /// </summary>
        public GetAzureWebsiteDeploymentCommand()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureWebsiteDeploymentCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        /// <param name="deploymentChannel">
        /// Channel used for communication with the git repository.
        /// </param>
        public GetAzureWebsiteDeploymentCommand(IWebsitesServiceManagement channel, IDeploymentServiceManagement deploymentChannel)
        {
            Channel = channel;
            DeploymentChannel = deploymentChannel;
        }

        internal override void ExecuteCommand()
        {
            base.ExecuteCommand();

            InvokeInDeploymentOperationContext(() =>
            {
                Deployments deployments = DeploymentChannel.GetDeployments(MaxResults ?? DefaultMaxResults);

                if (CommitId != null)
                {
                    Deployment deployment = deployments.FirstOrDefault(d => d.Id.Equals(CommitId));
                    if (deployment == null)
                    {
                        throw new Exception(string.Format(Resources.InvalidDeployment, CommitId));
                    }

                    WriteObject(deployment);
                } 
                else
                {
                    WriteObject(deployments, true);   
                }
            });
        }
    }
}

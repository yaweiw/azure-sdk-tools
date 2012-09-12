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
    using System.Management.Automation;
    using Common;
    using Services;

    /// <summary>
    /// Gets the git deployments.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebsiteDeployment")]
    public class GetAzureWebsiteDeploymentCommand : DeploymentBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The maximum number of results to display.")]
        [ValidateNotNullOrEmpty]
        public string MaxResults
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureWebsiteDeploymentCommand class.
        /// </summary>
        public GetAzureWebsiteDeploymentCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureWebsiteDeploymentCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public GetAzureWebsiteDeploymentCommand(IDeploymentServiceManagement channel)
        {
            Channel = channel;
        }

        internal override void ExecuteCommand()
        {
            throw new System.NotImplementedException();
        }
    }
}

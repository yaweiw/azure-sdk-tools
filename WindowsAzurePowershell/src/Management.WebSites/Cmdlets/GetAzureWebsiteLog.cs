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
    using Services.DeploymentEntities;

    /// <summary>
    /// Gets the azure logs.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebsiteLog")]
    public class GetAzureWebsiteLogCommand : DeploymentBaseCmdlet
    {

        /// <summary>
        /// Initializes a new instance of the SaveAzureWebsiteLogCommand class.
        /// </summary>
        public GetAzureWebsiteLogCommand()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SaveAzureWebsiteLogCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        /// <param name="deploymentChannel">
        /// Channel used for communication with the git repository.
        /// </param>
        public GetAzureWebsiteLogCommand(IWebsitesServiceManagement channel, IDeploymentServiceManagement deploymentChannel)
        {
            Channel = channel;
            DeploymentChannel = deploymentChannel;
        }

        internal override void ExecuteCommand()
        {
            base.ExecuteCommand();

            // List new deployments
            Deployments deployments = null;
            InvokeInDeploymentOperationContext(() => { deployments = DeploymentChannel.GetDeployments(GetAzureWebsiteDeploymentCommand.DefaultMaxResults); });

            Logs allLogs = new Logs();
            foreach (Deployment deployment in deployments)
            {
                Logs logs = null;
                InvokeInDeploymentOperationContext(() => { logs = DeploymentChannel.GetDeploymentLogs(deployment.Id); });
                allLogs.AddRange(logs);
            }

            WriteObject(allLogs, true);
        }
    }
}

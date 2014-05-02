// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
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
namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.PSCmdlets
{
    using System.Management.Automation;
    using Commands.BaseCommandInterfaces;
    using Commands.CommandInterfaces;
    using DataObjects;
    using GetAzureHDInsightClusters;
    using ServiceLocation;

    /// <summary>
    ///     Represents the New-AzureHDInsightClusterConfig  Power Shell Cmdlet.
    /// </summary>
    [Cmdlet(VerbsCommon.New, AzureHdInsightPowerShellConstants.AzureHDInsightClusterConfig)]
    [OutputType(typeof(AzureHDInsightConfig))]
    public class NewAzureHDInsightClusterConfigCmdlet : AzureHDInsightCmdlet, INewAzureHDInsightClusterConfigBase
    {
        private readonly INewAzureHDInsightClusterConfigCommand command;

        /// <summary>
        ///     Initializes a new instance of the NewAzureHDInsightClusterConfigCmdlet class.
        /// </summary>
        public NewAzureHDInsightClusterConfigCmdlet()
        {
            this.command = ServiceLocator.Instance.Locate<IAzureHDInsightCommandFactory>().CreateNewConfig();
        }

        /// <inheritdoc />
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "The number of data nodes to use for the HDInsight cluster.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetConfigClusterSizeInNodesOnly)]
        [Alias(AzureHdInsightPowerShellConstants.AliasNodes, AzureHdInsightPowerShellConstants.AliasSize)]
        public int ClusterSizeInNodes
        {
            get { return this.command.ClusterSizeInNodes; }
            set { this.command.ClusterSizeInNodes = value; }
        }

        /// <inheritdoc />
        [Parameter(Position = 1, Mandatory = false, HelpMessage = "The size of the head node VMs.",
            ParameterSetName = AzureHdInsightPowerShellConstants.ParameterSetConfigClusterSizeInNodesOnly)]
        public NodeVMSize HeadNodeVMSize
        {
            get { return command.HeadNodeVMSize; }
            set { command.HeadNodeVMSize = value; }
        }

        /// <summary>
        ///     Finishes the execution of the cmdlet by listing the clusters.
        /// </summary>
        protected override void EndProcessing()
        {
            this.command.EndProcessing().Wait();
            foreach (AzureHDInsightConfig output in this.command.Output)
            {
                this.WriteObject(output);
            }
            this.WriteDebugLog();
        }

        /// <inheritdoc />
        protected override void StopProcessing()
        {
            this.command.Cancel();
        }
    }
}

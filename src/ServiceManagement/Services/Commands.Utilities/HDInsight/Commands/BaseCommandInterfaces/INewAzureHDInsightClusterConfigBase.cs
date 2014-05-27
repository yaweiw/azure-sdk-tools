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
namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.BaseCommandInterfaces
{
    using ClusterProvisioning.Data;

    internal interface INewAzureHDInsightClusterConfigBase
    {
        /// <summary>
        ///     Gets or sets the size of the cluster in data nodes.
        /// </summary>
        int ClusterSizeInNodes { get; set; }

        /// <summary>
        /// Gets or sets the size of the head node VMs.
        /// </summary>
        /// <value>
        /// The size of the head node VM.
        /// </value>
        NodeVMSize HeadNodeVMSize { get; set; }

        /// <summary>
        /// Gets or sets the type of the head.
        /// </summary>
        /// <value>
        /// The type of cluster.
        /// </value>
        ClusterType ClusterType { get; set; }        
    }
}

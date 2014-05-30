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
namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandImplementations
{
    using CommandInterfaces;
    using DataObjects;
    using GetAzureHDInsightClusters;
    using GetAzureHDInsightClusters.Extensions;
    using System;
    using System.Globalization;
    using System.Threading.Tasks;

    internal class UseAzureHDInsightClusterCommand : AzureHDInsightClusterCommand<AzureHDInsightClusterConnection>, IUseAzureHDInsightClusterCommand
    {
        private const string GrantHttpAccessCmdletName = "Grant Azure HDInsight Http Services Access";

        public override async Task EndProcessing()
        {
            this.Name.ArgumentNotNullOrEmpty("Name");
            IHDInsightClient client = this.GetClient();
            var cluster = await client.GetClusterAsync(this.Name);
            var connection = new AzureHDInsightClusterConnection();
            connection.Credential = this.GetSubscriptionCertificateCredentials(this.CurrentSubscription);

            if (cluster == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Failed to connect to cluster :{0}", this.Name));
            }

            connection.Cluster = new AzureHDInsightCluster(cluster);

            if (cluster.State != ClusterState.Running)
            {
                throw new NotSupportedException(
                    string.Format(CultureInfo.InvariantCulture, "Cluster {0} is in an invalid state : {1}", this.Name, cluster.State.ToString()));
            }

            if (string.IsNullOrEmpty(cluster.HttpUserName))
            {
                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Cluster {0} is not configured for Http Services access.\r\nPlease use the {1} cmdlet to enable Http Services access.",
                        this.Name,
                        GrantHttpAccessCmdletName));
            }

            this.Output.Add(connection);
        }
    }
}

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
    using Hadoop.Client;
    using System.Linq;
    using System.Threading.Tasks;

    internal class GetAzureHDInsightJobCommand : AzureHDInsightJobCommand<AzureHDInsightJob>, IGetAzureHDInsightJobCommand
    {
        public override async Task EndProcessing()
        {
            IJobSubmissionClient client = this.GetClient(this.Cluster);
            this.Output.Clear();
            if (string.IsNullOrEmpty(this.JobId))
            {
                var jobsList = await client.ListJobsAsync();
                this.Output.AddRange(jobsList.Jobs.Select(hadoopJob => new AzureHDInsightJob(hadoopJob, this.Cluster)));
            }
            else
            {
                var jobDetail = await client.GetJobAsync(this.JobId);
                if (jobDetail != null)
                {
                    this.Output.Add(new AzureHDInsightJob(jobDetail, this.Cluster));
                }
            }
        }
    }
}

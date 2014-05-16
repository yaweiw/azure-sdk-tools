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
namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandInterfaces
{
    using BaseCommandInterfaces;
    using DataObjects;
    using GetAzureHDInsightClusters;
    using Hadoop.Client;
    using System;
    using System.Threading.Tasks;

    internal interface IWaitAzureHDInsightJobCommand : IAzureHDInsightCommand<AzureHDInsightJob>, IWaitAzureHDInsightJobBase
    {
        /// <summary>
        ///     Event that is fired when the client provisions a cluster.
        /// </summary>
        event EventHandler<WaitJobStatusEventArgs> JobStatusEvent;

        JobDetails JobDetailsStatus { get; }

        Task ProcessRecord();
    }
}

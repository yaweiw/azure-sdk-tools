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
    using System.Collections.Generic;
    using BaseCommandInterfaces;
    using DataObjects;
    using GetAzureHDInsightClusters;

    /// <summary>
    ///     Worker object for creating a cluster via PowerShell.
    /// </summary>
    internal interface INewAzureHDInsightClusterCommand : IAzureHDInsightCommand<AzureHDInsightCluster>, INewAzureHDInsightClusterBase
    {
        ICollection<AzureHDInsightStorageAccount> AdditionalStorageAccounts { get; }

        ConfigValuesCollection CoreConfiguration { get; set; }

        ConfigValuesCollection YarnConfiguration { get; set; }

        ConfigValuesCollection HdfsConfiguration { get; set; }

        HiveConfiguration HiveConfiguration { get; set; }

        AzureHDInsightMetastore HiveMetastore { get; set; }

        MapReduceConfiguration MapReduceConfiguration { get; set; }

        OozieConfiguration OozieConfiguration { get; set; }

        AzureHDInsightMetastore OozieMetastore { get; set; }

        ClusterState State { get; }

        string Location { get; set; }

        bool EnableHighAvailability { get; set; }
    }
}

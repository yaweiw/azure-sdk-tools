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
namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.DataObjects
{
    using System.Collections.Generic;

    /// <summary>
    ///     Represents an Azure Configuration to be used when creating new clusters.
    /// </summary>
    public class AzureHDInsightConfig
    {
        /// <summary>
        ///     Initializes a new instance of the AzureHDInsightConfig class.
        /// </summary>
        public AzureHDInsightConfig()
        {
            this.DefaultStorageAccount = new AzureHDInsightDefaultStorageAccount();
            this.AdditionalStorageAccounts = new List<AzureHDInsightStorageAccount>();
            this.CoreConfiguration = new ConfigValuesCollection();
            this.YarnConfiguration = new ConfigValuesCollection();
            this.HdfsConfiguration = new ConfigValuesCollection();
            this.MapReduceConfiguration = new MapReduceConfiguration();
            this.HiveConfiguration = new HiveConfiguration();
            this.OozieConfiguration = new OozieConfiguration();
        }

        /// <summary>
        ///     Gets the additional storage accounts for the HDInsight cluster.
        /// </summary>
        public ICollection<AzureHDInsightStorageAccount> AdditionalStorageAccounts { get; private set; }

        /// <summary>
        ///     Gets or sets the size of the cluster in data nodes.
        /// </summary>
        public int ClusterSizeInNodes { get; set; }

        /// <summary>
        ///     Gets a collection of configuration properties to customize the Core Hadoop service.
        /// </summary>
        public ConfigValuesCollection CoreConfiguration { get; private set; }

        /// <summary>
        ///     Gets a collection of configuration properties to customize the Yarn Hadoop service.
        /// </summary>
        public ConfigValuesCollection YarnConfiguration { get; private set; }

        /// <summary>
        ///     Gets or sets the default storage account for the HDInsight cluster.
        /// </summary>
        public AzureHDInsightDefaultStorageAccount DefaultStorageAccount { get; set; }

        /// <summary>
        ///     Gets a collection of configuration properties to customize the Hdfs Hadoop service.
        /// </summary>
        public ConfigValuesCollection HdfsConfiguration { get; private set; }

        /// <summary>
        ///     Gets a collection of configuration properties to customize the Hive Hadoop service.
        /// </summary>
        public HiveConfiguration HiveConfiguration { get; private set; }

        /// <summary>
        ///     Gets or sets the Hive Metastore.
        /// </summary>
        public AzureHDInsightMetastore HiveMetastore { get; set; }

        /// <summary>
        ///     Gets a collection of configuration properties to customize the MapReduce Hadoop service.
        /// </summary>
        public MapReduceConfiguration MapReduceConfiguration { get; private set; }

        /// <summary>
        ///     Gets a collection of configuration properties to customize the Oozie Hadoop service.
        /// </summary>
        public OozieConfiguration OozieConfiguration { get; private set; }

        /// <summary>
        ///     Gets or sets the Oozie Metastore.
        /// </summary>
        public AzureHDInsightMetastore OozieMetastore { get; set; }
    }
}

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
    using System;
    using System.Collections;
    using System.Threading.Tasks;

    internal class NewAzureHDInsightHiveJobDefinitionCommand
        : AzureHDInsightNewJobDefinitionCommand<AzureHDInsightHiveJobDefinition>, INewAzureHDInsightHiveJobDefinitionCommand
    {
        private readonly HiveJobCreateParameters hiveJobDefinition = new HiveJobCreateParameters();
        private string[] args = new string[] { };
        private Hashtable defines = new Hashtable();
        private string[] resources = new string[] { };

        public string[] Arguments
        {
            get { return this.args; }
            set { this.args = value; }
        }

        public override Hashtable Defines
        {
            get { return this.defines; }
            set { this.defines = value; }
        }

        public string File
        {
            get { return this.hiveJobDefinition.File; }
            set { this.hiveJobDefinition.File = value; }
        }

        public override string[] Files
        {
            get { return this.resources; }
            set { this.resources = value; }
        }

        public string JobName
        {
            get { return this.hiveJobDefinition.JobName; }
            set { this.hiveJobDefinition.JobName = value; }
        }

        public string Query
        {
            get { return this.hiveJobDefinition.Query; }
            set { this.hiveJobDefinition.Query = value; }
        }

        public override string StatusFolder
        {
            get { return this.hiveJobDefinition.StatusFolder; }
            set { this.hiveJobDefinition.StatusFolder = value; }
        }

        public override Task EndProcessing()
        {
            if (this.Query.IsNotNullOrEmpty() && this.File.IsNotNullOrEmpty())
            {
                throw new ArgumentException("Only Query or File can be specified, not both.");
            }

            var hiveJob = new AzureHDInsightHiveJobDefinition();
            hiveJob.JobName = this.JobName;
            hiveJob.Query = this.Query;
            hiveJob.File = this.File;

            if (hiveJob.Query.IsNullOrEmpty())
            {
                hiveJob.File.ArgumentNotNullOrEmpty("File");
            }

            hiveJob.StatusFolder = this.StatusFolder;
            if (this.Defines.IsNotNull())
            {
                hiveJob.Defines.AddRange(this.Defines.ToKeyValuePairs());
            }

            if (this.Files.IsNotNull())
            {
                hiveJob.Files.AddRange(this.Files);
            }

            if (this.Arguments.IsNotNull())
            {
                hiveJob.Arguments.AddRange(this.Arguments);
            }

            this.Output.Add(hiveJob);
            return TaskEx.FromResult(0);
        }
    }
}

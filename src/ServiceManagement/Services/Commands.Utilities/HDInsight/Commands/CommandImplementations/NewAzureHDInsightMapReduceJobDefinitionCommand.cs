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
    using System.Collections;
    using System.Threading.Tasks;

    internal class NewAzureHDInsightMapReduceJobJobDefinitionCommand
        : AzureHDInsightNewJobDefinitionCommand<AzureHDInsightMapReduceJobDefinition>, INewAzureHDInsightMapReduceJobDefinitionCommand
    {
        private readonly MapReduceJobCreateParameters mapReduceJobDefinition = new MapReduceJobCreateParameters();
        private string[] arguments = new string[] { };
        private Hashtable defines = new Hashtable();
        private string[] libjars = new string[] { };
        private string[] resources = new string[] { };

        public string[] Arguments
        {
            get { return this.arguments; }
            set { this.arguments = value; }
        }

        public string ClassName
        {
            get { return this.mapReduceJobDefinition.ClassName; }
            set { this.mapReduceJobDefinition.ClassName = value; }
        }

        public override Hashtable Defines
        {
            get { return this.defines; }
            set { this.defines = value; }
        }

        public override string[] Files
        {
            get { return this.resources; }
            set { this.resources = value; }
        }

        public string JarFile
        {
            get { return this.mapReduceJobDefinition.JarFile; }
            set { this.mapReduceJobDefinition.JarFile = value; }
        }

        public string JobName
        {
            get { return this.mapReduceJobDefinition.JobName; }
            set { this.mapReduceJobDefinition.JobName = value; }
        }

        public string[] LibJars
        {
            get { return this.libjars; }
            set { this.libjars = value; }
        }

        public override string StatusFolder
        {
            get { return this.mapReduceJobDefinition.StatusFolder; }
            set { this.mapReduceJobDefinition.StatusFolder = value; }
        }

        public override Task EndProcessing()
        {
            this.ClassName.ArgumentNotNullOrEmpty("ClassName");
            this.JarFile.ArgumentNotNullOrEmpty("JarFile");

            var mapReduceJob = new AzureHDInsightMapReduceJobDefinition();
            mapReduceJob.ClassName = this.ClassName;
            mapReduceJob.JarFile = this.JarFile;
            mapReduceJob.JobName = this.JobName;
            mapReduceJob.StatusFolder = this.StatusFolder;
            if (this.Defines.IsNotNull())
            {
                mapReduceJob.Defines.AddRange(this.Defines.ToKeyValuePairs());
            }

            if (this.Arguments.IsNotNull())
            {
                mapReduceJob.Arguments.AddRange(this.Arguments);
            }

            if (this.Files.IsNotNull())
            {
                mapReduceJob.Files.AddRange(this.Files);
            }

            if (this.LibJars.IsNotNull())
            {
                mapReduceJob.LibJars.AddRange(this.LibJars);
            }

            this.Output.Add(mapReduceJob);
            return TaskEx.FromResult(0);
        }
    }
}

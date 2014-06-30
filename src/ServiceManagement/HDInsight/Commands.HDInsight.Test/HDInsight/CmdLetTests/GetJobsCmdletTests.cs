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
namespace Microsoft.WindowsAzure.Commands.Test.HDInsight.CmdLetTests
{
    using CommandTests;
    using Hadoop.Client;
    using Management.HDInsight;
    using Management.HDInsight.Cmdlet.DataObjects;
    using Management.HDInsight.Cmdlet.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utilities.HDInsight.PowerShellTestAbstraction.Interfaces;
    using Utilities.HDInsight.Simulators;
    using Utilities.HDInsight.Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetJobsCmdletTests : HDInsightTestCaseBase
    {
        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        [TestCategory("Integration")]
        
        [TestCategory("Jobs")]
        [TestCategory("Get-AzureHDInsightJobs")]
        public void ICanCallThe_Get_HDInsightJobsCmdlet()
        {
            using (IRunspace runspace = this.GetPowerShellRunspace())
            {
                ClusterDetails testCluster = CmdletScenariosTestCaseBase.GetHttpAccessEnabledCluster();
                IPipelineResult results =
                    runspace.NewPipeline()
                                      .AddCommand(CmdletConstants.GetAzureHDInsightJob)
                                      .WithParameter(CmdletConstants.Cluster, testCluster.ConnectionUrl)
                            .WithParameter(CmdletConstants.Credential, GetPSCredential(testCluster.HttpUserName, testCluster.HttpPassword))
                                      .Invoke();
                IEnumerable<AzureHDInsightJob> jobHistory = results.Results.ToEnumerable<AzureHDInsightJob>();

                JobList expectedJobHistory = HDInsightGetJobsCommandTests.GetJobHistory(testCluster.ConnectionUrl);
                Assert.AreEqual(expectedJobHistory.Jobs.Count, jobHistory.Count(), "Should have {0} jobs.", expectedJobHistory.Jobs.Count);
            }
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        [TestCategory("Integration")]
        
        [TestCategory("Jobs")]
        [TestCategory("Get-AzureHDInsightJobs")]
        public void ICanCallThe_Get_HDInsightJobsCmdletWithJobId()
        {
            using (IRunspace runspace = this.GetPowerShellRunspace())
            {
                ClusterDetails testCluster = CmdletScenariosTestCaseBase.GetHttpAccessEnabledCluster();
                IPipelineResult results =
                    runspace.NewPipeline()
                            .AddCommand(CmdletConstants.GetAzureHDInsightJob)
                            .WithParameter(CmdletConstants.Cluster, testCluster.ConnectionUrl)
                            .WithParameter(CmdletConstants.Credential, GetPSCredential(testCluster.HttpUserName, testCluster.HttpPassword))
                            .Invoke();
                var jobDetail = results.Results.ElementAt(0).ImmediateBaseObject as AzureHDInsightJobBase;
                Assert.IsNotNull(jobDetail);

                AzureHDInsightJob getJobDetailObj = GetJobWithID(runspace, jobDetail.JobId, testCluster);
                Assert.IsNotNull(getJobDetailObj);
                Assert.AreEqual(jobDetail.JobId, getJobDetailObj.JobId);
            }
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        [TestCategory("Integration")]
        
        [TestCategory("Jobs")]
        [TestCategory("Get-AzureHDInsightJobs")]
        public void ICanCallThe_Get_HDInsightJobsCmdlet_WithDebug()
        {
            using (IRunspace runspace = this.GetPowerShellRunspace())
            {
                ClusterDetails testCluster = CmdletScenariosTestCaseBase.GetHttpAccessEnabledCluster();
                var logWriter = new PowershellLogWriter();
                BufferingLogWriterFactory.Instance = logWriter;
                IPipelineResult results =
                    runspace.NewPipeline()
                                      .AddCommand(CmdletConstants.GetAzureHDInsightJob)
                                      .WithParameter(CmdletConstants.Cluster, testCluster.ConnectionUrl)
                            .WithParameter(CmdletConstants.Credential, GetPSCredential(testCluster.HttpUserName, testCluster.HttpPassword))
                                      .WithParameter(CmdletConstants.Debug, null)
                                      .Invoke();
                IEnumerable<AzureHDInsightJob> jobHistory = results.Results.ToEnumerable<AzureHDInsightJob>();

                JobList expectedJobHistory = HDInsightGetJobsCommandTests.GetJobHistory(testCluster.ConnectionUrl);
                Assert.AreEqual(expectedJobHistory.Jobs.Count, jobHistory.Count(), "Should have {0} jobs.", expectedJobHistory.Jobs.Count);
                string expectedLogMessage = "Listing jobs";
                Assert.IsTrue(logWriter.Buffer.Any(message => message.Contains(expectedLogMessage)));
                BufferingLogWriterFactory.Reset();
            }
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        [TestCategory("Integration")]
        
        [TestCategory("Jobs")]
        [TestCategory("Get-AzureHDInsightJobs")]
        public void ICanCallThe_Get_HDInsightJobsCmdlet_WithNonExistantJobId()
        {
            using (IRunspace runspace = this.GetPowerShellRunspace())
            {
                ClusterDetails testCluster = CmdletScenariosTestCaseBase.GetHttpAccessEnabledCluster();
                string jobId = Guid.NewGuid().ToString();
                IPipelineResult results =
                    runspace.NewPipeline()
                                              .AddCommand(CmdletConstants.GetAzureHDInsightJob)
                                              .WithParameter(CmdletConstants.Cluster, testCluster.ConnectionUrl)
                            .WithParameter(CmdletConstants.Credential, GetPSCredential(testCluster.HttpUserName, testCluster.HttpPassword))
                                              .WithParameter(CmdletConstants.Id, jobId)
                                              .Invoke();
                Assert.AreEqual(results.Results.Count, 0);
            }
        }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            }

        internal static AzureHDInsightJob GetJobWithID(IRunspace runspace, string jobId, ClusterDetails cluster)
        {
            IPipelineResult getJobDetailResults =
                   runspace.NewPipeline()
                           .AddCommand(CmdletConstants.GetAzureHDInsightJob)
                           .WithParameter(CmdletConstants.Cluster, cluster.ConnectionUrl)
                        .WithParameter(CmdletConstants.Credential, GetPSCredential(cluster.HttpUserName, cluster.HttpPassword))
                           .WithParameter(CmdletConstants.Id, jobId)
                           .Invoke();

            return getJobDetailResults.Results.ToEnumerable<AzureHDInsightJob>().FirstOrDefault();
        }
    }
}

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

using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;

namespace Microsoft.WindowsAzure.Commands.Test.HDInsight.CommandTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CmdLetTests;
    using Commands.Utilities.Common;
    using Hadoop.Client;
    using Management.HDInsight;
    using Management.HDInsight.Cmdlet.Commands.CommandImplementations;
    using Management.HDInsight.Cmdlet.DataObjects;
    using Management.HDInsight.Cmdlet.GetAzureHDInsightClusters;
    using Management.HDInsight.Cmdlet.ServiceLocation;
    using Utilities.HDInsight.Simulators;
    using Utilities.HDInsight.Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HDInsightGetCommandTests : HDInsightTestCaseBase
    {
        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void ICanPerform_GetClusters_HDInsightGetCommand()
        {
            IHDInsightCertificateCredential creds = GetValidCredentials();
            var client = ServiceLocator.Instance.Locate<IAzureHDInsightCommandFactory>().CreateGet();
            client.CurrentSubscription = GetCurrentSubscription();
            client.EndProcessing();
            IEnumerable<AzureHDInsightCluster> containers = from container in client.Output
                                                            where container.Name.Equals(TestCredentials.WellKnownCluster.DnsName)
                                                            select container;
            Assert.AreEqual(1, containers.Count());
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void ICanPerform_GetClusters_HDInsightGetCommand_DnsName()
        {
            IHDInsightCertificateCredential creds = GetValidCredentials();
            var client = ServiceLocator.Instance.Locate<IAzureHDInsightCommandFactory>().CreateGet();
            client.CurrentSubscription = GetCurrentSubscription();
            client.Name = TestCredentials.WellKnownCluster.DnsName;
            client.EndProcessing();
            IEnumerable<AzureHDInsightCluster> containers = from container in client.Output
                                                            where container.Name.Equals(TestCredentials.WellKnownCluster.DnsName)
                                                            select container;
            Assert.AreEqual(1, containers.Count());
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void ICanPerform_GetClusters_HDInsightGetCommand_InvalidDnsName()
        {
            IHDInsightCertificateCredential creds = GetValidCredentials();
            var client = ServiceLocator.Instance.Locate<IAzureHDInsightCommandFactory>().CreateGet();
            client.CurrentSubscription = GetCurrentSubscription();
            client.Name = Guid.NewGuid().ToString("N");
            client.EndProcessing();
            Assert.IsFalse(client.Output.Any());
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void CommandsNeedCurrentSubscriptionSet()
        {
            IHDInsightCertificateCredential creds = GetValidCredentials();
            var getClustersCommand = new GetAzureHDInsightClusterCommand();
            try
            {
                getClustersCommand.GetClient();
                Assert.Fail("Should have failed.");
            }
            catch (ArgumentNullException noSubscriptionException)
            {
                Assert.AreEqual(noSubscriptionException.ParamName, "CurrentSubscription");
            }
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void CanGetSubscriptionsCertificateCredentialFromCurrentSubscription()
        {
            var getClustersCommand = new GetAzureHDInsightClusterCommand();
            var waSubscription = GetCurrentSubscription();
            var subscriptionCreds = getClustersCommand.GetSubscriptionCredentials(waSubscription);

            Assert.IsInstanceOfType(subscriptionCreds, typeof(HDInsightCertificateCredential));
            var asCertificateCreds = subscriptionCreds as HDInsightCertificateCredential;
            Assert.AreEqual(waSubscription.SubscriptionId, asCertificateCreds.SubscriptionId.ToString());
            Assert.AreEqual(waSubscription.Certificate, asCertificateCreds.Certificate);
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void CanGetAccessTokenCertificateCredentialFromCurrentSubscription()
        {
            string accessToken = Guid.NewGuid().ToString("N");
            var getClustersCommand = new GetAzureHDInsightClusterCommand();
            var waSubscription = new WindowsAzureSubscription()
                {
                    SubscriptionId = IntegrationTestBase.TestCredentials.SubscriptionId.ToString(),
                    ActiveDirectoryUserId = "BruceWayne",
                    TokenProvider = new FakeAccessTokenProvider(accessToken)
                };
            var accessTokenCreds = getClustersCommand.GetSubscriptionCredentials(waSubscription);
            Assert.IsInstanceOfType(accessTokenCreds, typeof(HDInsightAccessTokenCredential));
            var asAccessTokenCreds = accessTokenCreds as HDInsightAccessTokenCredential;
            Assert.AreEqual(accessToken, asAccessTokenCreds.AccessToken);
            Assert.AreEqual(waSubscription.SubscriptionId, asAccessTokenCreds.SubscriptionId.ToString());
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void CanGetJobSubmissionCertificateCredentialFromCurrentSubscription()
        {
            var getClustersCommand = new GetAzureHDInsightJobCommand();
            var waSubscription = GetCurrentSubscription();
            var subscriptionCreds = getClustersCommand.GetJobSubmissionClientCredentials(waSubscription, IntegrationTestBase.TestCredentials.WellKnownCluster.DnsName);

            Assert.IsInstanceOfType(subscriptionCreds, typeof(JobSubmissionCertificateCredential));
            var asCertificateCreds = subscriptionCreds as JobSubmissionCertificateCredential;
            Assert.AreEqual(waSubscription.SubscriptionId, asCertificateCreds.SubscriptionId.ToString());
            Assert.AreEqual(waSubscription.Certificate, asCertificateCreds.Certificate);
            Assert.AreEqual(IntegrationTestBase.TestCredentials.WellKnownCluster.DnsName, asCertificateCreds.Cluster);
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void CanGetJobSubmissionAccessTokenCredentialFromCurrentSubscription()
        {
            string accessToken = Guid.NewGuid().ToString("N");
            var getClustersCommand = new GetAzureHDInsightJobCommand();
            var waSubscription = new WindowsAzureSubscription()
            {
                SubscriptionId = IntegrationTestBase.TestCredentials.SubscriptionId.ToString(),
                ActiveDirectoryUserId = "BruceWayne",
                TokenProvider = new FakeAccessTokenProvider(accessToken)
            };
            var accessTokenCreds = getClustersCommand.GetJobSubmissionClientCredentials(waSubscription, IntegrationTestBase.TestCredentials.WellKnownCluster.DnsName);
            Assert.IsInstanceOfType(accessTokenCreds, typeof(HDInsightAccessTokenCredential));
            var asTokenCreds = accessTokenCreds as HDInsightAccessTokenCredential;
            Assert.IsNotNull(asTokenCreds);
            Assert.AreEqual(accessToken, asTokenCreds.AccessToken);
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void CanGetBasicAuthCredentialFromCredentials()
        {
            string accessToken = Guid.NewGuid().ToString("N");
            var getClustersCommand = new GetAzureHDInsightJobCommand();
            getClustersCommand.Credential = GetPSCredential(TestCredentials.AzureUserName, TestCredentials.AzurePassword);
            var waSubscription = new WindowsAzureSubscription()
            {
                SubscriptionId = IntegrationTestBase.TestCredentials.SubscriptionId.ToString(),
                ActiveDirectoryUserId = "BruceWayne",
                TokenProvider = new FakeAccessTokenProvider(accessToken)
            };

            var accessTokenCreds = getClustersCommand.GetJobSubmissionClientCredentials(waSubscription, IntegrationTestBase.TestCredentials.WellKnownCluster.DnsName);
            Assert.IsInstanceOfType(accessTokenCreds, typeof(BasicAuthCredential));
            var asBasicAuthCredentials = accessTokenCreds as BasicAuthCredential;
            Assert.IsNotNull(asBasicAuthCredentials);
            Assert.AreEqual(IntegrationTestBase.TestCredentials.AzureUserName, asBasicAuthCredentials.UserName);
            Assert.AreEqual(IntegrationTestBase.TestCredentials.AzurePassword, asBasicAuthCredentials.Password);
        }

        [TestMethod]
        [TestCategory("CheckIn")]
        public void GetJobSubmissionCredentialsThrowsInvalidOperationException()
        {
            string invalidClusterName = Guid.NewGuid().ToString("N");
            var getClustersCommand = new GetAzureHDInsightJobCommand();

            try
            {
                getClustersCommand.GetClient(invalidClusterName);
                Assert.Fail("Should have failed.");
            }
            catch (InvalidOperationException invalidOperationException)
            {
                Assert.AreEqual("Expected either a Subscription or Credential parameter.", invalidOperationException.Message);
            }
        }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }
    }
}

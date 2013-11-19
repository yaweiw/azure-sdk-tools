// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License.  You may obtain a copy
// of the License at http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
// WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandImplementations;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Tests.Simulators;

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Tests.CmdletAbstractionTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandInterfaces;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.DataObjects;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.ServiceLocation;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Tests.Utilities;

    [TestClass]
    public class HDInsightGetCommandTests : IntegrationTestBase
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

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }
    }
}

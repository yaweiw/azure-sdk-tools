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

namespace Microsoft.WindowsAzure.Commands.Test.Subscription
{
    using System;
    using System.Linq;
    using Commands.Subscription;
    using Commands.Utilities.Common;
    using Moq;
    using Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ImportAzurePublishSettingsNewTests
    {
        private WindowsAzureProfile profile;
        private ImportAzurePublishSettingsNewCommand cmdlet;

        [TestInitialize]
        public void Setup()
        {
            profile = new WindowsAzureProfile(new Mock<IProfileStore>().Object);
            cmdlet = new ImportAzurePublishSettingsNewCommand
            {
                Profile = profile,
                CommandRuntime = new MockCommandRuntime()
            };
        }


        [TestMethod]
        public void CanImportValidPublishSettings()
        {
            cmdlet.PublishSettingsFile = Data.ValidPublishSettings.First();

            cmdlet.ExecuteCmdlet();

            Assert.AreEqual(Data.Subscription1, profile.CurrentSubscription.Name);
            Assert.IsTrue(profile.CurrentSubscription.IsDefault);
        }

        [TestMethod]
        public void CanImportPublishSettingsV2File()
        {
            cmdlet.PublishSettingsFile = Data.ValidPublishSettings2.First();

            cmdlet.ExecuteCmdlet();

            Assert.AreEqual(Data.SampleSubscription1, profile.CurrentSubscription.Name);
            Assert.AreEqual(new Uri("https://newmanagement.core.windows.net/"),
                profile.CurrentSubscription.ManagementEndpoint);
            Assert.IsNotNull(profile.CurrentSubscription.Certificate);
            Assert.IsTrue(profile.CurrentSubscription.IsDefault);
        }

        [TestMethod]
        public void MultipleImportDoesntAffectExplicitlySetCurrentSubscription()
        {
            cmdlet.PublishSettingsFile = Data.ValidPublishSettings.First();
            cmdlet.ExecuteCmdlet();

            var currentSubscription = profile.CurrentSubscription;

            Assert.AreEqual(Data.Subscription1, currentSubscription.Name);
            Assert.IsTrue(currentSubscription.IsDefault);

            var newCurrentSubscription =
                profile.Subscriptions.FirstOrDefault(s => !s.SubscriptionId.Equals(currentSubscription.SubscriptionId));
            profile.CurrentSubscription = newCurrentSubscription;
            var newSubscriptionId = newCurrentSubscription.SubscriptionId;

            cmdlet.PublishSettingsFile = Data.ValidPublishSettings.First();
            cmdlet.ExecuteCmdlet();

            Assert.AreEqual(profile.CurrentSubscription.SubscriptionId, newSubscriptionId);
            Assert.IsTrue(profile.Subscriptions.Contains(profile.CurrentSubscription));
        }
    }
}
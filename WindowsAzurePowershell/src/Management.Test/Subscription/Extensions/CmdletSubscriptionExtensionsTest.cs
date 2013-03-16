namespace Microsoft.WindowsAzure.Management.Test.Tests.Extensions
{
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common.Extensions;
    using Stubs;
    using TestData;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CmdletSubscriptionExtensionsTest
    {
        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();

            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureAppDir;

            if (Directory.Exists(Data.AzureAppDir))
            {
                Directory.Delete(Data.AzureAppDir, true);
            }
        }

        [TestMethod]
        public void TestGetSubscriptions()
        {
            var globalComponents = GlobalComponents.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var cmdletStub = new CmdletStub();
            var subscriptions = cmdletStub.GetSubscriptions(null);

            // All subscriptions from both the publish settings file and the subscriptions file were imported
            Assert.AreEqual(3, subscriptions.Count);

            // There's a single default subscription
            Assert.AreEqual("Windows Azure Sandbox 9-220", subscriptions.Values.Single(subscription => subscription.IsDefault).SubscriptionName);

            globalComponents.DeleteGlobalComponents();
        }

        [TestMethod]
        public void TestGetCurrentSubscription()
        {
            var globalComponents = GlobalComponents.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var cmdletStub = new CmdletStub();
            var subscriptions = cmdletStub.GetSubscriptions(null);

            var currentSubscription = subscriptions.Values.First();
            cmdletStub.SetCurrentSubscription(currentSubscription.SubscriptionName, null);

            // Test
            var actualCurrentSubscription = cmdletStub.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionName, actualCurrentSubscription.SubscriptionName);
            Assert.AreEqual(currentSubscription.SubscriptionId, actualCurrentSubscription.SubscriptionId);

            globalComponents.DeleteGlobalComponents();
        }

        [TestMethod]
        public void TestSetDefaultSubscription()
        {
            var globalComponents = GlobalComponents.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var newPath = Path.Combine(GlobalPathInfo.GlobalSettingsDirectory, "test.xml");
            File.Copy(Data.ValidSubscriptionsData[0], newPath, true);

            var cmdletStub = new CmdletStub();
            var subscriptions = cmdletStub.GetSubscriptions(Data.ValidSubscriptionsData[0]);

            var newDefaultSubscription = subscriptions.Values.First(subscription => !subscription.IsDefault);
            cmdletStub.SetDefaultSubscription(newDefaultSubscription.SubscriptionName, newPath);

            // Test - reimport and make sure the current subscription after import is the correct one
            var subscriptionsManager = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory, newPath).SubscriptionManager;
            var defaultSubscription = subscriptionsManager.Subscriptions.Values.First(subscription => subscription.IsDefault);
            Assert.AreEqual(newDefaultSubscription.SubscriptionName, defaultSubscription.SubscriptionName);

            globalComponents.DeleteGlobalComponents();
        }

        [TestMethod]
        public void TestUpdateSubscriptions()
        {
            var globalComponents = GlobalComponents.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var cmdletStub = new CmdletStub();
            var subscriptions = cmdletStub.GetSubscriptions(Data.ValidSubscriptionsData[0]);

            var deleteSubscriptionKey = subscriptions.Keys.First();
            subscriptions.Remove(deleteSubscriptionKey);

            var newPath = Path.Combine(GlobalPathInfo.GlobalSettingsDirectory, "test.xml");
            cmdletStub.UpdateSubscriptions(subscriptions, newPath);

            var newSubscriptions = cmdletStub.GetSubscriptions(newPath);
            Assert.IsFalse(newSubscriptions.ContainsKey(deleteSubscriptionKey));

            globalComponents.DeleteGlobalComponents();
        }

        [TestMethod]
        public void TestGetSubscription()
        {
            var globalComponents = GlobalComponents.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var cmdletStub = new CmdletStub();
            var subscription = cmdletStub.GetSubscription("TestSubscription1", null);

            Assert.AreEqual("TestSubscription1", subscription.SubscriptionName);

            globalComponents.DeleteGlobalComponents();
        }
    }

    public class CmdletStub : PSCmdlet
    {
    }
}

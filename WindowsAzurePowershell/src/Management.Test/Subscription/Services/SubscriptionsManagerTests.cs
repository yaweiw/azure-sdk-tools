namespace Microsoft.WindowsAzure.Management.Test.Tests.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using Management.Utilities;
    using TestData;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common.XmlSchema;

    [TestClass]
    public class SubscriptionsManagerTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureAppDir;
            Directory.CreateDirectory(GlobalPathInfo.GlobalSettingsDirectory);
        }

        [TestMethod]
        public void TestImportSubscriptions()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                var publishSettings = General.DeserializeXmlFile<PublishData>(Data.ValidPublishSettings[i]);
                var subscriptionsManager = SubscriptionsManager.Import(
                    Data.ValidSubscriptionsData[i],
                    publishSettings);

                // All subscriptions from both the publish settings file and the subscriptions file were imported
                Assert.AreEqual(5, subscriptionsManager.Subscriptions.Count);
                Assert.IsTrue(Data.ValidSubscriptionName.SequenceEqual(subscriptionsManager.Subscriptions.Keys));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestImportSubscriptionsInvalidSubscriptionData()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                try
                {
                    var publishSettings = General.DeserializeXmlFile<PublishData>(Data.ValidPublishSettings[i]);
                    SubscriptionsManager.Import(
                        Data.InvalidSubscriptionsData[i],
                        publishSettings);
                }
                catch (InvalidOperationException exception)
                {
                    Assert.AreEqual(
                        string.Format(Resources.InvalidSubscriptionsDataSchema, Data.InvalidSubscriptionsData[i]),
                        exception.Message);
                    throw;
                }
            }
        }

        [TestMethod]
        public void TestSaveSubscriptions()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                var globalComponents = GlobalComponents.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings[i]);
                
                var subscriptionsManager = SubscriptionsManager.Import(
                    Data.ValidSubscriptionsData[i],
                    globalComponents.PublishSettings,
                    globalComponents.Certificate);

                var newSubscription = new SubscriptionData
                {
                    SubscriptionName = "newsubscription",
                    IsDefault = false,
                    SubscriptionId = "id"
                };

                subscriptionsManager.Subscriptions[newSubscription.SubscriptionName] = newSubscription;
                subscriptionsManager.SaveSubscriptions(Path.Combine(GlobalPathInfo.GlobalSettingsDirectory, "test.xml"));

                var newSubscriptionsManager = SubscriptionsManager.Import(
                    Path.Combine(GlobalPathInfo.GlobalSettingsDirectory, "test.xml"),
                    globalComponents.PublishSettings,
                    globalComponents.Certificate);

                var addedSubscription = newSubscriptionsManager.Subscriptions.Values.Single(
                    subscription => subscription.SubscriptionName == newSubscription.SubscriptionName);

                Assert.AreEqual(newSubscription.SubscriptionId, addedSubscription.SubscriptionId);

                globalComponents.DeleteGlobalComponents();
            }
        }
    }
}

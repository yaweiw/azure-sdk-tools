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

namespace Microsoft.WindowsAzure.Management.Test.Common
{
    using System;
    using System.IO;
    using System.Linq;
    using Management.Cmdlets;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using Management.Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common.XmlSchema;

    [TestClass]
    public class GlobalComponentsTests
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
        public void GlobalComponentsLoadExisting()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                var publishSettingsFile = Data.ValidPublishSettings[i];

                // Prepare
                new ImportAzurePublishSettingsCommand().ImportSubscriptionFile(publishSettingsFile, null);
                GlobalComponents globalComponents = GlobalComponents.Load(Data.AzureAppDir);
                PublishData actualPublishSettings = General.DeserializeXmlFile<PublishData>(Path.Combine(Data.AzureAppDir, Resources.PublishSettingsFileName));
                PublishData expectedPublishSettings = General.DeserializeXmlFile<PublishData>(publishSettingsFile);

                // Assert
                AzureAssert.AreEqualGlobalComponents(new GlobalPathInfo(Data.AzureAppDir), expectedPublishSettings, globalComponents);
                
                // Clean
                globalComponents.DeleteGlobalComponents();
            }
        }

        [TestMethod]
        public void GlobalComponentsLoadIgnoresPublishExisting()
        {
            var publishSettingsFile = Data.ValidPublishSettings.First();
            var subscriptionDataFile = Data.ValidSubscriptionsData.First();
            var outputSubscriptionDataFile = Path.Combine(Directory.GetParent(subscriptionDataFile).FullName, "outputNoPublish.xml");
            File.Copy(subscriptionDataFile, outputSubscriptionDataFile);

            // Create with both an existing ouput subscription data file and the publish settings file
            GlobalComponents globalComponents = GlobalComponents.CreateFromPublishSettings(Data.AzureAppDir, outputSubscriptionDataFile, publishSettingsFile);
            Assert.AreEqual(5, globalComponents.Subscriptions.Count);

            // Remove one of the subscriptions from the publish settings file
            globalComponents.Subscriptions.Remove("TestSubscription1");
            globalComponents.SaveSubscriptions();

            // Load and make sure the subscription is still gone although it still is in the publish settings file
            globalComponents = GlobalComponents.Load(Data.AzureAppDir, outputSubscriptionDataFile);
            Assert.AreEqual(4, globalComponents.Subscriptions.Count);

            // Clean
            globalComponents.DeleteGlobalComponents();
        }

        [TestMethod]
        public void GlobalComponentsCreateNew()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                // Prepare
                GlobalComponents globalComponents = GlobalComponents.CreateFromPublishSettings(Data.AzureAppDir, null, fileName);
                PublishData expectedPublishSettings = General.DeserializeXmlFile<PublishData>(fileName);

                // Assert
                AzureAssert.AreEqualGlobalComponents(new GlobalPathInfo(Data.AzureAppDir), expectedPublishSettings, globalComponents);

                // Clean
                globalComponents.DeleteGlobalComponents();
            }
        }

        [TestMethod]
        public void GlobalComponentsCreateNewEmptyAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidSubscriptionsData)
            {
                try
                {
                    GlobalComponents.Load(string.Empty, fileName);
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is ArgumentException);
                    Assert.AreEqual<string>(ex.Message, string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.AzureDirectoryName));
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalComponentsCreateNewNullAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidSubscriptionsData)
            {
                try
                {
                    GlobalComponents.Load(null, fileName);
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is ArgumentException);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalComponentsCreateNewInvalidAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                foreach (string invalidDirectoryName in Data.InvalidServiceRootName)
                {
                    try
                    {
                        GlobalComponents.Load(invalidDirectoryName, fileName);
                        Assert.Fail("No exception thrown");
                    }
                    catch (Exception ex)
                    {
                        Assert.IsTrue(ex is ArgumentException);
                        Assert.AreEqual<string>(ex.Message, "Illegal characters in path.");
                        Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                    }
                }
            }
        }

        [TestMethod]
        public void GlobalComponentsCreateNewInvalidPublishSettingsSchemaFail()
        {
            foreach (string fileName in Data.InvalidPublishSettings)
            {
                try
                {
                    GlobalComponents.CreateFromPublishSettings(Data.AzureAppDir, null, fileName);
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is InvalidOperationException);
                    Assert.AreEqual<string>(ex.Message, string.Format(Resources.InvalidPublishSettingsSchema, fileName));
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalComponentsLoadExistingEmptyAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                try
                {
                    GlobalComponents.Load("fake");
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is FileNotFoundException);
                    Assert.AreEqual<string>(ex.Message, Resources.GlobalComponents_Load_PublishSettingsNotFound);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalComponentsLoadExistingNullAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                try
                {
                    GlobalComponents.Load(null);
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is ArgumentException);
                    Assert.AreEqual<string>("Value cannot be null. Parameter name: 'azurePath'", ex.Message);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalComponentsLoadExistingInvalidDirectoryNameAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                foreach (string invalidDirectoryName in Data.InvalidServiceRootName)
                {
                    try
                    {
                        GlobalComponents.Load(invalidDirectoryName);
                        Assert.Fail("No exception thrown");
                    }
                    catch (Exception ex)
                    {
                        Assert.IsTrue(ex is ArgumentException);
                        Assert.AreEqual<string>(ex.Message, "Illegal characters in path.");
                        Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                    }
                }
            }
        }

        [TestMethod]
        public void GlobalComponentsLoadDoesNotExistAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                foreach (string invalidDirectoryName in Data.InvalidServiceRootName)
                {
                    try
                    {
                        GlobalComponents.Load("DoesNotExistDirectory");
                        Assert.Fail("No exception thrown");
                    }
                    catch (Exception ex)
                    {
                        Assert.IsTrue(ex is FileNotFoundException);
                        Assert.AreEqual<string>(ex.Message, Resources.GlobalComponents_Load_PublishSettingsNotFound);
                        Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                    }
                }
            }
        }

        [TestMethod]
        public void GlobalComponentsCreateNewEmptyPublishSettingsFileFail()
        {
            try
            {
                GlobalComponents.CreateFromPublishSettings(Data.AzureAppDir, null, string.Empty);
                Assert.Fail("No exception thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException);
                Assert.AreEqual<string>(ex.Message, string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.PublishSettings));
                Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
            }
        }

        [TestMethod]
        public void GlobalComponentsCreateNewNullPublishSettingsFileFail()
        {
            try
            {
                GlobalComponents.CreateFromPublishSettings(Data.AzureAppDir, null, null);
                Assert.Fail("No exception thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException);
                Assert.AreEqual<string>(ex.Message, string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.PublishSettings));
                Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
            }
        }

        [TestMethod]
        public void GlobalComponentsCreateNewInvalidPublishSettingsFileFail()
        {
            foreach (string invalidFileName in Data.InvalidFileName)
            {
                Action<ArgumentException> verification = ex =>
                {
                    Assert.AreEqual<string>(ex.Message, Resources.IllegalPath);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                };

                Testing.AssertThrows<ArgumentException>(() => GlobalComponents.CreateFromPublishSettings(Data.AzureAppDir, null, invalidFileName), verification);
            }
        }

        [TestMethod]
        public void GlobalComponentsLoadInvalidPublishSettingsSchemaFail()
        {
            Testing.AssertThrows<FileNotFoundException>(
                () => GlobalComponents.Load("DoesNotExistDirectory"),
                ex =>
                {
                    Assert.AreEqual<string>(ex.Message, Resources.GlobalComponents_Load_PublishSettingsNotFound);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                });
        }
    }
}
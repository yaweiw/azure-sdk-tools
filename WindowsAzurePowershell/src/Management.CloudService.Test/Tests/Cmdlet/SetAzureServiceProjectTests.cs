// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using CloudService.Cmdlet;
    using CloudService.Model;
    using CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;
    using TestData;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SetAzureServiceProjectTests : TestBase
    {
        FakeWriter writer;
        SetAzureServiceProjectCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            writer = new FakeWriter();
            cmdlet = new SetAzureServiceProjectCommand();
            cmdlet.Writer = writer;
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsSubscriptionValid()
        {
            foreach (string item in Data.ValidSubscriptionNames)
            {
                using (FileSystemHelper files = new FileSystemHelper(this))
                {
                    // Create new empty settings file
                    //
                    ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                    ServiceSettings settings = new ServiceSettings();
                    writer = new FakeWriter();
                    cmdlet.Writer = writer;
                    settings.Save(paths.Settings);

                    settings = cmdlet.SetAzureServiceProjectProcess(null, null, null, item, paths.Settings);

                    // Assert subscription is changed
                    //
                    Assert.AreEqual<string>(item, settings.Subscription);
                    ServiceSettings actualOutput = writer.OutputChannel[0] as ServiceSettings;
                    Assert.AreEqual<string>(item, actualOutput.Subscription);
                }
            }
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsSubscriptionEmptyFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                // Create new empty settings file
                //
                ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                ServiceSettings settings = new ServiceSettings();
                settings.Save(paths.Settings);

                Testing.AssertThrows<ArgumentException>(() => cmdlet.SetAzureServiceProjectProcess(null, null, null, string.Empty, paths.Settings), string.Format(Resources.InvalidOrEmptyArgumentMessage, "Subscription"));
            }
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsLocationValid()
        {
            foreach (KeyValuePair<Location, string> item in Microsoft.WindowsAzure.Management.CloudService.Model.ArgumentConstants.Locations)
            {
                using (FileSystemHelper files = new FileSystemHelper(this))
                {
                    // Create new empty settings file
                    //
                    ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                    ServiceSettings settings = new ServiceSettings();
                    writer = new FakeWriter();
                    cmdlet.Writer = writer;
                    settings.Save(paths.Settings);

                    settings = cmdlet.SetAzureServiceProjectProcess(item.Value, null, null, null, paths.Settings);

                    // Assert location is changed
                    //
                    Assert.AreEqual<string>(item.Value, settings.Location);
                    ServiceSettings actualOutput = writer.OutputChannel[0] as ServiceSettings;
                    Assert.AreEqual<string>(item.Value, settings.Location);
                }
            }
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsLocationEmptyFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                // Create new empty settings file
                //
                ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                ServiceSettings settings = new ServiceSettings();
                settings.Save(paths.Settings);

                Testing.AssertThrows<ArgumentException>(() => cmdlet.SetAzureServiceProjectProcess(string.Empty, null, null, null, paths.Settings), string.Format(Resources.InvalidOrEmptyArgumentMessage, "Location"));
            }
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsLocationInvalidFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                // Create new empty settings file
                //
                ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                ServiceSettings settings = new ServiceSettings();
                settings.Save(paths.Settings);

                Testing.AssertThrows<ArgumentException>(() => cmdlet.SetAzureServiceProjectProcess("MyHome", null, null, null, paths.Settings), string.Format(Resources.InvalidServiceSettingElement, "Location"));
            }
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsStorageTests()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                // Create new empty settings file
                //
                ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                ServiceSettings settings = new ServiceSettings();
                writer = new FakeWriter();
                cmdlet.Writer = writer;
                settings.Save(paths.Settings);

                settings = cmdlet.SetAzureServiceProjectProcess(null, null, "companystore", null, paths.Settings);

                // Assert storageAccountName is changed
                //
                Assert.AreEqual<string>("companystore", settings.StorageAccountName);
                ServiceSettings actualOutput = writer.OutputChannel[0] as ServiceSettings;
                Assert.AreEqual<string>("companystore", settings.StorageAccountName);
            }
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsStorageTestsEmptyFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                // Create new empty settings file
                //
                ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                ServiceSettings settings = new ServiceSettings();
                settings.Save(paths.Settings);

                Testing.AssertThrows<ArgumentException>(() => cmdlet.SetAzureServiceProjectProcess(null, null, string.Empty, null, paths.Settings), string.Format(Resources.InvalidOrEmptyArgumentMessage, "StorageAccountName"));
            }
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsSlotTests()
        {
            foreach (KeyValuePair<Slot, string> item in Microsoft.WindowsAzure.Management.CloudService.Model.ArgumentConstants.Slots)
            {
                using (FileSystemHelper files = new FileSystemHelper(this))
                {
                    // Create new empty settings file
                    //
                    ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                    ServiceSettings settings = new ServiceSettings();
                    settings.Save(paths.Settings);

                    cmdlet.SetAzureServiceProjectProcess(null, item.Value, null, null, paths.Settings);

                    // Assert slot is changed
                    //
                    settings = ServiceSettings.Load(paths.Settings);
                    Assert.AreEqual<string>(item.Value, settings.Slot);
                }
            }
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsSlotTestsEmptyFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                // Create new empty settings file
                //
                ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                ServiceSettings settings = new ServiceSettings();
                settings.Save(paths.Settings);

                Testing.AssertThrows<ArgumentException>(() => cmdlet.SetAzureServiceProjectProcess(null, string.Empty, null, null, paths.Settings), string.Format(Resources.InvalidOrEmptyArgumentMessage, "Slot"));
            }
        }

        [TestMethod]
        public void SetAzureServiceProjectTestsSlotTestsInvalidFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                // Create new empty settings file
                //
                ServicePathInfo paths = new ServicePathInfo(files.RootPath);
                ServiceSettings settings = new ServiceSettings();
                settings.Save(paths.Settings);

                Testing.AssertThrows<ArgumentException>(() => cmdlet.SetAzureServiceProjectProcess(null, "MyHome", null, null, paths.Settings), string.Format(Resources.InvalidServiceSettingElement, "Slot"));
            }
        }
    }
}

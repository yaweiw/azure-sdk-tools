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

namespace Microsoft.WindowsAzure.Commands.Test.Environment
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Commands.Subscription;
    using Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RemoveAzureEnvironmentTests : TestBase
    {
        private FileSystemHelper helper;

        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            helper = new FileSystemHelper(this);
            helper.CreateAzureSdkDirectoryAndImportPublishSettings();
        }

        [TestCleanup]
        public void Cleanup()
        {
            helper.Dispose();
        }

        [TestMethod]
        public void RemovesAzureEnvironment()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            string name = "test";
            GlobalSettingsManager.Instance.AddEnvironment(name, "test url");
            RemoveAzureEnvironmentCommand cmdlet = new RemoveAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                Name = name
            };

            cmdlet.ExecuteCmdlet();

            Testing.AssertThrows<KeyNotFoundException>(
                () => GlobalSettingsManager.Instance.GetEnvironment(name),
                string.Format(Resources.EnvironmentNotFound, name));
        }

        [TestMethod]
        public void ThrowsForUnknownEnvironment()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();
            RemoveAzureEnvironmentCommand cmdlet = new RemoveAzureEnvironmentCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                Name = "test2"
            };

            Testing.AssertThrows<KeyNotFoundException>(
                () => cmdlet.ExecuteCmdlet(),
                string.Format(Resources.EnvironmentNotFound, "test2"));
        }

        [TestMethod]
        public void ThrowsForPublicEnvironment()
        {
            Mock<ICommandRuntime> commandRuntimeMock = new Mock<ICommandRuntime>();

            foreach (string name in WindowsAzureEnvironment.PublicEnvironments.Keys)
            {
                RemoveAzureEnvironmentCommand cmdlet = new RemoveAzureEnvironmentCommand()
                {
                    CommandRuntime = commandRuntimeMock.Object,
                    Name = name
                };

                Testing.AssertThrows<InvalidOperationException>(
                    () => cmdlet.ExecuteCmdlet(),
                    string.Format(Resources.ChangePublicEnvironmentMessage, name));
            }
        }
    }
}
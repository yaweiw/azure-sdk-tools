﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.Storage.Test.Common.Cmdlet
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Storage.Common.Cmdlet;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [TestClass]
    public class SetAzureStorageServicePropertiesTest : StorageTestBase
    {
        /// <summary>
        /// StorageCmdletBase command
        /// </summary>
        public SetAzureStorageServiceProperties command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new SetAzureStorageServiceProperties
            {
                CommandRuntime = new MockCommandRuntime()
            };
        }

        [TestCleanup]
        public void CleanCommand()
        {
            command = null;
        }

        [TestMethod]
        public void GetLoggingOperationsTest()
        { 
            Assert.AreEqual(LoggingOperations.None, command.GetLoggingOperations("none"));
            Assert.AreEqual(LoggingOperations.All, command.GetLoggingOperations("all"));
            Assert.AreEqual(LoggingOperations.Read, command.GetLoggingOperations("Read"));
            Assert.AreEqual(LoggingOperations.Write, command.GetLoggingOperations("WrIte"));
            Assert.AreEqual(LoggingOperations.Delete, command.GetLoggingOperations("DELETE"));
            Assert.AreEqual(LoggingOperations.Read | LoggingOperations.Delete,
                command.GetLoggingOperations("Read, DELETE"));
            AssertThrows<ArgumentException>(() => command.GetLoggingOperations("DELETE,xxx"));
            AssertThrows<ArgumentException>(() => command.GetLoggingOperations("DELETE,all"));
            AssertThrows<ArgumentException>(() => command.GetLoggingOperations("DELETE,none"));
            AssertThrows<ArgumentException>(() => command.GetLoggingOperations("all,none"));
            AssertThrows<ArgumentException>(() => command.GetLoggingOperations("allnone"));
            AssertThrows<ArgumentException>(() => command.GetLoggingOperations("stdio"));
        }

        [TestMethod]
        public void GetMetricsLevelTest()
        {
            Assert.AreEqual(MetricsLevel.None, command.GetMetricsLevel("none"));
            Assert.AreEqual(MetricsLevel.Service, command.GetMetricsLevel("Service"));
            Assert.AreEqual(MetricsLevel.ServiceAndApi, command.GetMetricsLevel("ServiceAndApi"));
            AssertThrows<ArgumentException>(() => command.GetMetricsLevel("stdio"));
        }

        [TestMethod]
        public void SetValidLoggingPropertiesTest()
        {
            ServiceProperties properties = new ServiceProperties();
            properties.Logging.LoggingOperations = LoggingOperations.Read;
            Assert.AreEqual(properties.Logging.RetentionDays, null);
            Assert.AreEqual(properties.Logging.Version, null);
            command.SetValidLoggingProperties(properties);
            Assert.AreEqual(properties.Logging.RetentionDays, 1);
            Assert.AreEqual(properties.Logging.Version, "1.0");
            properties.Logging.RetentionDays = 10;
            properties.Logging.Version = "2.0";
            command.SetValidLoggingProperties(properties);
            Assert.AreEqual(properties.Logging.RetentionDays, 10);
            Assert.AreEqual(properties.Logging.Version, "2.0");
        }

        [TestMethod]
        public void SetValidMetricsPropertiesTest()
        {
            ServiceProperties properties = new ServiceProperties();
            properties.Metrics.MetricsLevel = MetricsLevel.ServiceAndApi;
            Assert.AreEqual(properties.Metrics.RetentionDays, null);
            Assert.AreEqual(properties.Metrics.Version, null);
            command.SetValidMetricsProperties(properties);
            Assert.AreEqual(properties.Metrics.RetentionDays, 1);
            Assert.AreEqual(properties.Metrics.Version, "1.0");
            properties.Metrics.RetentionDays = 10;
            properties.Metrics.Version = "2.0";
            command.SetValidMetricsProperties(properties);
            Assert.AreEqual(properties.Metrics.RetentionDays, 10);
            Assert.AreEqual(properties.Metrics.Version, "2.0");
        }
    }
}

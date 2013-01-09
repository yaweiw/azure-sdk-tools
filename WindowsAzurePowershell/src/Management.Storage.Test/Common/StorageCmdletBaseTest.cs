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

namespace Microsoft.WindowsAzure.Management.Storage.Test.Common
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// unit test for StorageCmdletBase
    /// </summary>
    [TestClass]
    public class StorageCmdletBaseTest : StorageTestBase
    {
        /// <summary>
        /// StorageCmdletBase command
        /// </summary>
        public StorageCmdletBase command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new StorageCmdletBase
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
        public void InitOperationContextTest()
        {
            command.InitOperationContext();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).VerboseChannel.Count());
            string log = (string)((MockCommandRuntime)command.CommandRuntime).VerboseChannel.FirstOrDefault();
            Assert.IsTrue(log.Length > 0);
        }

        [TestMethod]
        public void WriteVerboseLogTest()
        {
            string log = "WriteVerboseLogTest";
            command.WriteVerboseLog(log);
            string verboseLog = (string)((MockCommandRuntime)command.CommandRuntime).VerboseChannel.FirstOrDefault();
            Assert.IsTrue(verboseLog.EndsWith(log));
        }
    }
}

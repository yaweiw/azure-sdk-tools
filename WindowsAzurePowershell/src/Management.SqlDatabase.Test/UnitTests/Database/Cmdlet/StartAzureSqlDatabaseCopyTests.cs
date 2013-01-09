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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.Database.Cmdlet
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.CloudService.Test;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.MockServer;

    [TestClass]
    public class StartAzureSqlDatabaseCopyTests : TestBase
    {
        [TestCleanup]
        public void CleanupTest()
        {
            DatabaseTestHelper.SaveDefaultSessionCollection();
        }

        [TestMethod]
        public void StartAzureSqlDatabaseContinuousCopyWithSqlAuth()
        {
            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                // Create a context
                NewAzureSqlDatabaseServerContextTests.CreateServerContextSqlAuth(
                    powershell,
                    "$context");
                // Create 2 test databases
                NewAzureSqlDatabaseTests.CreateTestDatabasesWithSqlAuth(
                    powershell,
                    "$context");
                // Start a continuous database copy operation
                StartAzureSqlDatabaseCopyTests.StartDatabaseContinuousCopyWithSqlAuth(
                    powershell,
                    "$context",
                    "$copy",
                    "testdb1");
            }
        }

        /// <summary>
        /// Create continuous copy of testdb1 to partnersrv.
        /// </summary>
        /// <param name="powershell">The powershell instance containing the context.</param>
        /// <param name="contextVariable">The variable name that holds the server context.</param>
        /// <param name="copyVariable">The variable name that holds the copy object.</param>
        /// <param name="databaseName">The name of the database to copy.</param>
        public static void StartDatabaseContinuousCopyWithSqlAuth(
            System.Management.Automation.PowerShell powershell,
            string contextVariable,
            string copyVariable,
            string databaseName)
        {
            HttpSession testSession = DatabaseTestHelper.DefaultSessionCollection.GetSession(
                "UnitTest.Common.StartAzureSqlDatabaseContinuousCopyWithSqlAuth." + databaseName);
            DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
            testSession.RequestValidator =
                new Action<HttpMessage, HttpMessage.Request>(
                (expected, actual) =>
                {
                    Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                    Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                    switch (expected.Index)
                    {
                        // Request 0-1: Start database copy request
                        case 0:
                        case 1:
                            DatabaseTestHelper.ValidateHeadersForODataRequest(
                                expected.RequestInfo,
                                actual);
                            break;
                        default:
                            Assert.Fail("No more requests expected.");
                            break;
                    }
                });

            using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
            {
                Collection<PSObject> databaseCopy;
                using (new MockHttpServer(
                    exceptionManager,
                    MockHttpServer.DefaultServerPrefixUri,
                    testSession))
                {
                    databaseCopy = powershell.InvokeBatchScript(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            @"{0} = Start-AzureSqlDatabaseCopy " +
                            @"-Context $context " +
                            @"-DatabaseName {1} " +
                            @"-PartnerServer partnersrv " +
                            @"-ContinuousCopy",
                            copyVariable,
                            databaseName),
                        copyVariable);
                }

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();

                Assert.IsTrue(
                    databaseCopy.Single().BaseObject is Services.Server.DatabaseCopy,
                    "Expecting a Database Copy object");
                Services.Server.DatabaseCopy databaseCopyObj =
                    (Services.Server.DatabaseCopy)databaseCopy.Single().BaseObject;
                Assert.AreEqual(
                    "testserver",
                    databaseCopyObj.SourceServerName,
                    "Unexpected source server name");
                Assert.AreEqual(
                    databaseName,
                    databaseCopyObj.SourceDatabaseName,
                    "Unexpected source database name");
                Assert.AreEqual(
                    "partnersrv",
                    databaseCopyObj.DestinationServerName,
                    "Unexpected destination server name");
                Assert.AreEqual(
                    databaseName,
                    databaseCopyObj.DestinationDatabaseName,
                    "Unexpected destination database name");
                Assert.IsTrue(
                    databaseCopyObj.IsContinuous,
                    "Expected copy to be continuous");
            }
        }
    }
}

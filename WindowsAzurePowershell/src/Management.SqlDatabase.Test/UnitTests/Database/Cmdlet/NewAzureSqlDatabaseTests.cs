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
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.MockServer;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;

    [TestClass]
    public class NewAzureSqlDatabaseTests : TestBase
    {
        [TestCleanup]
        public void CleanupTest()
        {
            DatabaseTestHelper.SaveDefaultSessionCollection();
        }

        [TestMethod]
        public void NewAzureSqlDatabaseWithSqlAuth()
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
            }
        }

        [TestMethod]
        public void NewAzureSqlDatabaseWithSqlAuthDuplicateName()
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

                // Issue another create testdb1, causing a failure
                HttpSession testSession = DatabaseTestHelper.DefaultSessionCollection.GetSession(
                    "UnitTest.NewAzureSqlDatabaseWithSqlAuthDuplicateName");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                        switch (expected.Index)
                        {
                            // Request 0-1: Create testdb1
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
                    Services.Server.ServerDataServiceSqlAuth context;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        Collection<PSObject> ctxPsObject = powershell.InvokeBatchScript("$context");
                        context =
                            (Services.Server.ServerDataServiceSqlAuth)ctxPsObject.First().BaseObject;
                        powershell.InvokeBatchScript(
                            @"New-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1 " +
                            @"-Force");
                    }
                }

                Assert.AreEqual(1, powershell.Streams.Error.Count, "Expecting errors");
                Assert.AreEqual(2, powershell.Streams.Warning.Count, "Expecting tracing IDs");
                Assert.AreEqual(
                    "Database 'testdb1' already exists. Choose a different database name.",
                    powershell.Streams.Error.First().Exception.Message,
                    "Unexpected error message");
                Assert.IsTrue(
                    powershell.Streams.Warning.Any(w => w.Message.StartsWith("Client Session Id")),
                    "Expecting Client Session Id");
                Assert.IsTrue(
                    powershell.Streams.Warning.Any(w => w.Message.StartsWith("Client Request Id")),
                    "Expecting Client Request Id");
                powershell.Streams.ClearStreams();
            }
        }

        /// <summary>
        /// Create $testdb1 and $testdb2 on the given context.
        /// </summary>
        /// <param name="powershell">The powershell instance containing the context.</param>
        /// <param name="contextVariable">The variable name that holds the server context.</param>
        public static void CreateTestDatabasesWithSqlAuth(
            System.Management.Automation.PowerShell powershell,
            string contextVariable)
        {
            HttpSession testSession = DatabaseTestHelper.DefaultSessionCollection.GetSession(
                "UnitTest.Common.CreateTestDatabasesWithSqlAuth");
            DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
            testSession.RequestValidator =
                new Action<HttpMessage, HttpMessage.Request>(
                (expected, actual) =>
                {
                    Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                    Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                    switch (expected.Index)
                    {
                        // Request 0-1: Create testdb1
                        // Request 2-3: Create testdb2
                        case 0:
                        case 1:
                        case 2:
                        case 3:
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
                Collection<PSObject> database1, database2;
                using (new MockHttpServer(
                    exceptionManager,
                    MockHttpServer.DefaultServerPrefixUri,
                    testSession))
                {
                    database1 = powershell.InvokeBatchScript(
                        @"$testdb1 = New-AzureSqlDatabase " +
                        @"-Context $context " +
                        @"-DatabaseName testdb1 " +
                        @"-Force",
                        @"$testdb1");
                    database2 = powershell.InvokeBatchScript(
                        @"$testdb2 = New-AzureSqlDatabase " +
                        @"-Context $context " +
                        @"-DatabaseName testdb2 " +
                        @"-Collation Japanese_CI_AS " +
                        @"-Edition Web " +
                        @"-MaxSizeGB 5 " +
                        @"-Force",
                        @"$testdb2");
                }

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();

                Assert.IsTrue(
                    database1.Single().BaseObject is Services.Server.Database,
                    "Expecting a Database object");
                Services.Server.Database database1Obj =
                    (Services.Server.Database)database1.Single().BaseObject;
                Assert.AreEqual("testdb1", database1Obj.Name, "Expected db name to be testdb1");

                Assert.IsTrue(
                    database2.Single().BaseObject is Services.Server.Database,
                    "Expecting a Database object");
                Services.Server.Database database2Obj =
                    (Services.Server.Database)database2.Single().BaseObject;
                Assert.AreEqual("testdb2", database2Obj.Name, "Expected db name to be testdb2");
                Assert.AreEqual(
                    "Japanese_CI_AS",
                    database2Obj.CollationName,
                    "Expected collation to be Japanese_CI_AS");
                Assert.AreEqual("Web", database2Obj.Edition, "Expected edition to be Web");
                Assert.AreEqual(5, database2Obj.MaxSizeGB, "Expected max size to be 5 GB");
            }
        }
    }
}

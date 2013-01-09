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
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.CloudService.Test;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.MockServer;

    [TestClass]
    public class GetAzureSqlDatabaseCopyTests : TestBase
    {
        [TestCleanup]
        public void CleanupTest()
        {
            DatabaseTestHelper.SaveDefaultSessionCollection();
        }

        [TestMethod]
        public void GetAzureSqlDatabaseContinuousCopyWithSqlAuth()
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
                // Start two continuous database copy operation
                StartAzureSqlDatabaseCopyTests.StartDatabaseContinuousCopyWithSqlAuth(
                    powershell,
                    "$context",
                    "$copy1",
                    "testdb1");
                StartAzureSqlDatabaseCopyTests.StartDatabaseContinuousCopyWithSqlAuth(
                    powershell,
                    "$context",
                    "$copy2",
                    "testdb2");

                HttpSession testSession = DatabaseTestHelper.DefaultSessionCollection.GetSession(
                    "UnitTest.GetAzureSqlDatabaseContinuousCopyWithSqlAuth");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                        switch (expected.Index)
                        {
                            // Request 0-1: Get all database copies request
                            case 0:
                            case 1:
                            // Request 2-3: Get database copies for testdb1
                            case 2:
                            case 3:
                            // Request 4-5: Get database copies for testdb2
                            case 4:
                            case 5:
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
                    Collection<PSObject> databaseCopies, databaseCopy1, databaseCopy2;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        databaseCopies = powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabaseCopy " +
                            @"-Context $context");
                        databaseCopy1 = powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabaseCopy " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1");
                        databaseCopy2 = powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabaseCopy " +
                            @"-Context $context " +
                            @"-DatabaseName testdb2");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    Assert.AreEqual(2, databaseCopies.Count, "Expecting 2 Database Copy objects");
                    Assert.IsTrue(
                        databaseCopy1.First().BaseObject is Services.Server.DatabaseCopy,
                        "Expecting a Database Copy object");
                    Services.Server.DatabaseCopy databaseCopyObj =
                        (Services.Server.DatabaseCopy)databaseCopy1.Single().BaseObject;
                    Assert.AreEqual(
                        "testserver",
                        databaseCopyObj.SourceServerName,
                        "Expected source server name to be testserver");
                    Assert.AreEqual(
                        "testdb1",
                        databaseCopyObj.SourceDatabaseName,
                        "Expected source database name to be testdb1");
                    Assert.AreEqual(
                        "partnersrv",
                        databaseCopyObj.DestinationServerName,
                        "Expected destination server name to be partnersrv");
                    Assert.AreEqual(
                        "testdb1",
                        databaseCopyObj.DestinationDatabaseName,
                        "Expected destination database name to be testdb1");
                    Assert.IsTrue(
                        databaseCopyObj.IsContinuous,
                        "Expected copy to be continuous");

                    Assert.IsTrue(
                        databaseCopy2.First().BaseObject is Services.Server.DatabaseCopy,
                        "Expecting a Database Copy object");
                    databaseCopyObj =
                        (Services.Server.DatabaseCopy)databaseCopy2.Single().BaseObject;
                    Assert.AreEqual(
                        "testserver",
                        databaseCopyObj.SourceServerName,
                        "Expected source server name to be testserver");
                    Assert.AreEqual(
                        "testdb2",
                        databaseCopyObj.SourceDatabaseName,
                        "Expected source database name to be testdb2");
                    Assert.AreEqual(
                        "partnersrv",
                        databaseCopyObj.DestinationServerName,
                        "Expected destination server name to be partnersrv");
                    Assert.AreEqual(
                        "testdb2",
                        databaseCopyObj.DestinationDatabaseName,
                        "Expected destination database name to be testdb2");
                    Assert.IsTrue(
                        databaseCopyObj.IsContinuous,
                        "Expected copy to be continuous");
                }
            }
        }

        /// <summary>
        /// Wait for Continuous Copy operation to become catchup.
        /// </summary>
        /// <param name="powershell">The powershell instance containing the context.</param>
        /// <param name="contextVariable">The variable name that holds the server context.</param>
        /// <param name="dbCopyVariable">The variable name that holds the db copy object.</param>
        public static void WaitContinuousCopyCatchup(
            System.Management.Automation.PowerShell powershell,
            string contextVariable,
            string dbCopyVariable)
        {
            if (DatabaseTestHelper.CommonServiceBaseUri == null)
            {
                // Don't need to wait during playback
                return;
            }

            HttpSession testSession = new HttpSession();
            testSession.Messages = new HttpMessageCollection();
            DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);

            using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
            {
                using (new MockHttpServer(
                    exceptionManager,
                    MockHttpServer.DefaultServerPrefixUri,
                    testSession))
                {
                    for (int i = 0; i < 20; i++)
                    {
                        Collection<PSObject> databaseCopy = powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"{0} | Get-AzureSqlDatabaseCopy " +
                                @"-Context $context",
                                dbCopyVariable));
                        Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                        Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                        powershell.Streams.ClearStreams();

                        Assert.IsTrue(
                            databaseCopy.First().BaseObject is Services.Server.DatabaseCopy,
                            "Expecting a Database Copy object");
                        Services.Server.DatabaseCopy databaseCopyObj =
                            (Services.Server.DatabaseCopy)databaseCopy.Single().BaseObject;
                        if (databaseCopyObj.ReplicationStateDescription == "CATCH_UP")
                        {
                            break;
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
            }
        }
    }
}

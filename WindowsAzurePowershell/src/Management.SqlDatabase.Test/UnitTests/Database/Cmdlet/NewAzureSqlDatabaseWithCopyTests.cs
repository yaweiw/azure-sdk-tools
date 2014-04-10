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
    public class NewAzureSqlDatabaseWithCopyTests : TestBase
    {
        [TestCleanup]
        public void CleanupTest()
        {
            DatabaseTestHelper.SaveDefaultSessionCollection();
        }

        [TestMethod]
        public void NewAzureSqlDatabaseWithCopy()
        {
            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                // Create a context
                NewAzureSqlDatabaseServerContextTests.CreateServerContextSqlAuth(
                    powershell,
                    "$context");

                // Create a database along with a continuous copy
                NewAzureSqlDatabaseWithCopyTests.CreateDatabaseWithCopy(
                    powershell,
                    "$context",
                    "$db",
                    "testdb",
                    "partnersrv");
            }
        }

        /// <summary>
        /// Create a new database in the given server context along with a continuous copy at the specified partner server
        /// </summary>
        /// <param name="powershell">The powershell instance containing the context.</param>
        /// <param name="contextVariable">The variable name that holds the server context.</param>
        /// <param name="databaseVariable">The variable name that holds the database object.</param>
        /// <param name="databaseName">The name of the database to create and to copy.</param>
        /// <param name="partnerServer">The name of the partner server where the continuous copy goes to</param>
        public static void CreateDatabaseWithCopy(
            System.Management.Automation.PowerShell powershell,
            string contextVariable,
            string databaseVariable,
            string databaseName,
            string partnerServer)
        {
            HttpSession testSession = DatabaseTestHelper.DefaultSessionCollection.GetSession(
                "UnitTest.Common.CreateDatabaseWithCopy");
            DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
            testSession.RequestValidator =
                new Action<HttpMessage, HttpMessage.Request>(
                (expected, actual) =>
                {
                    Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                    Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                    switch (expected.Index)
                    {
                        // Request 0-1: Create database with copy
                        case 0:
                        case 1:
                        // Request 2-3: Get database copy
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
                Collection<PSObject> database, databaseCopy;
                using (new MockHttpServer(
                    exceptionManager,
                    MockHttpServer.DefaultServerPrefixUri,
                    testSession))
                {
                    database = powershell.InvokeBatchScript(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            @"{0} = New-AzureSqlDatabaseWithCopy " +
                            @"-Context {1} " +
                            @"-DatabaseName {2} " +
                            @"-PartnerServer {3} " +
                            @"-Collation Japanese_CI_AS " +
                            @"-Edition Web " +
                            @"-MaxSizeGB 5 " +
                            @"-MaxLagInMinutes 15 " +
                            @"-Force",
                            databaseVariable,
                            contextVariable,
                            databaseName,
                            partnerServer),
                        databaseVariable);
                    databaseCopy = powershell.InvokeBatchScript(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            @"Get-AzureSqlDatabaseCopy " +
                            @"-Context {0} " +
                            @"-DatabaseName {1} " +
                            @"-PartnerServer {2}",
                            contextVariable,
                            databaseName,
                            partnerServer)
                        );
                }

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();

                // Validate Database object returned
                Assert.IsTrue(
                    database.Single().BaseObject is Services.Server.Database,
                    "Expecting a Database object");
                Services.Server.Database databaseObj =
                    (Services.Server.Database)database.Single().BaseObject;
                Assert.AreEqual(databaseName, databaseObj.Name, "Expected database name to match");
                Assert.AreEqual("Japanese_CI_AS", databaseObj.CollationName, "Expected collation to be Japanese_CI_AS");
                Assert.AreEqual("Web", databaseObj.Edition, "Expected edition to be Web");
                Assert.AreEqual(5, databaseObj.MaxSizeGB, "Expected max size to be 5 GB");

                // Validate DatabaseCopy object returned
                Assert.IsTrue(
                    databaseCopy.First().BaseObject is Services.Server.DatabaseCopy,
                    "Expected a DatabaseCopy object");
                Services.Server.DatabaseCopy databaseCopyObj =
                    (Services.Server.DatabaseCopy)databaseCopy.First().BaseObject;
                Assert.AreEqual("testserver", databaseCopyObj.SourceServerName, "Expected source server name to be testserver");
                Assert.AreEqual(
                    databaseName,
                    databaseCopyObj.SourceDatabaseName,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Expected source database name to be {0}",
                        databaseName)
                    );
                Assert.AreEqual(
                    partnerServer,
                    databaseCopyObj.DestinationServerName,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Expected destination server name to be {0}",
                        partnerServer)
                    );
                Assert.AreEqual(
                    databaseName,
                    databaseCopyObj.DestinationDatabaseName,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Expected destination database name to be {0}",
                        databaseName)
                    );
                Assert.IsTrue(databaseCopyObj.IsContinuous, "Expected copy to be continuous");
                Assert.AreEqual(15, databaseCopyObj.MaximumLag, "Expected maximum lag to be 15 minutes");
                Assert.AreEqual("CATCH_UP", databaseCopyObj.ReplicationStateDescription, "Expected replication state to be CATCH_UP");
            }
        }
    }
}

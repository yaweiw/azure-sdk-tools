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
    public class SetAzureSqlDatabaseTests : TestBase
    {
        [TestCleanup]
        public void CleanupTest()
        {
            DatabaseTestHelper.SaveDefaultSessionCollection();
        }

        [TestMethod]
        public void SetAzureSqlDatabaseSizeWithSqlAuth()
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

                HttpSession testSession = DatabaseTestHelper.DefaultSessionCollection.GetSession(
                    "UnitTests.SetAzureSqlDatabaseSizeWithSqlAuth");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                        switch (expected.Index)
                        {
                            // Request 0-1: Set testdb1 with new MaxSize
                            case 0:
                            case 1:
                            // Request 2: Get updated testdb1
                            case 2:
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
                    // Create context with both ManageUrl and ServerName overriden
                    Collection<PSObject> database;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        database = powershell.InvokeBatchScript(
                            @"Set-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1 " +
                            @"-MaxSizeGB 5 " +
                            @"-Force " +
                            @"-PassThru");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    Assert.IsTrue(
                        database.Single().BaseObject is Services.Server.Database,
                        "Expecting a Database object");
                    Services.Server.Database databaseObj =
                        (Services.Server.Database)database.Single().BaseObject;
                    Assert.AreEqual("testdb1", databaseObj.Name, "Expected db name to be testdb1");
                    Assert.AreEqual("Web", databaseObj.Edition, "Expected edition to be Web");
                    Assert.AreEqual(5, databaseObj.MaxSizeGB, "Expected max size to be 5 GB");
                }
            }
        }

        [TestMethod]
        public void SetAzureSqlDatabaseNameWithSqlAuth()
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

                HttpSession testSession = DatabaseTestHelper.DefaultSessionCollection.GetSession(
                    "UnitTests.SetAzureSqlDatabaseNameWithSqlAuth");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                        switch (expected.Index)
                        {
                            // Request 1-2: Set testdb1 with new name of testdb2
                            case 0:
                            case 1:
                            // Request 3: Get updated testdb2
                            case 2:
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
                    // Create context with both ManageUrl and ServerName overriden
                    Collection<PSObject> database;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        database = powershell.InvokeBatchScript(
                           @"Set-AzureSqlDatabase " +
                           @"-Context $context " +
                           @"-DatabaseName testdb1 " +
                           @"-NewName testdb3 " +
                           @"-Force " +
                           @"-PassThru");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    Assert.IsTrue(
                        database.Single().BaseObject is Services.Server.Database,
                        "Expecting a Database object");
                    Services.Server.Database databaseObj =
                        (Services.Server.Database)database.Single().BaseObject;
                    Assert.AreEqual("testdb3", databaseObj.Name, "Expected db name to be testdb3");
                    Assert.AreEqual("Web", databaseObj.Edition, "Expected edition to be Web");
                    Assert.AreEqual(1, databaseObj.MaxSizeGB, "Expected max size to be 1 GB");
                }
            }
        }
    }
}

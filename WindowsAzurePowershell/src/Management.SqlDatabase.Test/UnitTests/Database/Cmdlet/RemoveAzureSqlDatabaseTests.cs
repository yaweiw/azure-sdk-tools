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
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.MockServer;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;

    [TestClass]
    public class RemoveAzureSqlDatabaseTests : TestBase
    {
        [TestCleanup]
        public void CleanupTest()
        {
            DatabaseTestHelper.SaveDefaultSessionCollection();
        }

        [TestMethod]
        public void RemoveAzureSqlDatabaseWithSqlAuth()
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
                    "UnitTests.RemoveAzureSqlDatabaseWithSqlAuth");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                        (expected, actual) =>
                        {
                            Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                            Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                            switch (expected.Index)
                            {
                                // Request 0-3: Remove database requests
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                // Request 4: Get all database request
                                case 4:
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
                    Collection<PSObject> databases;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        powershell.InvokeBatchScript(
                            @"Remove-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1 " +
                            @"-Force");
                        powershell.InvokeBatchScript(
                            @"Remove-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb2 " +
                            @"-Force");

                        databases = powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabase " +
                            @"-Context $context");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    Assert.AreEqual(1, databases.Count, "Expecting only master database object");
                }
            }
        }
    }
}

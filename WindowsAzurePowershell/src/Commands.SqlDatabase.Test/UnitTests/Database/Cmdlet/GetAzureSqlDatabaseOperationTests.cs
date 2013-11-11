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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.Database.Cmdlet
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Commands.Test.Utilities.Common;
    using MockServer;
    using Services;
    using Services.Server;
    using SqlDatabase.Database.Cmdlet;

    [TestClass]
    public class GetAzureSqlDatabaseOperationTests : TestBase
    {

        /// <summary>
        /// Create a database on the given context then get the operations on that database.
        /// </summary>
        /// <param name="powershell">The powershell instance containing the context.</param>
        /// <param name="contextVariable">The variable name that holds the server context.</param>
        [TestMethod]
        public void GetAzureSqlDatabaseOperationWithSqlAuth()
        {
            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                NewAzureSqlDatabaseServerContextTests.CreateServerContextSqlAuth(
                    powershell,
                    "$context");
                HttpSession testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                    "UnitTest.Common.CreatePremiumDatabasesWithSqlAuth");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                        (expected, actual) =>
                        {
                            Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                            Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                            switch (expected.Index)
                            {
                                // Request 0-7: Create and Query $testdb                                                                
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                // Request 8: Delete $testdb
                                case 8:
                                // Request 9-11: Query Database Operations                                
                                case 9:
                                case 10:
                                case 11:                                
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
                    string testsDBName = string.Format("getAzureSqlDatabaseOperationTestsDB_{0}",
                        Guid.NewGuid().ToString());
                    Collection<PSObject> database, operationsByName, operationsByDatabase, operationsById;
                    using (new MockHttpServer(
                            exceptionManager,
                            MockHttpServer.DefaultServerPrefixUri,
                            testSession))
                    {
                        database = powershell.InvokeBatchScript(
                            string.Format(
                                @"$testdb = New-AzureSqlDatabase " +
                                @"-Context $context " +
                                @"-DatabaseName {0} " +
                                @"-Force", testsDBName),
                            @"$testdb");

                        powershell.InvokeBatchScript(
                            string.Format(
                                @"Remove-AzureSqlDatabase " +
                                @"-Context $context " +
                                @"-DatabaseName {0} " +
                                @"-Force", testsDBName));

                        operationsByName = powershell.InvokeBatchScript(
                            string.Format(
                                @"$operations = Get-AzureSqlDatabaseOperation " +
                                @"-ConnectionContext $context " +
                                @"-DatabaseName {0}",
                                testsDBName),
                            @"$operations");

                        operationsByDatabase = powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabaseOperation " +
                            @"-ConnectionContext $context " +
                            @"-Database $testdb");

                        operationsById = powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabaseOperation " +
                            @"-ConnectionContext $context " +
                            @"-OperationGuid $operations[0].Id"
                            );
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    VerifyGetOperationsResult(testsDBName, operationsByName);
                    VerifyGetOperationsResult(testsDBName, operationsByDatabase);
                    // Update this verification once Task 1615375:Adding Drop record in dm_operation_status resolved
                    VerifyGetOperationsResult(testsDBName, operationsById);
                }
            }
        }

        private static void VerifyGetOperationsResult(string testsDBName, Collection<PSObject> operationsByName)
        {
            DatabaseOperation[] operations = operationsByName.Select(r => r.BaseObject as DatabaseOperation).ToArray(); ;
            // Task 1615375:Adding Drop record in dm_operation_status
            // There is a known issue that Drop record is not included in the DatabaseOperation log
            // Once that's done We should change the assert to 
            // Assert.AreEqual(2, operations.Length, "Expecting one DatabaseOperation");
            Assert.AreEqual(1, operations.Length, "Expecting one DatabaseOperation.");
            Assert.IsNotNull(operations[0], "Expecting a DatabaseOperation object.");
            Assert.AreEqual(testsDBName, operations[0].DatabaseName, "Database name does NOT match.");
            Assert.AreEqual("CREATE DATABASE", operations[0].Name, "Operation name does NOT match.");
            Assert.AreEqual(100, operations[0].PercentComplete, "Operation should be 100 percent complete.");
            Assert.AreEqual("COMPLETED", operations[0].State, "Operation state should be COMPLETED.");

        }

        /// <summary>
        /// Helper function to remove the test databases.
        /// </summary>
        public static void RemoveTestDatabasesWithSqlAuth()
        {
            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                // Create a context
                NewAzureSqlDatabaseServerContextTests.CreateServerContextSqlAuth(
                    powershell,
                    "$context");

                // Remove the 2 test databases
                NewAzureSqlPremiumDatabaseTests.RemoveTestDatabasesWithSqlAuth(
                    powershell,
                    "$context");
            }
        }

        /// <summary>
        /// Removes all existing db which name starting with PremiumTest on the given context.
        /// </summary>
        /// <param name="powershell">The powershell instance containing the context.</param>
        /// <param name="contextVariable">The variable name that holds the server context.</param>
        public static void RemoveTestDatabasesWithSqlAuth(
            System.Management.Automation.PowerShell powershell,
            string contextVariable)
        {
            HttpSession testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                "UnitTest.Common.RemoveTestDatabasesWithSqlAuth");
            DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
            testSession.RequestValidator =
                new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                        switch (expected.Index)
                        {
                            // Request 0-11: Remove database requests
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
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
                using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                {
                    powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabase $context | " +
                        @"? {$_.Name.contains(""NewAzureSqlPremiumDatabaseTests"")} " +
                        @"| Remove-AzureSqlDatabase -Context $context -Force");
                }

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();
            }
        }
    }
}
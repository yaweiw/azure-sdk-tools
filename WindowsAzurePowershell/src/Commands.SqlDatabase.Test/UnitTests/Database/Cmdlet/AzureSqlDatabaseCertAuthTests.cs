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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.Database.Cmdlet
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Server;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.MockServer;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.Server.Cmdlet;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.Utilities;
    using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;

    [TestClass]
    public class AzureSqlDatabaseTests : TestBase
    {
        [TestCleanup]
        public void CleanupTest()
        {
            // Save the mock session results
            MockServerHelper.SaveDefaultSessionCollection();
        }

        /// <summary>
        /// Test Get/Set/Remove a database using certificate authentication.
        /// </summary>
        [TestMethod]
        public void AzureSqlDatabaseCertTests()
        {
            // This test uses the https endpoint, setup the certificates.
            MockHttpServer.SetupCertificates();

            using (PowerShell powershell = PowerShell.Create())
            {
                // Setup the subscription used for the test
                WindowsAzureSubscription subscription =
                    UnitTestHelper.SetupUnitTestSubscription(powershell);

                powershell.Runspace.SessionStateProxy.SetVariable(
                    "serverName",
                    SqlDatabaseTestSettings.Instance.ServerName);

                // Create a new server
                HttpSession testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                    "UnitTest.AzureSqlDatabaseCertTests");
                ServerTestHelper.SetDefaultTestSessionSettings(testSession);

                // Uncomment one of these two when testing against onebox or production
                // When testing production use RDFE 
                // testSession.ServiceBaseUri = new Uri("https://management.core.windows.net");
                // When testing onebox use Mock RDFE
                //testSession.ServiceBaseUri = new Uri("https://management.dev.mscds.com:12346/");

                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                        (expected, actual) =>
                        {
                            Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                            Assert.IsTrue(
                                actual.UserAgent.Contains(ApiConstants.UserAgentHeaderValue),
                                "Missing proper UserAgent string.");
                            Assert.IsTrue(
                                UnitTestHelper.GetUnitTestClientCertificate().Equals(actual.Certificate),
                                "Expected correct client certificate");
                        });

                Collection<PSObject> newDatabaseResult1 = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            @"New-AzureSqlDatabase" +
                            @" -ServerName $serverName" +
                            @" -DatabaseName testdbcert1");
                    });

                Collection<PSObject> newDatabaseResult2 = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            @"New-AzureSqlDatabase" +
                            @" -ServerName $serverName" +
                            @" -DatabaseName testdbcert2" +
                            @" -Edition Business" +
                            @" -MaxSizeGB 10" +
                            @" -Collation Japanese_CI_AS");
                    });

                Collection<PSObject> getDatabaseResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabase" +
                            @" $serverName");
                    });

                Collection<PSObject> getSingleDatabaseResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabase" +
                            @" $serverName" +
                            @" -DatabaseName testdbcert1");
                    });

                Collection<PSObject> setDatabaseNameResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        powershell.Runspace.SessionStateProxy.SetVariable("db", newDatabaseResult1.FirstOrDefault());
                        return powershell.InvokeBatchScript(
                            @"$db | Set-AzureSqlDatabase" +
                            @" -NewDatabaseName testdbcert3" +
                            @" -PassThru");
                    });

                Collection<PSObject> setDatabaseSizeResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        powershell.Runspace.SessionStateProxy.SetVariable("db", newDatabaseResult1.FirstOrDefault());
                        return powershell.InvokeBatchScript(
                            @"$db | Set-AzureSqlDatabase" +
                            @" -MaxSizeGB 5" +
                            @" -PassThru");
                    });

                Collection<PSObject> P1 = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            @"$P1 = Get-AzureSqlDatabaseServiceObjective" +
                            @" -Server $serverName" +
                            @" -ServiceObjectiveName ""Reserved P1""",
                            @"$P1");
                    });

                Collection<PSObject> P2 = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        powershell.InvokeBatchScript(
                            @"$SLO = Get-AzureSqlDatabaseServiceObjective" +
                            @" -Server $serverName");

                        return powershell.InvokeBatchScript(
                            @"$P2 = Get-AzureSqlDatabaseServiceObjective" +
                            @" -Server $serverName" +
                            @" -ServiceObjective $SLO[1]",
                            @"$P2");
                    });

                Collection<PSObject> newPremiumP1DatabaseResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {  
                        return powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"New-AzureSqlDatabase" +
                                @" -ServerName {0}" +
                                @" -DatabaseName ""testdbcertPremiumDBP1""" +
                                @" -Edition Premium" +
                                @" -ServiceObjective $P1",
                                "testserver"));
                    });

                Collection<PSObject> newPremiumP2DatabaseResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"New-AzureSqlDatabase" +
                                @" -ServerName {0}" +
                                @" -DatabaseName ""testdbcertPremiumDBP2""" +
                                @" -Edition Premium" +
                                @" -ServiceObjective $P2",
                                "testserver"));
                    });
                // There is a known issue about the Get-AzureSqlDatabaseOperation that it returns all
                // operations which has the required database name no matter it's been deleted and recreated.
                // So when run it against the mock session, please use the hard coded testsDBName.
                // Run against onebox, please use the one with NewGuid(). 
                // This unit test should be updated once that behavior get changed which was already been 
                // created as a task.

                //string getOperationDbName = "testdbcertGetOperationDbName_" + Guid.NewGuid().ToString();
                string getOperationDbName = "testdbcertGetOperationDbName_5d8b5785-0490-402c-b42f-6a5f5d6fbed8";
                Collection<PSObject> newOperationDbResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"$getOperationDb = New-AzureSqlDatabase" +
                                @" -ServerName testserver" +
                                @" -DatabaseName ""{0}""",
                                getOperationDbName),
                                @"$getOperationDb");
                    });
                
                Collection<PSObject> getDatabaseOperationByDbResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {                        
                        return powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"Get-AzureSqlDatabaseOperation" +
                                @" -ServerName testserver" +
                                @" -Database $getOperationDb"));
                    });

                Collection<PSObject> getDatabaseOperationByNameResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"$getOperation = Get-AzureSqlDatabaseOperation" +
                                @" -ServerName testserver" +
                                @" -DatabaseName ""{0}""",
                                getOperationDbName),
                                @"$getOperation");
                    });

                Collection<PSObject> getDatabaseOperationByIdResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        return powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"Get-AzureSqlDatabaseOperation" +
                                @" -ServerName testserver" +
                                @" -OperationGuid $getOperation[0].Id"));
                    });

                Collection<PSObject> removeDatabaseResult = MockServerHelper.ExecuteWithMock(
                    testSession,
                    MockHttpServer.DefaultHttpsServerPrefixUri,
                    () =>
                    {
                        powershell.Runspace.SessionStateProxy.SetVariable("db1", newDatabaseResult1.FirstOrDefault());
                        powershell.Runspace.SessionStateProxy.SetVariable("db2", newDatabaseResult2.FirstOrDefault());
                        powershell.Runspace.SessionStateProxy.SetVariable("premiumP1", newPremiumP1DatabaseResult.FirstOrDefault());
                        powershell.Runspace.SessionStateProxy.SetVariable("premiumP2", newPremiumP2DatabaseResult.FirstOrDefault());
                        powershell.Runspace.SessionStateProxy.SetVariable("operationDb", newOperationDbResult.FirstOrDefault());
                        powershell.InvokeBatchScript(
                            @"$db1 | Remove-AzureSqlDatabase" +
                            @" -Force");
                        powershell.InvokeBatchScript(
                            @"$db2 | Remove-AzureSqlDatabase" +
                            @" -Force");
                        powershell.InvokeBatchScript(
                            @"$premiumP1 | Remove-AzureSqlDatabase" +
                            @" -Force");
                        powershell.InvokeBatchScript(
                            @"$premiumP2 | Remove-AzureSqlDatabase" +
                            @" -Force");
                        powershell.InvokeBatchScript(
                            @"$operationDb | Remove-AzureSqlDatabase" +
                            @" -Force");
                        return powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabase" +
                            @" $serverName");
                    });

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Unexpected Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Unexpected Warnings during run!");

                // Validate New-AzureSqlDatabase
                Database[] databases = new Database[] { newDatabaseResult1.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                // Note: Because the object is piped, this is the final state of the 
                // database object, after all the Set- cmdlet has run.
                Assert.AreEqual("testdbcert3", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(5, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);

                databases = new Database[] { newDatabaseResult2.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                Assert.AreEqual("testdbcert2", databases[0].Name);
                Assert.AreEqual("Business", databases[0].Edition);
                Assert.AreEqual(10, databases[0].MaxSizeGB);
                Assert.AreEqual("Japanese_CI_AS", databases[0].CollationName);

                // Validate Get-AzureSqlDatabase                
                databases = getDatabaseResult.Select(r => r.BaseObject as Database).ToArray();
                Assert.AreEqual(3, databases.Length, "Expecting 3 databases");
                Assert.IsNotNull(databases[0], "Expecting a Database object.");
                Assert.IsNotNull(databases[1], "Expecting a Database object.");
                Assert.IsNotNull(databases[2], "Expecting a Database object.");
                Assert.AreEqual("master", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(5, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);
                Assert.AreEqual(true, databases[0].IsSystemObject);
                Assert.AreEqual("testdbcert1", databases[1].Name);
                Assert.AreEqual("Web", databases[1].Edition);
                Assert.AreEqual(1, databases[1].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[1].CollationName);
                Assert.AreEqual(false, databases[1].IsSystemObject);
                Assert.AreEqual("testdbcert2", databases[2].Name);
                Assert.AreEqual("Business", databases[2].Edition);
                Assert.AreEqual(10, databases[2].MaxSizeGB);
                Assert.AreEqual("Japanese_CI_AS", databases[2].CollationName);
                Assert.AreEqual(false, databases[2].IsSystemObject);

                databases = new Database[] { getSingleDatabaseResult.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                Assert.AreEqual("testdbcert1", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(1, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);

                // Validate Set-AzureSqlDatabase
                databases = new Database[] { setDatabaseNameResult.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                Assert.AreEqual("testdbcert3", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(1, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);

                databases = new Database[] { setDatabaseSizeResult.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting one database");
                Assert.IsNotNull(databases[0],
                    "Expecting a Database object.");
                Assert.AreEqual("testdbcert3", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(5, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);

                // Validate New-AzureSqlDatabase for Premium Edition Database
                VerifyCreatePremiumDb(newPremiumP1DatabaseResult, "testdbcertPremiumDBP1", (P1.Single().BaseObject as ServiceObjective).Id.ToString());
                VerifyCreatePremiumDb(newPremiumP2DatabaseResult, "testdbcertPremiumDBP2", (P2.Single().BaseObject as ServiceObjective).Id.ToString());

                // Validate Get-AzureSqlDatabaseServiceObjective
                var SLOP1 = P1.Single().BaseObject as ServiceObjective;
                Assert.AreEqual(SLOP1.Name, "Reserved P1");
                Assert.AreEqual(SLOP1.Description, "Resource capacity is reserved.");
                Assert.IsNotNull(SLOP1.DimensionSettings, "Expecting some Dimension Setting objects.");
                Assert.AreEqual(SLOP1.DimensionSettings.Count(), 1, "Expecting 1 Dimension Setting.");
                Assert.AreEqual(SLOP1.DimensionSettings[0].Description, "Resource capacity is reserved.", "Expecting Dimension Setting description as Resource capacity is reserved.");
                
                var SLOP2 = P2.Single().BaseObject as ServiceObjective;
                Assert.AreEqual(SLOP2.Name, "Reserved P2");
                Assert.AreEqual(SLOP2.Description, "Resource capacity is reserved.");
                Assert.IsNotNull(SLOP2.DimensionSettings, "Expecting some Dimension Setting objects.");
                Assert.AreEqual(SLOP2.DimensionSettings.Count(), 1, "Expecting 1 Dimension Setting.");
                Assert.AreEqual(SLOP2.DimensionSettings[0].Description, "Resource capacity is reserved.", "Expecting Dimension Setting description as Resource capacity is reserved.");
                // Validate Get-AzureSqlDatabaseOperation
                VerifyGetAzureSqlDatabaseOperation(getOperationDbName, getDatabaseOperationByDbResult);
                VerifyGetAzureSqlDatabaseOperation(getOperationDbName, getDatabaseOperationByNameResult);
                VerifyGetAzureSqlDatabaseOperation(getOperationDbName, getDatabaseOperationByIdResult);
                
                // Validate Remove-AzureSqlDatabase
                databases = new Database[] { removeDatabaseResult.Single().BaseObject as Database };
                Assert.AreEqual(1, databases.Length, "Expecting no databases");
                Assert.IsNotNull(databases[0], "Expecting a Database object.");
                Assert.AreEqual("master", databases[0].Name);
                Assert.AreEqual("Web", databases[0].Edition);
                Assert.AreEqual(5, databases[0].MaxSizeGB);
                Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);
            }
        }

        private static void VerifyGetAzureSqlDatabaseOperation(string getOperationDbName, Collection<PSObject> getDatabaseOperationByIdResult)
        {
            var operations = getDatabaseOperationByIdResult.Select(r => r.BaseObject as DatabaseOperation).ToArray();
            Assert.AreEqual(operations.Count(), 1, "Expecting 1 operation");
            Assert.AreEqual(operations[0].Name, "CREATE DATABASE", "Expecting CREATE DATABASE operation");
            Assert.AreEqual(operations[0].State, "COMPLETED", "Expecting operation COMPLETED");
            Assert.AreEqual(operations[0].DatabaseName, getOperationDbName, string.Format("Expecting Database name: {0}", getOperationDbName));
            Assert.AreEqual(operations[0].PercentComplete, 100, "Expecting operation completed 100%");
        }

        private static Database[] VerifyCreatePremiumDb(Collection<PSObject> newPremiumP1DatabaseResult, string databaseName, string serviceObjectiveId)
        {
            Database[] databases = new Database[] { newPremiumP1DatabaseResult.Single().BaseObject as Database };
            Assert.AreEqual(1, databases.Length, "Expecting one database");
            Assert.IsNotNull(databases[0], "Expecting a Database object.");
            Assert.AreEqual(databases[0].Name, databaseName, string.Format("Expecting Database Name:{0}, actual is:{1}", databaseName, databases[0].Name));
            /* SQL Server: Defect 1655888: When creating a premium database, 
             * the immediate returned value do not have valid Edition and Max Database Size info                 
             * We should active the following asserts once the defect is fixed.
             Assert.AreEqual("Premium", databases[0].Edition);
             Assert.AreEqual(10, databases[0].MaxSizeGB);
             Assert.AreEqual(databases[0].AssignedServiceObjectiveId, serviceObjectiveId, string.Format("Expecting Database Edition:{0}, actual is:{1}", serviceObjectiveId, databases[0].AssignedServiceObjectiveId));
             */
            Assert.AreEqual("SQL_Latin1_General_CP1_CI_AS", databases[0].CollationName);
            return databases;
        }
    }
}


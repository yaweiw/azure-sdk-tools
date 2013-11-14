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
    using System.Linq;
    using System.Threading;
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.MockServer;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.Server.Cmdlet;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.Utilities;
    using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Server;

    [TestClass]
    public class AzureSqlDatabaseCopyCertAuthTests : TestBase
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
        public void AzureSqlDatabaseCopyCertTests()
        {
            // This test uses the https endpoint, setup the certificates.
            MockHttpServer.SetupCertificates();

            using (PowerShell powershell = PowerShell.Create())
            {
                // Setup the subscription used for the test
                WindowsAzureSubscription subscription =
                    UnitTestHelper.SetupUnitTestSubscription(powershell);

                // Set names for the servers we'll use in PowerShell.
                powershell.Runspace.SessionStateProxy.SetVariable(
                    "homeServerName",
                    SqlDatabaseTestSettings.Instance.ServerName);

                powershell.Runspace.SessionStateProxy.SetVariable(
                    "partnerServerName", "partnersrv");

                // Create a new server
                HttpSession testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                    "UnitTest.AzureSqlDatabaseCopyCertTests");

                ServerTestHelper.SetDefaultTestSessionSettings(testSession);

                // Uncomment one of these two when testing against OneBox or production
                // When testing production use RDFE
                // testSession.ServiceBaseUri = new Uri("https://management.core.windows.net");
                // When testing OneBox use Mock RDFE
                // testSession.ServiceBaseUri = new Uri("https://management.dev.mscds.com:12346/MockRDFE/");

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

                using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
                using (new MockHttpServer(exceptionManager, MockHttpServer.DefaultHttpsServerPrefixUri, testSession))
                {
                    // Create some databases to be used in the tests.

                    Collection<PSObject> createDatabaseResponse1 = powershell.InvokeBatchScript(
                        @"$db1 = New-AzureSqlDatabase" +
                        @" -ServerName $homeServerName" +
                        @" -DatabaseName testdbcopycertauth1",
                        @"$db1");

                    Collection<PSObject> createDatabaseResponse2 = powershell.InvokeBatchScript(
                        @"$db2 = New-AzureSqlDatabase" +
                        @" -ServerName $homeServerName" +
                        @" -DatabaseName testdbcopycertauth2",
                        @"$db2");

                    Collection<PSObject> createDatabaseResponse3 = powershell.InvokeBatchScript(
                        @"$db3 = New-AzureSqlDatabase" +
                        @" -ServerName $partnerServerName" +
                        @" -DatabaseName testdbcopycertauth3",
                        @"$db3");

                    Collection<PSObject> createDatabaseResponse4 = powershell.InvokeBatchScript(
                        @"$db4 = New-AzureSqlDatabase" +
                        @" -ServerName $partnerServerName" +
                        @" -DatabaseName testdbcopycertauth4",
                        @"$db4");

                    // Call Start-AzureSqlDatabaseCopy with different parameter sets.
                    // After each call, we wait for seeding completion before moving on.
                    // We test for seeding completion using Get-AzureSqlDatabaseCopy calls.

                    Collection<PSObject> startCopyResponse1 = powershell.InvokeBatchScript(
                        @"$copy1 = Start-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -PartnerServer $partnerServerName" +
                        @" -DatabaseName testdbcopycertauth1" +
                        @" -MaxLagInMinutes 60" +
                        @" -ContinuousCopy",
                        @"$copy1");

                    WaitForSeedingCompletion(powershell, "testserver", "testdbcopycertauth1", "partnersrv", 60);

                    Collection<PSObject> startCopyResponse2 = powershell.InvokeBatchScript(
                        @"$copy2 = Start-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -Database $db2" +
                        @" -PartnerServer $partnerServerName" +
                        @" -PartnerDatabase testdbcopycertauth2" +
                        @" -MaxLagInMinutes 1440" +
                        @" -Force" +
                        @" -ContinuousCopy",
                        @"$copy2");

                    WaitForSeedingCompletion(powershell, "testserver", "testdbcopycertauth2", "partnersrv", 1440);

                    Collection<PSObject> startCopyResponse3 = powershell.InvokeBatchScript(
                        @"$copy3 = Start-AzureSqlDatabaseCopy" +
                        @" -ServerName $partnerServerName" +
                        @" -DatabaseName testdbcopycertauth3" +
                        @" -PartnerServer $homeServerName" +
                        @" -ContinuousCopy",
                        @"$copy3");

                    // null = no RPO (the default)
                    WaitForSeedingCompletion(powershell, "partnersrv", "testdbcopycertauth3", "testserver", null);

                    Collection<PSObject> startCopyResponse4 = powershell.InvokeBatchScript(
                        @"$copy4 = Start-AzureSqlDatabaseCopy" +
                        @" -ServerName $partnerServerName" +
                        @" -Database $db4" +
                        @" -PartnerServer $homeServerName" +
                        @" -PartnerDatabase testdbcopycertauth4" +
                        @" -MaxLagInMinutes 300" +
                        @" -Force" +
                        @" -ContinuousCopy",
                        @"$copy4");

                    WaitForSeedingCompletion(powershell, "partnersrv", "testdbcopycertauth4", "testserver", 300);

                    // Do some Get-AzureSqlDatabaseCopy calls with different parameter sets.

                    Collection<PSObject> getCopyResponse1 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -DatabaseCopy $copy1");

                    Collection<PSObject> getCopyResponse2 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -DatabaseName testdbcopycertauth2" +
                        @" -PartnerServer $partnerServerName" +
                        @" -PartnerDatabase testdbcopycertauth2");

                    Collection<PSObject> getCopyResponse3 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $partnerServerName" +
                        @" -Database $db1");

                    Collection<PSObject> getCopyResponse4 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName");

                    Collection<PSObject> getCopyResponse5 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -PartnerServer $partnerServerName" +
                        @" -PartnerDatabase testdbcopycertauth1");

                    // Call Stop-AzureSqlDatbaseCopy with different parameter sets.

                    powershell.InvokeBatchScript(
                        @"Stop-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -DatabaseCopy $copy1");

                    powershell.InvokeBatchScript(
                        @"Stop-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -Database $db2" +
                        @" -ForcedTermination");

                    powershell.InvokeBatchScript(
                        @"Stop-AzureSqlDatabaseCopy" +
                        @" -ServerName $partnerServerName" +
                        @" -DatabaseName testdbcopycertauth3" +
                        @" -PartnerServer $homeServerName" +
                        @" -PartnerDatabase testdbcopycertauth3");

                    powershell.InvokeBatchScript(
                        @"Stop-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -DatabaseName testdbcopycertauth4" +
                        @" -ForcedTermination");

                    // Try to get the copies to verify that they've been terminated.

                    Collection<PSObject> getCopyResponse6 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -Database $db1");

                    Collection<PSObject> getCopyResponse7 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -Database $db2");

                    Collection<PSObject> getCopyResponse8 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $partnerServerName" +
                        @" -Database $db3");

                    Collection<PSObject> getCopyResponse9 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName" +
                        @" -Database $db4");

                    Collection<PSObject> getCopyResponse10 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName $homeServerName");

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Unexpected Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Unexpected Warnings during run!");

                    // Do a quick smoke test for the DB creates.
                    Database[] databases = new Collection<PSObject>[]
                        {
                            createDatabaseResponse1,
                            createDatabaseResponse2,
                            createDatabaseResponse3,
                            createDatabaseResponse4
                        }.Select(cpso => cpso.Single().BaseObject as Database).ToArray();

                    databases.ForEach(db => Assert.IsNotNull(db, "Expected object of type Database"));
                    Assert.AreEqual("testdbcopycertauth1", databases[0].Name);
                    Assert.AreEqual("testdbcopycertauth2", databases[1].Name);
                    Assert.AreEqual("testdbcopycertauth3", databases[2].Name);
                    Assert.AreEqual("testdbcopycertauth4", databases[3].Name);

                    // Verify the DatabaseCopy objects returned by the StartCopies.
                    VerifyCopyResponse(startCopyResponse1, "testserver", "testdbcopycertauth1", "partnersrv", false, 60);
                    VerifyCopyResponse(startCopyResponse2, "testserver", "testdbcopycertauth2", "partnersrv", false, 1440);
                    VerifyCopyResponse(startCopyResponse3, "partnersrv", "testdbcopycertauth3", "testserver", false, null);
                    VerifyCopyResponse(startCopyResponse4, "partnersrv", "testdbcopycertauth4", "testserver", false, 300);

                    // Verify the DatabaseCopy objects returned by the GetCopies.
                    VerifyCopyResponse(getCopyResponse1, "testserver", "testdbcopycertauth1", "partnersrv", false, 60);
                    VerifyCopyResponse(getCopyResponse2, "testserver", "testdbcopycertauth2", "partnersrv", false, 1440);
                    // MaxLag is null on the target side.
                    VerifyCopyResponse(getCopyResponse3, "testserver", "testdbcopycertauth1", "partnersrv", true, null);

                    Assert.AreEqual(4, getCopyResponse4.Count);
                    DatabaseCopy[] allCopies = getCopyResponse4.Select(obj => obj.BaseObject as DatabaseCopy).ToArray();
                    foreach (var copy in allCopies)
                    {
                        Assert.IsNotNull(copy, "Expected object of type DatabaseCopy");
                    }

                    Array.Sort(allCopies,
                               (dbc1, dbc2) => string.Compare(dbc1.SourceDatabaseName, dbc2.SourceDatabaseName));

                    VerifyCopyResponse(allCopies[0], "testserver", "testdbcopycertauth1", "partnersrv", false, 60);
                    VerifyCopyResponse(allCopies[1], "testserver", "testdbcopycertauth2", "partnersrv", false, 1440);
                    VerifyCopyResponse(allCopies[2], "partnersrv", "testdbcopycertauth3", "testserver", true, null);
                    VerifyCopyResponse(allCopies[3], "partnersrv", "testdbcopycertauth4", "testserver", true, null);

                    VerifyCopyResponse(getCopyResponse5, "testserver", "testdbcopycertauth1", "partnersrv", false, 60);

                    // Verify that the terminations worked.
                    Assert.AreEqual(0, getCopyResponse6.Count, "Expected copies to have been terminated");
                    Assert.AreEqual(0, getCopyResponse7.Count, "Expected copies to have been terminated");
                    Assert.AreEqual(0, getCopyResponse8.Count, "Expected copies to have been terminated");
                    Assert.AreEqual(0, getCopyResponse9.Count, "Expected copies to have been terminated");
                    Assert.AreEqual(0, getCopyResponse10.Count, "Expected copies to have been terminated");
                }
            }
        }

        private void WaitForSeedingCompletion(PowerShell powershell, string sourceServer, string sourceDb,
                                              string destServer, int? maximumLag)
        {
            for (int i = 0; i < 20; i++)
            {
                Collection<PSObject> testCopyCompleteResponse = powershell
                    .InvokeBatchScript(
                        @"Get-AzureSqlDatabaseCopy" +
                        @" -ServerName " + sourceServer +
                        @" -DatabaseName " + sourceDb);

                VerifyCopyResponse(testCopyCompleteResponse, sourceServer, sourceDb, destServer, false, maximumLag);

                var testCopyComplete = (DatabaseCopy)testCopyCompleteResponse.First().BaseObject;
                if (testCopyComplete.ReplicationStateDescription == "CATCH_UP")
                {
                    return;
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Assert.Fail("Continuous copy of source database " + sourceDb + " failed to reach CATCH_UP state");
        }

        private void VerifyCopyResponse(Collection<PSObject> result, string sourceServer, string sourceDb,
                                        string destServer, bool isLocalDatabaseReplicationTarget, int? maximumLag)
        {
            Assert.AreEqual(1, result.Count, "Expected exactly one result from cmdlet");
            var copy = result.First().BaseObject as DatabaseCopy;
            Assert.IsNotNull(copy, "Expected object of type DatabaseCopy");
            VerifyCopyResponse(copy, sourceServer, sourceDb, destServer, isLocalDatabaseReplicationTarget, maximumLag);
        }

        private void VerifyCopyResponse(DatabaseCopy copy, string sourceServer, string sourceDb,
                                        string destServer, bool isLocalDatabaseReplicationTarget, int? maximumLag)
        {
            Assert.AreEqual(sourceServer, copy.SourceServerName);
            Assert.AreEqual(sourceDb, copy.SourceDatabaseName);
            Assert.AreEqual(destServer, copy.DestinationServerName);
            // Different names for destination databases aren't currently supported.
            Assert.AreEqual(sourceDb, copy.DestinationDatabaseName);
            Assert.IsTrue(copy.IsContinuous);
            Assert.AreEqual(isLocalDatabaseReplicationTarget, copy.IsLocalDatabaseReplicationTarget);
            Assert.AreEqual(maximumLag, copy.MaximumLag);

            switch (copy.ReplicationStateDescription)
            {
                case "PENDING":
                    Assert.IsTrue((int)copy.PercentComplete.GetValueOrDefault(0) == 0);
                    break;

                case "SEEDING":
                    Assert.IsTrue(copy.IsInterlinkConnected);
                    break;

                case "CATCH_UP":
                    Assert.IsTrue(copy.PercentComplete.HasValue);
                    Assert.AreEqual(100, copy.PercentComplete.Value);
                    Assert.IsTrue(copy.IsInterlinkConnected);
                    break;

                case "":
                    // After forced terminate, on the other side.
                    Assert.IsFalse(copy.IsInterlinkConnected);
                    break;

                default:
                    Assert.Fail("Unexpected ReplicationStateDescription: " +
                                copy.ReplicationStateDescription);
                    break;
            }
        }
    }
}

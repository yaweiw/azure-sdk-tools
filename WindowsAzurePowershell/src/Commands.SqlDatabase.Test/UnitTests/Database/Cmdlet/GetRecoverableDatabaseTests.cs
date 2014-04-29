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
    public class GetRecoverableDatabaseTests : TestBase
    {
        private static PowerShell powershell;

        private static string serverName;

        [ClassInitialize]
        public static void InitializeClass(TestContext context)
        {
            powershell = PowerShell.Create();

            MockHttpServer.SetupCertificates();

            UnitTestHelper.SetupUnitTestSubscription(powershell);

            serverName = SqlDatabaseTestSettings.Instance.ServerName;
            powershell.Runspace.SessionStateProxy.SetVariable("serverName", serverName);
        }

        [TestCleanup]
        public void CleanupTest()
        {
            powershell.Streams.ClearStreams();

            // Save the mock session results
            MockServerHelper.SaveDefaultSessionCollection();
        }

        /// <summary>
        /// Test Get/Set/Remove a database using certificate authentication.
        /// </summary>
        [TestMethod]
        public void GetRecoverableDatabaseWithCertAuth()
        {
            var testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                "UnitTest.GetRecoverableDatabaseWithCertAuth");
            ServerTestHelper.SetDefaultTestSessionSettings(testSession);

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

            using (var exceptionManager = new AsyncExceptionManager())
            {
                Collection<PSObject> databases, database1, database2;

                using(new MockHttpServer(exceptionManager, MockHttpServer.DefaultHttpsServerPrefixUri, testSession))
                {
                    databases = powershell.InvokeBatchScript(
                        @"Get-AzureSqlRecoverableDatabase -TargetServerName $serverName");
                }

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();

                // Expecting testdb1, testdb2, possibly dropped databases from previous runs
                Assert.IsTrue(
                    databases.Count >= 2,
                    "Expecting at-least two RecoverableDatabase objects");

                Assert.IsTrue(
                    databases[0].BaseObject is RecoverableDatabase,
                    "Expecting a RecoverableDatabase object");

                Assert.IsTrue(
                    databases[1].BaseObject is RecoverableDatabase,
                    "Expecting a RecoverableDatabase object");

                var database1Object = (RecoverableDatabase)databases[0].BaseObject;

                var database2Object = (RecoverableDatabase)databases[1].BaseObject;

                using (new MockHttpServer(
                    exceptionManager, MockHttpServer.DefaultHttpsServerPrefixUri, testSession))
                {
                    database1 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlRecoverableDatabase " +
                        @"-TargetServerName $serverName " +
                        @"-SourceDatabaseName " + database1Object.Name);
                    database2 = powershell.InvokeBatchScript(
                        @"Get-AzureSqlRecoverableDatabase " +
                        @"-TargetServerName $serverName " +
                        @"-SourceDatabaseName " + database2Object.Name);
                }

                Assert.IsTrue(
                    database1.Single().BaseObject is RecoverableDatabase,
                    "Expecting a RecoverableDatabase object");
                var refreshedDatabase1Object = (RecoverableDatabase)database1.Single().BaseObject;
                Assert.AreEqual(
                    database1Object.Name, refreshedDatabase1Object.Name,
                    "Expected db name to be " + database1Object.Name);

                Assert.IsTrue(
                    database2.Single().BaseObject is RecoverableDatabase,
                    "Expecting a RecoverableDatabase object");
                var refreshedDatabase2Object = (RecoverableDatabase)database2.Single().BaseObject;
                Assert.AreEqual(
                    database2Object.Name, refreshedDatabase2Object.Name,
                    "Expected db name to be " + database2Object.Name);
                Assert.AreEqual(
                    database2Object.Edition, refreshedDatabase2Object.Edition,
                    "Expected edition to be " + database2Object.Edition);
            }
        }

        [TestMethod]
        public void GetRecoverableDatabaseWithCertAuthByPipe()
        {
            var testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                "UnitTest.GetRecoverableDatabaseWithCertAuthByPipe");
            ServerTestHelper.SetDefaultTestSessionSettings(testSession);

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

            using (var exceptionManager = new AsyncExceptionManager())
            {
                Collection<PSObject> databases, database1, database2;
                using (new MockHttpServer(
                    exceptionManager, MockHttpServer.DefaultHttpsServerPrefixUri, testSession))
                {
                    databases = powershell.InvokeBatchScript(
                        @"Get-AzureSqlRecoverableDatabase " +
                        @"-TargetServerName $serverName");
                }

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();

                // Expecting testdb1, testdb2, possibly dropped databases from previous runs
                Assert.IsTrue(
                    databases.Count >= 2,
                    "Expecting at-least two RecoverableDatabase objects");

                Assert.IsTrue(
                    databases[0].BaseObject is RecoverableDatabase,
                    "Expecting a RecoverableDatabase object");

                Assert.IsTrue(
                    databases[1].BaseObject is RecoverableDatabase,
                    "Expecting a RecoverableDatabase object");

                var database1Object = (RecoverableDatabase)databases[0].BaseObject;

                var database2Object = (RecoverableDatabase)databases[1].BaseObject;

                using (new MockHttpServer(
                    exceptionManager, MockHttpServer.DefaultHttpsServerPrefixUri, testSession))
                {
                    powershell.InvokeBatchScript(
                        @"$testdb1 = Get-AzureSqlRecoverableDatabase " +
                        @"-TargetServerName $serverName " +
                        @"-SourceDatabaseName " + database1Object.Name);

                    powershell.InvokeBatchScript(
                        @"$testdb2 = Get-AzureSqlRecoverableDatabase " +
                        @"-TargetServerName $serverName " +
                        @"-SourceDatabaseName " + database2Object.Name);

                    database1 = powershell.InvokeBatchScript(
                        @"$testdb1 | Get-AzureSqlRecoverableDatabase");

                    database2 = powershell.InvokeBatchScript(
                        @"$testdb2 | Get-AzureSqlRecoverableDatabase");
                }

                Assert.IsTrue(
                    database1.Single().BaseObject is RecoverableDatabase,
                    "Expecting a RecoverableDatabase object");
                var refreshedDatabase1Object = (RecoverableDatabase)database1.Single().BaseObject;
                Assert.AreEqual(
                    database1Object.Name, refreshedDatabase1Object.Name,
                    "Expected db name to be " + database1Object.Name);

                Assert.IsTrue(
                    database2.Single().BaseObject is RecoverableDatabase,
                    "Expecting a RecoverableDatabase object");
                var refreshedDatabase2Object = (RecoverableDatabase)database2.Single().BaseObject;
                Assert.AreEqual(
                    database2Object.Name, refreshedDatabase2Object.Name,
                    "Expected db name to be " + database2Object.Name);
                Assert.AreEqual(
                    database2Object.Edition, refreshedDatabase2Object.Edition,
                    "Expected edition to be " + database2Object.Edition);
            }
        }

        [TestMethod]
        public void GetRecoverableDatabaseWithCertAuthNonExistentDb()
        {
            var testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                "UnitTest.GetRecoverableDatabaseWithCertAuthNonExistentDb");
            ServerTestHelper.SetDefaultTestSessionSettings(testSession);

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

            using (var exceptionManager = new AsyncExceptionManager())
            {
                using (new MockHttpServer(
                    exceptionManager, MockHttpServer.DefaultHttpsServerPrefixUri, testSession))
                {
                    powershell.InvokeBatchScript(
                        @"Get-AzureSqlRecoverableDatabase " +
                        @"-TargetServerName $serverName " +
                        @"-SourceDatabaseName testdbnonexistent");
                }

                Assert.AreEqual(
                    1, powershell.Streams.Error.Count,
                    "Expecting errors");
                Assert.AreEqual(
                    1, powershell.Streams.Warning.Count,
                    "Expecting tracing IDs");
                Assert.AreEqual(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Resource with the name '{0}' does not exist. To continue, specify a valid resource name.",
                        "testdbnonexistent"),
                    powershell.Streams.Error.First().Exception.Message,
                    "Unexpected error message");
                Assert.IsTrue(
                    powershell.Streams.Warning[0].Message.StartsWith("Request Id"),
                    "Expecting Client Request Id");
            }
        }
    }
}

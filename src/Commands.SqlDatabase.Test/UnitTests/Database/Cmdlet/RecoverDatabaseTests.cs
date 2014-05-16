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
    using Commands.Test.Utilities.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.Server.Cmdlet;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.Utilities;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using MockServer;
    using Services.Server;
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    [TestClass]
    public class RecoverDatabaseTests : TestBase
    {
        private static PowerShell powershell;

        private static string serverName;

        /// <summary>
        /// Initialize the necessary environment for the tests.
        /// </summary>
        [TestInitialize]
        public void SetupTest()
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

        [TestMethod]
        public void RecoverAzureSqlDatabaseWithDatabaseNameWithCertAuth()
        {
            var testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                "UnitTests.RecoverAzureSqlDatabaseWithDatabaseNameWithCertAuth");
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
                Collection<PSObject> operation;
                using (new MockHttpServer(
                    exceptionManager, MockHttpServer.DefaultHttpsServerPrefixUri, testSession))
                {
                    operation = powershell.InvokeBatchScript(
                        @"Start-AzureSqlDatabaseRecovery " +
                        @"-TargetServerName $serverName " +
                        @"-SourceDatabaseName testdb1 " +
                        @"-TargetDatabaseName testdb1-restored");
                }

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();

                // Expecting one operation object
                Assert.AreEqual(1, operation.Count, "Expecting one operation object");

                Assert.IsTrue(
                    operation[0].BaseObject is RecoverDatabaseOperation,
                    "Expecting a RecoverDatabaseOperation object");

                var operationObject = (RecoverDatabaseOperation)operation[0].BaseObject;
                Assert.IsTrue(
                    operationObject.RequestID != Guid.Empty,
                    "Expecting a non-empty operation ID");
                Assert.AreEqual(
                    operationObject.TargetDatabaseName, "testdb1-restored",
                    "Target database name mismatch");
            }
        }

        [TestMethod]
        public void RecoverAzureSqlDatabaseWithDatabaseObjectWithCertAuth()
        {
            var testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                "UnitTests.RecoverAzureSqlDatabaseWithDatabaseObjectWithCertAuth");
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
                Collection<PSObject> operation;
                using (new MockHttpServer(
                    exceptionManager, MockHttpServer.DefaultHttpsServerPrefixUri, testSession))
                {
                    operation = powershell.InvokeBatchScript(
                        @"Get-AzureSqlRecoverableDatabase " +
                        @"-TargetServerName $serverName " +
                        @"-SourceDatabaseName testdb1" + " | " +
                        @"Start-AzureSqlDatabaseRecovery " +
                        @"-TargetDatabaseName testdb1-restored");
                }

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();

                // Expecting one operation object
                Assert.AreEqual(1, operation.Count, "Expecting one operation object");

                Assert.IsTrue(
                    operation[0].BaseObject is RecoverDatabaseOperation,
                    "Expecting a RecoverDatabaseOperation object");

                var operationObject = (RecoverDatabaseOperation)operation[0].BaseObject;
                Assert.AreNotEqual(
                    operationObject.RequestID, Guid.Empty,
                    "Expecting a non-empty operation ID");
                Assert.AreEqual(
                    operationObject.TargetDatabaseName, "testdb1-restored",
                    "Target database name mismatch");
            }
        }
    }
}

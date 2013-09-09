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
    using System.Management.Automation;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MockServer;
    using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;

    [TestClass]
    public class StopAzureSqlDatabaseCopyTests : TestBase
    {
        [TestCleanup]
        public void CleanupTest()
        {
            MockServerHelper.SaveDefaultSessionCollection();
        }

        [TestMethod]
        public void StopAzureSqlDatabaseContinuousCopyWithSqlAuth()
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
                GetAzureSqlDatabaseCopyTests.WaitContinuousCopyCatchup(
                    powershell,
                    "$context",
                    "$copy1");

                HttpSession testSession = MockServerHelper.DefaultSessionCollection.GetSession(
                    "UnitTest.StopAzureSqlDatabaseContinuousCopyWithSqlAuth");
                DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);
                testSession.RequestValidator =
                    new Action<HttpMessage, HttpMessage.Request>(
                    (expected, actual) =>
                    {
                        Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                        Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                        switch (expected.Index)
                        {
                            // Request 0-2: Stop Database Copy
                            case 0:
                            case 1:
                            case 2:
                            // Request 3-4: Retrieve all copies
                            case 3:
                            case 4:
                                DatabaseTestHelper.ValidateHeadersForODataRequest(
                                    expected.RequestInfo,
                                    actual);
                                break;
                            default:
                                //Assert.Fail("No more requests expected.");
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
                            @"Stop-AzureSqlDatabaseCopy " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1");
                    }
                }

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();

                WaitForCopyTermination(powershell, "$context");
            }
        }

        /// <summary>
        /// Helper method for wait for all copy to terminate.
        /// </summary>
        /// <param name="powershell">The powershell instance containing the context.</param>
        /// <param name="contextVariable">The variable name that will hold the new context.</param>
        public static void WaitForCopyTermination(
            System.Management.Automation.PowerShell powershell,
            string contextVariable)
        {
            if (MockServerHelper.CommonServiceBaseUri == null)
            {
                // Don't need to wait during playback
                return;
            }

            HttpSession testSession = new HttpSession();
            testSession.Messages = new HttpMessageCollection();
            DatabaseTestHelper.SetDefaultTestSessionSettings(testSession);

            Collection<PSObject> databaseCopies = null;
            using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
            {
                using (new MockHttpServer(
                    exceptionManager,
                    MockHttpServer.DefaultServerPrefixUri,
                    testSession))
                {
                    for (int i = 0; i < 20; i++)
                    {
                        databaseCopies = powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"Get-AzureSqlDatabaseCopy " +
                                @"-Context {0}",
                                contextVariable));
                        if (databaseCopies.Count == 0)
                        {
                            break;
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
            }

            Assert.AreEqual(0, databaseCopies.Count, "Expecting 0 Database Copy objects");
        }
    }
}

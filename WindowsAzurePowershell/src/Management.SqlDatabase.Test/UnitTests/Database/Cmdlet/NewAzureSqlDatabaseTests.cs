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
    public class NewAzureSqlDatabaseTests : TestBase
    {
        private HttpSessionCollection sessionCollection;

        [TestInitialize]
        public void SetupTest()
        {
            this.sessionCollection = HttpSessionCollection.Load("MockSessions.xml");
        }

        [TestCleanup]
        public void CleanupTest()
        {
            this.sessionCollection.Save("MockSessions.xml");
        }

        [TestMethod]
        public void NewAzureSqlDatabaseWithSqlAuth()
        {
            HttpSession testSession = this.sessionCollection.GetSession(
                "UnitTests.NewAzureSqlDatabaseWithSqlAuth");
            //testSession.ServiceBaseUri = new Uri("https://kvxv0mrmun.database.windows.net");
            testSession.RequestValidator =
                new Action<HttpMessage, HttpMessage.Request>(
                (expected, actual) =>
                {
                    Assert.AreEqual(expected.RequestInfo.Method, actual.Method);
                    Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                    switch (expected.Index)
                    {
                        // Request 0-2: Create context
                        case 0:
                        case 1:
                        case 2:
                            break;
                        // Request 3-4: Create testdb1
                        // Request 5-6: Create testdb2
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            Assert.IsTrue(
                                actual.Headers.Contains(DataServiceConstants.AccessTokenHeader),
                                "AccessToken header does not exist in the request");
                            Assert.AreEqual(
                                expected.RequestInfo.Headers[DataServiceConstants.AccessTokenHeader],
                                actual.Headers[DataServiceConstants.AccessTokenHeader],
                                "AccessToken header does not match");
                            Assert.IsTrue(
                                actual.Headers.Contains("x-ms-client-session-id"),
                                "session-id header does not exist in the request");
                            Assert.IsTrue(
                                actual.Headers.Contains("x-ms-client-request-id"),
                                "request-id header does not exist in the request");
                            Assert.IsTrue(
                                actual.Cookies.Contains(DataServiceConstants.AccessCookie),
                                "AccessCookie does not exist in the request");
                            Assert.AreEqual(
                                expected.RequestInfo.Cookies[DataServiceConstants.AccessCookie],
                                actual.Cookies[DataServiceConstants.AccessCookie],
                                "AccessCookie does not match");
                            Assert.IsTrue(
                                actual.Headers.Contains("DataServiceVersion"),
                                "DataServiceVersion header does not exist in the request");
                            Assert.AreEqual(
                                expected.RequestInfo.Headers["DataServiceVersion"],
                                actual.Headers["DataServiceVersion"],
                                "DataServiceVersion header does not match");
                            break;
                        default:
                            Assert.Fail("No more requests expected.");
                            break;
                    }
                });

            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                UnitTestHelper.ImportSqlDatabaseModule(powershell);
                UnitTestHelper.CreateTestCredential(powershell);

                using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
                {
                    // Create context with both ManageUrl and ServerName overriden
                    Collection<PSObject> database1, database2;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"$context = New-AzureSqlDatabaseServerContext " +
                                @"-ServerName kvxv0mrmun " +
                                @"-ManageUrl {0} " +
                                @"-Credential $credential",
                                MockHttpServer.DefaultServerPrefixUri.AbsoluteUri));
                        powershell.Streams.ClearStreams();

                        database1 = powershell.InvokeBatchScript(
                            @"$db = New-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1 " +
                            @"-Force",
                            @"$db");
                        database2 = powershell.InvokeBatchScript(
                            @"$db = New-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb2 " +
                            @"-Collation Japanese_CI_AS " +
                            @"-Edition Web " +
                            @"-MaxSizeGB 5 " +
                            @"-Force",
                            @"$db");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    Assert.IsTrue(
                        database1.Single().BaseObject is Services.Server.Database,
                        "Expecting a Database object");
                    Services.Server.Database database1Obj =
                        (Services.Server.Database)database1.Single().BaseObject;
                    Assert.AreEqual(
                        "testdb1",
                        database1Obj.Name,
                        "Expected db name to be testdb1");

                    Assert.IsTrue(
                        database2.Single().BaseObject is Services.Server.Database,
                        "Expecting a Database object");
                    Services.Server.Database database2Obj =
                        (Services.Server.Database)database2.Single().BaseObject;
                    Assert.AreEqual(
                        "testdb2",
                        database2Obj.Name,
                        "Expected db name to be testdb2");
                    Assert.AreEqual(
                        "Japanese_CI_AS",
                        database2Obj.CollationName,
                        "Expected collation to be Japanese_CI_AS");
                    Assert.AreEqual(
                        "Web",
                        database2Obj.Edition,
                        "Expected edition to be Web");
                    Assert.AreEqual(
                        5,
                        database2Obj.MaxSizeGB,
                        "Expected max size to be 5 GB");
                }
            }
        }
    }
}

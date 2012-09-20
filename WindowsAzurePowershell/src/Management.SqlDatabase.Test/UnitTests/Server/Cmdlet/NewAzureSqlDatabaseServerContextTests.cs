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

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.CloudService.Test;
using Microsoft.WindowsAzure.Management.Extensions;
using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
using Microsoft.WindowsAzure.Management.SqlDatabase.Server.Cmdlet;
using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;
using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server;
using Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.MockServer;
using Microsoft.WindowsAzure.Management.Test.Stubs;

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.Server.Cmdlet
{
    [TestClass]
    public class NewAzureSqlDatabaseServerContextTests : TestBase
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
        public void TestGetManageUrl()
        {
            NewAzureSqlDatabaseServerContext contextCmdlet = new NewAzureSqlDatabaseServerContext();

            contextCmdlet.ServerName = "server0001";
            Assert.AreEqual(
                new Uri("https://server0001.database.windows.net"),
                UnitTestHelper.InvokePrivate(
                    contextCmdlet,
                    "GetManageUrl",
                    NewAzureSqlDatabaseServerContext.ServerNameWithSqlAuthParamSet));
            contextCmdlet.ServerName = "server0002";
            Assert.AreEqual(
                new Uri("https://server0002.database.windows.net"),
                UnitTestHelper.InvokePrivate(
                    contextCmdlet,
                    "GetManageUrl",
                    NewAzureSqlDatabaseServerContext.ServerNameWithCertAuthParamSet));
            contextCmdlet.FullyQualifiedServerName = "server0003.database.windows.net";
            Assert.AreEqual(
                new Uri("https://server0003.database.windows.net"),
                UnitTestHelper.InvokePrivate(
                    contextCmdlet,
                    "GetManageUrl",
                    NewAzureSqlDatabaseServerContext.FullyQualifiedServerNameWithSqlAuthParamSet));
            contextCmdlet.FullyQualifiedServerName = "server0004.database.windows.net";
            Assert.AreEqual(
                new Uri("https://server0004.database.windows.net"),
                UnitTestHelper.InvokePrivate(
                    contextCmdlet,
                    "GetManageUrl",
                    NewAzureSqlDatabaseServerContext.FullyQualifiedServerNameWithCertAuthParamSet));
            contextCmdlet.ManageUrl = new Uri("https://server0005.database.windows.net");
            Assert.AreEqual(
                new Uri("https://server0005.database.windows.net"),
                UnitTestHelper.InvokePrivate(
                    contextCmdlet,
                    "GetManageUrl",
                    NewAzureSqlDatabaseServerContext.ManageUrlWithSqlAuthParamSet));
            contextCmdlet.ManageUrl = new Uri("https://server0006.database.windows.net");
            Assert.AreEqual(
                new Uri("https://server0006.database.windows.net"),
                UnitTestHelper.InvokePrivate(
                    contextCmdlet,
                    "GetManageUrl",
                    NewAzureSqlDatabaseServerContext.ManageUrlWithCertAuthParamSet));

            try
            {
                UnitTestHelper.InvokePrivate(
                    contextCmdlet,
                    "GetManageUrl",
                    "InvalidParamterSet");
                Assert.Fail("GetManageUrl with invalid parameter set should not succeed.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(Resources.UnknownParameterSet, ex.Message);
            }
        }

        [TestMethod]
        public void NewAzureSqlDatabaseServerContextWithSqlAuth()
        {
            HttpSession testSession = this.sessionCollection.GetSession(
                "UnitTests.NewAzureSqlDatabaseServerContextWithSqlAuth");
            testSession.RequestValidator =
                new Action<HttpMessage, HttpMessage.Request>(
                (expected, actual) =>
                {
                    Assert.AreEqual(expected.RequestInfo.UserAgent, actual.UserAgent);
                    switch (expected.Index)
                    {
                        // Request 0-1: Create context with both ManageUrl and ServerName overriden
                        case 0:
                            // GetAccessToken call
                            Assert.IsTrue(
                                actual.RequestUri.AbsoluteUri.EndsWith("GetAccessToken"),
                                "Incorrect Uri specified for GetAccessToken");
                            Assert.IsTrue(
                                actual.Headers.Contains("sqlauthorization"),
                                "sqlauthorization header does not exist in the request");
                            Assert.AreEqual(
                                expected.RequestInfo.Headers["sqlauthorization"],
                                actual.Headers["sqlauthorization"],
                                "sqlauthorization header does not match");
                            Assert.IsNull(
                                actual.RequestText,
                                "There should be no request text for GetAccessToken");
                            break;
                        case 1:
                            // $metadata call
                            Assert.IsTrue(
                                actual.RequestUri.AbsoluteUri.EndsWith("$metadata"),
                                "Incorrect Uri specified for $metadata");
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
                            break;
                        // Request 2-3: Create context with just ManageUrl and a derived servername,
                        // no need to validate
                        case 2:
                        case 3:
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
                    Collection<PSObject> serverContext;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        serverContext = powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"$context = New-AzureSqlDatabaseServerContext " +
                                @"-ServerName testserver " +
                                @"-ManageUrl {0} " +
                                @"-Credential $credential",
                                MockHttpServer.DefaultServerPrefixUri.AbsoluteUri),
                            @"$context");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    PSObject contextPsObject = serverContext.Single();
                    Assert.IsTrue(
                        contextPsObject.BaseObject is ServerDataServiceSqlAuth,
                        "Expecting a ServerDataServiceSqlAuth object");

                    // Create context with just ManageUrl and a derived servername
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        serverContext = powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"$context = New-AzureSqlDatabaseServerContext " +
                                @"-ManageUrl {0} " +
                                @"-Credential $credential",
                                MockHttpServer.DefaultServerPrefixUri.AbsoluteUri),
                            @"$context");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();

                    contextPsObject = serverContext.Single();
                    Assert.IsTrue(
                        contextPsObject.BaseObject is ServerDataServiceSqlAuth,
                        "Expecting a ServerDataServiceSqlAuth object");
                }
            }
        }

        [TestMethod]
        public void NewAzureSqlDatabaseServerContextWithSqlAuthNegativeCases()
        {
            HttpSession testSession = this.sessionCollection.GetSession(
                "UnitTests.NewAzureSqlDatabaseServerContextWithSqlAuthNegativeCases");

            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                UnitTestHelper.ImportSqlDatabaseModule(powershell);
                UnitTestHelper.CreateTestCredential(powershell);

                using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
                {
                    // Test warning when different $metadata is received.
                    Collection<PSObject> serverContext;
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        serverContext = powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"$context = New-AzureSqlDatabaseServerContext " +
                                @"-ServerName testserver " +
                                @"-ManageUrl {0} " +
                                @"-Credential $credential",
                                MockHttpServer.DefaultServerPrefixUri.AbsoluteUri),
                            @"$context");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(1, powershell.Streams.Warning.Count, "Should have warning!");
                    Assert.AreEqual(
                        Resources.WarningModelOutOfDate,
                        powershell.Streams.Warning.First().Message);
                    powershell.Streams.ClearStreams();

                    PSObject contextPsObject = serverContext.Single();
                    Assert.IsTrue(
                        contextPsObject.BaseObject is ServerDataServiceSqlAuth,
                        "Expecting a ServerDataServiceSqlAuth object");

                    // Test error case
                    using (new MockHttpServer(
                        exceptionManager,
                        MockHttpServer.DefaultServerPrefixUri,
                        testSession))
                    {
                        powershell.InvokeBatchScript(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                @"$context = New-AzureSqlDatabaseServerContext " +
                                @"-ServerName testserver " +
                                @"-ManageUrl {0} " +
                                @"-Credential $credential",
                                MockHttpServer.DefaultServerPrefixUri.AbsoluteUri),
                            @"$context");
                    }

                    Assert.AreEqual(1, powershell.Streams.Error.Count, "Should have errors!");
                    Assert.AreEqual(2, powershell.Streams.Warning.Count, "Should have warning!");
                    Assert.AreEqual(
                        "Test error message",
                        powershell.Streams.Error.First().Exception.Message);
                    Assert.IsTrue(
                        powershell.Streams.Warning.Any(
                            (w) => w.Message.StartsWith("Client Session Id:")),
                        "Client session Id not written to warning");
                    Assert.IsTrue(
                        powershell.Streams.Warning.Any(
                            (w) => w.Message.StartsWith("Client Request Id:")),
                        "Client request Id not written to warning");
                    powershell.Streams.ClearStreams();
                }
            }
        }
    }
}

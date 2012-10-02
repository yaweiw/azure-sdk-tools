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
        private HttpSessionCollection sessionCollection;
        private static readonly string realServerName = null;

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
        public void SetAzureSqlDatabaseWithSqlAuth()
        {
            HttpSession testSession = this.sessionCollection.GetSession(
                "UnitTests.SetAzureSqlDatabaseWithSqlAuth");
            if (realServerName != null)
            {
                testSession.ServiceBaseUri = new Uri(
                            Uri.UriSchemeHttps + Uri.SchemeDelimiter +
                            realServerName + DataServiceConstants.AzureSqlDatabaseDnsSuffix);
            }

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
                        case 3:
                        case 4:
                        // Request 5: Get testdb1
                        case 5:
                        // Request 6-7: Set testdb1 with new MaxSize
                        case 6:
                        case 7:
                        // Request 8: Get updated testdb1
                        case 8:
                        // Request 9-10: Set testdb1 with new name of testdb2
                        case 9:
                        case 10:
                        // Request 8: Get updated testdb2
                        case 11:
                            DatabaseTestHelper.ValidateHeadersForODataRequest(
                                expected.RequestInfo,
                                actual);
                            break;
                        default:
                            //Assert.Fail("No more requests expected.");
                            break;
                    }
                });
            testSession.ResponseModifier =
                new Action<HttpMessage>(
                    (message) =>
                    {
                        DatabaseTestHelper.FixODataResponseUri(
                            message.ResponseInfo,
                            testSession.ServiceBaseUri,
                            MockHttpServer.DefaultServerPrefixUri);
                    });

            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {
                UnitTestHelper.ImportSqlDatabaseModule(powershell);
                UnitTestHelper.CreateTestCredential(powershell);

                using (AsyncExceptionManager exceptionManager = new AsyncExceptionManager())
                {
                    // Create context with both ManageUrl and ServerName overriden
                    Collection<PSObject> database;
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
                        Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                        powershell.Streams.ClearStreams();

                        powershell.InvokeBatchScript(
                            @"New-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1 " +
                            @"-Force");

                        database = powershell.InvokeBatchScript(
                            @"Get-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1");

                        Assert.IsTrue(
                            database.Single().BaseObject is Services.Server.Database,
                            "Expecting a Database object");
                        Services.Server.Database databaseObj =
                            (Services.Server.Database)database.Single().BaseObject;
                        Assert.AreEqual("testdb1", databaseObj.Name, "Expected db name to be testdb1");
                        Assert.AreEqual("Web", databaseObj.Edition, "Expected edition to be Web");
                        Assert.AreEqual(1, databaseObj.MaxSizeGB, "Expected max size to be 1 GB");

                        database = powershell.InvokeBatchScript(
                            @"Set-AzureSqlDatabase " +
                            @"-Context $context " +
                            @"-DatabaseName testdb1 " +
                            @"-MaxSizeGB 5 " +
                            @"-Force " +
                            @"-PassThru");
                        Assert.IsTrue(
                            database.Single().BaseObject is Services.Server.Database,
                            "Expecting a Database object");
                        databaseObj = (Services.Server.Database)database.Single().BaseObject;
                        Assert.AreEqual("testdb1", databaseObj.Name, "Expected db name to be testdb1");
                        Assert.AreEqual("Web", databaseObj.Edition, "Expected edition to be Web");
                        Assert.AreEqual(5, databaseObj.MaxSizeGB, "Expected max size to be 5 GB");

                        database = powershell.InvokeBatchScript(
                           @"Set-AzureSqlDatabase " +
                           @"-Context $context " +
                           @"-DatabaseName testdb1 " +
                           @"-NewName testdb2 " +
                           @"-Force " +
                           @"-PassThru");
                        Assert.IsTrue(
                            database.Single().BaseObject is Services.Server.Database,
                            "Expecting a Database object");
                        databaseObj = (Services.Server.Database)database.Single().BaseObject;
                        Assert.AreEqual("testdb2", databaseObj.Name, "Expected db name to be testdb2");
                        Assert.AreEqual("Web", databaseObj.Edition, "Expected edition to be Web");
                        Assert.AreEqual(5, databaseObj.MaxSizeGB, "Expected max size to be 5 GB");
                    }

                    Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                    Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                    powershell.Streams.ClearStreams();
                }
            }
        }
    }
}

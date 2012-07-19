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

using System.Linq;
using System.ServiceModel;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.CloudService.Test;
using Microsoft.WindowsAzure.Management.SqlDatabase.Server.Cmdlet;
using Microsoft.WindowsAzure.Management.SqlDatabase.Services;
using Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTest;
using Microsoft.WindowsAzure.Management.Test.Stubs;

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test.UnitTests.Server.Cmdlet
{
    [TestClass]
    public class ServerCmdletTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void NewAzureSqlDatabaseServerProcessTest()
        {
            MockCommandRuntime commandRuntime = new MockCommandRuntime();
            SimpleSqlDatabaseManagement channel = new SimpleSqlDatabaseManagement();
            channel.NewServerThunk = ar =>
            {
                string newServerName = "NewServerName";

                Assert.AreEqual("MyLogin", ((NewSqlDatabaseServerInput)ar.Values["input"]).AdministratorLogin);
                Assert.AreEqual("MyPassword", ((NewSqlDatabaseServerInput)ar.Values["input"]).AdministratorLoginPassword);
                Assert.AreEqual("MyLocation", ((NewSqlDatabaseServerInput)ar.Values["input"]).Location);

                XmlElement operationResult = new XmlDocument().CreateElement("ServerName", "http://schemas.microsoft.com/sqlazure/2010/12/");
                operationResult.InnerText = newServerName;
                return operationResult;
            };

            NewAzureSqlDatabaseServer newAzureSqlDatabaseServer = new NewAzureSqlDatabaseServer(channel) { ShareChannel = true };
            newAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            var newServerResult = newAzureSqlDatabaseServer.NewAzureSqlDatabaseServerProcess("MyLogin", "MyPassword", "MyLocation");
            Assert.AreEqual("NewServerName", newServerResult.ServerName);
            Assert.AreEqual("Success", newServerResult.OperationStatus);

            Assert.AreEqual(0, commandRuntime.ErrorRecords.Count);
        }

        [TestMethod]
        public void GetAzureSqlDatabaseServerProcessTest()
        {
            MockCommandRuntime commandRuntime = new MockCommandRuntime();
            SimpleSqlDatabaseManagement channel = new SimpleSqlDatabaseManagement();
            SqlDatabaseServerList serverList = new SqlDatabaseServerList();

            channel.NewServerThunk = ar =>
            {
                string newServerName = "TestServer" + serverList.Count.ToString();
                serverList.Add(new SqlDatabaseServer()
                {
                    Name = newServerName,
                    AdministratorLogin = ((NewSqlDatabaseServerInput)ar.Values["input"]).AdministratorLogin,
                    Location = ((NewSqlDatabaseServerInput)ar.Values["input"]).Location
                });

                XmlElement operationResult = new XmlDocument().CreateElement("ServerName", "http://schemas.microsoft.com/sqlazure/2010/12/");
                operationResult.InnerText = newServerName;
                return operationResult;
            };

            channel.GetServersThunk = ar =>
            {
                return serverList;
            };

            // Add two servers
            NewAzureSqlDatabaseServer newAzureSqlDatabaseServer = new NewAzureSqlDatabaseServer(channel) { ShareChannel = true };
            newAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            var newServerResult = newAzureSqlDatabaseServer.NewAzureSqlDatabaseServerProcess("MyLogin0", "MyPassword0", "MyLocation0");
            Assert.AreEqual("TestServer0", newServerResult.ServerName);
            Assert.AreEqual("Success", newServerResult.OperationStatus);

            newServerResult = newAzureSqlDatabaseServer.NewAzureSqlDatabaseServerProcess("MyLogin1", "MyPassword1", "MyLocation1");
            Assert.AreEqual("TestServer1", newServerResult.ServerName);
            Assert.AreEqual("Success", newServerResult.OperationStatus);

            // Get all servers
            GetAzureSqlDatabaseServer getAzureSqlDatabaseServer = new GetAzureSqlDatabaseServer(channel) { ShareChannel = true };
            getAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            var getServerResult = getAzureSqlDatabaseServer.GetAzureSqlDatabaseServersProcess(null);
            Assert.AreEqual(2, getServerResult.Count());
            var firstServer = getServerResult.First();
            Assert.AreEqual("TestServer0", firstServer.ServerName);
            Assert.AreEqual("MyLogin0", firstServer.AdministratorLogin);
            Assert.AreEqual("MyLocation0", firstServer.Location);
            Assert.AreEqual("Success", firstServer.OperationStatus);
            var lastServer = getServerResult.Last();
            Assert.AreEqual("TestServer1", lastServer.ServerName);
            Assert.AreEqual("MyLogin1", lastServer.AdministratorLogin);
            Assert.AreEqual("MyLocation1", lastServer.Location);
            Assert.AreEqual("Success", lastServer.OperationStatus);

            // Get one server
            getServerResult = getAzureSqlDatabaseServer.GetAzureSqlDatabaseServersProcess("TestServer1");
            Assert.AreEqual(1, getServerResult.Count());
            firstServer = getServerResult.First();
            Assert.AreEqual("TestServer1", firstServer.ServerName);
            Assert.AreEqual("MyLogin1", firstServer.AdministratorLogin);
            Assert.AreEqual("MyLocation1", firstServer.Location);
            Assert.AreEqual("Success", firstServer.OperationStatus);

            Assert.AreEqual(0, commandRuntime.ErrorRecords.Count);
        }

        [TestMethod]
        public void RemoveAzureSqlDatabaseServerProcessTest()
        {
            MockCommandRuntime commandRuntime = new MockCommandRuntime();
            SimpleSqlDatabaseManagement channel = new SimpleSqlDatabaseManagement();
            SqlDatabaseServerList serverList = new SqlDatabaseServerList();

            channel.NewServerThunk = ar =>
            {
                string newServerName = "TestServer" + serverList.Count.ToString();
                serverList.Add(new SqlDatabaseServer()
                {
                    Name = newServerName,
                    AdministratorLogin = ((NewSqlDatabaseServerInput)ar.Values["input"]).AdministratorLogin,
                    Location = ((NewSqlDatabaseServerInput)ar.Values["input"]).Location
                });

                XmlElement operationResult = new XmlDocument().CreateElement("ServerName", "http://schemas.microsoft.com/sqlazure/2010/12/");
                operationResult.InnerText = newServerName;
                return operationResult;
            };

            channel.GetServersThunk = ar =>
            {
                return serverList;
            };

            channel.RemoveServerThunk = ar =>
            {
                string serverName = (string)ar.Values["serverName"];
                var serverToDelete = serverList.SingleOrDefault((server) => server.Name == serverName);
                if (serverToDelete == null)
                {
                    throw new CommunicationException("Server does not exist!");
                }

                serverList.Remove(serverToDelete);
            };

            // Add two servers
            NewAzureSqlDatabaseServer newAzureSqlDatabaseServer = new NewAzureSqlDatabaseServer(channel) { ShareChannel = true };
            newAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            var newServerResult = newAzureSqlDatabaseServer.NewAzureSqlDatabaseServerProcess("MyLogin0", "MyPassword0", "MyLocation0");
            Assert.AreEqual("TestServer0", newServerResult.ServerName);
            Assert.AreEqual("Success", newServerResult.OperationStatus);

            newServerResult = newAzureSqlDatabaseServer.NewAzureSqlDatabaseServerProcess("MyLogin1", "MyPassword1", "MyLocation1");
            Assert.AreEqual("TestServer1", newServerResult.ServerName);
            Assert.AreEqual("Success", newServerResult.OperationStatus);

            // Get all servers
            GetAzureSqlDatabaseServer getAzureSqlDatabaseServer = new GetAzureSqlDatabaseServer(channel) { ShareChannel = true };
            getAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            var getServerContext = getAzureSqlDatabaseServer.GetAzureSqlDatabaseServersProcess(null);
            Assert.AreEqual(2, getServerContext.Count());

            // Remove TestServer0
            RemoveAzureSqlDatabaseServer removeAzureSqlDatabaseServer = new RemoveAzureSqlDatabaseServer(channel) { ShareChannel = true };
            removeAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            var removeServerContext = removeAzureSqlDatabaseServer.RemoveAzureSqlDatabaseServerProcess("TestServer0");

            // Verify only one server is left
            getAzureSqlDatabaseServer = new GetAzureSqlDatabaseServer(channel) { ShareChannel = true };
            getAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            var getServerResult = getAzureSqlDatabaseServer.GetAzureSqlDatabaseServersProcess(null);
            Assert.AreEqual(1, getServerContext.Count());
            var firstServer = getServerResult.First();
            Assert.AreEqual("TestServer1", firstServer.ServerName);
            Assert.AreEqual("MyLogin1", firstServer.AdministratorLogin);
            Assert.AreEqual("MyLocation1", firstServer.Location);
            Assert.AreEqual("Success", firstServer.OperationStatus);
            
            Assert.AreEqual(0, commandRuntime.ErrorRecords.Count);

            // Remove TestServer0 again
            removeAzureSqlDatabaseServer = new RemoveAzureSqlDatabaseServer(channel) { ShareChannel = true };
            removeAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            removeServerContext = removeAzureSqlDatabaseServer.RemoveAzureSqlDatabaseServerProcess("TestServer0");
            Assert.AreEqual(1, commandRuntime.ErrorRecords.Count);
            Assert.IsTrue(commandRuntime.WarningOutput.Length > 0);
        }

        [TestMethod]
        public void SetAzureSqlDatabaseServerAdminPasswordProcessTest()
        {
            MockCommandRuntime commandRuntime = new MockCommandRuntime();
            SimpleSqlDatabaseManagement channel = new SimpleSqlDatabaseManagement();

            string password = null;

            channel.NewServerThunk = ar =>
            {
                string newServerName = "NewServerName";

                Assert.AreEqual("MyLogin", ((NewSqlDatabaseServerInput)ar.Values["input"]).AdministratorLogin);
                Assert.AreEqual("MyPassword", ((NewSqlDatabaseServerInput)ar.Values["input"]).AdministratorLoginPassword);
                Assert.AreEqual("MyLocation", ((NewSqlDatabaseServerInput)ar.Values["input"]).Location);
                password = ((NewSqlDatabaseServerInput)ar.Values["input"]).AdministratorLoginPassword;

                XmlElement operationResult = new XmlDocument().CreateElement("ServerName", "http://schemas.microsoft.com/sqlazure/2010/12/");
                operationResult.InnerText = newServerName;
                return operationResult;
            };

            channel.SetPasswordThunk = ar =>
            {
                Assert.AreEqual("NewServerName", (string)ar.Values["serverName"]);
                var passwordElement = (XmlElement)ar.Values["password"];
                Assert.AreEqual("AdministratorLoginPassword", passwordElement.Name);
                password = passwordElement.InnerText;
            };

            NewAzureSqlDatabaseServer newAzureSqlDatabaseServer = new NewAzureSqlDatabaseServer(channel) { ShareChannel = true };
            newAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            var newServerResult = newAzureSqlDatabaseServer.NewAzureSqlDatabaseServerProcess("MyLogin", "MyPassword", "MyLocation");
            Assert.AreEqual("NewServerName", newServerResult.ServerName);
            Assert.AreEqual("Success", newServerResult.OperationStatus);

            SetAzureSqlDatabaseServer setAzureSqlDatabaseServer = new SetAzureSqlDatabaseServer(channel) { ShareChannel = true };
            setAzureSqlDatabaseServer.CommandRuntime = commandRuntime;
            var setPasswordResult = setAzureSqlDatabaseServer.ResetAzureSqlDatabaseServerAdminPasswordProcess("NewServerName", "NewPassword");
            Assert.AreEqual("NewServerName", setPasswordResult.ServerName);
            Assert.AreEqual("Success", setPasswordResult.OperationStatus);
            Assert.AreEqual("NewPassword", password);
            
            Assert.AreEqual(0, commandRuntime.ErrorRecords.Count);
        }
    }
}

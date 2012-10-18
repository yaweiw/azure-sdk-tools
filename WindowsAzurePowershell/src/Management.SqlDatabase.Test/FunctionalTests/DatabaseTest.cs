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


namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test
{
    using System.Xml.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Test.Utilities;

    [TestClass]
    public class DatabaseTest
    {
        private string userName;
        private string password;
        private string manageUrl;

        private const string CreateDatabaseScript = @"Database\CreateDatabase.ps1";

        [TestInitialize]
        public void Setup()
        {
            XElement root = XElement.Load("SqlDatabaseSettings.xml");
            this.userName = root.Element("SqlAuthUserName").Value;
            this.password = root.Element("SqlAuthPassword").Value;
            this.manageUrl = root.Element("ManageUrl").Value;
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void CreateDatabase()
        {
            string arguments = string.Format("-Name \"{0}\" -ManageUrl \"{1}\" -UserName \"{2}\" -Password \"{3}\"", "testdbfromcmdlet", this.manageUrl, this.userName, this.password);
            bool testResult = PSScriptExecutor.ExecuteScript(DatabaseTest.CreateDatabaseScript, arguments);
            Assert.IsTrue(testResult);
        }
    }
}

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
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.SqlDatabase.Firewall.Cmdlet;
using Microsoft.WindowsAzure.Management.SqlDatabase.Test.Utilities;
using Microsoft.WindowsAzure.Management.Utilities;
using Microsoft.WindowsAzure.Management.XmlSchema;

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Test
{
    [TestClass]
    public class FunctionalTest
    {
        private string subscriptionID;
        private string serializedCert;
        private string serverLocation;

        private const string ServerTestScript = "CreateGetDeleteServer.ps1";
        private const string FirewallTestScript = "CreateGetDropFirewall.ps1";
        private const string ResetPasswordScript = "ResetPassword.ps1";
        private const string FormatValidationScript = "FormatValidation.ps1";

        [TestInitialize]
        public void Setup()
        {

            PublishData publishData = General.DeserializeXmlFile<PublishData>("Azure.publishsettings");
            PublishDataPublishProfile publishProfile = publishData.Items[0];
            this.serializedCert = publishProfile.ManagementCertificate;
            this.subscriptionID = publishProfile.Subscription[0].Id;

            XElement root = XElement.Load("SqlDatabaseSettings.xml");
            this.serverLocation = root.Element("ServerLocation").Value;

            new NewAzureSqlDatabaseServerFirewallRule();
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void ServerTest()
        {
            string arguments = string.Format("-subscriptionID \"{0}\" -serializedCert \"{1}\" -serverLocation \"{2}\"", this.subscriptionID, this.serializedCert, this.serverLocation);
            bool testResult = PSScriptExecutor.ExecuteScript(FunctionalTest.ServerTestScript, arguments);
            Assert.IsTrue(testResult);
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void FirewallTest()
        {
            string arguments = string.Format("-subscriptionID \"{0}\" -serializedCert \"{1}\" -serverLocation \"{2}\"", this.subscriptionID, this.serializedCert, this.serverLocation);
            bool testResult = PSScriptExecutor.ExecuteScript(FunctionalTest.FirewallTestScript, arguments);
            Assert.IsTrue(testResult);
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void ResetServerPassword()
        {
            string arguments = string.Format("-subscriptionID \"{0}\" -serializedCert \"{1}\" -serverLocation \"{2}\"", this.subscriptionID, this.serializedCert, this.serverLocation);
            bool testResult = PSScriptExecutor.ExecuteScript(FunctionalTest.ResetPasswordScript, arguments);
            Assert.IsTrue(testResult);
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void OutputObjectFormatValidation()
        {
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid() + ".txt");
            string arguments = string.Format("-subscriptionID \"{0}\" -serializedCert \"{1}\" -serverLocation \"{2}\" -OutputFile \"{3}\"", this.subscriptionID, this.serializedCert, this.serverLocation, outputFile);
            bool testResult = PSScriptExecutor.ExecuteScript(FunctionalTest.FormatValidationScript, arguments);
            Assert.IsTrue(testResult);

            string actualFormat = GetMaskedData(outputFile);
            Console.WriteLine(actualFormat);
            string expectedFormat = GetMaskedData("ExpectedFormat.txt");
            Assert.AreEqual(expectedFormat, actualFormat, "Format of output object didn't match");
        }

        private string GetMaskedData(string fileName)
        {
            string mask = "xxxxxxxxxx";
            // The code expects the first line of the file contains the list of dynamic data (such as servername, operation id) separated by comma.
            // These dynamic data will be replaced with xxxxxxxxxx.
            string dynamicContentLine = File.ReadAllLines(fileName)[0];
            string[] dynamicContents = dynamicContentLine.Split(',');
            string data = File.ReadAllText(fileName);

            foreach (string dynamicContent in dynamicContents)
            {
                data = data.Replace(dynamicContent, mask);
            }
            return data;
        }
    }
}

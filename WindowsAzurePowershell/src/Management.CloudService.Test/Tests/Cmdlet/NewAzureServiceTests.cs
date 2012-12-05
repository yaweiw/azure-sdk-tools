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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.Cmdlet
{
    using System.Management.Automation;
    using CloudService.Cmdlet;
    using CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.Model;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using System.IO;
    using System;

    [TestClass]
    public class NewAzureServiceTests : TestBase
    {
        FakeWriter writer;
        NewAzureServiceProjectCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            writer = new FakeWriter();
            cmdlet = new NewAzureServiceProjectCommand();
            cmdlet.Writer = writer;
        }

        [TestMethod]
        public void NewAzureServiceSuccessfull()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                // Setup
                string expectedName = "test";
                string expectedRootPath = Path.Combine(files.RootPath, expectedName);
                string expectedServiceCreatedMessage = string.Format(Resources.NewServiceCreatedMessage, expectedRootPath);
                cmdlet.ServiceName = expectedName;

                // Test
                cmdlet.NewAzureServiceProcess(files.RootPath, expectedName);

                // Assert
                PSObject actualPSObject = writer.OutputChannel[0] as PSObject;
                string actualServiceCreatedMessage = writer.VerboseChannel[0];
                
                Assert.AreEqual<string>(expectedName, actualPSObject.Members[Parameters.ServiceName].Value.ToString());
                Assert.AreEqual<string>(expectedRootPath, actualPSObject.Members[Parameters.RootPath].Value.ToString());
                Assert.AreEqual<string>(expectedServiceCreatedMessage, actualServiceCreatedMessage);
                AzureAssert.AzureServiceExists(expectedRootPath, Resources.GeneralScaffolding, expectedName);
            }
        }

        [TestMethod]
        public void NewAzureServiceWithInvalidNames()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                foreach (string name in TestData.Data.InvalidServiceName)
                {
                    cmdlet.ServiceName = name;
                    Testing.AssertThrows<ArgumentException>(() => cmdlet.ExecuteCmdlet());
                }
            }
        }
    }
}
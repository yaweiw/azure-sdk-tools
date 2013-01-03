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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.CloudService.Cmdlet;
    using Microsoft.WindowsAzure.Management.CloudService.Model;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;

    [TestClass]
    public class GetAzureServiceProjectRuntimesTests : TestBase
    {
        private const string serviceName = "AzureService";

        MockCommandRuntime mockCommandRuntime;

        private GetAzureServiceProjectRoleRuntimeCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            cmdlet = new GetAzureServiceProjectRoleRuntimeCommand();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet.CommandRuntime = mockCommandRuntime;
        }

        /// <summary>
        /// Verify that the correct runtimes are returned in the correct format from a given runtime manifest
        /// </summary>
        [TestMethod]
        public void TestGetRuntimes()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                string manifest = RuntimePackageHelper.GetTestManifest(files);
                CloudRuntimeCollection expected = service.GetCloudRuntimes(service.Paths, manifest);

                cmdlet.GetAzureRuntimesProcess(string.Empty, Path.Combine(files.RootPath, serviceName), manifest);

                List<CloudRuntimePackage> actual = mockCommandRuntime.OutputPipeline[0] as List<CloudRuntimePackage>;

                Assert.AreEqual<int>(expected.Count, actual.Count);
                Assert.IsTrue(expected.All<CloudRuntimePackage>( p => actual.Any<CloudRuntimePackage>(p2 => p2.PackageUri.Equals(p.PackageUri))));
            }
        }
    }
}

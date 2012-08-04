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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.CloudService.Model;
using Microsoft.WindowsAzure.Management.CloudService.Properties;
using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.Cmdlet
{
    [TestClass]
    public class GetAzureServiceProjectRuntimesTests : TestBase
    {
        private const string serviceName = "AzureService";

        [TestMethod]
        public void TestGetRuntimes()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                service.AddWebRole(Resources.NodeScaffolding);
                string manifest = RuntimeHelper.GetTestManifest(files);
                CloudRuntimeCollection collection = service.GetCloudRuntimes(service.Paths, manifest);
                RuntimeHelper.ValidateNodeList(manifest, collection);
            }
        }
    }
}

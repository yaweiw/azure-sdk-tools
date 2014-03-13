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

using Microsoft.Azure.Commands.ResourceManagement.Models;
using Microsoft.Azure.Commands.ResourceManagement.Templates;
using Moq;
using System.IO;
using System.Management.Automation;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManagement.Test.Resources
{
    public class SaveAzureResourceGroupGalleryTemplateCommandTests
    {
        private SaveAzureResourceGroupGalleryTemplateCommand cmdlet;

        private Mock<ResourcesClient> resourcesClientMock;

        private Mock<ICommandRuntime> commandRuntimeMock;

        public SaveAzureResourceGroupGalleryTemplateCommandTests()
        {
            resourcesClientMock = new Mock<ResourcesClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new SaveAzureResourceGroupGalleryTemplateCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                ResourceClient = resourcesClientMock.Object
            };
        }

        [Fact]
        public void SavesGalleryTemplateFile()
        {
            cmdlet.Name = "fileName";
            cmdlet.Path = "filePath";
            cmdlet.PassThru = true;

            cmdlet.ExecuteCmdlet();

            resourcesClientMock.Verify(f => f.DownloadGalleryTemplateFile("fileName", "filePath"), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Once());
        }

        [Fact]
        public void CreatesDefaultPathForGalleryTemplate()
        {
            string expectedPath = Path.Combine(Directory.GetCurrentDirectory(), "fileName");

            cmdlet.Name = "fileName";

            cmdlet.ExecuteCmdlet();

            resourcesClientMock.Verify(f => f.DownloadGalleryTemplateFile("fileName", expectedPath), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }
    }
}

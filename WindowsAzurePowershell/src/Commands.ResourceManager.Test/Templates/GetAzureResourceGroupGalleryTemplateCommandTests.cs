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

using Microsoft.Azure.Commands.ResourceManager.Models;
using Microsoft.Azure.Commands.ResourceManager.Templates;
using Microsoft.Azure.Gallery;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Test.Resources
{
    public class GetAzureResourceGroupGalleryTemplateCommandTests
    {
        private GetAzureResourceGroupGalleryTemplateCommand cmdlet;

        private Mock<GalleryTemplatesClient> galleryTemplatesClientMock;

        private Mock<ICommandRuntime> commandRuntimeMock;

        public GetAzureResourceGroupGalleryTemplateCommandTests()
        {
            galleryTemplatesClientMock = new Mock<GalleryTemplatesClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new GetAzureResourceGroupGalleryTemplateCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                GalleryTemplatesClient = galleryTemplatesClientMock.Object
            };
        }

        [Fact]
        public void GetsGalleryTemplates()
        {
            FilterGalleryTemplatesOptions options = new FilterGalleryTemplatesOptions()
            {
                Category = "category",
                Name = "hobba",
                Publisher = "Microsoft"
            };
            FilterGalleryTemplatesOptions actual = new FilterGalleryTemplatesOptions();
            List<GalleryItem> result = new List<GalleryItem>()
            {
                new GalleryItem()
                {
                    Publisher = "Microsoft",
                    Name = "T1"
                },
                new GalleryItem()
                {
                    Publisher = "Microsoft",
                    Name = "T2"
                },
            };
            galleryTemplatesClientMock.Setup(f => f.FilterGalleryTemplates(It.IsAny<FilterGalleryTemplatesOptions>()))
                .Returns(result)
                .Callback((FilterGalleryTemplatesOptions o) => actual = o);

            cmdlet.Category = options.Category;
            cmdlet.Name = options.Name;
            cmdlet.Publisher = options.Publisher;

            cmdlet.ExecuteCmdlet();

            Assert.Equal(2, result.Count);
            Assert.True(result.All(g => g.Publisher == "Microsoft"));

            commandRuntimeMock.Verify(f => f.WriteObject(result, true), Times.Once());
        }
    }
}

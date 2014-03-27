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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Commands.ResourceManager.Models;
using Microsoft.Azure.Gallery;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Test.Models
{
    public class ExtensionsTests
    {
        [Fact]
        public void ToPSGalleryItemCreatesANewItem()
        {
            var item = new GalleryItem()
                {
                    Name = "Name",
                    Publisher = "Microsoft",
                    DefinitionTemplates = new DefinitionTemplates()
                        {
                            DefaultDeploymentTemplateId = "DefaultUri",
                            DeploymentTemplateFileUrls = new Dictionary<string, string>()
                                {
                                    {"DefaultUri", "fakeurl"}
                                }
                        }
                };

            var psitem = item.ToPSGalleryItem();

            Assert.Equal(item.Name, psitem.Name);
            Assert.Equal(item.Publisher, psitem.Publisher);
            Assert.Equal(item.DefinitionTemplates.DefaultDeploymentTemplateId, psitem.DefinitionTemplates.DefaultDeploymentTemplateId);
            Assert.Equal(item.DefinitionTemplates.DeploymentTemplateFileUrls["DefaultUri"], psitem.DefinitionTemplates.DeploymentTemplateFileUrls["DefaultUri"]);
            Assert.Equal("fakeurl", psitem.DefinitionTemplatesText);
        }
    }
}

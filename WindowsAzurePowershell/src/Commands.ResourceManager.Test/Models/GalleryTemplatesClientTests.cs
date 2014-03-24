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
using Microsoft.Azure.Gallery;
using Microsoft.Azure.Gallery.Models;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Common.OData;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Commands.ResourceManager.Test.Models
{
    public class GalleryTemplatesClientTests : TestBase
    {
        private GalleryTemplatesClient galleryTemplatesClient;

        private Mock<IGalleryClient> galleryClientMock;

        private string templateFile = @"Resources\sampleTemplateFile.json";

        private string templateParameterFile = @"Resources\sampleTemplateParameterFile.json";

        public GalleryTemplatesClientTests()
        {
            galleryClientMock = new Mock<IGalleryClient>();
            galleryTemplatesClient = new GalleryTemplatesClient(galleryClientMock.Object);
        }

        [Fact]
        public void ConstructsDynamicParameter()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "__AllParameterSets" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "Mode1, Mode2, Mode3",
                DefaultValue = "Mode1",
                MaxLength = "5",
                MinLength = "1",
                Type = "string"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = galleryTemplatesClient.ConstructDynamicParameter(parameters, parameter);

            Assert.Equal("computeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(string), dynamicParameter.ParameterType);
            Assert.Equal(3, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateSetAttribute validateSetAttribute = (ValidateSetAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(3, validateSetAttribute.ValidValues.Count);
            Assert.True(validateSetAttribute.IgnoreCase);
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[0]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[1]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[2]));
            Assert.False(validateSetAttribute.ValidValues[0].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[1].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[2].Contains(' '));

            ValidateLengthAttribute validateLengthAttribute = (ValidateLengthAttribute)dynamicParameter.Attributes[2];
            Assert.Equal(int.Parse(value.MinLength), validateLengthAttribute.MinLength);
            Assert.Equal(int.Parse(value.MaxLength), validateLengthAttribute.MaxLength);
        }

        [Fact]
        public void ResolvesDuplicatedDynamicParameterName()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "__AllParameterSets" };
            string key = "Name";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "Mode1, Mode2, Mode3",
                MaxLength = "5",
                MinLength = "1",
                Type = "bool"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = galleryTemplatesClient.ConstructDynamicParameter(parameters, parameter);

            Assert.Equal(key + "FromTemplate", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(bool), dynamicParameter.ParameterType);
            Assert.Equal(3, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.True(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateSetAttribute validateSetAttribute = (ValidateSetAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(3, validateSetAttribute.ValidValues.Count);
            Assert.True(validateSetAttribute.IgnoreCase);
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[0]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[1]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[2]));
            Assert.False(validateSetAttribute.ValidValues[0].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[1].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[2].Contains(' '));

            ValidateLengthAttribute validateLengthAttribute = (ValidateLengthAttribute)dynamicParameter.Attributes[2];
            Assert.Equal(int.Parse(value.MinLength), validateLengthAttribute.MinLength);
            Assert.Equal(int.Parse(value.MaxLength), validateLengthAttribute.MaxLength);
        }

        [Fact]
        public void ResolvesDuplicatedDynamicParameterNameCaseInsensitive()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "__AllParameterSets" };
            string key = "name";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "Mode1, Mode2, Mode3",
                MaxLength = "5",
                MinLength = "1",
                Type = "bool"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = galleryTemplatesClient.ConstructDynamicParameter(parameters, parameter);

            Assert.Equal(key + "FromTemplate", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(bool), dynamicParameter.ParameterType);
            Assert.Equal(3, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.True(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateSetAttribute validateSetAttribute = (ValidateSetAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(3, validateSetAttribute.ValidValues.Count);
            Assert.True(validateSetAttribute.IgnoreCase);
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[0]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[1]));
            Assert.True(value.AllowedValues.Contains(validateSetAttribute.ValidValues[2]));
            Assert.False(validateSetAttribute.ValidValues[0].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[1].Contains(' '));
            Assert.False(validateSetAttribute.ValidValues[2].Contains(' '));

            ValidateLengthAttribute validateLengthAttribute = (ValidateLengthAttribute)dynamicParameter.Attributes[2];
            Assert.Equal(int.Parse(value.MinLength), validateLengthAttribute.MinLength);
            Assert.Equal(int.Parse(value.MaxLength), validateLengthAttribute.MaxLength);
        }

        [Fact]
        public void ConstructsDynamicParameterWithRangeValidation()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "__AllParameterSets" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "1-10",
                DefaultValue = "Mode1",
                Type = "securestring"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = galleryTemplatesClient.ConstructDynamicParameter(parameters, parameter);

            Assert.Equal("computeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(SecureString), dynamicParameter.ParameterType);
            Assert.Equal(2, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateRangeAttribute validateRangeAttribute = (ValidateRangeAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(1, validateRangeAttribute.MinRange);
            Assert.Equal(10, validateRangeAttribute.MaxRange);
        }

        [Fact]
        public void ConstructsDynamicParameterNoValidation()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "__AllParameterSets" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "",
                DefaultValue = "Mode1",
                Type = "securestring"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = galleryTemplatesClient.ConstructDynamicParameter(parameters, parameter);

            Assert.Equal("computeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(SecureString), dynamicParameter.ParameterType);
            Assert.Equal(1, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);
        }

        [Fact]
        public void ConstructsDynamicParameterWithMinRangeValidation()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "__AllParameterSets" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "5-",
                DefaultValue = "Mode1",
                Type = "securestring"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = galleryTemplatesClient.ConstructDynamicParameter(parameters, parameter);

            Assert.Equal("computeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(SecureString), dynamicParameter.ParameterType);
            Assert.Equal(2, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateRangeAttribute validateRangeAttribute = (ValidateRangeAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(5, validateRangeAttribute.MinRange);
            Assert.Equal(int.MaxValue, validateRangeAttribute.MaxRange);
        }

        [Fact]
        public void ConstructsDynamicParameterWithMaxRangeValidation()
        {
            string[] parameters = { "Name", "Location", "Mode" };
            string[] parameterSetNames = { "__AllParameterSets" };
            string key = "computeMode";
            TemplateFileParameter value = new TemplateFileParameter()
            {
                AllowedValues = "-200",
                DefaultValue = "Mode1",
                Type = "securestring"
            };
            KeyValuePair<string, TemplateFileParameter> parameter = new KeyValuePair<string, TemplateFileParameter>(key, value);

            RuntimeDefinedParameter dynamicParameter = galleryTemplatesClient.ConstructDynamicParameter(parameters, parameter);

            Assert.Equal("computeMode", dynamicParameter.Name);
            Assert.Equal(value.DefaultValue, dynamicParameter.Value);
            Assert.Equal(typeof(SecureString), dynamicParameter.ParameterType);
            Assert.Equal(2, dynamicParameter.Attributes.Count);

            ParameterAttribute parameterAttribute = (ParameterAttribute)dynamicParameter.Attributes[0];
            Assert.False(parameterAttribute.Mandatory);
            Assert.True(parameterAttribute.ValueFromPipelineByPropertyName);
            Assert.Equal(parameterSetNames[0], parameterAttribute.ParameterSetName);

            ValidateRangeAttribute validateRangeAttribute = (ValidateRangeAttribute)dynamicParameter.Attributes[1];
            Assert.Equal(0, validateRangeAttribute.MinRange);
            Assert.Equal(200, validateRangeAttribute.MaxRange);
        }

        [Fact]
        public void GetsDynamicParametersForTemplateFile()
        {
            RuntimeDefinedParameterDictionary result = galleryTemplatesClient.GetTemplateParametersFromFile(
                templateFile,
                null,
                null,
                new[] { "TestPS" });

            Assert.Equal(4, result.Count);

            Assert.Equal("string", result["string"].Name);
            Assert.Equal(typeof(string), result["String"].ParameterType);

            Assert.Equal("int", result["int"].Name);
            Assert.Equal(typeof(int), result["int"].ParameterType);

            Assert.Equal("securestring", result["securestring"].Name);
            Assert.Equal(typeof(SecureString), result["securestring"].ParameterType);

            Assert.Equal("bool", result["bool"].Name);
            Assert.Equal(typeof(bool), result["bool"].ParameterType);
        }

        [Fact]
        public void GetTemplateParametersFromFileMergesObjects()
        {
            Hashtable hashtable = new Hashtable();
            hashtable["Bool"] = true;
            hashtable["Foo"] = "bar";
            RuntimeDefinedParameterDictionary result = galleryTemplatesClient.GetTemplateParametersFromFile(
                templateFile,
                null,
                templateParameterFile,
                new[] { "TestPS" });

            Assert.Equal(4, result.Count);

            Assert.Equal("string", result["string"].Name);
            Assert.Equal(typeof(string), result["string"].ParameterType);
            Assert.Equal("myvalue", result["string"].Value);


            Assert.Equal("int", result["int"].Name);
            Assert.Equal(typeof(int), result["int"].ParameterType);
            Assert.Equal("12", result["int"].Value);

            Assert.Equal("bool", result["bool"].Name);
            Assert.Equal(typeof(bool), result["bool"].ParameterType);
            Assert.Equal("True", result["bool"].Value);
        }

        [Fact]
        public void FiltersGalleryTemplates()
        {
            string filterString = FilterString.Generate<ItemListFilter>(f => f.Publisher == "Microsoft");
            ItemListParameters actual = new ItemListParameters();
            galleryClientMock.Setup(f => f.Items.ListAsync(It.IsAny<ItemListParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ItemListResult
                {
                    Items = new List<GalleryItem>()
                    {
                        new GalleryItem()
                        {
                            Name = "Template1",
                            Publisher = "Microsoft"
                        },
                        new GalleryItem()
                        {
                            Name = "Template2",
                            Publisher = "Microsoft"
                        }
                    }
                }))
                .Callback((ItemListParameters p, CancellationToken c) => actual = p);

            FilterGalleryTemplatesOptions options = new FilterGalleryTemplatesOptions()
            {
                Publisher = "Microsoft"
            };

            List<GalleryItem> result = galleryTemplatesClient.FilterGalleryTemplates(options);

            Assert.Equal(2, result.Count);
            Assert.True(result.All(g => g.Publisher == "Microsoft"));
            Assert.Equal(filterString, actual.Filter);
        }

        [Fact]
        public void FiltersGalleryTemplatesUsingComplexQuery()
        {
            string filterString = "Publisher eq 'Microsoft' and CategoryIds/any(c: c eq 'awesome')";
            ItemListParameters actual = new ItemListParameters();
            galleryClientMock.Setup(f => f.Items.ListAsync(It.IsAny<ItemListParameters>(), new CancellationToken()))
                .Returns(Task.Factory.StartNew(() => new ItemListResult
                {
                    Items = new List<GalleryItem>()
                    {
                        new GalleryItem()
                        {
                            Name = "Template1",
                            Publisher = "Microsoft"
                        },
                        new GalleryItem()
                        {
                            Name = "Template2",
                            Publisher = "Microsoft"
                        }
                    }
                }))
                .Callback((ItemListParameters p, CancellationToken c) => actual = p);

            FilterGalleryTemplatesOptions options = new FilterGalleryTemplatesOptions()
            {
                Publisher = "Microsoft",
                Category = "awesome"
            };

            List<GalleryItem> result = galleryTemplatesClient.FilterGalleryTemplates(options);

            Assert.Equal(2, result.Count);
            Assert.Equal(filterString, actual.Filter);
        }

        [Fact]
        public void DownloadsGalleryTemplateFile()
        {
            string galleryTemplateFileName = "myFile";
            string expectedFilePath = Path.Combine(Path.GetTempPath(), galleryTemplateFileName + ".json");
            try
            {
                galleryClientMock.Setup(f => f.Items.GetAsync(galleryTemplateFileName, new CancellationToken()))
                                 .Returns(Task.Factory.StartNew(() => new ItemGetParameters()
                                 {
                                     Item = new GalleryItem()
                                     {
                                         Name = galleryTemplateFileName,
                                         Publisher = "Microsoft",
                                         DefinitionTemplates = new DefinitionTemplates()
                                         {
                                             DefaultDeploymentTemplateId = "DefaultUri",
                                             DeploymentTemplateFileUrls = new Dictionary<string, string>()
                                            {
                                                {"DefaultUri", "fakeurl"}
                                            }
                                         }
                                     }
                                 }));

                galleryTemplatesClient.DownloadGalleryTemplateFile(
                    galleryTemplateFileName,
                    expectedFilePath);

                Assert.Equal(string.Empty, File.ReadAllText(expectedFilePath));
            }
            finally
            {
                File.Delete(expectedFilePath);
            }
        }

        [Fact]
        public void DownloadsGalleryTemplateFileFromDirectoryName()
        {
            string galleryTemplateFileName = "myFile";
            string expectedFilePath = Path.Combine(Path.GetTempPath(), galleryTemplateFileName + ".json");
            try
            {
                galleryClientMock.Setup(f => f.Items.GetAsync(galleryTemplateFileName, new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ItemGetParameters()
                    {
                        Item = new GalleryItem()
                        {
                            Name = galleryTemplateFileName,
                            Publisher = "Microsoft",
                            DefinitionTemplates = new DefinitionTemplates()
                            {
                                DefaultDeploymentTemplateId = "DefaultUri",
                                DeploymentTemplateFileUrls = new Dictionary<string, string>()
                                {
                                    { "DefaultUri", "fakeurl" }
                                }
                            }
                        }
                    }));

                galleryTemplatesClient.DownloadGalleryTemplateFile(
                    galleryTemplateFileName,
                    Path.GetTempPath());

                Assert.Equal(string.Empty, File.ReadAllText(expectedFilePath));
            }
            finally
            {
                File.Delete(expectedFilePath);
            }
        }

        [Fact]
        public void DownloadsGalleryTemplateFileFromFileName()
        {
            string galleryTemplateFileName = "myFile.adeek";
            string expectedFilePath = Path.Combine(Path.GetTempPath(), galleryTemplateFileName + ".json");
            try
            {
                galleryClientMock.Setup(f => f.Items.GetAsync(galleryTemplateFileName, new CancellationToken()))
                    .Returns(Task.Factory.StartNew(() => new ItemGetParameters()
                    {
                        Item = new GalleryItem()
                        {
                            Name = galleryTemplateFileName,
                            Publisher = "Microsoft",
                            DefinitionTemplates = new DefinitionTemplates()
                            {
                                DefaultDeploymentTemplateId = "DefaultUri",
                                DeploymentTemplateFileUrls = new Dictionary<string, string>()
                                {
                                    {"DefaultUri", "http://onesdkauremustinvalid-uri12"}
                                }
                            }
                        }
                    }));

                galleryTemplatesClient.DownloadGalleryTemplateFile(
                    galleryTemplateFileName,
                    expectedFilePath);

                Assert.Equal(string.Empty, File.ReadAllText(expectedFilePath));
            }
            finally
            {
                File.Delete(expectedFilePath);
            }
        }
    }
}

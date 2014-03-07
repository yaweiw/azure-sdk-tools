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
using System.Runtime.Serialization.Formatters;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.Commands.Common.Test.Common
{
    [TestClass]
    public class JsonUtilitiesTests
    {
        [TestMethod]
        public void PatchWorksWithStandardStructures()
        {
            var originalProperties = new Dictionary<string, object>
                {
                    {"name", "site1"},
                    {"siteMode", "Standard"},
                    {"computeMode", "Dedicated"},
                    {"list", new [] {1,2,3}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key1", "value1"},
                            {"key2", "value2"}
                        }}};

            var originalPropertiesSerialized = JsonConvert.SerializeObject(originalProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var patchProperties = new Dictionary<string, object>
                {
                    {"siteMode", "Dedicated"},
                    {"newMode", "NewValue"},
                    {"list", new [] {4,5,6}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key3", "value3"}
                        }}};

            var patchPropertiesSerialized = JsonConvert.SerializeObject(patchProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            JToken actualJson = JToken.Parse(JsonUtilities.Patch(originalPropertiesSerialized, patchPropertiesSerialized));

            Assert.AreEqual("site1", actualJson["name"].ToObject<string>());
            Assert.AreEqual("Dedicated", actualJson["siteMode"].ToObject<string>());
            Assert.AreEqual("Dedicated", actualJson["computeMode"].ToObject<string>());
            Assert.AreEqual("NewValue", actualJson["newMode"].ToObject<string>());
            Assert.AreEqual("[4,5,6]", actualJson["list"].ToString(Formatting.None));
            Assert.AreEqual("value1", actualJson["misc"]["key1"].ToObject<string>());
            Assert.AreEqual("value2", actualJson["misc"]["key2"].ToObject<string>());
            Assert.AreEqual("value3", actualJson["misc"]["key3"].ToObject<string>());
        }

        [TestMethod]
        public void PatchWorksWithListInRoot()
        {
            var originalProperties = new[] {1, 2, 3};

            var originalPropertiesSerialized = JsonConvert.SerializeObject(originalProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var patchProperties = new[] {4, 5, 6};

            var patchPropertiesSerialized = JsonConvert.SerializeObject(patchProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var actual = JsonUtilities.Patch(originalPropertiesSerialized, patchPropertiesSerialized);

            Assert.AreEqual("[4,5,6]", actual);
        }

        [TestMethod]
        public void PatchWorksWithValueInRoot()
        {
            var originalProperties = "foo";

            var originalPropertiesSerialized = JsonConvert.SerializeObject(originalProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var patchProperties = "bar";

            var patchPropertiesSerialized = JsonConvert.SerializeObject(patchProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var actual = JsonUtilities.Patch(originalPropertiesSerialized, patchPropertiesSerialized);

            Assert.AreEqual("\"bar\"", actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PatchWorksWithMismatchInRoot()
        {
            var originalProperties = new Dictionary<string, object>
                {
                    {"name", "site1"},
                    {"siteMode", "Standard"},
                    {"computeMode", "Dedicated"},
                    {"list", new [] {1,2,3}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key1", "value1"},
                            {"key2", "value2"}
                        }}};

            var originalPropertiesSerialized = JsonConvert.SerializeObject(originalProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var patchProperties = "bar";

            var patchPropertiesSerialized = JsonConvert.SerializeObject(patchProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            JsonUtilities.Patch(originalPropertiesSerialized, patchPropertiesSerialized);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PatchWorksWithMismatchInBody()
        {
            var originalProperties = new Dictionary<string, object>
                {
                    {"name", "site1"},
                    {"siteMode", "Standard"},
                    {"computeMode", "Dedicated"},
                    {"list", new [] {1,2,3}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key1", "value1"},
                            {"key2", "value2"}
                        }}};

            var originalPropertiesSerialized = JsonConvert.SerializeObject(originalProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var patchProperties = new Dictionary<string, object>
                {
                    {"siteMode", "Dedicated"},
                    {"list", "foo"},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key3", "value3"}
                        }}};

            var patchPropertiesSerialized = JsonConvert.SerializeObject(patchProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            JToken.Parse(JsonUtilities.Patch(originalPropertiesSerialized, patchPropertiesSerialized));
        }

        [TestMethod]
        public void PatchWorksWithEmptyPatchValue()
        {
            var originalProperties = new Dictionary<string, object>
                {
                    {"name", "site1"},
                    {"siteMode", "Standard"},
                    {"computeMode", "Dedicated"},
                    {"list", new [] {1,2,3}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key1", "value1"},
                            {"key2", "value2"}
                        }}};

            var originalPropertiesSerialized = JsonConvert.SerializeObject(originalProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var actual = JsonUtilities.Patch(originalPropertiesSerialized, "");

            Assert.AreEqual(originalPropertiesSerialized, actual);
        }

        [TestMethod]
        public void PatchWorksWithNullPatchValue()
        {
            var originalProperties = new Dictionary<string, object>
                {
                    {"name", "site1"},
                    {"siteMode", "Standard"},
                    {"computeMode", "Dedicated"},
                    {"list", new [] {1,2,3}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key1", "value1"},
                            {"key2", "value2"}
                        }}};

            var originalPropertiesSerialized = JsonConvert.SerializeObject(originalProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var actual = JsonUtilities.Patch(originalPropertiesSerialized, null);

            Assert.AreEqual(originalPropertiesSerialized, actual);
        }

        [TestMethod]
        public void PatchWorksWithEmptySourceValue()
        {
            var patchProperties = new Dictionary<string, object>
                {
                    {"siteMode", "Dedicated"},
                    {"newMode", "NewValue"},
                    {"list", new [] {4,5,6}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key3", "value3"}
                        }}};

            var patchPropertiesSerialized = JsonConvert.SerializeObject(patchProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var actual = JsonUtilities.Patch("", patchPropertiesSerialized);

            Assert.AreEqual(patchPropertiesSerialized, actual);
        }

        [TestMethod]
        public void PatchWorksWithNullSourceValue()
        {
            var patchProperties = new Dictionary<string, object>
                {
                    {"siteMode", "Dedicated"},
                    {"newMode", "NewValue"},
                    {"list", new [] {4,5,6}},
                    {"misc", new Dictionary<string, object>
                        {
                            {"key3", "value3"}
                        }}};

            var patchPropertiesSerialized = JsonConvert.SerializeObject(patchProperties, new JsonSerializerSettings
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                TypeNameHandling = TypeNameHandling.None
            });

            var actual = JsonUtilities.Patch("", patchPropertiesSerialized);

            Assert.AreEqual(patchPropertiesSerialized, actual);
        }
    }
}

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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.WindowsAzure.Commands.Common.Test
{
    [TestClass]
    public class ConversionUtilitiesTests
    {
        [TestMethod]
        public void DeserializeJsonWorksForSimpleCases()
        {
            const string json1 =
                @"{
                     ""foo1"": ""bar1"",
                     ""foo2"": ""bar2"",
                     ""num"": 25,
                     ""address"": 
                     {
                         ""streetAddress"": ""123 Main Str"",
                         ""city"": ""Some City"",
                     },
                     ""list"": 
                     [
                         {
                           ""val1"": ""a"",
                           ""val2"": ""b""
                         },
                         {
                           ""val3"": ""c"",
                           ""val4"": ""d""
                         }
                     ]
                 }";

            Dictionary<string, object> result;
            result = JsonUtilities.DeserializeJson(json1);
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(2, ((Dictionary<string, object>)result["address"]).Count);
            Assert.AreEqual(2, ((List<object>)result["list"]).Count);
        }

        [TestMethod]
        public void DeserializeJsonWorksForEmptyObjects()
        {
            const string json1 =
                @"{
                     ""foo1"": ""bar1"",
                     ""foo2"": ""bar2"",
                     ""num"": 25,
                     ""address"": 
                     { },
                     ""list"": 
                     [ ]
                 }";

            Dictionary<string, object> result;
            result = JsonUtilities.DeserializeJson(json1);
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(0, ((Dictionary<string, object>)result["address"]).Count);
            Assert.AreEqual(0, ((List<object>)result["list"]).Count);
        }

        [TestMethod]
        public void DeserializeJsonAcceptsBadArguments()
        {
            Dictionary<string, object> result;
            result = JsonUtilities.DeserializeJson(null);
            Assert.IsNull(result);

            result = JsonUtilities.DeserializeJson(string.Empty);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void DeserializeJsonAcceptsBadJson()
        {
            const string json1 =
                @"{
                     ""foo1"": ""bar1"",
                     ""foo2"": ""bar2"",
                     ""num"": 25,
                     ""address"": 
                     {
                         ""streetAddress"": ""123 Main Str"",
                         ""city"": ""Some City"",
                     },
                     ""list"": 
                     [
                         {
                           ""val1"": ""a"",
                           ""val2"": ""b""
                         },
                         {
                           ""val3"": ""c"",
                           ""val4"": ""d""                         
                 }";

            Dictionary<string, object> result;
            result = JsonUtilities.DeserializeJson(json1);
            Assert.IsNull(result);
        }
    }
}

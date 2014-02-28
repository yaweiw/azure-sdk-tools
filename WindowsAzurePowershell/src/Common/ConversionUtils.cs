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

using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class ConversionUtils
    {
        public static Dictionary<string, object> ToDictionary(this Hashtable hashtable, bool addValueLayer)
        {
            if (hashtable == null)
            {
                return null;
            }
            else
            {
                var dictionary = new Dictionary<string, object>();
                foreach (var entry in hashtable.Cast<DictionaryEntry>())
                {
                    var valueAsHashtable = entry.Value as Hashtable;

                    if (valueAsHashtable != null)
                    {
                        dictionary[(string)entry.Key] = valueAsHashtable.ToDictionary(addValueLayer);
                    }
                    else
                    {
                        if (addValueLayer)
                        {
                            dictionary[(string) entry.Key] = new Hashtable() {{"value", entry.Value.ToString()}};
                        }
                        else
                        {
                            dictionary[(string) entry.Key] = entry.Value.ToString();
                        }
                    }
                }
                return dictionary;
            }
        }

        public static Hashtable ToHashtable<TV>(this IDictionary<string, TV> dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }
            else
            {
                return new Hashtable((Dictionary<string, TV>)dictionary);
            }
        }

        public static Dictionary<string, object> DeserializeJson(string jsonString)
        {
            Dictionary<string, object> result = new Dictionary<string,object>();
            if (jsonString == null)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return result;
            }

            try
            {
                JToken responseDoc = JToken.Parse(jsonString);

                if (responseDoc != null && responseDoc.Type == JTokenType.Object)
                {
                    result = DeserializeJObject(responseDoc as JObject);
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        private static Dictionary<string, object> DeserializeJObject(JObject jsonObject)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (jsonObject == null || jsonObject.Type == JTokenType.Null)
            {
                return result;
            }
            foreach (var property in jsonObject)
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    result[property.Key] = DeserializeJObject(property.Value as JObject);
                }
                else if (property.Value.Type == JTokenType.Array)
                {
                    result[property.Key] = DeserializeJArray(property.Value as JArray);
                }
                else
                {
                    result[property.Key] = DeserializeJValue(property.Value as JValue);
                }
            }
            return result;
        }

        private static List<object> DeserializeJArray(JArray jsonArray)
        {
            List<object> result = new List<object>();
            if (jsonArray == null || jsonArray.Type == JTokenType.Null)
            {
                return result;
            }
            foreach (var token in jsonArray)
            {
                if (token.Type == JTokenType.Object)
                {
                    result.Add(DeserializeJObject(token as JObject));
                }
                else if (token.Type == JTokenType.Array)
                {
                    result.Add(DeserializeJArray(token as JArray));
                }
                else
                {
                    result.Add(DeserializeJValue(token as JValue));
                }
            }
            return result;
        }

        private static object DeserializeJValue(JValue jsonObject)
        {
            if (jsonObject == null || jsonObject.Type == JTokenType.Null)
            {
                return null;
            }
            
            return jsonObject.Value;
        }
    }
}

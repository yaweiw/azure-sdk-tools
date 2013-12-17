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

namespace Microsoft.WindowsAzure.Commands.Utilities.Common.HttpRecorder
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class Names : Dictionary<string, Queue<string>>
    {
        /// <summary>
        /// Empty collection
        /// </summary>
        public Names() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="names"></param>
        public Names(Dictionary<string, Queue<string>> names) : base(names) { }

        public void EnqueueName(string testName, string assetName)
        {
            if (!base.ContainsKey(testName))
            {
                this[testName] = new Queue<string>();
            }
            this[testName].Enqueue(assetName);
        }

        public void EnqueueName(string testName, string[] assetNames)
        {
            if (!base.ContainsKey(testName))
            {
                this[testName] = new Queue<string>();
            }

            assetNames.ForEach(a => this[testName].Enqueue(a));
        }
    }
}

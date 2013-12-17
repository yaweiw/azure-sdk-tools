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

    public class Records : Dictionary<string, Queue<RecordEntry>>
    {
        private void EnqueueRecord(RecordEntry record, IRecordMatcher matcher)
        {
            string recordKey = matcher.GetMatchingKey(record);
            if (!base.ContainsKey(recordKey))
            {
                this[recordKey] = new Queue<RecordEntry>();
            }
            this[recordKey].Enqueue(record);
        }

        /// <summary>
        /// Empty collection
        /// </summary>
        public Records() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="records"></param>
        public Records(Dictionary<string, Queue<RecordEntry>> records) : base(records) { }

        public void AddRecord(RecordEntry record, IRecordMatcher matcher)
        {
            EnqueueRecord(record, matcher);
        }
    }
}

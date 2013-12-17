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
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class HttpMockServer : DelegatingHandler
    {
        private IRecordMatcher matcher;

        private Records records;

        private static string recordsTempDir;

        private static string recordsZipPath;

        private static string namesPath;

        private static string modeEnvironmentVariableName;

        private static HttpRecorderMode mode;

        private static int instanceCount;

        private static List<RecordEntry> rawRecords;

        private static Names names;

        private static HttpRecorderMode GetCurrentMode()
        {
            string input = Environment.GetEnvironmentVariable(modeEnvironmentVariableName);
            HttpRecorderMode mode;

            if (string.IsNullOrEmpty(input))
            {
                mode = HttpRecorderMode.None;
            }
            else
            {
                mode = (HttpRecorderMode)Enum.Parse(typeof(HttpRecorderMode), input, true);
            }

            return mode;
        }

        private static void ReadRecordedNames()
        {
            Dictionary<string, string[]> savedNames = General.DeserializeJson<Dictionary<string, string[]>>(namesPath)
                ?? new Dictionary<string, string[]>();
            savedNames.ForEach(r => names.EnqueueName(r.Key, r.Value));
        }

        private static void ReadRecordedSessions()
        {
            General.Decompress(recordsZipPath, recordsTempDir);
            foreach (string recordFile in Directory.GetFiles(recordsTempDir))
            {
                rawRecords = General.DeserializeJson<List<RecordEntry>>(recordFile) ?? new List<RecordEntry>();
            }
            Directory.Delete(recordsTempDir, true);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (mode == HttpRecorderMode.Playback)
            {
                // Will throw KeyNotFoundException if the request is not recorded
                return Tasks.FromResult(records[matcher.GetMatchingKey(request)].Dequeue().GetResponse());
            }
            else
            {
                return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>(response =>
                {
                    HttpResponseMessage result = response.Result;
                    if (mode == HttpRecorderMode.Record)
                    {
                        rawRecords.Add(new RecordEntry(result));
                    }

                    return result;
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            instanceCount--;

            if (mode == HttpRecorderMode.Record && instanceCount == 0)
            {
                Directory.CreateDirectory(recordsTempDir);
                int filesCount = 0;
                foreach (KeyValuePair<string, Queue<RecordEntry>> item in records)
                {
                    General.SerializeJson<List<RecordEntry>>(
                        rawRecords,
                        Path.Combine(recordsTempDir, "record" + filesCount++ + ".json"));
                }
                General.Compress(recordsTempDir, recordsZipPath);
                General.SerializeJson(names, namesPath);
            }
        }

        public static string GetAssetName(string testName)
        {
            if (GetCurrentMode() == HttpRecorderMode.Playback)
            {
                return names[testName].Dequeue();
            }
            else
            {
                string generated = "onesdk" + new Random().Next(9999);

                if (names.ContainsKey(testName))
                {
                    while (names[testName].Any(n => n.Equals(generated)))
                    {
                        generated = "onesdk" + new Random().Next(9999);
                    };
                }

                names.EnqueueName(testName, generated);

                return generated;
            }
        }

        public void InjectRecordEntry(RecordEntry record)
        {
            if (mode == HttpRecorderMode.Playback)
            {
                records.AddRecord(record, matcher);
            }
        }

        static HttpMockServer()
        {
            recordsZipPath = "records.zip";
            recordsTempDir = "RecordsTempDir";
            namesPath = "names.json";
            modeEnvironmentVariableName = "AZURE_TEST_MODE";
            mode = GetCurrentMode();
            names = new Names();
            rawRecords = new List<RecordEntry>();

            if (mode == HttpRecorderMode.Playback)
            {
                ReadRecordedSessions();
                ReadRecordedNames();
            }
        }

        public HttpMockServer(IRecordMatcher matcher)
        {
            this.matcher = matcher;
            this.records = new Records();
            rawRecords.ForEach(r => records.AddRecord(r, matcher));
            instanceCount++;
        }
    }
}

public enum HttpRecorderMode
{
    Record,
    Playback,
    None
}
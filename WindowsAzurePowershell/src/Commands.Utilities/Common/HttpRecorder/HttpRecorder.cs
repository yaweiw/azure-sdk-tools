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
    using Microsoft.WindowsAzure.Commands.Utilities.Common.HttpRecorder;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;
    using System.Xml.Serialization;

    public class HttpRecorder : DelegatingHandler
    {
        private Dictionary<string, RecordEntry> records;

        private HttpRecorderMode mode;

        private JavaScriptSerializer javaScriptSerializer;

        private static string recordsPath = "records.json";

        private void ReadRecordedSessions()
        {
            string json = File.ReadAllText(recordsPath);
            List<RecordEntry> savedRecords = javaScriptSerializer.Deserialize<List<RecordEntry>>(json) ?? new List<RecordEntry>();
            savedRecords.ForEach(r => records[RecordEntry.GetKey(r)] = r);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (mode == HttpRecorderMode.Playback)
            {
                // Will throw KeyNotFoundException if the request is not recorded
                return Tasks.FromResult(records[RecordEntry.GetKey(request)].GetResponse());
            }
            else
            {
                return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>(response =>
                {
                    HttpResponseMessage result = response.Result;
                    if (mode == HttpRecorderMode.Record)
                    {
                        records[RecordEntry.GetKey(result)] = new RecordEntry(result);
                    }

                    return result;
                });
            }
        }

        public static string ModeEnvironmentVariableName = "AZURE_TEST_MODE";

        public HttpRecorder()
        {
            javaScriptSerializer = new JavaScriptSerializer();
            records = new Dictionary<string, RecordEntry>();
            string input = Environment.GetEnvironmentVariable(ModeEnvironmentVariableName);
            
            if (string.IsNullOrEmpty(input))
            {
                mode = HttpRecorderMode.None;
            }
            else
            {
                mode = (HttpRecorderMode)Enum.Parse(typeof(HttpRecorderMode), input, true);

                if (mode == HttpRecorderMode.Playback)
                {
                    ReadRecordedSessions();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (mode == HttpRecorderMode.Record)
            {
                ReadRecordedSessions();
                File.WriteAllText(recordsPath, General.TryFormatJson(javaScriptSerializer.Serialize(records.Values.ToList())));
            }
        }
    }
}

public enum HttpRecorderMode
{
    Record,
    Playback,
    None
}
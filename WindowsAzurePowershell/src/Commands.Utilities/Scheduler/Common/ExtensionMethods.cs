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

namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Common
{
    using Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class ExtensionMethods
    {
        public static string ToCloudServiceName(this string region)
        {
            return Constants.CloudServiceNameFirst + region.Trim().Replace(" ", string.Empty) + Constants.CloudServiceNameSecond;
        }

        public static Dictionary<string, string> ToDictionary(this Hashtable hashTable)
        {
            return hashTable.Cast<DictionaryEntry>().ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString());
        }

        public static PSJobHistoryError ToJobHistoryError(this PSJobHistory history)
        {
            return new PSJobHistoryError
            {
                JobName = history.JobName,
                Status = history.Status,
                Details = history.Details,
                EndTime = history.EndTime,
                Occurence = history.Occurence,
                StartTime = history.StartTime,
                Retry = history.Retry
            };
        }

        public static bool IsErrorActionSet(this PSCreateJobParams jobRequest)
        {
            return (!string.IsNullOrEmpty(jobRequest.ErrorActionBody) ||
                !string.IsNullOrEmpty(jobRequest.ErrorActionMethod) ||
                jobRequest.ErrorActionUri !=null ||
                jobRequest.ErrorActionHeaders != null ||
                !string.IsNullOrEmpty(jobRequest.ErrorActionQueueBody) ||
                !string.IsNullOrEmpty(jobRequest.ErrorActionQueueName) ||
                !string.IsNullOrEmpty(jobRequest.ErrorActionSasToken) ||
                !string.IsNullOrEmpty(jobRequest.ErrorActionStorageAccount));
        }

        public static bool IsRecurrenceSet(this PSCreateJobParams jobRequest)
        {
            return (!string.IsNullOrEmpty(jobRequest.Frequency) ||
                jobRequest.Interval != null ||
                jobRequest.ExecutionCount != null ||
                jobRequest.EndTime != null );
        }

        public static bool IsActionSet(this PSCreateJobParams jobRequest)
        {
            return (!string.IsNullOrEmpty(jobRequest.Method) ||
                jobRequest.Uri != null ||
                !string.IsNullOrEmpty(jobRequest.Body) ||
                jobRequest.Headers != null);
        }

        public static bool IsStorageActionSet(this PSCreateJobParams jobRequest)
        {
            return (!string.IsNullOrEmpty(jobRequest.StorageAccount) ||
                !string.IsNullOrEmpty(jobRequest.QueueName) ||
                !string.IsNullOrEmpty(jobRequest.SasToken) ||
                !string.IsNullOrEmpty(jobRequest.StorageQueueMessage));
        }
    }
}

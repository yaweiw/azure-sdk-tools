using Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Common
{
    public static class ExtensionMethods
    {
        public static string toCloudServiceName(this string region)
        {
            return Constants.CloudServiceNameFirst + region.Trim().Replace(" ", string.Empty) + Constants.CloudServiceNameSecond;
        }

        public static Dictionary<string, string> toDictionary(this Hashtable hashTable)
        {
            return hashTable.Cast<DictionaryEntry>().ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString());
        }

        public static PSJobHistoryError toJobHistoryError(this PSJobHistory history)
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

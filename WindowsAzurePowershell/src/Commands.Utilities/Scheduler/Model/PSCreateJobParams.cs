using System;
using System.Collections;

namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSCreateJobParams
    {
        public string Region { get; set; }

        public string JobCollectionName { get; set; }

        public string JobName { get; set; }

        public string Method { get; set; }

        public Uri Uri { get; set; }

        public string StorageAccount { get; set; }

        public string QueueName { get; set; }

        public string SasToken { get; set; }

        public string StorageQueueMessage { get; set; }

        public string Body { get; set; }

        public DateTime? StartTime { get; set; }

        public int? Interval { get; set; }

        public string Frequency { get; set; }

        public DateTime? EndTime { get; set; }

        public int? ExecutionCount { get; set; }

        public string JobState { get; set; }

        public Hashtable Headers { get; set; }

        public string ErrorActionMethod { get; set; }

        public Uri ErrorActionUri { get; set; }

        public string ErrorActionBody { get; set; }

        public Hashtable ErrorActionHeaders { get; set; }

        public string ErrorActionStorageAccount { get; set; }

        public string ErrorActionQueueName { get; set; }

        public string ErrorActionSasToken { get; set; }

        public string ErrorActionQueueBody { get; set; }

    }
}

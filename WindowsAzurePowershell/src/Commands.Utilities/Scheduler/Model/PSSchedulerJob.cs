using System;

namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSSchedulerJob
    {
        public string JobCollectionName { get; internal set; }

        public string JobName { get; internal set; }

        public DateTime? Lastrun { get; internal set; }

        public DateTime? Nextrun { get; internal set; }

        public DateTime? StartTime { get; internal set; }

        public string Status { get; internal set; }

        public string Recurrence { get; internal set; }

        public int? Failures { get; internal set; }

        public int? Faults { get; internal set; }

        public int? Executions { get; internal set; }

        public string EndSchedule { get; internal set; }
        
    }
}

using System;

namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSJobHistory
    {
        public string JobName { get; internal set; }
        public string Status { get; internal set; }
        public int? Retry { get; internal set; }
        public int? Occurence { get; internal set; }
        public DateTime? StartTime { get; internal set; }
        public DateTime? EndTime { get; internal set; }
        public PSJobHistoryDetail Details { get; internal set; }
    }
}

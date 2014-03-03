
namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSJobDetail: PSSchedulerJob
    {
        public string CloudService { get; internal set; }

        public string ActionType { get; internal set; }
    }
}

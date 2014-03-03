
namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSJobHistoryError: PSJobHistory
    {
        public string ErrorAction { get; internal set; }
    }
}

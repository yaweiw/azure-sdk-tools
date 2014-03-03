
namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSJobHistoryHttpDetail: PSJobHistoryDetail
    {
        public string HostName { get; internal set; }
        public string Response { get; internal set; }
        public string ResponseBody { get; internal set; }
    }
}

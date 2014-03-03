
namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSJobHistoryStorageDetail: PSJobHistoryDetail
    {
        public string StorageAccountName { get; internal set; }
        public string StorageQueueName { get; internal set; }
        public string ResponseStatus { get; internal set; }
        public string ResponseBody { get; internal set; }
    }
}

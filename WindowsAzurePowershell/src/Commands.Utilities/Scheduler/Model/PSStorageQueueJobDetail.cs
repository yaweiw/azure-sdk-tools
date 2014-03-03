
namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSStorageQueueJobDetail: PSJobDetail
    {
        public string QueueMessage { get; internal set; }

        public string StorageAccountName { get; internal set; }

        public string StorageQueueName { get; internal set; }

        public string SasToken { get; internal set; }

    }
}

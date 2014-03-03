
namespace Microsoft.WindowsAzure.Commands.Utilities.Scheduler.Model
{
    public class PSJobCollection
    {
        public string CloudServiceName { get; internal set; }

        public string Location { get; internal set; }

        public string JobCollectionName { get; internal set; }

        public string Plan { get; internal set; }

        public string State { get; internal set; }

        public string MaxJobCount { get; internal set; }

        public string MaxRecurrence { get; internal set; }

        public string Uri { get; internal set; }
    }
}


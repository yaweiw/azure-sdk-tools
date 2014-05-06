namespace Microsoft.WindowsAzure.Commands.TrafficManager.Models
{
    using System.Runtime.Serialization;
    using Microsoft.WindowsAzure.Management.TrafficManager.Models;

    public class TrafficManagerEndpoint
    {
        [DataMember(IsRequired = true)]
        public string DomainName { get; set; }

        [DataMember(IsRequired = true)]
        public string Location { get; set; }

        [DataMember(IsRequired = true)]
        public EndpointType Type { get; set; }

        [DataMember(IsRequired = true)]
        public EndpointStatus Status { get; set; }

        [DataMember(IsRequired = true)]
        public DefinitionEndpointMonitorStatus MonitorStatus { get; set; }

        [DataMember(IsRequired = true)]
        public int Weight { get; set; }
    }
}
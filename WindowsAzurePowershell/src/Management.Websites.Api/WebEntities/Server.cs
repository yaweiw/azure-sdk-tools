//------------------------------------------------------------------------------
// <copyright file="Server.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public enum ServerStates
    {
        [EnumMember]
        NotReady = 0,
        [EnumMember]
        Offline = 1,
        [EnumMember]
        Installing = 2,
        [EnumMember]
        Ready = 3,
    }

    /// <summary>
    /// Class that represents a Server.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Server
    {

        /// <summary>
        /// Name of the server
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Current status
        /// </summary>
        [DataMember]
        public string Status { get; set; }

        /// <summary>
        /// Last line of the log
        /// </summary>
        [DataMember]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Server state
        /// </summary>
        [DataMember]
        public ServerStates ServerState { get; set; }

        /// <summary>
        /// Current cpu utilization
        /// </summary>
        [DataMember]
        public int CpuPercentage { get; set; }

        /// <summary>
        /// Current memory utilization
        /// </summary>
        [DataMember]
        public int MemoryPercentage { get; set; }

        /// <summary>
        /// Number of running sites
        /// </summary>
        [DataMember]
        public int RunningSitesNumber { get; set; }

        [DataMember(IsRequired = false)]
        public string[] SslBindings { get; set; }

        /// <summary>
        /// Compute mode
        /// </summary>
        [DataMember(IsRequired = false)]
        public ComputeModeOptions? ComputeMode { get; set; }

        /// <summary>
        /// Worker size
        /// </summary>
        [DataMember(IsRequired = false)]
        public WorkerSizeOptions? WorkerSize { get; set; }
    }

    /// <summary>
    /// Collection of servers
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Servers : List<Server>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Servers() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="sites"></param>
        public Servers(List<Server> list) : base(list) { }
    }

    /// <summary>
    /// Class that represents a SSL binding.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class ServerSslBinding
    {
        /// <summary>
        /// IPAddress of the binding
        /// </summary>
        [DataMember]
        public string IPAddress { get; set; }

        /// <summary>
        /// Current memory utilization
        /// </summary>
        [DataMember]
        public int Port { get; set; }

        /// <summary>
        /// Current memory utilization
        /// </summary>
        [DataMember(IsRequired = false)]
        public string HostName { get; set; }
    }

    /// <summary>
    /// Collection of servers
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class ServerSslBindings : List<ServerSslBinding>
    {
        /// <summary>
        /// Empty collection
        /// </summary>
        public ServerSslBindings() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="sites"></param>
        public ServerSslBindings(List<ServerSslBinding> list) : base(list) { }

        public ServerSslBindings(IEnumerable<ServerSslBinding> list) : base(list) { }
    }
}

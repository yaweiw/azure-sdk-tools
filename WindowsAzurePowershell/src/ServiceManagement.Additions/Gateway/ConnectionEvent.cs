// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Samples.Management.Service
{
    using System.Runtime.Serialization;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;

    [DataContract(Name = "LastEvent", Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.WindowsAzure.Network.Gateway")]
    public class ConnectionEvent : IExtensibleDataObject
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Data { get; set; }

        [DataMember(Order = 2)]
        public int Id { get; set; }

        [DataMember(Order = 3)]
        public string Message { get; set; }

        [DataMember(Order = 4)]
        public string Timestamp { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}

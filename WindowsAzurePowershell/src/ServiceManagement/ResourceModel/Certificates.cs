// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [CollectionDataContract(Name = "Certificates", ItemName = "Certificate", Namespace = Constants.ServiceManagementNS)]
    public class CertificateList : List<Certificate>
    {
        public CertificateList()
        {
        }

        public CertificateList(IEnumerable<Certificate> certificateList)
            : base(certificateList)
        {
        }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Certificate : IExtensibleDataObject
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public Uri CertificateUrl { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Thumbprint { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string ThumbprintAlgorithm { get; set; }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string Data { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class SubscriptionCertificate : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string PublicKey { get; set; }

        [DataMember(Order = 2)]
        public string Thumbprint { get; set; }

        [DataMember(Order = 3)]
        public string Data { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class CertificateFile : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Data { get; set; }

        [DataMember(Order = 2)]
        public string CertificateFormat { get; set; }

        [DataMember(Order = 3)]
        public string Password { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}

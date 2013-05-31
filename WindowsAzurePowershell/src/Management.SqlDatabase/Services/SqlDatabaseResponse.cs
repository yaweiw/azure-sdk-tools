﻿// ----------------------------------------------------------------------------------
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------﻿

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Services
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Get the database data contract.
    /// </summary>
    [DataContract(Name = "ServiceResource", Namespace = Constants.ServiceManagementNamespace)]
    public class SqlDatabaseResponse : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public string Id { get; set; }

        [DataMember(Order = 3)]
        public string Edition { get; set; }

        [DataMember(Order = 4)]
        public string MaxSizeGB { get; set; }

        [DataMember(Order = 5)]
        public string CollationName { get; set; }

        [DataMember(Order = 6)]
        public string CreationDate { get; set; }

        [DataMember(Order = 7)]
        public string IsFederationRoot { get; set; }

        [DataMember(Order = 8)]
        public string IsSystemObject { get; set; }

        [DataMember(Order = 9)]
        public string MaxSizeBytes { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }

    }
}

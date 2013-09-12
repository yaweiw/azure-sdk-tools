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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Services
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Get the database data contract.
    /// </summary>
    [DataContract(Name = "ServiceResource", Namespace = Constants.ServiceManagementNamespace)]
    public class SqlDatabaseResponse : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the name of the database
        /// </summary>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the id of the database
        /// </summary>
        [DataMember(Order = 2)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the edition of the database
        /// </summary>
        [DataMember(Order = 3)]
        public string Edition { get; set; }

        /// <summary>
        /// Gets or sets the max size in GB
        /// </summary>
        [DataMember(Order = 4)]
        public string MaxSizeGB { get; set; }

        /// <summary>
        /// Gets or sets the collation name
        /// </summary>
        [DataMember(Order = 5)]
        public string CollationName { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        [DataMember(Order = 6)]
        public string CreationDate { get; set; }

        /// <summary>
        /// Gets or sets whether or not the database is federation root
        /// </summary>
        [DataMember(Order = 7)]
        public string IsFederationRoot { get; set; }

        /// <summary>
        /// Gets or sets whether or not the database is a system object
        /// </summary>
        [DataMember(Order = 8)]
        public string IsSystemObject { get; set; }

        /// <summary>
        /// Gets the size of the database in MB.
        /// </summary>
        [DataMember(Order = 9)]
        public string SizeMB { get; set; }

        /// <summary>
        /// Gets or sets the maximum size in bytes.
        /// </summary>
        [DataMember(Order = 10)]
        public string MaxSizeBytes { get; set; }

        /// <summary>
        /// Gets the service objective currently assigned to the database.
        /// </summary>
        [DataMember(Order = 15)]
        public string ServiceObjectiveId { get; private set; }

        /// <summary>
        /// Gets the service objective pending assignment to the database.
        /// </summary>
        [DataMember(Order = 16)]
        public string AssignedServiceObjectiveId { get; private set; }

        /// <summary>
        /// Gets the service objective assignment state.
        /// </summary>
        [DataMember(Order = 17)]
        public string ServiceObjectiveAssignmentState { get; private set; }

        /// <summary>
        /// Gets the service objective assignment state description.
        /// </summary>
        [DataMember(Order = 18)]
        public string ServiceObjectiveAssignmentStateDescription { get; private set; }

        /// <summary>
        /// Gets the last known service objective assignment error code.
        /// </summary>
        [DataMember(Order = 19)]
        public string ServiceObjectiveAssignmentErrorCode { get; private set; }

        /// <summary>
        /// Gets the last known service objective assignment error description.
        /// </summary>
        [DataMember(Order = 20)]
        public string ServiceObjectiveAssignmentErrorDescription { get; private set; }

        /// <summary>
        /// Gets the last known successful service objective assignment date.
        /// </summary>
        [DataMember(Order = 21)]
        public string ServiceObjectiveAssignmentSuccessDate { get; private set; }

        /// <summary>
        /// Gets or sets the extension data
        /// </summary>
        public ExtensionDataObject ExtensionData { get; set; }
    }
}

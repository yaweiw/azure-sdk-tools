// ----------------------------------------------------------------------------------
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
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Services
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a server quota response to a web request
    /// </summary>
    [DataContract(Name = "ServiceResource", Namespace = Constants.ServiceManagementNamespace)]
    public class ServerQuotaResponse : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource type.
        /// </summary>
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the resource state.
        /// </summary>
        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the self link to the resource instance.
        /// </summary>
        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string SelfLink { get; set; }

        /// <summary>
        /// Gets or sets the link to the resource parent instance.
        /// </summary>
        [DataMember(Order = 5, EmitDefaultValue = false)]
        public string ParentLink { get; set; }

        /// <summary>
        /// Gets or sets the quota value.
        /// </summary>
        [DataMember(Order = 6, EmitDefaultValue = false)]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the extension data
        /// </summary>
        public ExtensionDataObject ExtensionData { get; set; }
    }
}

// ----------------------------------------------------------------------------------
//
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

namespace Microsoft.WindowsAzure.Commands.Utilities.Store.ResourceModel
{
    using System.Runtime.Serialization;
    using Microsoft.WindowsAzure.ServiceManagement;

    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Resource
    {
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public string ResourceProviderNamespace { get; set; }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Type { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        public string Plan { get; set; }

        [DataMember(Order = 5, EmitDefaultValue = false)]
        public string PromotionCode { get; set; }

        [DataMember(Order = 6, EmitDefaultValue = false)]
        public string SchemaVersion { get; set; }

        [DataMember(Order = 7, EmitDefaultValue = false)]
        public string ETag { get; set; }

        [DataMember(Order = 8, EmitDefaultValue = false)]
        public string State { get; set; }

        [DataMember(Order = 9, EmitDefaultValue = false)]
        public UsageMeterList UsageMeters { get; set; }

        [DataMember(Order = 10, EmitDefaultValue = false)]
        public OutputItemList OutputItems { get; set; }

        [DataMember(Order = 11, EmitDefaultValue = false)]
        public OperationStatus OperationStatus { get; set; }
    }
}

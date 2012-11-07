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
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// A key value/pair of an extended property of an object.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class ExtendedProperty : IExtensibleDataObject
    {
        /// <summary>
        /// The name of the extended property object.
        /// </summary>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// The value of the extended property.
        /// </summary>
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public string Value { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// A collection of extended property objects
    /// </summary>
    [CollectionDataContract(Name = "ExtendedPropertiesList", ItemName = "ExtendedProperty", Namespace = Constants.ServiceManagementNS)]
    public class ExtendedPropertiesList : List<ExtendedProperty>
    {
        /// <summary>
        /// Constructs a new default instance of ExtendedPropertiesList.
        /// </summary>
        public ExtendedPropertiesList()
        {
            // Empty
        }

        /// <summary>
        /// Constructs a new instance of ExtendedPropertiesList given another collection of extended properties.
        /// </summary>
        /// <param name="propertyList">Collection of extended properties</param>
        public ExtendedPropertiesList(IEnumerable<ExtendedProperty> propertyList)
            : base(propertyList)
        {
            // Empty
        }
    }
}
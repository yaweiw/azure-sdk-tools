/**
* Copyright Microsoft Corporation 2012
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Microsoft.WindowsAzure.ServiceManagement
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract(Name = "Error", Namespace = Constants.ServiceManagementNS)]
    public class ServiceManagementError : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string Code { get; set; }

        [DataMember(Order = 2)]
        public string Message { get; set; }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public ConfigurationWarningsList ConfigurationWarnings { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }


    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class ConfigurationWarning : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string WarningCode { get; set; }

        [DataMember(Order = 2)]
        public string WarningMessage { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }

        public override string ToString()
        {
            return string.Format("WarningCode:{0} WarningMessage:{1}", WarningCode, WarningMessage);
        }
    }

    [CollectionDataContract(Namespace = Constants.ServiceManagementNS)]
    public class ConfigurationWarningsList : List<ConfigurationWarning>
    {
        public override string ToString()
        {
            StringBuilder warnings = new StringBuilder(string.Format("ConfigurationWarnings({0}):\n", this.Count));

            foreach (ConfigurationWarning warning in this)
            {
                warnings.Append(warning + "\n");
            }
            return warnings.ToString();
        }
    }
}

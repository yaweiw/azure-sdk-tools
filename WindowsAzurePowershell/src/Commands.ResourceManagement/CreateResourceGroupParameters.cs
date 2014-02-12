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

using Microsoft.WindowsAzure.Commands.Utilities.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Commands.ResourceManagement
{
    public class CreateResourceGroupParameters
    {
        public string Name { get; set; }

        public string Location { get; set; }

        public string DeploymentName { get; set; }

        public string GalleryTemplateName { get; set; }

        public string TemplateFile { get; set; }

        public Hashtable ParameterObject { get; set; }

        public string TemplateVersion { get; set; }

        public string TemplateHash { get; set; }

        public string TemplateHashAlgorithm { get; set; }

        public string StorageAccountName { get; set; }

        public bool Async { get; set; }

        public CreateResourceGroupParameters(Dictionary<string, object> dictionary)
        {
            Name = General.GetValue<string>(dictionary, "Name");
            Location = General.GetValue<string>(dictionary, "Location");
            DeploymentName = General.GetValue<string>(dictionary, "DeploymentName");
            GalleryTemplateName = General.GetValue<string>(dictionary, "GalleryTemplateName");
            TemplateFile = General.GetValue<string>(dictionary, "TemplateFile");
            ParameterObject = General.GetValue<Hashtable>(dictionary, "ParameterObject");
            TemplateVersion = General.GetValue<string>(dictionary, "TemplateVersion");
            TemplateHash = General.GetValue<string>(dictionary, "TemplateHash");
            TemplateHashAlgorithm = General.GetValue<string>(dictionary, "TemplateHashAlgorithm");
            StorageAccountName = General.GetValue<string>(dictionary, "StorageAccountName");
            Async = General.GetValue<bool>(dictionary, "Async");
        }
    }
}

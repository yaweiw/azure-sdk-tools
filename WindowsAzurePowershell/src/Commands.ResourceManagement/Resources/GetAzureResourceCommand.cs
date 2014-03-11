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

using Microsoft.Azure.Commands.ResourceManagement.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.ResourceManagement
{
    /// <summary>
    /// Get an existing resource.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureResource", DefaultParameterSetName = BaseParameterSetName), OutputType(typeof(List<PSResource>))]
    public class GetAzureResourceCommand : ResourceBaseCmdlet
    {
        internal const string BaseParameterSetName = "List resources";
        internal const string ParameterSetNameWithId = "Get a single resource";

        [Alias("ResourceName")]
        [Parameter(ParameterSetName = ParameterSetNameWithId, Position=0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(ParameterSetName = ParameterSetNameWithId, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name.")]
        [Parameter(ParameterSetName = BaseParameterSetName, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(ParameterSetName = ParameterSetNameWithId, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource type. In the format ResourceProvider/type.")]
        [Parameter(ParameterSetName = BaseParameterSetName, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource type. In the format ResourceProvider/type.")]
        [ValidateNotNullOrEmpty]
        public string ResourceType { get; set; }

        [Parameter(ParameterSetName = ParameterSetNameWithId, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the parent resource if needed. In the format of greatgranda/grandpa/dad.")]
        [ValidateNotNullOrEmpty]
        public string ParentResourceName { get; set; }

        public override void ExecuteCmdlet()
        {
            BasePSResourceParameters parameters = new BasePSResourceParameters()
            {
                Name = Name,
                ResourceGroupName = ResourceGroupName,
                ResourceType = ResourceType,
                ParentResourceName = ParentResourceName,
            };

            WriteObject(ResourceClient.FilterPSResources(parameters), true);
        }
    }
}

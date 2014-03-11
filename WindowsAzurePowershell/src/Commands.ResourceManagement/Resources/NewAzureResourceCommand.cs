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

using System;
using Microsoft.Azure.Commands.ResourceManagement.Models;
using System.Collections;
using System.Management.Automation;
using Microsoft.Azure.Commands.ResourceManagement.Properties;
using Microsoft.Azure.Management.Resources;

namespace Microsoft.Azure.Commands.ResourceManagement
{
    /// <summary>
    /// Creates a new resource.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureResource"), OutputType(typeof(PSResource))]
    public class NewAzureResourceCommand : ResourceBaseCmdlet
    {
        [Alias("ResourceName")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource name.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group name.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource type. In the format ResourceProvider/type.")]
        [ValidateNotNullOrEmpty]
        public string ResourceType { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource location.")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the parent resource if needed. In the format of greatgranda/grandpa/dad.")]
        [ValidateNotNullOrEmpty]
        public string ParentResourceName { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "A hash table which represents resource properties.")]
        public Hashtable PropertyObject { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            CreatePSResourceParameters parameters = new CreatePSResourceParameters()
            {
                Name = Name,
                ResourceGroupName = ResourceGroupName,
                ResourceType = ResourceType,
                Location = Location,
                ParentResourceName = ParentResourceName,
                PropertyObject = PropertyObject,
                Force = Force.IsPresent,
                ConfirmAction = ConfirmAction
            };

            WriteObject(ResourceClient.CreateResource(parameters));
        }
    }
}

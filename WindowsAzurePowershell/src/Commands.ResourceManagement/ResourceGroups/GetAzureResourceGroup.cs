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

using System.Collections;
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Commands.ResourceManagement.Entities;

namespace Microsoft.Azure.Commands.ResourceManagement.ResourceGroups
{
    /// <summary>
    /// Creates a new resource group.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureResourceGroup"), OutputType(typeof(Group))]
    public class GetAzureResourceGroup : ResourceBaseCmdlet
    {
        [Parameter(ParameterSetName = "name", 
            Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the resource group.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(ParameterSetName = "tag", 
            Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Tags of the resource group.")]
        public Hashtable Tag
        {
            get;
            set;
        }

        public override void ExecuteCmdlet()
        {
            var groups = ResourceClient.GetResourceGroups(this);
            if (groups != null)
            {
                var groupsList = groups.ToList();
                if (groupsList.Count == 1)
                {
                    WriteObject(groupsList[0]);
                }
                else
                {
                    WriteObject(groupsList);
                }
            }
        }
    }
}

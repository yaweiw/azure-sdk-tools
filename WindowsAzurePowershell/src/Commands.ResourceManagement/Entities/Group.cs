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
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.Azure.Commands.ResourceManagement.Entities
{
    public class Group
    {
        public string Name { get; set; }

        public string Location { get; set; }

        public Hashtable Tag { get; internal set; }

        public static Group CreateFromResultGroup(ResourceGroup resourceGroup)
        {
            if (resourceGroup == null)
            {
                return null;
            }

            var group = new Group();
            group.Location = resourceGroup.Location;
            group.Name = resourceGroup.Name;
            if (resourceGroup.Tags != null)
            {
                group.Tag = resourceGroup.Tags.ToHashtable();
            }
            return group;
        }
    }
}

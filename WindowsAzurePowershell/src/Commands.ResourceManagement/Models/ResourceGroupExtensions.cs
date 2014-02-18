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

using Microsoft.Azure.Management.Resources.Models;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public static class ResourceGroupExtensions
    {
        public static PSResourceGroup ToPSResourceGroup(this ResourceGroup resourceGroup, ResourcesClient client)
        {
            List<Resource> resources = client.FilterResources(new FilterResourcesOptions() { ResourceGroup = resourceGroup.Name });
            return new PSResourceGroup()
            {
                Name = resourceGroup.Name,
                Location = resourceGroup.Location,
                Resources = resources,
                ResourcesTable = ConstructResourcesTable(resources)
            };
        }

        private static string ConstructResourcesTable(List<Resource> resources)
        {
            StringBuilder resourcesTable = new StringBuilder();

            if (resources.Count > 0)
            {
                string rowFormat = "{0, -15}  {1, -25}  {2, -10}\r\n";
                resourcesTable.AppendLine();
                resourcesTable.AppendFormat(rowFormat, "Name", "Type", "Location");
                resourcesTable.AppendFormat(rowFormat, GenerateSeparator(15, "="), GenerateSeparator(25, "="), GenerateSeparator(10, "="));

                foreach (Resource resource in resources)
                {
                    resourcesTable.AppendFormat(rowFormat, resource.Name, resource.Type, resource.Location);
                }
            }

            return resourcesTable.ToString();
        }

        private static string GenerateSeparator(int amount, string separator)
        {
            string result = string.Empty;
            while (amount-- != 0) result += separator;
            return result;
        }
    }
}

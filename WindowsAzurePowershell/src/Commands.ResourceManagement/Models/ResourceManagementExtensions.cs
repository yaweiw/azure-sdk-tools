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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public static class ResourceManagementExtensions
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

        public static PSResourceGroupDeployment ToPSResourceGroupDeployment(this DeploymentOperationsCreateResult result, ResourcesClient client)
        {
            Dictionary<string, DeploymentVariable> outputs = new Dictionary<string, DeploymentVariable>();
            Dictionary<string, DeploymentVariable> parameters = new Dictionary<string, DeploymentVariable>();
            PSResourceGroupDeployment deploymentObject = new PSResourceGroupDeployment();

            if (result != null)
            {
                deploymentObject.Name = result.Name;
                deploymentObject.ResourceGroup = result.ResourceGroup;

                if (result.Properties != null)
                {
                    deploymentObject.Mode = result.Properties.Mode;
                    deploymentObject.ProvisioningState = result.Properties.ProvisioningState;
                    deploymentObject.TemplateLink = result.Properties.TemplateLink;
                    deploymentObject.Timestamp = result.Properties.Timestamp;

                    if (!string.IsNullOrEmpty(result.Properties.Outputs))
                    {
                        outputs = JsonConvert.DeserializeObject<Dictionary<string, DeploymentVariable>>(result.Properties.Outputs);
                        deploymentObject.Outputs = outputs;
                        deploymentObject.OutputsString = ToString(outputs);
                    }

                    if (!string.IsNullOrEmpty(result.Properties.Parameters))
                    {
                        parameters = JsonConvert.DeserializeObject<Dictionary<string, DeploymentVariable>>(result.Properties.Parameters);
                        deploymentObject.Parameters = parameters;
                        deploymentObject.ParametersString = ToString(parameters);
                    }

                    if (result.Properties.TemplateLink != null)
                    {
                        deploymentObject.TemplateLinkString = ToString(result.Properties.TemplateLink);
                    }
                }
            }

            return deploymentObject;
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

        private static string ToString(TemplateLink templateLink)
        {
            StringBuilder result = new StringBuilder();

            if (templateLink != null)
            {
                result.AppendLine();
                result.AppendLine(string.Format("{0, -15}: {1}", "Uri", templateLink.Uri));
                result.AppendLine(string.Format("{0, -15}: {1}", "ContentVersion", templateLink.ContentVersion));
                result.AppendLine(string.Format("{0, -15}:", "ContentHash"));

                if (templateLink.ContentHash != null)
                {
                    result.AppendLine(string.Format("{0, -25}:", "Algorithm", templateLink.ContentHash.Algorithm));
                    result.AppendLine(string.Format("{0, -25}:", "Value", templateLink.ContentHash.Value));
                }
            }

            return result.ToString();
        }

        private static string ToString(Dictionary<string, DeploymentVariable> dictionary)
        {
            StringBuilder result = new StringBuilder();

            if (dictionary.Count > 0)
            {
                string rowFormat = "{0, -15}  {1, -25}  {2, -10}\r\n";
                result.AppendLine();
                result.AppendFormat(rowFormat, "Name", "Type", "Value");
                result.AppendFormat(rowFormat, GenerateSeparator(15, "="), GenerateSeparator(25, "="), GenerateSeparator(10, "="));

                foreach (KeyValuePair<string, DeploymentVariable> pair in dictionary)
                {
                    result.AppendFormat(rowFormat, pair.Key, pair.Value.Type, pair.Value.Value);
                }
            }

            return result.ToString();
        }
    }
}
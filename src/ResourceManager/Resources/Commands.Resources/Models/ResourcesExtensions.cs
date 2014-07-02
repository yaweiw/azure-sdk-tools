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

using Microsoft.Azure.Gallery;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Management.Monitoring.Events.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Azure.Commands.Resources.Models
{
    public static class ResourcesExtensions
    {
        public static PSResourceGroup ToPSResourceGroup(this ResourceGroup resourceGroup, ResourcesClient client)
        {
            List<PSResource> resources = client.FilterResources(new FilterResourcesOptions { ResourceGroup = resourceGroup.Name })
                .Select(r => r.ToPSResource(client)).ToList();
            return new PSResourceGroup()
            {
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Resources = resources,
                ResourcesTable = ConstructResourcesTable(resources),
                ProvisioningState = resourceGroup.ProvisioningState
            };
        }

        public static PSResourceGroupDeployment ToPSResourceGroupDeployment(this DeploymentGetResult result, string resourceGroup)
        {
            PSResourceGroupDeployment deployment = new PSResourceGroupDeployment();

            if (result != null)
            {
                deployment = CreatePSResourceGroupDeployment(result.Deployment.DeploymentName, resourceGroup, result.Deployment.Properties);
            }

            return deployment;
        }

        public static PSResourceGroupDeployment ToPSResourceGroupDeployment(this DeploymentOperationsCreateResult result)
        {
            PSResourceGroupDeployment deployment = new PSResourceGroupDeployment();

            if (result != null)
            {
                deployment = CreatePSResourceGroupDeployment(result.Name, result.ResourceGroup, result.Properties);
            }

            return deployment;
        }

        public static PSResourceGroupDeployment ToPSResourceGroupDeployment(this Deployment result, string resourceGroup)
        {
            PSResourceGroupDeployment deployment = new PSResourceGroupDeployment();

            if (result != null)
            {
                deployment = CreatePSResourceGroupDeployment(result.DeploymentName, resourceGroup, result.Properties);
            }

            return deployment;
        }

        public static PSResourceManagerError ToPSResourceManagerError(this ResourceManagementError error)
        {
            return new PSResourceManagerError
                {
                    Code = error.Code,
                    Message = error.Message
                };
        }

        public static PSResource ToPSResource(this Resource resource, ResourcesClient client)
        {
            ResourceIdentifier identifier = new ResourceIdentifier(resource.Id);
            return new PSResource()
            {
                Name = identifier.ResourceName,
                Location = resource.Location,
                ResourceType = identifier.ResourceType,
                ResourceGroupName = identifier.ResourceGroupName,
                ParentResource = identifier.ParentResource,
                Properties = JsonUtilities.DeserializeJson(resource.Properties),
                PropertiesText = resource.Properties,
                Tags = resource.Tags.ToHashtable()
            };
        }

        public static PSResourceProviderType ToPSResourceProviderType(this ProviderResourceType resourceType, string providerNamespace)
        {
            PSResourceProviderType result = new PSResourceProviderType();
            if (resourceType != null)
            {
                resourceType.Locations = resourceType.Locations ?? new List<string>();
                for (int i = 0; i < ResourcesClient.KnownLocationsNormalized.Count; i++)
                {
                    if (resourceType.Locations.Remove(ResourcesClient.KnownLocationsNormalized[i]))
                    {
                        resourceType.Locations.Add(ResourcesClient.KnownLocations[i]);
                    }
                }

                result.Name = string.IsNullOrEmpty(providerNamespace) ? resourceType.Name : string.Join("/", providerNamespace, resourceType.Name);
                result.Locations = resourceType.Locations.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                result.LocationsString = string.Join(", ", result.Locations);
            }

            return result;
        }

        public static PSGalleryItem ToPSGalleryItem(this GalleryItem gallery)
        {
            PSGalleryItem psGalleryItem = new PSGalleryItem();
            foreach (PropertyInfo prop in gallery.GetType().GetProperties())
            {
                (typeof(PSGalleryItem)).GetProperty(prop.Name).SetValue(psGalleryItem, prop.GetValue(gallery, null), null);
            }

            return psGalleryItem;
        }

        public static PSDeploymentEventData ToPSDeploymentEventData(this EventData eventData)
        {
            if (eventData == null)
            {
                return null;
            }
            PSDeploymentEventData psObject = new PSDeploymentEventData
                {
                    Authorization = eventData.Authorization.ToPSDeploymentEventDataAuthorization(),
                    ResourceUri = eventData.ResourceUri,
                    SubscriptionId = eventData.SubscriptionId,
                    EventId = eventData.EventDataId,
                    EventName = eventData.EventName.LocalizedValue,
                    EventSource = eventData.EventSource.LocalizedValue,
                    Channels = eventData.EventChannels.ToString(),
                    Level = eventData.Level.ToString(),
                    Description = eventData.Description,
                    Timestamp = eventData.EventTimestamp,
                    OperationId = eventData.OperationId,
                    OperationName = eventData.OperationName.LocalizedValue,
                    Status = eventData.Status.LocalizedValue,
                    SubStatus = eventData.SubStatus.LocalizedValue,
                    Caller = GetEventDataCaller(eventData.Claims),
                    CorrelationId = eventData.CorrelationId,
                    ResourceGroupName = eventData.ResourceGroupName,
                    ResourceProvider = eventData.ResourceProviderName.LocalizedValue,
                    HttpRequest = eventData.HttpRequest.ToPSDeploymentEventDataHttpRequest(),
                    Claims = eventData.Claims,
                    Properties = eventData.Properties
                };
            return psObject;
        }

        public static PSDeploymentEventDataHttpRequest ToPSDeploymentEventDataHttpRequest(this HttpRequestInfo httpRequest)
        {
            if (httpRequest == null)
            {
                return null;
            }
            PSDeploymentEventDataHttpRequest psObject = new PSDeploymentEventDataHttpRequest
            {
                ClientId = httpRequest.ClientRequestId,
                Method = httpRequest.Method,
                Url = httpRequest.Uri,
                ClientIpAddress = httpRequest.ClientIpAddress
            };
            return psObject;
        }

        public static PSDeploymentEventDataAuthorization ToPSDeploymentEventDataAuthorization(this SenderAuthorization authorization)
        {
            if (authorization == null)
            {
                return null;
            }
            PSDeploymentEventDataAuthorization psObject = new PSDeploymentEventDataAuthorization
            {
                Action = authorization.Action,
                Role = authorization.Role,
                Scope = authorization.Scope,
                Condition = authorization.Condition
            };
            return psObject;
        }

        private static string ConstructResourcesTable(List<PSResource> resources)
        {
            StringBuilder resourcesTable = new StringBuilder();

            if (resources.Count > 0)
            {
                int maxNameLength = Math.Max("Name".Length, resources.Where(r => r.Name != null).DefaultIfEmpty(EmptyResource).Max(r => r.Name.Length));
                int maxTypeLength = Math.Max("Type".Length, resources.Where(r => r.ResourceType != null).DefaultIfEmpty(EmptyResource).Max(r => r.ResourceType.Length));
                int maxLocationLength = Math.Max("Location".Length, resources.Where(r => r.Location != null).DefaultIfEmpty(EmptyResource).Max(r => r.Location.Length));

                string rowFormat = "{0, -" + maxNameLength + "}  {1, -" + maxTypeLength + "}  {2, -" + maxLocationLength + "}\r\n";
                resourcesTable.AppendLine();
                resourcesTable.AppendFormat(rowFormat, "Name", "Type", "Location");
                resourcesTable.AppendFormat(rowFormat, 
                    GeneralUtilities.GenerateSeparator(maxNameLength, "="),
                    GeneralUtilities.GenerateSeparator(maxTypeLength, "="),
                    GeneralUtilities.GenerateSeparator(maxLocationLength, "="));

                foreach (PSResource resource in resources)
                {
                    resourcesTable.AppendFormat(rowFormat, resource.Name, resource.ResourceType, resource.Location);
                }
            }

            return resourcesTable.ToString();
        }

        private static string ToString(TemplateLink templateLink)
        {
            StringBuilder result = new StringBuilder();

            if (templateLink != null)
            {
                result.AppendLine();
                result.AppendLine(string.Format("{0, -15}: {1}", "Uri", templateLink.Uri));
                result.AppendLine(string.Format("{0, -15}: {1}", "ContentVersion", templateLink.ContentVersion));
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
                result.AppendFormat(rowFormat, GeneralUtilities.GenerateSeparator(15, "="), GeneralUtilities.GenerateSeparator(25, "="), GeneralUtilities.GenerateSeparator(10, "="));

                foreach (KeyValuePair<string, DeploymentVariable> pair in dictionary)
                {
                    result.AppendFormat(rowFormat, pair.Key, pair.Value.Type, pair.Value.Value);
                }
            }

            return result.ToString();
        }

        private static PSResourceGroupDeployment CreatePSResourceGroupDeployment(
            string name,
            string gesourceGroup,
            DeploymentProperties properties)
        {
            Dictionary<string, DeploymentVariable> outputs = new Dictionary<string, DeploymentVariable>();
            Dictionary<string, DeploymentVariable> parameters = new Dictionary<string, DeploymentVariable>();
            PSResourceGroupDeployment deploymentObject = new PSResourceGroupDeployment();

            deploymentObject.DeploymentName = name;
            deploymentObject.ResourceGroupName = gesourceGroup;

            if (properties != null)
            {
                deploymentObject.Mode = properties.Mode;
                deploymentObject.ProvisioningState = properties.ProvisioningState;
                deploymentObject.TemplateLink = properties.TemplateLink;
                deploymentObject.Timestamp = properties.Timestamp;
                deploymentObject.CorrelationId = properties.CorrelationId;

                if (!string.IsNullOrEmpty(properties.Outputs))
                {
                    outputs = JsonConvert.DeserializeObject<Dictionary<string, DeploymentVariable>>(properties.Outputs);
                    deploymentObject.Outputs = outputs;
                    deploymentObject.OutputsString = ToString(outputs);
                }

                if (!string.IsNullOrEmpty(properties.Parameters))
                {
                    parameters = JsonConvert.DeserializeObject<Dictionary<string, DeploymentVariable>>(properties.Parameters);
                    deploymentObject.Parameters = parameters;
                    deploymentObject.ParametersString = ToString(parameters);
                }

                if (properties.TemplateLink != null)
                {
                    deploymentObject.TemplateLinkString = ToString(properties.TemplateLink);
                }
            }

            return deploymentObject;
        }

        private static string GetEventDataCaller(Dictionary<string, string> claims)
        {
            string name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

            if (claims == null || !claims.ContainsKey(name))
            {
                return null;
            }
            else
            {
                return claims[name];
            }
        }

        private static PSResource EmptyResource
        {
            get
            {
                return new PSResource()
                {
                    Name = string.Empty,
                    Location = string.Empty,
                    ParentResource = string.Empty,
                    PropertiesText = string.Empty,
                    ResourceGroupName = string.Empty,
                    Properties = new Dictionary<string, string>(),
                    ResourceType = string.Empty
                };
            }
        }
    }
}
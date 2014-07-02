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

using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Commands.Common.Storage;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Management.Monitoring.Events;
using Microsoft.WindowsAzure.Management.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using ProjectResources = Microsoft.Azure.Commands.Resources.Properties.Resources;

namespace Microsoft.Azure.Commands.Resources.Models
{
    public partial class ResourcesClient
    {
        /// <summary>
        /// Used when provisioning the deployment status.
        /// </summary>
        private List<DeploymentOperation> operations;

        public IResourceManagementClient ResourceManagementClient { get; set; }
        
        public IStorageClientWrapper StorageClientWrapper { get; set; }

        public GalleryTemplatesClient GalleryTemplatesClient { get; set; }

        public IEventsClient EventsClient { get; set; }

        public Action<string> VerboseLogger { get; set; }

        public Action<string> ErrorLogger { get; set; }

        /// <summary>
        /// Creates new ResourceManagementClient
        /// </summary>
        /// <param name="subscription">Subscription containing resources to manipulate</param>
        public ResourcesClient(WindowsAzureSubscription subscription)
            : this(
                subscription.CreateClientFromResourceManagerEndpoint<ResourceManagementClient>(),
                new StorageClientWrapper(subscription.CreateClient<StorageManagementClient>()),
                new GalleryTemplatesClient(subscription),
                subscription.CreateClientFromResourceManagerEndpoint<EventsClient>())
        {

        }

        /// <summary>
        /// Creates new ResourcesClient instance
        /// </summary>
        /// <param name="resourceManagementClient">The IResourceManagementClient instance</param>
        /// <param name="storageClientWrapper">The IStorageClientWrapper instance</param>
        /// <param name="galleryTemplatesClient">The IGalleryClient instance</param>
        /// <param name="eventsClient">The IEventsClient instance</param>
        public ResourcesClient(
            IResourceManagementClient resourceManagementClient,
            IStorageClientWrapper storageClientWrapper,
            GalleryTemplatesClient galleryTemplatesClient,
            IEventsClient eventsClient)
        {
            ResourceManagementClient = resourceManagementClient;
            StorageClientWrapper = storageClientWrapper;
            GalleryTemplatesClient = galleryTemplatesClient;
            EventsClient = eventsClient;
        }

        /// <summary>
        /// Parameterless constructor for mocking
        /// </summary>
        public ResourcesClient()
        {

        }

        private static string DeploymentTemplateStorageContainerName = "deployment-templates";

        private string GetDeploymentParameters(Hashtable templateParameterObject)
        {
            if (templateParameterObject != null)
            {
                return SerializeHashtable(templateParameterObject, addValueLayer: true);
            }
            else
            {
                return null;
            }
        }

        private List<Provider> ListResourceProviders()
        {
            ProviderListResult result = ResourceManagementClient.Providers.List(null);
            List<Provider> providers = new List<Provider>(result.Providers);

            while (!string.IsNullOrEmpty(result.NextLink))
            {
                result = ResourceManagementClient.Providers.ListNext(result.NextLink);
                providers.AddRange(result.Providers);
            }
            return providers;
        }

        public string SerializeHashtable(Hashtable templateParameterObject, bool addValueLayer)
        {
            if (templateParameterObject == null)
            {
                return null;
            }
            Dictionary<string, object> parametersDictionary = templateParameterObject.ToDictionary(addValueLayer);
            return JsonConvert.SerializeObject(parametersDictionary, new JsonSerializerSettings
                {
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.Indented
                });
        }

        private Uri GetTemplateUri(string templateFile, string galleryTemplateName, string storageAccountName)
        {
            Uri templateFileUri;

            if (!string.IsNullOrEmpty(templateFile))
            {
                if (Uri.IsWellFormedUriString(templateFile, UriKind.Absolute))
                {
                    templateFileUri = new Uri(templateFile);
                }
                else
                {
                    storageAccountName = GetStorageAccountName(storageAccountName);
                    templateFileUri = StorageClientWrapper.UploadFileToBlob(new BlobUploadParameters
                    {
                        StorageName = storageAccountName,
                        FileLocalPath = templateFile,
                        OverrideIfExists = true,
                        ContainerPublic = false,
                        ContainerName = DeploymentTemplateStorageContainerName
                    });
                    WriteVerbose(string.Format(
                        "Uploading template '{0}' to {1}.",
                        Path.GetFileName(templateFile), templateFileUri));
                }
            }
            else
            {
                templateFileUri = new Uri(GalleryTemplatesClient.GetGalleryTemplateFile(galleryTemplateName));
            }

            return templateFileUri;
        }

        private string GetStorageAccountName(string storageAccountName)
        {
            string currentStorageName = null;
            if (WindowsAzureProfile.Instance != null && WindowsAzureProfile.Instance.CurrentSubscription != null)
            {
                currentStorageName = WindowsAzureProfile.Instance.CurrentSubscription.CurrentStorageAccountName;
            }

            string storageName = string.IsNullOrEmpty(storageAccountName) ? currentStorageName : storageAccountName;

            if (string.IsNullOrEmpty(storageName))
            {
                throw new ArgumentException(ProjectResources.StorageAccountNameNeedsToBeSpecified);
            }

            return storageName;
        }

        private ResourceGroup CreateResourceGroup(string name, string location, Hashtable tags)
        {
            Dictionary<string, string> tagDictionary = null;
            if (tags != null)
            {
                tagDictionary = tags.ToStringDictionary();
            }
            var result = ResourceManagementClient.ResourceGroups.CreateOrUpdate(name,
                new BasicResourceGroup
                {
                    Location = location,
                    Tags = tagDictionary
                });

            WriteVerbose(string.Format("Create resource group '{0}' in location '{1}'", name, location));

            return result.ResourceGroup;
        }
        
        private void WriteVerbose(string progress)
        {
            if (VerboseLogger != null)
            {
                VerboseLogger(progress);
            }
        }

        private void WriteError(string error)
        {
            if (ErrorLogger != null)
            {
                ErrorLogger(error);
            }
        }

        private Deployment ProvisionDeploymentStatus(string resourceGroup, string deploymentName, BasicDeployment deployment)
        {
            operations = new List<DeploymentOperation>();

            return WaitDeploymentStatus(
                resourceGroup,
                deploymentName,
                deployment,
                WriteDeploymentProgress,
                ProvisioningState.Canceled,
                ProvisioningState.Succeeded,
                ProvisioningState.Failed);
        }

        private void WriteDeploymentProgress(string resourceGroup, string deploymentName, BasicDeployment deployment)
        {
            const string normalStatusFormat = "Resource {0} '{1}' provisioning status is {2}";
            const string failureStatusFormat = "Resource {0} '{1}' failed with message '{2}'";
            List<DeploymentOperation> newOperations = new List<DeploymentOperation>();
            DeploymentOperationsListResult result = null;
            
            do
            {
                result = ResourceManagementClient.DeploymentOperations.List(resourceGroup, deploymentName, null);
                newOperations = GetNewOperations(operations, result.Operations);
                operations.AddRange(newOperations);

            } while (!string.IsNullOrEmpty(result.NextLink));

            foreach (DeploymentOperation operation in newOperations)
            {
                string statusMessage = string.Empty;

                if (operation.Properties.ProvisioningState != ProvisioningState.Failed)
                {
                    statusMessage = string.Format(normalStatusFormat,
                        operation.Properties.TargetResource.ResourceType,
                        operation.Properties.TargetResource.ResourceName,
                        operation.Properties.ProvisioningState.ToLower());

                    WriteVerbose(statusMessage);
                }
                else
                {
                    string errorMessage = ParseErrorMessage(operation.Properties.StatusMessage);

                    statusMessage = string.Format(failureStatusFormat,
                        operation.Properties.TargetResource.ResourceType,
                        operation.Properties.TargetResource.ResourceName,
                        errorMessage);

                    WriteError(statusMessage);
                }
            }
        }

        public static string ParseErrorMessage(string statusMessage)
        {
            try
            {
                if (JsonUtilities.IsJson(statusMessage))
                {
                    JObject statusMessageJson = JObject.Parse(statusMessage);
                    if (statusMessageJson.GetValue("message", StringComparison.CurrentCultureIgnoreCase) != null)
                    {
                        return statusMessageJson.GetValue("message", StringComparison.CurrentCultureIgnoreCase).ToString();
                    }
                    else if (statusMessageJson.GetValue("error", StringComparison.CurrentCultureIgnoreCase) != null)
                    {
                        JObject errorToken = statusMessageJson.GetValue("error", StringComparison.CurrentCultureIgnoreCase) as JObject;
                        return errorToken.GetValue("message", StringComparison.CurrentCultureIgnoreCase).ToString();
                    }
                }
                else if (XmlUtilities.IsXml(statusMessage))
                {
                    return XmlUtilities.DeserializeXmlString<ResourceManagementError>(statusMessage).Message;
                }

                return statusMessage;
            }
            catch
            {
                return statusMessage;
            }
        }

        private Deployment WaitDeploymentStatus(
            string resourceGroup,
            string deploymentName,
            BasicDeployment basicDeployment,
            Action<string, string, BasicDeployment> job,
            params string[] status)
        {
            Deployment deployment = new Deployment();

            do
            {
                if (job != null)
                {
                    job(resourceGroup, deploymentName, basicDeployment);
                }

                deployment = ResourceManagementClient.Deployments.Get(resourceGroup, deploymentName).Deployment;
                Thread.Sleep(2000);

            } while (!status.Any(s => s.Equals(deployment.Properties.ProvisioningState, StringComparison.OrdinalIgnoreCase)));

            return deployment;
        }

        private List<DeploymentOperation> GetNewOperations(List<DeploymentOperation> old, IList<DeploymentOperation> current)
        {
            List<DeploymentOperation> newOperations = new List<DeploymentOperation>();
            foreach (DeploymentOperation operation in current)
            {
                DeploymentOperation temp = old.Find(o => o.OperationId.Equals(operation.OperationId));
                if (temp != null)
                {
                    if (!temp.Properties.ProvisioningState.Equals(operation.Properties.ProvisioningState))
                    {
                        newOperations.Add(operation);
                    }
                }
                else
                {
                    newOperations.Add(operation);
                }
            }

            return newOperations;
        }

        private BasicDeployment CreateBasicDeployment(ValidatePSResourceGroupDeploymentParameters parameters)
        {
            BasicDeployment deployment = new BasicDeployment()
            {
                Mode = DeploymentMode.Incremental,
                TemplateLink = new TemplateLink()
                {
                    Uri = GetTemplateUri(parameters.TemplateFile, parameters.GalleryTemplateIdentity, parameters.StorageAccountName),
                    ContentVersion = parameters.TemplateVersion
                },
                Parameters = GetDeploymentParameters(parameters.TemplateParameterObject)
            };

            return deployment;
        }

        private List<ResourceManagementError> CheckBasicDeploymentErrors(string resourceGroup, string deploymentName, BasicDeployment deployment)
        {
            List<ResourceManagementError> errors = new List<ResourceManagementError>();
            DeploymentValidateResponse validationResult = ResourceManagementClient.Deployments.Validate(
                resourceGroup,
                deploymentName,
                deployment);
            if (!validationResult.IsValid)
            {
                if (validationResult.Error != null)
                {
                    errors.Add(validationResult.Error);
                    if (validationResult.Error.Details != null && validationResult.Error.Details.Count > 0)
                    {
                        errors.AddRange(validationResult.Error.Details);
                    }
                }
            }

            return errors;
        }
    }
}
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

using Microsoft.Azure.Commands.ResourceManagement.Properties;
using Microsoft.Azure.Gallery;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Storage;
using Microsoft.WindowsAzure.Management.Monitoring.Events;
using Microsoft.WindowsAzure.Management.Storage;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Threading;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public partial class ResourcesClient
    {
        /// <summary>
        /// Used when provisioning the deployment status.
        /// </summary>
        private List<DeploymentOperation> operations;

        public IResourceManagementClient ResourceManagementClient { get; set; }
        
        public IStorageClientWrapper StorageClientWrapper { get; set; }

        public IGalleryClient GalleryClient { get; set; }

        public IEventsClient EventsClient { get; set; }

        public Action<string> ProgressLogger { get; set; }

        /// <summary>
        /// Creates new ResourceManagementClient
        /// </summary>
        /// <param name="subscription">Subscription containing resources to manipulate</param>
        public ResourcesClient(WindowsAzureSubscription subscription)
            : this(
                subscription.CreateClientFromCloudServiceEndpoint<ResourceManagementClient>(),
                new StorageClientWrapper(subscription.CreateClient<StorageManagementClient>()),
                subscription.CreateGalleryClient<GalleryClient>(),
                subscription.CreateClientFromCloudServiceEndpoint<EventsClient>())
        {

        }

        /// <summary>
        /// Creates new ResourcesClient instance
        /// </summary>
        /// <param name="resourceManagementClient">The IResourceManagementClient instance</param>
        /// <param name="storageClientWrapper">The IStorageClientWrapper instance</param>
        /// <param name="galleryClient">The IGalleryClient instance</param>
        /// <param name="eventsClient">The IEventsClient instance</param>
        public ResourcesClient(
            IResourceManagementClient resourceManagementClient,
            IStorageClientWrapper storageClientWrapper,
            IGalleryClient galleryClient,
            IEventsClient eventsClient)
        {
            ResourceManagementClient = resourceManagementClient;
            StorageClientWrapper = storageClientWrapper;
            GalleryClient = galleryClient;
            EventsClient = eventsClient;
        }

        /// <summary>
        /// Parameterless constructor for mocking
        /// </summary>
        public ResourcesClient()
        {

        }

        private static string DeploymentTemplateStorageContainerName = "deployment-templates";

        private string GetDeploymentParameters(Hashtable parameterObject)
        {
            if (parameterObject != null)
            {
                return SerializeHashtable(parameterObject, addValueLayer: true);
            }
            else
            {
                return null;
            }
        }

        private void RegisterResourceProviders()
        {
            ListResourceProviders().Where(p => p.RegistrationState == "NotRegistered")
                .ForEach(p => ResourceManagementClient.Providers.Register(p.Namespace));
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

        private string SerializeHashtable(Hashtable parameterObject, bool addValueLayer)
        {
            if (parameterObject == null)
            {
                return null;
            }
            Dictionary<string, object> parametersDictionary = parameterObject.ToDictionary(addValueLayer);
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
                    WriteProgress(string.Format(
                        "Uploading template '{0}' to {1}.",
                        Path.GetFileName(templateFile),
                        templateFileUri.ToString()));
                }
            }
            else
            {
                templateFileUri = new Uri(GetGalleryTemplateFile(galleryTemplateName));
            }

            return templateFileUri;
        }

        private string GetStorageAccountName(string storageAccountName)
        {
            string currentStorageName = null;
            if (WindowsAzureProfile.Instance.CurrentSubscription != null)
            {
                currentStorageName = WindowsAzureProfile.Instance.CurrentSubscription.CurrentStorageAccountName;
            }

            string storageName = string.IsNullOrEmpty(storageAccountName) ? currentStorageName : storageAccountName;

            if (string.IsNullOrEmpty(storageName))
            {
                throw new ArgumentException(Resources.StorageAccountNameNeedsToBeSpecified);
            }

            return storageName;
        }

        private ContentHash GetTemplateContentHash(string templateHash, string templateHashAlgorithm)
        {
            ContentHash contentHash = null;

            if (!string.IsNullOrEmpty(templateHash))
            {
                contentHash = new ContentHash();
                contentHash.Value = templateHash;
                contentHash.Algorithm = string.IsNullOrEmpty(templateHashAlgorithm) ? ContentHashAlgorithm.Sha256 :
                    (ContentHashAlgorithm)Enum.Parse(typeof(ContentHashAlgorithm), templateHashAlgorithm);
            }

            return contentHash;
        }

        private ResourceGroup CreateResourceGroup(string name, string location)
        {
            var result = ResourceManagementClient.ResourceGroups.CreateOrUpdate(name,
                new BasicResourceGroup
                {
                    Location = location
                });

            WriteProgress(string.Format("Create resource group '{0}' in location '{1}'", name, location));

            return result.ResourceGroup;
        }

        private Type GetParameterType(string resourceParameterType)
        {
            Debug.Assert(!string.IsNullOrEmpty(resourceParameterType));
            const string stringType = "string";
            const string intType = "int";
            const string boolType = "bool";
            const string secureStringType = "SecureString";
            Type typeObject = typeof(object);

            if (resourceParameterType.Equals(stringType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(string);
            }
            else if (resourceParameterType.Equals(intType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(int);
            }
            else if (resourceParameterType.Equals(secureStringType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(SecureString);
            }
            else if (resourceParameterType.Equals(boolType, StringComparison.OrdinalIgnoreCase))
            {
                typeObject = typeof(bool);
            }

            return typeObject;
        }

        private Attribute GetValidationAttribute(string allowedSetString)
        {
            Attribute attribute;
            bool isRangeSet = allowedSetString.Count(c => c == '-') == 1 &&
                              allowedSetString.Count(c => c == ',') == 0;
            if (isRangeSet)
            {
                string[] ranges = allowedSetString.Trim().Split('-');
                int minRange = 0;
                int maxRange = int.MaxValue;
                if (string.IsNullOrEmpty(ranges[0]) && !string.IsNullOrEmpty(ranges[1]))
                {
                    maxRange = int.Parse(ranges[1]);
                }
                else if (!string.IsNullOrEmpty(ranges[0]) && string.IsNullOrEmpty(ranges[1]))
                {
                    minRange = int.Parse(ranges[0]);
                }
                else
                {
                    minRange = int.Parse(ranges[0]);
                    maxRange = int.Parse(ranges[1]);
                }

                attribute = new ValidateRangeAttribute(minRange, maxRange);
            }
            else
            {
                attribute = new ValidateSetAttribute(allowedSetString.Split(',').Select(v => v.Trim()).ToArray())
                {
                    IgnoreCase = true,
                };
            }

            return attribute;
        }
        
        private void WriteProgress(string progress)
        {
            if (ProgressLogger != null)
            {
                ProgressLogger(progress);
            }
        }

        private void ProvisionDeploymentStatus(string resourceGroup, string deploymentName)
        {
            operations = new List<DeploymentOperation>();

            WaitDeploymentStatus(
                resourceGroup,
                deploymentName,
                WriteDeploymentProgress,
                ProvisioningState.Canceled,
                ProvisioningState.Succeeded,
                ProvisioningState.Failed);
        }

        private void WriteDeploymentProgress(string resourceGroup, string deploymentName)
        {
            const string normalStatusFormat = "Resource {0} '{1}' provisioning status in location '{2}' is {3}";
            const string failureStatusFormat = "Resource {0} '{1}' in location '{2}' failed with message {3}";
            List<DeploymentOperation> newOperations = new List<DeploymentOperation>();
            DeploymentOperationsListResult result = null;
            string location = ResourceManagementClient.ResourceGroups.Get(resourceGroup).ResourceGroup.Location;

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
                        location,
                        operation.Properties.ProvisioningState);
                }
                else
                {
                    statusMessage = string.Format(failureStatusFormat,
                        operation.Properties.TargetResource.ResourceType,
                        operation.Properties.TargetResource.ResourceName,
                        location,
                        operation.Properties.StatusMessage);
                }

                WriteProgress(statusMessage);
            }
        }

        private void WaitDeploymentStatus(string resourceGroup, string deploymentName, Action<string, string> job, params string[] status)
        {
            DeploymentProperties deployment = new DeploymentProperties();

            do
            {
                if (job != null)
                {
                    job(resourceGroup, deploymentName);
                }

                deployment = ResourceManagementClient.Deployments.Get(resourceGroup, deploymentName).Deployment.Properties;
                Thread.Sleep(2000);

            } while (!status.Any(s => s.Equals(deployment.ProvisioningState, StringComparison.OrdinalIgnoreCase)));
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

        internal RuntimeDefinedParameter ConstructDynamicParameter(string[] staticParameters, KeyValuePair<string, TemplateFileParameter> parameter)
        {
            const string duplicatedParameterSuffix = "FromTemplate";
            string name = GeneralUtilities.ToUpperFirstLetter(parameter.Key);
            object defaultValue = parameter.Value.DefaultValue;

            RuntimeDefinedParameter runtimeParameter = new RuntimeDefinedParameter()
            {
                Name = staticParameters.Contains(name) ? name + duplicatedParameterSuffix : name,
                ParameterType = GetParameterType(parameter.Value.Type),
                Value = defaultValue
            };
            runtimeParameter.Attributes.Add(new ParameterAttribute()
            {
                Mandatory = defaultValue == null ? true : false,
                ValueFromPipelineByPropertyName = true,
                HelpMessage = "dynamically generated template parameter"
            });

            if (!string.IsNullOrEmpty(parameter.Value.AllowedValues))
            {
                runtimeParameter.Attributes.Add(GetValidationAttribute(parameter.Value.AllowedValues));
            }

            if (!string.IsNullOrEmpty(parameter.Value.MinLength) &&
                !string.IsNullOrEmpty(parameter.Value.MaxLength))
            {
                runtimeParameter.Attributes.Add(new ValidateLengthAttribute(int.Parse(parameter.Value.MinLength), int.Parse(parameter.Value.MaxLength)));
            }

            return runtimeParameter;
        }

        private BasicDeployment CreateBasicDeployment(ValidatePSResourceGroupDeploymentParameters parameters)
        {
            BasicDeployment deployment = new BasicDeployment()
            {
                Mode = DeploymentMode.Incremental,
                TemplateLink = new TemplateLink()
                {
                    Uri = GetTemplateUri(parameters.TemplateFile, parameters.GalleryTemplateName, parameters.StorageAccountName),
                    ContentVersion = parameters.TemplateVersion,
                    ContentHash = GetTemplateContentHash(parameters.TemplateHash, parameters.TemplateHashAlgorithm)
                },
                Parameters = GetDeploymentParameters(parameters.ParameterObject)
            };

            return deployment;
        }

        private List<ResourceManagementError> CheckBasicDeploymentErrors(string resourceGroup, BasicDeployment deployment)
        {
            List<ResourceManagementError> errors = new List<ResourceManagementError>();
            try
            {
                errors.AddRange(ResourceManagementClient.Deployments.Validate(
                    resourceGroup,
                    DeploymentValidationMode.Full,
                    deployment).Errors);
            }
            catch
            {
                // To Do: remove the try-catch when the API is available.
            }

            return errors;
        }
    }
}
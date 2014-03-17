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
using Microsoft.WindowsAzure;
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

        public Action<string> ErrorLogger { get; set; }

        /// <summary>
        /// Creates new ResourceManagementClient
        /// </summary>
        /// <param name="subscription">Subscription containing resources to manipulate</param>
        public ResourcesClient(WindowsAzureSubscription subscription)
            : this(
                subscription.CreateClientFromCloudServiceEndpoint<ResourceManagementClient>(),
                new StorageClientWrapper(subscription.CreateClient<StorageManagementClient>()),
                subscription.CreateGalleryClientFromGalleryEndpoint<GalleryClient>(),
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

        private string SerializeHashtable(Hashtable templateParameterObject, bool addValueLayer)
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

        private void WriteError(string error)
        {
            if (ErrorLogger != null)
            {
                ErrorLogger(error);
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
            const string failureStatusFormat = "Resource {0} '{1}' in location '{2}' failed with message '{3}'";
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

                    WriteProgress(statusMessage);
                }
                else
                {
                    string errorMessage = GetDeploymentOperationErrorMessage(operation.Properties.StatusMessage);

                    statusMessage = string.Format(failureStatusFormat,
                        operation.Properties.TargetResource.ResourceType,
                        operation.Properties.TargetResource.ResourceName,
                        location,
                        errorMessage);

                    WriteError(statusMessage);
                }
            }
        }

        private string GetDeploymentOperationErrorMessage(string statusMessage)
        {
            string errorMessage = null;

            if (JsonUtilities.IsJson(statusMessage))
            {
                errorMessage = JsonConvert.DeserializeObject<ResourceManagementError>(statusMessage).Message;
            }
            else if (XmlUtilities.IsXml(statusMessage))
            {
                errorMessage = XmlUtilities.DeserializeXmlString<ResourceManagementError>(statusMessage).Message;
            }
            else
            {
                errorMessage = statusMessage;
            }

            return errorMessage;
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
            string name = parameter.Key;
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
                    ContentVersion = parameters.TemplateVersion
                },
                Parameters = GetDeploymentParameters(parameters.TemplateParameterObject)
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

        /// <summary>
        /// Verify Storage account has been specified. 
        /// </summary>
        /// <param name="storageAccountName"></param>
        private void ValidateStorageAccount(string storageAccountName)
        {
            GetStorageAccountName(storageAccountName);
        }

        private RuntimeDefinedParameterDictionary ParseTemplateAndExtractParameters(string templateContent, Hashtable templateParameterObject, string templateParameterFilePath, string[] staticParameters)
        {
            RuntimeDefinedParameterDictionary dynamicParameters = new RuntimeDefinedParameterDictionary();

            if (!string.IsNullOrEmpty(templateContent))
            {
                TemplateFile templateFile = JsonConvert.DeserializeObject<TemplateFile>(templateContent);

                foreach (KeyValuePair<string, TemplateFileParameter> parameter in templateFile.Parameters)
                {
                    RuntimeDefinedParameter dynamicParameter = ConstructDynamicParameter(staticParameters, parameter);
                    dynamicParameters.Add(dynamicParameter.Name, dynamicParameter);
                }
            }
            if (templateParameterObject != null)
            {
                UpdateParametersWithObject(dynamicParameters, templateParameterObject);
            }
            if (templateParameterFilePath != null && File.Exists(templateParameterFilePath))
            {
                var parametersFromFile = JsonConvert.DeserializeObject<Dictionary<string, TemplateFileParameter>>(File.ReadAllText(templateParameterFilePath));
                UpdateParametersWithObject(dynamicParameters, new Hashtable(parametersFromFile));
            }
            return dynamicParameters;
        }

        private void UpdateParametersWithObject(RuntimeDefinedParameterDictionary dynamicParameters, Hashtable templateParameterObject)
        {
            if (templateParameterObject != null)
            {
                foreach (KeyValuePair<string, RuntimeDefinedParameter> dynamicParameter in dynamicParameters)
                {
                    try
                    {
                        foreach (string key in templateParameterObject.Keys)
                        {
                            if (key.Equals(dynamicParameter.Key, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (templateParameterObject[key] is TemplateFileParameter)
                                {
                                    dynamicParameter.Value.Value = (templateParameterObject[key] as TemplateFileParameter).Value;
                                }
                                else
                                {
                                    dynamicParameter.Value.Value = templateParameterObject[key];
                                }
                                dynamicParameter.Value.IsSet = true;
                                ((ParameterAttribute)dynamicParameter.Value.Attributes[0]).Mandatory = false;
                            }
                        }
                    }
                    catch
                    {
                        throw new ArgumentException(string.Format(Resources.FailureParsingTemplateParameterObject,
                                                                  dynamicParameter.Key,
                                                                  templateParameterObject[dynamicParameter.Key]));
                    }
                }
            }
        }
    }
}
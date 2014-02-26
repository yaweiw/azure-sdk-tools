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
using System.Text;
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

        public Action<string> ProgressLogger { get; set; }

        /// <summary>
        /// Creates new ResourceManagementClient
        /// </summary>
        /// <param name="subscription">Subscription containing resources to manipulate</param>
        public ResourcesClient(WindowsAzureSubscription subscription)
            : this(
                subscription.CreateCloudServiceClient<ResourceManagementClient>(),
                new StorageClientWrapper(subscription.CreateClient<StorageManagementClient>()),
                subscription.CreateGalleryClient<GalleryClient>())
        {

        }

        /// <summary>
        /// Creates new ResourcesClient instance
        /// </summary>
        /// <param name="resourceManagementClient">The IResourceManagementClient instance</param>
        /// <param name="storageClientWrapper">The IStorageClientWrapper instance</param>
        /// <param name="galleryClient">The IGalleryClient instance</param>
        public ResourcesClient(
            IResourceManagementClient resourceManagementClient,
            IStorageClientWrapper storageClientWrapper,
            IGalleryClient galleryClient)
        {
            ResourceManagementClient = resourceManagementClient;
            StorageClientWrapper = storageClientWrapper;
            GalleryClient = galleryClient;
        }

        /// <summary>
        /// Parameterless constructor for mocking
        /// </summary>
        public ResourcesClient()
        {

        }

        private static string DeploymentTemplateStorageContainerName = "deployment-templates";

        private string GetDeploymentParameters(string parameterFile, Hashtable parameterObject)
        {
            string deploymentParameters = null;

            if (parameterObject != null)
            {
                Dictionary<string, object> parametersDictionary = parameterObject.ToMultidimentionalDictionary();
                deploymentParameters = JsonConvert.SerializeObject(parametersDictionary, new JsonSerializerSettings
                {
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                    TypeNameHandling = TypeNameHandling.None
                });

            }
            else
            {
                if (!string.IsNullOrEmpty(parameterFile))
                {
                    deploymentParameters = File.ReadAllText(parameterFile);
                }
            }

            return deploymentParameters;
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
                        FileRemoteName = Path.GetFileName(templateFile),
                        OverrideIfExists = true,
                        ContainerPublic = true,
                        ContainerName = DeploymentTemplateStorageContainerName
                    });
                    WriteProgress(string.Format(
                        "Upload template '{0}' to {1}.",
                        Path.GetFileName(templateFile),
                        templateFileUri.ToString()));
                }
            }
            else
            {
                templateFileUri = GetGalleryTemplateFile(galleryTemplateName);
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
            const string statusFormat = "{0} operation on '{1}' of type {2} in location '{3}' is {4}";
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
                string statusMessage = string.Format(statusFormat,
                    operation.Properties.Details.Operation,
                    operation.Properties.TargetResource.ResourceName,
                    operation.Properties.TargetResource.ResourceType,
                    location,
                    operation.Properties.ProvisioningState);

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

                deployment = ResourceManagementClient.Deployments.Get(resourceGroup, deploymentName).Properties;
                Thread.Sleep(2000);

            } while (!status.Any(s => s.Equals(deployment.ProvisioningState, StringComparison.OrdinalIgnoreCase)));
        }

        private List<DeploymentOperation> GetNewOperations(List<DeploymentOperation> old, IList<DeploymentOperation> current)
        {
            List<DeploymentOperation> newOperations = new List<DeploymentOperation>();
            foreach (DeploymentOperation operation in current)
            {
                if (!old.Exists(o => o.OperationId.Equals(operation.OperationId)))
                {
                    if (operation.Properties.Details == null)
                    {
                        operation.Properties.Details = new OperationDetails()
                        {
                            Operation = "Unknown"
                        };
                    }

                    newOperations.Add(operation);
                }
            }

            return newOperations;
        }

        internal RuntimeDefinedParameter ConstructDynamicParameter(string[] parameters, string[] parameterSetNames, KeyValuePair<string, TemplateFileParameter> parameter)
        {
            const string duplicatedParameterSuffix = "FromTemplate";
            string name = General.ToUpperFirstLetter(parameter.Key);
            object defaultValue = parameter.Value.DefaultValue;

            RuntimeDefinedParameter runtimeParameter = new RuntimeDefinedParameter()
            {
                Name = parameters.Contains(name) ? name + duplicatedParameterSuffix : name,
                ParameterType = GetParameterType(parameter.Value.Type),
                Value = defaultValue
            };
            foreach (string parameterSetName in parameterSetNames)
            {
                runtimeParameter.Attributes.Add(new ParameterAttribute()
                {
                    ParameterSetName = parameterSetName,
                    Mandatory = defaultValue == null ? true : false,
                    ValueFromPipelineByPropertyName = true,
                    HelpMessage = "dynamically generated template parameter"
                });
            }

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

        private void ValidateDeployment(string resourceGroup, BasicDeployment deployment)
        {
            DeploymentValidateResponse result = ResourceManagementClient.Deployments.Validate(
                resourceGroup,
                DeploymentValidationMode.Full,
                deployment);

            if (result.Errors.Count != 0)
            {
                string errorFormat = "Code={0}; Message={1}; Target={2}\r\n";
                StringBuilder errors = new StringBuilder();
                result.Errors.ForEach(e => errors.AppendFormat(errorFormat, e.Code, e.Message, e.Target));
                throw new ArgumentException(errors.ToString());
            }
        }
    }
}
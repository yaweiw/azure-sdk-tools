// ----------------------------------------------------------------------------------
//
// Copyright 2012 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.Storage.Blob
{
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Storage.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    /// <summary>
    /// Get azure storage container
    /// </summary>
    [Cmdlet(VerbsCommon.Get, StorageNouns.Container,
        DefaultParameterSetName = NameParameterSet)]
    public class GetAzureStorageContainerCommand : StorageBlobBaseCmdlet
    {
        /// <summary>
        /// default parameter set name
        /// </summary>
        private const string NameParameterSet = "ContainerName";

        /// <summary>
        /// prefix parameter set name
        /// </summary>
        private const string PrefixParameterSet = "ContainerPrefix";

        [Alias("N", "Container")]
        [Parameter(Position = 0, HelpMessage = "Container Name",
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            ParameterSetName = NameParameterSet)]
        public string Name { get; set; }

        [Parameter(HelpMessage = "Container Prefix",
            ParameterSetName = PrefixParameterSet, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Prefix { get; set; }

        /// <summary>
        /// list containers by container name pattern.
        /// </summary>
        /// <param name="name">container name pattern</param>
        internal List<CloudBlobContainer> ListContainersByName(string name)
        {
            List<CloudBlobContainer> containerList = new List<CloudBlobContainer>();
            ContainerListingDetails details = ContainerListingDetails.Metadata;
            string prefix = string.Empty;
            BlobRequestOptions requestOptions = null;

            if (String.IsNullOrEmpty(name) || WildcardPattern.ContainsWildcardCharacters(name))
            {
                IEnumerable<CloudBlobContainer> containers = blobClient.ListContainers(prefix, details, requestOptions, operationContext);

                if (String.IsNullOrEmpty(name))
                {
                    containerList.AddRange(containers);
                }
                else
                {
                    WildcardOptions options = WildcardOptions.IgnoreCase |
                          WildcardOptions.Compiled;
                    WildcardPattern wildcard = new WildcardPattern(name, options);
                    foreach (CloudBlobContainer container in containers)
                    {
                        if (wildcard.IsMatch(container.Name))
                        {
                            containerList.Add(container);
                        }
                    }
                }
            }
            else
            {
                if (!NameUtil.IsValidContainerName(name))
                {
                    throw new ArgumentException(String.Format(Resources.InvalidContainerName, name));
                }

                CloudBlobContainer container = blobClient.GetContainerReference(name);
                if (blobClient.IsContainerExists(container, requestOptions, operationContext))
                {
                    containerList.Add(container);
                }
                else
                {
                    throw new ResourceNotFoundException(String.Format(Resources.ContainerNotFound, name));
                }
            }

            return containerList;
        }

        /// <summary>
        /// list containers by container name prefix
        /// </summary>
        /// <param name="prefix">container name prefix</param>
        internal List<CloudBlobContainer> ListContainersByPrefix(string prefix)
        {
            List<CloudBlobContainer> containerList = new List<CloudBlobContainer>();
            ContainerListingDetails details = ContainerListingDetails.Metadata;
            BlobRequestOptions requestOptions = null;

            if (!NameUtil.IsValidContainerPrefix(prefix))
            {
                throw new ArgumentException(String.Format(Resources.InvalidContainerName, prefix));
            }

            IEnumerable<CloudBlobContainer> containers = blobClient.ListContainers(prefix, details, requestOptions, operationContext);
            containerList.AddRange(containers);
            return containerList;
        }

        /// <summary>
        /// convert CloudBlobContainer to AzureStorageContainer
        /// </summary>
        /// <param name="containerList">cloud blob container list</param>
        internal void WriteContainersWithAcl(List<CloudBlobContainer> containerList)
        {
            if (null == containerList)
            {
                return;
            }

            BlobRequestOptions requestOptions = null;
            AccessCondition accessCondition = null;

            foreach (CloudBlobContainer container in containerList)
            {
                BlobContainerPermissions permissions = blobClient.GetContainerPermissions(container, accessCondition, requestOptions, operationContext);
                AzureStorageContainer azureContainer = new AzureStorageContainer(container, permissions);
                //output the container when it's ready will reduce the latency.
                SafeWriteObjectWithContext(azureContainer);
            }
        }

        /// <summary>
        /// execute command
        /// </summary>
        internal override void ExecuteCommand()
        {
            List<CloudBlobContainer> containerList = null;

            if (PrefixParameterSet == ParameterSetName)
            {
                containerList = ListContainersByPrefix(Prefix);
            }
            else
            {
                containerList = ListContainersByName(Name);
            }

            WriteContainersWithAcl(containerList);
        }
    }
}

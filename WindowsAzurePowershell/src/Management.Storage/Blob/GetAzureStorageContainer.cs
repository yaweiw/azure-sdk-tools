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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Storage.Model;

    [Cmdlet(VerbsCommon.Get, "AzureStorageContainer",
        DefaultParameterSetName = "ContainerName")]
    public class GetAzureStorageContainerCommand : StorageBlobBaseCmdlet
    {
        [Alias("N", "Container")]
        [Parameter(Position = 0, HelpMessage = "Container Name",
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            ParameterSetName = "ContainerName")]
        public string Name { get; set; }

        [Parameter(HelpMessage = "Container Prefix",
            ParameterSetName = "ContainerPrefix", Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Prefix { get; set; }

        internal void ListContainersByName(string name = null)
        {
            List<CloudBlobContainer> containerList = new List<CloudBlobContainer>();
            ContainerListingDetails details = ContainerListingDetails.Metadata;
            string prefix = string.Empty;
            BlobRequestOptions reqesutOptions = null;

            if (String.IsNullOrEmpty(name) || WildcardPattern.ContainsWildcardCharacters(name))
            {
                IEnumerable<CloudBlobContainer> containers = blobClient.ListContainers(prefix, details, reqesutOptions, operationContext);

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
                //FIXME Does this api return the metadata?
                CloudBlobContainer container = blobClient.GetContainerReferenceFromServer(name, reqesutOptions, operationContext);
                if (null != container)
                {
                    containerList.Add(container);
                }
                else
                {
                    throw new ResourceNotFoundException(String.Format(Resources.ContainerNotFound, name));
                }
            }
            WriteContainersWithAcl(containerList);
        }

        internal void ListContainersByPrefix(string prefix)
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
            WriteContainersWithAcl(containerList);
        }

        internal void WriteContainersWithAcl(List<CloudBlobContainer> containerList)
        {
            if (null == containerList)
            {
                return;
            }
            BlobRequestOptions reqesutOptions = null;
            AccessCondition accessCondition = null;
            foreach (CloudBlobContainer container in containerList)
            {
                BlobContainerPermissions permissions = blobClient.GetContainerPermissions(container, accessCondition, reqesutOptions, operationContext);
                AzureStorageContainer azureContainer = new AzureStorageContainer(container, permissions);
                SafeWriteObjectWithContext(azureContainer);
            }
        }

        internal override void ExecuteCommand()
        {
            SafeWriteTips(String.Format(Resources.BlobEndPointTips, blobClient.GetBaseUri()));
            if ("ContainerPrefix" == ParameterSetName)
            {
                ListContainersByPrefix(Prefix);
            }
            else
            {
                ListContainersByName(Name);
            }
        }
    }
}

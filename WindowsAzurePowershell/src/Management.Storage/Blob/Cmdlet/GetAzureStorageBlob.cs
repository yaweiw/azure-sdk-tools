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

namespace Microsoft.WindowsAzure.Management.Storage.Blob.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.Contract;
    using System.Security.Permissions;

    [Cmdlet(VerbsCommon.Get, "AzureStorageBlob", DefaultParameterSetName = "BlobName")]
    public class GetAzureStorageBlobCommand : StorageCloudBlobCmdletBase
    {
        [Parameter(Position = 0, HelpMessage = "Blob name", ParameterSetName = "BlobName")]
        public string Blob 
        {
            get
            {
                return blobName;
            }
            set
            {
                blobName = value;
            }
        }
        private string blobName = String.Empty;

        [Parameter(HelpMessage = "Blob Prefix", ParameterSetName = "BlobPrefix")]
        public string Prefix 
        {
            get
            {
                return blobPrefix;
            }
            set
            {
                blobPrefix = value;
            }
        }
        private string blobPrefix = String.Empty;

        [Alias("N", "Name")]
        [Parameter(Position = 1, Mandatory = true, HelpMessage = "Container name",
                   ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Container
        {
            get
            {
                return containerName;
            }
            set
            {
                containerName = value;
            }
        }
        private string containerName = String.Empty;

        /// <summary>
        /// Initializes a new instance of the GetAzureStorageBlobCommand class.
        /// </summary>
        public GetAzureStorageBlobCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureStorageBlobCommand class.
        /// </summary>
        /// <param name="channel">IStorageBlobManagement channel</param>
        public GetAzureStorageBlobCommand(IStorageBlobManagement channel)
        {
            Channel = channel;
        }

        internal void ListBlobsByName(string containerName, string blobName)
        {
            if (!NameUtil.IsValidContainerName(containerName))
            {
                throw new ArgumentException(String.Format(Resources.InvalidContainerName, containerName));
            }

            BlobRequestOptions requestOptions = null;
            AccessCondition accessCondition = null;
            CloudBlobContainer container = Channel.GetContainerReference(containerName);

            if (Channel.IsContainerExists(container, requestOptions, OperationContext))
            {
                throw new ArgumentException(String.Format(Resources.ContainerNotFound, containerName));
            }

            bool useFlatBlobListing = true;
            string prefix = string.Empty;
            List<ICloudBlob> blobList = new List<ICloudBlob>();
            BlobListingDetails details = BlobListingDetails.All; //FIXME copy/snapshot/...
            if (String.IsNullOrEmpty(blobName) || WildcardPattern.ContainsWildcardCharacters(blobName))
            {
                //FIXME checkt the details can be returned
                IEnumerable<IListBlobItem> blobs = Channel.ListBlobs(container, prefix, useFlatBlobListing, details, 
                    requestOptions, OperationContext);

                if (String.IsNullOrEmpty(blobName))
                {
                    foreach (ICloudBlob blob in blobs) //FIXME fast convert?
                    {
                        blobList.Add(blob);
                    }
                }
                else
                {
                    WildcardOptions options = WildcardOptions.IgnoreCase |
                              WildcardOptions.Compiled;
                    WildcardPattern wildcard = new WildcardPattern(blobName, options);
                    foreach (ICloudBlob blob in blobs)
                    {
                        if (wildcard.IsMatch(blob.Name))
                        {
                            blobList.Add(blob);
                        }
                    }
                }
            }
            else
            {
                if (!NameUtil.IsValidBlobName(blobName))
                {
                    throw new ArgumentException(String.Format(Resources.InValidBlobName, blobName));
                }

                ICloudBlob blob = Channel.GetBlobReferenceFromServer(container, blobName, accessCondition, 
                    requestOptions, OperationContext);
                if (null == blob)
                {
                    throw new ResourceNotFoundException(String.Format(Resources.BlobNotFound, blobName, containerName));
                }
                else
                {
                    blobList.Add(blob);
                }
            }
            WriteBlobsWithContext(blobList);
        }

        internal void ListBlobsByPrefix(string containerName, string prefix)
        {
            if (!NameUtil.IsValidContainerName(containerName))
            {
                throw new ArgumentException(String.Format(Resources.InvalidContainerName, containerName));
            }

            BlobRequestOptions requestOptions = null;
            CloudBlobContainer container = Channel.GetContainerReferenceFromServer(containerName, requestOptions, OperationContext);

            if (null == container)
            {
                throw new ArgumentException(String.Format(Resources.ContainerNotFound, containerName));
            }

            bool useFlatBlobListing = true;
            List<ICloudBlob> blobList = new List<ICloudBlob>();
            BlobListingDetails details = BlobListingDetails.All; //FIXME copy/snapshot/...

            IEnumerable<IListBlobItem> blobs = Channel.ListBlobs(container, prefix, useFlatBlobListing, details,
                    requestOptions, OperationContext);

            foreach (ICloudBlob blob in blobs) //FIXME fast convert?
            {
                blobList.Add(blob);
            }
            WriteBlobsWithContext(blobList);
        }

        internal void WriteBlobsWithContext(List<ICloudBlob> blobList)
        {
            if (null == blobList)
            {
                return;
            }
            foreach (ICloudBlob blob in blobList)
            {
                AzureStorageBlob azureBlob = new AzureStorageBlob(blob);
                SafeWriteObjectWithContext(azureBlob);
            }
        }

        /// <summary>
        /// execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            if ("BlobPrefix" == ParameterSetName)
            {
                ListBlobsByPrefix(containerName, blobPrefix);
            }
            else
            {
                ListBlobsByName(containerName, blobName);
            }
        }
    }
}
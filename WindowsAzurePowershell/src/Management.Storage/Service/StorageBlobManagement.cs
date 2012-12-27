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
// ---------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Storage.Common
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    class BlobManagement : IBlobManagement
    {
        private CloudBlobClient blobClient;

        public BlobManagement(CloudBlobClient client)
        {
            if (null == client)
            {
                //FIXME catch this exception?
                throw new RuntimeException("Test");
            }
            blobClient = client;
        }

        public IEnumerable<CloudBlobContainer> ListContainers(string prefix, ContainerListingDetails detailsIncluded,
            BlobRequestOptions options, OperationContext operationContext)
        {
            return blobClient.ListContainers(prefix, detailsIncluded, options, operationContext);
        }

        public CloudBlobContainer GetContainerReferenceFromServer(string name, BlobRequestOptions options,
            OperationContext context)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(name);
            if (container.Exists(options, context))
            {
                return container;
            }
            return null;
        }


        public BlobContainerPermissions GetContainerPermissions(CloudBlobContainer container, AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext)
        {
            return container.GetPermissions(accessCondition, options, operationContext);
        }


        public string GetBaseUri()
        {
            return blobClient.BaseUri.ToString();
        }


        public CloudBlobContainer GetContainerReference(string name)
        {
            return blobClient.GetContainerReference(name);
        }


        public bool CreateContainerIfNotExists(CloudBlobContainer container, BlobRequestOptions requestOptions,
            OperationContext operationContext)
        {
            return container.CreateIfNotExists(requestOptions, operationContext);
        }


        public void DeleteContainer(CloudBlobContainer container, AccessCondition accessCondition, BlobRequestOptions options,
            OperationContext operationContext)
        {
            container.Delete(accessCondition, options, operationContext);
        }


        public void SetContainerPermissions(CloudBlobContainer container, BlobContainerPermissions permissions,
            AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            container.SetPermissions(permissions, accessCondition, options, operationContext);
        }


        public ICloudBlob GetBlobReferenceFromServer(CloudBlobContainer container, string blobName, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            try
            {
                ICloudBlob blob = container.GetBlobReferenceFromServer(blobName, accessCondition, options, operationContext);
                return blob;
            }
            catch(StorageException e)
            {
                if (StorageExceptionUtil.IsNotFoundException(e))
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public IEnumerable<IListBlobItem> ListBlobs(CloudBlobContainer container, string prefix, bool useFlatBlobListing, BlobListingDetails blobListingDetails, BlobRequestOptions options, OperationContext operationContext)
        {
            return container.ListBlobs(prefix, useFlatBlobListing, blobListingDetails, options, operationContext);
        }


        public bool IsContainerExists(CloudBlobContainer container, BlobRequestOptions options, OperationContext operationContext)
        {
            if (null == container)
            {
                return false;
            }
            else
            {
                return container.Exists(options, operationContext);
            }
        }

        public bool IsBlobExists(ICloudBlob blob, BlobRequestOptions options, OperationContext operationContext)
        {
            if (null == blob)
            {
                return false;
            }
            else
            {
                return blob.Exists(options, operationContext);
            }
        }

        public void DeleteICloudBlob(ICloudBlob blob, DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            blob.Delete(deleteSnapshotsOption, accessCondition, options, operationContext);
        }
    }
}

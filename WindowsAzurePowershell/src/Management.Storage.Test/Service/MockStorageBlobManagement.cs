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

namespace Microsoft.WindowsAzure.Management.Storage.Test.Service
{
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage;

    public class MockStorageBlobManagement : IBlobManagement
    {
        public List<CloudBlobContainer> containerList = new List<CloudBlobContainer>();
        public Dictionary<string, BlobContainerPermissions> containerPermissions = new Dictionary<string, BlobContainerPermissions>();
        public Dictionary<string, List<ICloudBlob>> containerBlobs = new Dictionary<string, List<ICloudBlob>>();
        public String BaseUri { get; set; }

        private string BlobEndPoint = "http://127.0.0.1/account/";

        public IEnumerable<CloudBlobContainer> ListContainers(string prefix, ContainerListingDetails detailsIncluded,
            BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return containerList;
            }
            else
            {
                List<CloudBlobContainer> prefixContainerList = new List<CloudBlobContainer>();
                foreach (CloudBlobContainer container in containerList)
                {
                    if (container.Name.StartsWith(prefix))
                    {
                        prefixContainerList.Add(container);
                    }
                }
                return prefixContainerList;
            }
        }

        public CloudBlobContainer GetContainerReferenceFromServer(string name, BlobRequestOptions options = null, OperationContext context = null)
        {
            foreach (CloudBlobContainer container in containerList)
            {
                if (container.Name == name)
                {
                    return container;
                }
            }
            return null;
        }

        public BlobContainerPermissions GetContainerPermissions(CloudBlobContainer container, AccessCondition accessCondition = null,
            BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            BlobContainerPermissions defaultPermission = new BlobContainerPermissions();
            defaultPermission.PublicAccess = BlobContainerPublicAccessType.Off;
            if (containerPermissions.ContainsKey(container.Name))
            {
                return containerPermissions[container.Name];
            }
            else
            {
                return defaultPermission;
            }
        }


        public string GetBaseUri()
        {
            return BaseUri;
        }


        public CloudBlobContainer GetContainerReference(string name)
        {
            Uri containerUri = new Uri(String.Format("{0}{1}/", BlobEndPoint, name));
            return new CloudBlobContainer(containerUri);
        }

        public bool CreateContainerIfNotExists(CloudBlobContainer container, BlobRequestOptions requestOptions = null,
            OperationContext operationContext = null)
        {
            CloudBlobContainer containerRef =  GetContainerReferenceFromServer(container.Name);
            if (containerRef != null)
            {
                return false;
            }
            else
            {
                containerRef = GetContainerReference(container.Name);
                containerList.Add(containerRef);
                return true;
            }
        }


        public void DeleteContainer(CloudBlobContainer container, AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext)
        {
            foreach (CloudBlobContainer containerRef in containerList)
            {
                if (container.Name == containerRef.Name)
                {
                    containerList.Remove(containerRef);
                    return;
                }
            }
        }


        public void SetContainerPermissions(CloudBlobContainer container, BlobContainerPermissions permissions,
            AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            String name = container.Name;
            if (containerPermissions.ContainsKey(name))
            {
                containerPermissions[name] = permissions;
            }
            else
            {
                containerPermissions.Add(name, permissions);
            }
        }


        public ICloudBlob GetBlobReferenceFromServer(CloudBlobContainer container, string blobName, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            string containerName = container.Name;
            if (containerBlobs.ContainsKey(containerName))
            {
                List<ICloudBlob> blobList = containerBlobs[containerName];
                foreach (ICloudBlob blob in blobList)
                {
                    if (blob.Name == blobName)
                    {
                        return blob;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<IListBlobItem> ListBlobs(CloudBlobContainer container, string prefix, bool useFlatBlobListing, BlobListingDetails blobListingDetails, BlobRequestOptions options, OperationContext operationContext)
        {
            string containerName = container.Name;
            if (containerBlobs.ContainsKey(containerName))
            {
                List<ICloudBlob> blobList = containerBlobs[containerName];
                if (string.IsNullOrEmpty(prefix))
                {
                    return blobList;
                }
                List<ICloudBlob> prefixBlobs = new List<ICloudBlob>();
                foreach (ICloudBlob blob in blobList)
                {
                    if (blob.Name.StartsWith(prefix))
                    {
                        prefixBlobs.Add(blob);
                    }
                }
                return prefixBlobs;
            }
            else
            {
                return new List<ICloudBlob>();
            }
        }


        public bool IsContainerExists(CloudBlobContainer container, BlobRequestOptions options, OperationContext operationContext)
        {
            if (null == container)
            {
                return false;
            }
            foreach (CloudBlobContainer containerRef in containerList)
            {
                if (containerRef.Name == container.Name)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsBlobExists(ICloudBlob blob, BlobRequestOptions options, OperationContext operationContext)
        {
            CloudBlobContainer container = blob.Container;
            if (!containerBlobs.ContainsKey(container.Name))
            {
                return false;
            }
            else
            {
                List<ICloudBlob> blobList = containerBlobs[container.Name];
                foreach (ICloudBlob blobRef in blobList)
                {
                    if (blobRef.Name == blob.Name)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void DeleteICloudBlob(ICloudBlob blob, DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            CloudBlobContainer container = blob.Container;
            if (!containerBlobs.ContainsKey(container.Name))
            {
                return;
            }
            else
            {
                List<ICloudBlob> blobList = containerBlobs[container.Name];
                foreach (ICloudBlob blobRef in blobList)
                {
                    if (blobRef.Name == blob.Name)
                    {
                        blobList.Remove(blobRef);
                        return;
                    }
                }
            }
        }
    }
}

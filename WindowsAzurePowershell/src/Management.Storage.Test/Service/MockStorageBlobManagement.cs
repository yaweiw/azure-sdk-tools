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
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// mock blob management
    /// </summary>
    public class MockStorageBlobManagement : IBlobManagement
    {
        /// <summary>
        /// container list
        /// </summary>
        private List<CloudBlobContainer> containerList = new List<CloudBlobContainer>();

        public List<CloudBlobContainer> ContainerList
        {
            get
            {
                return containerList;
            }
        }
        
        /// <summary>
        /// container permissions list
        /// </summary>
        private Dictionary<string, BlobContainerPermissions> containerPermissions = new Dictionary<string, BlobContainerPermissions>();
        public Dictionary<string, BlobContainerPermissions> ContainerPermissions = new Dictionary<string, BlobContainerPermissions>();

        /// <summary>
        /// container blobs list
        /// </summary>
        private Dictionary<string, List<ICloudBlob>> containerBlobs = new Dictionary<string, List<ICloudBlob>>();
        public Dictionary<string, List<ICloudBlob>> ContainerBlobs
        {
            get
            {
                return containerBlobs;
            }
        }

        private string BlobEndPoint = "http://127.0.0.1/account/";

        public IEnumerable<CloudBlobContainer> ListContainers(string prefix, ContainerListingDetails detailsIncluded, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return ContainerList;
            }
            else
            {
                List<CloudBlobContainer> prefixContainerList = new List<CloudBlobContainer>();

                foreach (CloudBlobContainer container in ContainerList)
                {
                    if (container.Name.StartsWith(prefix))
                    {
                        prefixContainerList.Add(container);
                    }
                }

                return prefixContainerList;
            }
        }

        public BlobContainerPermissions GetContainerPermissions(CloudBlobContainer container, AccessCondition accessCondition = null, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            BlobContainerPermissions defaultPermission = new BlobContainerPermissions();
            defaultPermission.PublicAccess = BlobContainerPublicAccessType.Off;
            if (ContainerPermissions.ContainsKey(container.Name))
            {
                return ContainerPermissions[container.Name];
            }
            else
            {
                return defaultPermission;
            }
        }

        public CloudBlobContainer GetContainerReference(string name)
        {
            Uri containerUri = new Uri(String.Format("{0}{1}/", BlobEndPoint, name));
            return new CloudBlobContainer(containerUri);
        }

        public bool CreateContainerIfNotExists(CloudBlobContainer container, BlobRequestOptions requestOptions = null, OperationContext operationContext = null)
        {
            CloudBlobContainer containerRef =  GetContainerReference(container.Name);
            if (IsContainerExists(containerRef, requestOptions, operationContext))
            {
                return false;
            }
            else
            {
                containerRef = GetContainerReference(container.Name);
                ContainerList.Add(containerRef);
                return true;
            }
        }


        public void DeleteContainer(CloudBlobContainer container, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            foreach (CloudBlobContainer containerRef in ContainerList)
            {
                if (container.Name == containerRef.Name)
                {
                    ContainerList.Remove(containerRef);
                    return;
                }
            }
        }


        public void SetContainerPermissions(CloudBlobContainer container, BlobContainerPermissions permissions, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            String name = container.Name;
            if (ContainerPermissions.ContainsKey(name))
            {
                ContainerPermissions[name] = permissions;
            }
            else
            {
                ContainerPermissions.Add(name, permissions);
            }
        }


        public ICloudBlob GetBlobReferenceFromServer(CloudBlobContainer container, string blobName, AccessCondition accessCondition, BlobRequestOptions options, OperationContext operationContext)
        {
            string containerName = container.Name;
            if (ContainerBlobs.ContainsKey(containerName))
            {
                List<ICloudBlob> blobList = ContainerBlobs[containerName];
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
            if (ContainerBlobs.ContainsKey(containerName))
            {
                List<ICloudBlob> blobList = ContainerBlobs[containerName];
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
            foreach (CloudBlobContainer containerRef in ContainerList)
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
            if (!ContainerBlobs.ContainsKey(container.Name))
            {
                return false;
            }
            else
            {
                List<ICloudBlob> blobList = ContainerBlobs[container.Name];
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
            if (!ContainerBlobs.ContainsKey(container.Name))
            {
                return;
            }
            else
            {
                List<ICloudBlob> blobList = ContainerBlobs[container.Name];
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

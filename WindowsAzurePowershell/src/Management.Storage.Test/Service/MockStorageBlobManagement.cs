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

        /// <summary>
        /// get a list of cloudblobcontainer in azure
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="detailsIncluded"></param>
        /// <param name="options"></param>
        /// <param name="operationContext"></param>
        /// <returns></returns>
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

        /// <summary>
        /// get container presssions
        /// </summary>
        /// <param name="container"></param>
        /// <param name="accessCondition"></param>
        /// <param name="options"></param>
        /// <param name="operationContext"></param>
        /// <returns></returns>
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

        /// <summary>
        /// get an CloudBlobContainer instance in local
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CloudBlobContainer GetContainerReference(string name)
        {
            Uri containerUri = new Uri(String.Format("{0}{1}/", BlobEndPoint, name));
            return new CloudBlobContainer(containerUri);
        }

        /// <summary>
        /// create the container if not exists
        /// </summary>
        /// <param name="container"></param>
        /// <param name="requestOptions"></param>
        /// <param name="operationContext"></param>
        /// <returns>true if the container did not already exist and was created; otherwise false.</returns>
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

        /// <summary>
        /// delete container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="accessCondition"></param>
        /// <param name="options"></param>
        /// <param name="operationContext"></param>
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

        /// <summary>
        /// set container permissions
        /// </summary>
        /// <param name="container"></param>
        /// <param name="permissions"></param>
        /// <param name="accessCondition"></param>
        /// <param name="options"></param>
        /// <param name="operationContext"></param>
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

        /// <summary>
        /// get blob reference with properties and meta data from server
        /// </summary>
        /// <param name="container"></param>
        /// <param name="blobName"></param>
        /// <param name="accessCondition"></param>
        /// <param name="options"></param>
        /// <param name="operationContext"></param>
        /// <returns>return an ICloudBlob if the specific blob is existing on azure, otherwise return null</returns>
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

        /// <summary>
        /// list all blobs in sepecific containers
        /// </summary>
        /// <param name="container"></param>
        /// <param name="prefix"></param>
        /// <param name="useFlatBlobListing"></param>
        /// <param name="blobListingDetails"></param>
        /// <param name="options"></param>
        /// <param name="operationContext"></param>
        /// <returns></returns>
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

        /// <summary>
        /// whether the container is exists or not
        /// </summary>
        /// <param name="container"></param>
        /// <param name="options"></param>
        /// <param name="operationContext"></param>
        /// <returns></returns>
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

        /// <summary>
        /// whether the blob is exists or not
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="options"></param>
        /// <param name="operationContext"></param>
        /// <returns></returns>
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

        /// <summary>
        /// delete azure blob
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="deleteSnapshotsOption"></param>
        /// <param name="accessCondition"></param>
        /// <param name="options"></param>
        /// <param name="operationContext"></param>
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

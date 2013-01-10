﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.ServiceManagement.Storage.Blob.Contract
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// blob management interface
    /// </summary>
    public interface IStorageBlobManagement
    {
        /// <summary>
        /// get a list of cloudblobcontainer in azure
        /// </summary>
        /// <param name="prefix">container prefix</param>
        /// <param name="detailsIncluded">container listing details</param>
        /// <param name="options">blob request option</param>
        /// <param name="OperationContext">operation context</param>
        /// <returns>An enumerable collection of cloudblobcontainer</returns>
        IEnumerable<CloudBlobContainer> ListContainers(string prefix, ContainerListingDetails detailsIncluded, BlobRequestOptions options, OperationContext OperationContext);

        /// <summary>
        /// get container presssions
        /// </summary>
        /// <param name="container">a cloudblobcontainer object</param>
        /// <param name="accessCondition">access condition</param>
        /// <param name="options">blob request option</param>
        /// <param name="OperationContext">operation context</param>
        /// <returns>the container's permission</returns>
        BlobContainerPermissions GetContainerPermissions(CloudBlobContainer container, AccessCondition accessCondition, BlobRequestOptions options, OperationContext OperationContext);

        /// <summary>
        /// set container permissions
        /// </summary>
        /// <param name="container">a cloudblobcontainer object</param>
        /// <param name="permissions">the container's permission</param>
        /// <param name="accessCondition">access condition</param>
        /// <param name="options">blob request option</param>
        /// <param name="OperationContext">operation context</param>
        void SetContainerPermissions(CloudBlobContainer container, BlobContainerPermissions permissions, AccessCondition accessCondition, BlobRequestOptions options, OperationContext OperationContext);

        /// <summary>
        /// get an CloudBlobContainer instance in local
        /// </summary>
        /// <param name="name">container name</param>
        /// <returns>a CloudBlobContainer in local memory</returns>
        CloudBlobContainer GetContainerReference(String name);

        /// <summary>
        /// get blob reference with properties and meta data from server
        /// </summary>
        /// <param name="container">a cloudblobcontainer object</param>
        /// <param name="blobName">blob name</param>
        /// <param name="accessCondition">access condition</param>
        /// <param name="options">blob request options</param>
        /// <param name="OperationContext">operation context</param>
        /// <returns>return an ICloudBlob if the specific blob exists on azure, otherwise return null</returns>
        ICloudBlob GetBlobReferenceFromServer(CloudBlobContainer container, string blobName, AccessCondition accessCondition, BlobRequestOptions options, OperationContext OperationContext);

        /// <summary>
        /// whether the container is exists or not
        /// </summary>
        /// <param name="container">a cloudblobcontainer object</param>
        /// <param name="options">blob request option</param>
        /// <param name="OperationContext">operation context</param>
        /// <returns>true if the specific container exists, otherwise return false</returns>
        bool IsContainerExists(CloudBlobContainer container, BlobRequestOptions options, OperationContext OperationContext);

        /// <summary>
        /// whether the blob is exists or not
        /// </summary>
        /// <param name="blob">a icloudblob object</param>
        /// <param name="options">blob request option</param>
        /// <param name="OperationContext">operation context</param>
        /// <returns>true if the specific blob exists, otherwise return false</returns>
        bool IsBlobExists(ICloudBlob blob, BlobRequestOptions options, OperationContext OperationContext);

        /// <summary>
        /// create the container if not exists
        /// </summary>
        /// <param name="container">a cloudblobcontainer object</param>
        /// <param name="options">blob request option</param>
        /// <param name="OperationContext">operation context</param>
        /// <returns>true if the container did not already exist and was created; otherwise false.</returns>
        bool CreateContainerIfNotExists(CloudBlobContainer container, BlobRequestOptions requestOptions, OperationContext OperationContext);

        /// <summary>
        /// delete container
        /// </summary>
        /// <param name="container">a cloudblobcontainer object</param>
        /// <param name="accessCondition">access condition</param>
        /// <param name="options">blob request option</param>
        /// <param name="OperationContext">operation context</param>
        void DeleteContainer(CloudBlobContainer container, AccessCondition accessCondition, BlobRequestOptions options, OperationContext OperationContext);

        /// <summary>
        /// list all blobs in sepecific containers
        /// </summary>
        /// <param name="container">a cloudblobcontainer object</param>
        /// <param name="prefix">blob prefix</param>
        /// <param name="useFlatBlobListing">use flat blob listing(whether treat "container/" as directory)</param>
        /// <param name="blobListingDetails">blob listing details</param>
        /// <param name="options">blob request option</param>
        /// <param name="OperationContext">operation context</param>
        /// <returns>an enumerable collection of icloudblob</returns>
        IEnumerable<IListBlobItem> ListBlobs(CloudBlobContainer container, string prefix, bool useFlatBlobListing, BlobListingDetails blobListingDetails, BlobRequestOptions options, OperationContext OperationContext);

        /// <summary>
        /// delete azure blob
        /// </summary>
        /// <param name="blob">ICloudblob object</param>
        /// <param name="deleteSnapshotsOption">delete snapshots option</param>
        /// <param name="accessCondition">access condition</param>
        /// <param name="OperationContext">operation context</param>
        /// <returns>an enumerable collection of icloudblob</returns>
        void DeleteICloudBlob(ICloudBlob blob, DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition, BlobRequestOptions options, OperationContext OperationContext);
    }
}

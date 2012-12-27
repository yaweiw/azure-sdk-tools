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
    using System.Text;

    public interface IBlobManagement
    {
        IEnumerable<CloudBlobContainer> ListContainers(string prefix, ContainerListingDetails detailsIncluded,
            BlobRequestOptions options, OperationContext operationContext);

        BlobContainerPermissions GetContainerPermissions(CloudBlobContainer container, AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext);
        void SetContainerPermissions(CloudBlobContainer container, BlobContainerPermissions permissions, AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext);

        String GetBaseUri();
        //the following methods are not existing in CloudBlobClient
        CloudBlobContainer GetContainerReferenceFromServer(string name, BlobRequestOptions options, OperationContext context);
        CloudBlobContainer GetContainerReference(String name);
        ICloudBlob GetBlobReferenceFromServer(CloudBlobContainer container, string blobName, AccessCondition accessCondition, 
            BlobRequestOptions options, OperationContext operationContext);
        bool IsContainerExists(CloudBlobContainer container, BlobRequestOptions options, OperationContext operationContext);
        bool IsBlobExists(ICloudBlob blob, BlobRequestOptions options, OperationContext operationContext);

        //true if the container did not already exist and was created; otherwise false.
        bool CreateContainerIfNotExists(CloudBlobContainer container, BlobRequestOptions requestOptions, OperationContext operationContext);
        void DeleteContainer(CloudBlobContainer container, AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext);

        IEnumerable<IListBlobItem> ListBlobs(CloudBlobContainer container, string prefix, bool useFlatBlobListing, 
            BlobListingDetails blobListingDetails, BlobRequestOptions options, OperationContext operationContext);
        void DeleteICloudBlob(ICloudBlob blob, DeleteSnapshotsOption deleteSnapshotsOption, AccessCondition accessCondition,
            BlobRequestOptions options, OperationContext operationContext);
    }
}

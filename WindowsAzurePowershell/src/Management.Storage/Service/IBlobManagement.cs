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
            BlobRequestOptions options = null, OperationContext operationContext = null);
        BlobContainerPermissions GetContainerPermissions(CloudBlobContainer container, AccessCondition accessCondition = null,
            BlobRequestOptions options = null, OperationContext operationContext = null);
        String GetBaseUri();
        //the following methods are not existing in CloudBlobClient
        CloudBlobContainer GetContainerReferenceFromServer(string name, BlobRequestOptions options = null, OperationContext context = null);
    }
}

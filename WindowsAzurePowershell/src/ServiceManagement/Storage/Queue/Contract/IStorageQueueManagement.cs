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
// ---------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.ServiceManagement.Storage.Queue.Contract
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IStorageQueueManagement
    {
        IEnumerable<CloudQueue>  ListQueues(string prefix, QueueListingDetails queueListingDetails, QueueRequestOptions options, OperationContext operationContext);
        void FetchAttributes(CloudQueue queue, QueueRequestOptions options, OperationContext operationContext);
        CloudQueue GetQueueReference(String name);

        bool IsQueueExists(CloudQueue queue, QueueRequestOptions requestOptions, OperationContext operationContext);

        bool CreateQueueIfNotExists(CloudQueue queue, QueueRequestOptions options, OperationContext operationContext);
        void DeleteQueue(CloudQueue queue, QueueRequestOptions options, OperationContext operationContext);
    }
}

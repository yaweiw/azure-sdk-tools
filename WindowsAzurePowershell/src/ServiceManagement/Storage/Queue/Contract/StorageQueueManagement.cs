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

    public class StorageQueueManagement : IStorageQueueManagement
    {
        private CloudQueueClient queueClient;

        public StorageQueueManagement(CloudQueueClient client)
        {
            queueClient = client;
        }

        public IEnumerable<CloudQueue> ListQueues(string prefix, QueueListingDetails queueListingDetails, QueueRequestOptions options, OperationContext operationContext)
        {
            return queueClient.ListQueues(prefix, queueListingDetails, options, operationContext);
        }


        public CloudQueue GetQueueReferenceFromServer(string name, QueueRequestOptions options, OperationContext operationContext)
        {
            CloudQueue queue = queueClient.GetQueueReference(name);
            if (queue.Exists(options, operationContext))
            {
                return queue;
            }
            else
            {
                return null;
            }
        }

        public void FetchAttributes(CloudQueue queue, QueueRequestOptions options, OperationContext operationContext)
        {
            queue.FetchAttributes(options, operationContext);
        }


        public CloudQueue GetQueueReference(string name)
        {
            return queueClient.GetQueueReference(name);
        }

        public bool CreateQueueIfNotExists(CloudQueue queue, QueueRequestOptions options, OperationContext operationContext)
        {
            return queue.CreateIfNotExists(options, operationContext);
        }


        public void DeleteQueue(CloudQueue queue, QueueRequestOptions options, OperationContext operationContext)
        {
            queue.Delete(options, operationContext);
        }


        public bool IsQueueExists(CloudQueue queue, QueueRequestOptions requestOptions, OperationContext operationContext)
        {
            return queue.Exists(requestOptions, operationContext);
        }
    }
}

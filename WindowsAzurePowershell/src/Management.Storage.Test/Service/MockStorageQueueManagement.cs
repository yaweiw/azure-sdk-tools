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

namespace Microsoft.WindowsAzure.Management.Storage.Test.Service
{
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Queue.Contract;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class MockStorageQueueManagement : IStorageQueueManagement
    {
        public List<CloudQueue> queueList = new List<CloudQueue>();
        private string QueueEndPoint = "http://127.0.0.1/account/";

        public IEnumerable<CloudQueue> ListQueues(string prefix, QueueListingDetails queueListingDetails, QueueRequestOptions options, OperationContext operationContext)
        {
            if(string.IsNullOrEmpty(prefix))
            {
                return queueList;
            }
            else
            {
                List<CloudQueue> prefixQueues = new List<CloudQueue>();
                foreach(CloudQueue queue in queueList)
                {
                    if(queue.Name.StartsWith(prefix))
                    {
                        prefixQueues.Add(queue);
                    }
                }
                return prefixQueues;
            }
        }

        public void FetchAttributes(CloudQueue queue, QueueRequestOptions options, OperationContext operationContext)
        {
            return;
        }


        public CloudQueue GetQueueReference(string name)
        {
            Uri queueUri = new Uri(String.Format("{0}{1}", QueueEndPoint, name));
            return new CloudQueue(queueUri);
        }

        public bool CreateQueueIfNotExists(CloudQueue queue, QueueRequestOptions options, OperationContext operationContext)
        {
            CloudQueue queueRef = GetQueueReference(queue.Name);
            if (IsQueueExists(queueRef, options, operationContext))
            {
                return false;
            }
            else
            {
                queueRef = GetQueueReference(queue.Name);
                queueList.Add(queueRef);
                return true;
            }
        }


        public void DeleteQueue(CloudQueue queue, QueueRequestOptions options, OperationContext operationContext)
        {
            foreach (CloudQueue queueRef in queueList)
            {
                if (queue.Name == queueRef.Name)
                {
                    queueList.Remove(queueRef);
                    return;
                }
            }
        }


        public bool IsQueueExists(CloudQueue queue, QueueRequestOptions requestOptions, OperationContext operationContext)
        {
            foreach (CloudQueue queueRef in queueList)
            {
                if (queue.Name == queueRef.Name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

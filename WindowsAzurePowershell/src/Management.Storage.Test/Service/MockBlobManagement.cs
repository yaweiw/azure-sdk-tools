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

    public class MockBlobManagement : IBlobManagement
    {
        public List<CloudBlobContainer> containerList = new List<CloudBlobContainer>();
        public Dictionary<string, BlobContainerPermissions> containerPermissions = new Dictionary<string, BlobContainerPermissions>();
        public String BaseUri { get; set; }

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
    }
}

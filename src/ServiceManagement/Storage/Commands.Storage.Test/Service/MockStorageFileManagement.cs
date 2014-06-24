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

namespace Microsoft.WindowsAzure.Management.Storage.Test.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Commands.Storage.Model.Contract;
    using Microsoft.WindowsAzure.Commands.Storage.Model.ResourceModel;
    using Microsoft.WindowsAzure.Management.Storage.Test.Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.File;

    public sealed class MockStorageFileManagement : IStorageFileManagement
    {
        private const string MockupConnectionString = "DefaultEndpointsProtocol=http;AccountName=MockUp;AccountKey=FjUfNl1KiJttbXlsdkMzBTC7WagvrRM9/g6UPBuy0ypCpAbYTL6/KA+dI/7gyoWvLFYmah3IviUP1jykOHHOlA==";

        private CloudFileClient client = CloudStorageAccount.Parse(MockupConnectionString).CreateCloudFileClient();

        private Dictionary<string, IListFileItem[]> enumerationResults = new Dictionary<string, IListFileItem[]>();

        public void SetsEnumerationResults(string directoryName, IEnumerable<IListFileItem> enumerationItems)
        {
            this.enumerationResults[directoryName] = enumerationItems.ToArray();
        }

        public CloudFileShare GetShareReference(string shareName)
        {
            return client.GetShareReference(shareName);
        }

        public Task EnumerateFilesAndDirectoriesAsync(CloudFileDirectory directory, Action<IListFileItem> enumerationAction, FileRequestOptions options, OperationContext operationContext, CancellationToken token)
        {
            IListFileItem[] enumerationItems;
            if (this.enumerationResults.TryGetValue(directory.Name, out enumerationItems))
            {
                foreach (var item in enumerationItems)
                {
                    enumerationAction(item);
                }

                return TaskEx.FromResult(true);
            }
            else
            {
                throw new MockupException("DirectoryNotFound");
            }
        }

        public Task FetchShareAttributesAsync(CloudFileShare share, AccessCondition accessCondition, FileRequestOptions options, OperationContext operationContext, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task EnumerateSharesAsync(string prefix, ShareListingDetails detailsIncluded, Action<CloudFileShare> enumerationAction, FileRequestOptions options, OperationContext operationContext, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task CreateDirectoryAsync(CloudFileDirectory directory, FileRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DirectoryExistsAsync(CloudFileDirectory directory, FileRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FileExistsAsync(CloudFile file, FileRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task CreateShareAsync(CloudFileShare share, FileRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            return TaskEx.FromResult(share);
        }

        public Task DeleteDirectoryAsync(CloudFileDirectory directory, AccessCondition accessCondition, FileRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteShareAsync(CloudFileShare share, AccessCondition accessCondition, FileRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFileAsync(CloudFile file, AccessCondition accessCondition, FileRequestOptions options, OperationContext operationContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public AzureStorageContext StorageContext
        {
            get { throw new NotImplementedException(); }
        }
    }
}

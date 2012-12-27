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

namespace Microsoft.WindowsAzure.Management.Storage.Model
{
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Storage context
    /// </summary>
    public class StorageContext
    {
        /// <summary>
        /// storage account name used in this context
        /// </summary>
        public string StorageAccountName { get; private set; }

        /// <summary>
        /// blob end point of the storage context
        /// </summary>
        public string BlobEndPoint { get; private set; }
        
        /// <summary>
        /// table end point of the storage context
        /// </summary>
        public string TableEndPoint { get; private set; }

        /// <summary>
        /// queue end point of the storage context
        /// </summary>
        public string QueueEndPoint { get; private set; }

        /// <summary>
        /// self reference, it could enable New-AzureStorageContext can be used in pipeline 
        /// </summary>
        public StorageContext Context { get; private set; }

        /// <summary>
        /// name place holder, and force pipeline to ingore this property
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// storage account in context
        /// </summary>
        public CloudStorageAccount StorageAccount { get; private set; }

        /// <summary>
        /// create a storage context usign cloud storage account
        /// </summary>
        /// <param name="account"></param>
        public StorageContext(CloudStorageAccount account)
        {
            StorageAccount = account;
            BlobEndPoint = account.BlobEndpoint.ToString();
            TableEndPoint = account.TableEndpoint.ToString();
            QueueEndPoint = account.QueueEndpoint.ToString();
            StorageAccountName = account.Credentials.AccountName;
            Context = this;
            Name = String.Empty;
        }
    }
}

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

    public class StorageContext
    {
        public string StorageAccountName { get; private set; }
        public string BlobEndPoint { get; private set; }
        public string TableEndPoint { get; private set; }
        public string QueueEndPoint { get; private set; }

        //Enable New-AzureStorageContext can be used in pipeline 
        public StorageContext Context { get; private set; }

        //FIXME force pipeline to ingore this property
        public string Name { get; private set; }

        public CloudStorageAccount StorageAccount { get; private set; }

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

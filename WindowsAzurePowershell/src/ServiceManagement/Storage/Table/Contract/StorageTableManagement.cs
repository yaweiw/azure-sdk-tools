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

namespace Microsoft.WindowsAzure.ServiceManagement.Storage.Table.Contract
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Collections.Generic;

    public class StorageTableManagement : IStorageTableManagement
    {
        private CloudTableClient tableClient;

        public StorageTableManagement(CloudTableClient client)
        {
            tableClient = client;
        }

        public IEnumerable<CloudTable> ListTables(string prefix, TableRequestOptions requestOptions, OperationContext operationContext)
        {
            return tableClient.ListTables(prefix, requestOptions, operationContext);
        }

        public CloudTable GetTableReference(string name)
        {
            return tableClient.GetTableReference(name);
        }

        public bool CreateTableIfNotExists(CloudTable table, TableRequestOptions requestOptions, OperationContext operationContext)
        {
            return table.CreateIfNotExists(requestOptions, operationContext);
        }


        public void Delete(CloudTable table, TableRequestOptions requestOptions = null, OperationContext operationContext = null)
        {
            table.Delete(requestOptions, operationContext);
        }


        public bool IsTableExists(CloudTable table, TableRequestOptions requestOptions, OperationContext operationContext)
        {
            return table.Exists(requestOptions, operationContext);
        }
    }
}
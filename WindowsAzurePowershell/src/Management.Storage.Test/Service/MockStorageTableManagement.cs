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
    using Microsoft.WindowsAzure.ServiceManagement.Storage.Table.Contract;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class MockStorageTableManagement : IStorageTableManagement
    {
        public List<CloudTable> tableList = new List<CloudTable>();
        private string TableEndPoint = "http://127.0.0.1/account/";

        public IEnumerable<CloudTable> ListTables(string prefix, TableRequestOptions requestOptions, OperationContext operationContext)
        {
            if (String.IsNullOrEmpty(prefix))
            {
                return tableList;
            }
            else
            {
                List<CloudTable> prefixTables = new List<CloudTable>();
                foreach (CloudTable table in tableList)
                {
                    //FIXME make sure azure and startswith are the same case sensity
                    if (table.Name.StartsWith(prefix))
                    {
                        prefixTables.Add(table);
                    }
                }
                return prefixTables;
            }
        }

        public CloudTable GetTableReferenceFromServer(string name, TableRequestOptions requestOptions, OperationContext operationContext)
        {
            foreach (CloudTable table in tableList)
            {
                if (table.Name == name)
                {
                    return table;
                }
            }
            return null;
        }


        public CloudTable GetTableReference(string name)
        {
            Uri tableUri = new Uri(String.Format("{0}{1}", TableEndPoint, name));
            CloudTableClient tableClient = new CloudTableClient(new Uri(TableEndPoint));
            return new CloudTable(tableUri, tableClient);
        }

        public bool CreateTableIfNotExists(CloudTable table, TableRequestOptions requestOptions, OperationContext operationContext)
        {
            CloudTable tableRef = GetTableReferenceFromServer(table.Name, requestOptions, operationContext);
            if (tableRef != null)
            {
                return false;
            }
            else
            {
                tableRef = GetTableReference(table.Name);
                tableList.Add(tableRef);
                return true;
            }
        }


        public void Delete(CloudTable table, TableRequestOptions requestOptions = null, OperationContext operationContext = null)
        {
            foreach (CloudTable tableRef in tableList)
            {
                if (table.Name == tableRef.Name)
                {
                    tableList.Remove(tableRef);
                    return;
                }
            }
        }


        public bool IsTableExists(CloudTable table, TableRequestOptions requestOptions, OperationContext operationContext)
        {
            foreach (CloudTable tableRef in tableList)
            {
                if (table.Name == tableRef.Name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

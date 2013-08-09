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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Services
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a list of <see cref="SqlDatabaseResponse"/> objects.
    /// </summary>
    [CollectionDataContract(Name = "ServiceResources", ItemName = "ServiceResource",
        Namespace = Constants.ServiceManagementNamespace)]
    public class SqlDatabaseList : List<SqlDatabaseResponse>
    {
        /// <summary>
        /// Default constructor.  Creates an emtpy list of responses.
        /// </summary>
        public SqlDatabaseList()
        {
        }

        /// <summary>
        /// Initializes the list with a list of <see cref="SqlDatabaseResponse"/>
        /// </summary>
        /// <param name="databases">the initial list of <see cref="SqlDatabaseResponse"/> 
        /// to populate this list.</param>
        public SqlDatabaseList(IEnumerable<SqlDatabaseResponse> databases)
            : base(databases)
        {
        }
    }
}

// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server
{
    /// <summary>
    /// Common interface for all server based operations.
    /// </summary>
    public interface IServerDataServiceContext
    {
        /// <summary>
        /// Gets the per session tracing Id.
        /// </summary>
        string ClientSessionId { get; }

        /// <summary>
        /// Gets the previous request's client request Id.
        /// </summary>
        string ClientRequestId { get; }

        /// <summary>
        /// Gets the name of the server for this context.
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// Retrieves the list of all databases on the server.
        /// </summary>
        /// <returns>An array of all databases on the server.</returns>
        Database[] GetDatabases();

        /// <summary>
        /// Retrieve information on database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to retrieve.</param>
        /// <returns>An object containing the information about the specific database.</returns>
        Database GetDatabase(string databaseName);

        /// <summary>
        /// Creates a new Sql Database.
        /// </summary>
        /// <param name="databaseName">The name for the new database.</param>
        /// <param name="databaseMaxSize">The max size for the database.</param>
        /// <param name="databaseCollation">The collation for the database.</param>
        /// <param name="databaseEdition">The edition for the database.</param>
        /// <returns>The newly created Sql Database.</returns>
        Database CreateNewDatabase(
            string databaseName,
            int? databaseMaxSize,
            string databaseCollation,
            DatabaseEdition databaseEdition);
    }
}

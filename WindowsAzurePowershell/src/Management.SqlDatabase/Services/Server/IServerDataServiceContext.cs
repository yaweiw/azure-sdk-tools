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

        /// <summary>
        /// Creates a new Sql Database in the given server context along with a continuous copy at the specified partner server
        /// </summary>
        /// <param name="databaseName">The name for the new database.</param>
        /// <param name="partnerServer">The name for the partner server.</param>
        /// <param name="databaseMaxSize">The max size for the database.</param>
        /// <param name="databaseCollation">The collation for the database.</param>
        /// <param name="databaseEdition">The edition for the database.</param>
        /// <param name="maxLagInMinutes">The maximum lag for the continuous copy operation.</param>
        /// <returns>The newly created Sql Database.</returns>
        Database CreateNewDatabaseWithCopy(
            string databaseName,
            string partnerServer,
            int? databaseMaxSize,
            string databaseCollation,
            DatabaseEdition databaseEdition,
            int? maxLagInMinutes);

        /// <summary>
        /// Updates the property on the database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to update.</param>
        /// <param name="newDatabaseName">
        /// The new database name, or <c>null</c> to not update.
        /// </param>
        /// <param name="databaseMaxSize">
        /// The new database max size, or <c>null</c> to not update.
        /// </param>
        /// <param name="databaseEdition">
        /// The new database edition, or <c>null</c> to not update.
        /// </param>
        /// <returns>The updated database object.</returns>
        Database UpdateDatabase(
            string databaseName,
            string newDatabaseName,
            int? databaseMaxSize,
            DatabaseEdition? databaseEdition);

        /// <summary>
        /// Removes the database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to remove.</param>
        void RemoveDatabase(string databaseName);

        /// <summary>
        /// Retrieve all database copy objects with matching parameters.
        /// </summary>
        /// <param name="databaseName">The name of the database to copy.</param>
        /// <param name="partnerServer">The name for the partner server.</param>
        /// <param name="partnerDatabaseName">The name of the database on the partner server.</param>
        /// <returns>All database copy objects with matching parameters.</returns>
        DatabaseCopy[] GetDatabaseCopy(
            string databaseName,
            string partnerServer,
            string partnerDatabaseName);

        /// <summary>
        /// Refreshes the given database copy object.
        /// </summary>
        /// <param name="databaseCopy">The object to refresh.</param>
        /// <returns>The refreshed database copy object.</returns>
        DatabaseCopy GetDatabaseCopy(DatabaseCopy databaseCopy);

        /// <summary>
        /// Start database copy on the database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to copy.</param>
        /// <param name="partnerServer">The database to copy.</param>
        /// <param name="partnerDatabaseName">The database to copy.</param>
        /// <param name="maxLagInMinutes">The database to copy.</param>
        /// <param name="continuousCopy"><c>true</c> to make this a continuous copy.</param>
        /// <returns></returns>
        DatabaseCopy StartDatabaseCopy(
            string databaseName,
            string partnerServer,
            string partnerDatabaseName,
            int? maxLagInMinutes,
            bool continuousCopy);

        /// <summary>
        /// Terminate an ongoing database copy operation.
        /// </summary>
        /// <param name="databaseCopy">The database copy to terminate.</param>
        /// <param name="forcedTermination"><c>true</c> to forcefully terminate the copy.</param>
        void StopDatabaseCopy(
            DatabaseCopy databaseCopy,
            bool forcedTermination);
    }
}

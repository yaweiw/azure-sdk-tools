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
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;

    /// <summary>
    /// Implementation of the <see cref="IServerDataServiceContext"/> with Sql Authentication.
    /// </summary>
    public partial class ServerDataServiceSqlAuth : ServerDataServiceContext, IServerDataServiceContext
    {
        #region Constants

        /// <summary>
        /// Model name used in the connection type string
        /// </summary>
        private const string ServerModelConnectionType = "Server2";

        #endregion

        #region Private data

        /// <summary>
        /// A Guid that identifies this session for end-to-end tracing
        /// </summary>
        private readonly Guid sessionActivityId;

        /// <summary>
        /// The connection type identifying the model and connection parameters to use
        /// </summary>
        private readonly DataServiceConnectionType connectionType;

        /// <summary>
        /// The access token to use in requests
        /// </summary>
        private readonly AccessTokenResult accessToken;

        /// <summary>
        /// A collection of entries to be added to each request's header. HTTP headers are case-insensitive. 
        /// </summary>
        private readonly Dictionary<string, string> supplementalHeaderEntries =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Serializes some instance-level operations
        /// </summary>
        private readonly object instanceSyncObject = new object();

        /// <summary>
        /// The name of the server we are connected to.
        /// </summary>
        private readonly string serverName;

        /// <summary>
        /// The previous request's client request Id.
        /// </summary>
        private string clientRequestId;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDataServiceSqlAuth"/> class.
        /// </summary>
        /// <param name="managementServiceUri">The server's management service Uri.</param>
        /// <param name="connectionType">The server connection type with the server name</param>
        /// <param name="sessionActivityId">An activity ID provided by the user that should be associated with this session.</param>
        /// <param name="accessToken">The authentication access token to be used for executing web requests.</param>
        private ServerDataServiceSqlAuth(
            Uri managementServiceUri,
            DataServiceConnectionType connectionType,
            Guid sessionActivityId,
            AccessTokenResult accessToken)
            : base(new Uri(managementServiceUri, connectionType.RelativeEntityUri))
        {
            this.sessionActivityId = sessionActivityId;
            this.connectionType = connectionType;
            this.accessToken = accessToken;

            // Generate a requestId and retrieve the server name
            this.clientRequestId = SqlDatabaseManagementHelper.GenerateClientTracingId();
            this.serverName = this.Servers.First().Name;
        }

        #region Public Properties

        /// <summary>
        /// Gets the session activity Id associated with this context.
        /// </summary>
        public Guid SessionActivityId
        {
            get
            {
                return this.sessionActivityId;
            }
        }

        /// <summary>
        /// Gets the client per session tracing Id.
        /// </summary>
        public string ClientSessionId
        {
            get
            {
                return SqlDatabaseManagementCmdletBase.clientSessionId;
            }
        }

        /// <summary>
        /// Gets the previous request's client request Id.
        /// </summary>
        public string ClientRequestId
        {
            get
            {
                return this.clientRequestId;
            }
        }

        /// <summary>
        /// Gets the name of the server for this context.
        /// </summary>
        public string ServerName
        {
            get
            {
                return this.serverName;
            }
        }

        #endregion

        /// <summary>
        /// Creates and returns a new instance of the <see cref="ServerDataServiceSqlAuth"/> class which
        /// connects to the specified server using the specified credentials. If the server name
        /// is null, the default server name from the serviceRoot Uri will be used.
        /// </summary>
        /// <param name="managementServiceUri">The server's management service <see cref="Uri"/>.</param>
        /// <param name="sessionActivityId">An activity ID provided by the user that should be associated with this session.</param>
        /// <param name="credentials">The credentials to be used to authenticate the user.</param>
        /// <param name="serverName">The name of the server to connect to. (Optional)</param>
        /// <returns>An instance of <see cref="ServerDataServiceSqlAuth"/> class.</returns>
        public static ServerDataServiceSqlAuth Create(
            Uri managementServiceUri,
            Guid sessionActivityId,
            SqlAuthenticationCredentials credentials,
            string serverName)
        {
            if (managementServiceUri == null)
            {
                throw new ArgumentNullException("managementServiceUri");
            }

            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }

            // Retrieve GetAccessToken operation Uri
            Uri accessUri = DataConnectionUtility.GetAccessTokenUri(managementServiceUri);

            // Synchronously call GetAccessToken
            AccessTokenResult result = DataServiceAccess.GetAccessToken(accessUri, credentials);

            // Validate the retrieved access token
            AccessTokenResult.ValidateAccessToken(managementServiceUri, result);

            // Create and return a ServerDataService object
            return Create(managementServiceUri, sessionActivityId, result, serverName);
        }

        /// <summary>
        /// Creates and returns a new instance of the <see cref="ServerDataServiceSqlAuth"/> class which
        /// connects to the specified server using the specified credentials.
        /// </summary>
        /// <param name="managementServiceUri">The server's management service <see cref="Uri"/>.</param>
        /// <param name="sessionActivityId">An activity ID provided by the user that should be associated with this session.</param>
        /// <param name="accessTokenResult">The accessToken to be used to authenticate the user.</param>
        /// <param name="serverName">The name of the server to connect to. (Optional)</param>
        /// <returns>An instance of <see cref="ServerDataServiceSqlAuth"/> class.</returns>
        public static ServerDataServiceSqlAuth Create(
            Uri managementServiceUri,
            Guid sessionActivityId,
            AccessTokenResult accessTokenResult,
            string serverName)
        {
            if (managementServiceUri == null)
            {
                throw new ArgumentNullException("managementServiceUri");
            }

            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            // Create a ServerDataServiceSqlAuth object
            if (serverName == null)
            {
                return new ServerDataServiceSqlAuth(
                    managementServiceUri,
                    new DataServiceConnectionType(ServerModelConnectionType),
                    sessionActivityId,
                    accessTokenResult);
            }
            else
            {
                return new ServerDataServiceSqlAuth(
                    managementServiceUri,
                    new DataServiceConnectionType(ServerModelConnectionType, serverName),
                    sessionActivityId,
                    accessTokenResult);
            }
        }

        /// <summary>
        /// Retrieves the metadata for the context as a <see cref="XDocument"/>
        /// </summary>
        /// <returns>The metadata for the context as a <see cref="XDocument"/></returns>
        public XDocument RetrieveMetadata()
        {
            // Create a new request Id for this operation
            this.clientRequestId = SqlDatabaseManagementHelper.GenerateClientTracingId();

            XDocument doc = DataConnectionUtility.GetMetadata(this, EnhanceRequest);
            return doc;
        }

        #region IServerDataServiceContext Members

        /// <summary>
        /// Creates a new Sql Database.
        /// </summary>
        /// <param name="databaseName">The name for the new database.</param>
        /// <param name="databaseMaxSize">The max size for the database.</param>
        /// <param name="databaseCollation">The collation for the database.</param>
        /// <param name="databaseEdition">The edition for the database.</param>
        /// <returns>The newly created Sql Database.</returns>
        public Database CreateNewDatabase(
            string databaseName,
            int? databaseMaxSize,
            string databaseCollation,
            DatabaseEdition databaseEdition)
        {
            // Create a new request Id for this operation
            this.clientRequestId = SqlDatabaseManagementHelper.GenerateClientTracingId();

            // Create a new database object
            Database database = CreateNewDatabaseInternal(
                databaseName,
                databaseMaxSize,
                databaseCollation,
                databaseEdition);

            // Save changes
            try
            {
                this.SaveChanges(SaveChangesOptions.None);

                // Re-Query the database for server side updated information
                database = this.RefreshEntity(database);
                if (database == null)
                {
                    throw new ApplicationException(Resources.ErrorRefreshingDatabase);
                }
            }
            catch
            {
                this.ClearTrackedEntity(database);
                throw;
            }

            // Load the extra properties for this object.
            database.LoadExtraProperties(this);

            return database;
        }

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
        public Database CreateNewDatabaseWithCopy(
            string databaseName,
            string partnerServer,
            int? databaseMaxSize,
            string databaseCollation,
            DatabaseEdition databaseEdition,
            int? maxLagInMinutes)
        {
            // Create a new request Id for this operation
            this.clientRequestId = SqlDatabaseManagementHelper.GenerateClientTracingId();

            // Create a new database object
            Database database = CreateNewDatabaseInternal(
                databaseName,
                databaseMaxSize,
                databaseCollation,
                databaseEdition);

            // Create a new database copy object with all the required properties
            DatabaseCopy databaseCopy = new DatabaseCopy();
            databaseCopy.SourceServerName = this.ServerName;
            databaseCopy.SourceDatabaseName = databaseName;
            databaseCopy.DestinationServerName = partnerServer;
            databaseCopy.DestinationDatabaseName = databaseCopy.SourceDatabaseName;
            databaseCopy.IsContinuous = true;

            // Set the optional Maximum Lag (RPO) value
            databaseCopy.MaximumLag = maxLagInMinutes;

            // Add the database copy object to context
            this.AddToDatabaseCopies(databaseCopy);

            // Establish the association between the database and database copy objects
            this.AddLink(database, "DatabaseCopies", databaseCopy);
            this.SetLink(databaseCopy, "Database", database);

            // Save changes
            try
            {
                this.SaveChanges(SaveChangesOptions.Batch);

                // Re-Query the database for server side updated information
                database = this.RefreshEntity(database);
                if (database == null)
                {
                    throw new ApplicationException(Resources.ErrorRefreshingDatabase);
                }
            }
            catch
            {
                this.ClearTrackedEntity(database);
                this.ClearTrackedEntity(databaseCopy);
                throw;
            }

            // Load the extra properties for this object.
            database.LoadExtraProperties(this);

            return database;
        }

        /// <summary>
        /// Retrieves the list of all databases on the server.
        /// </summary>
        /// <returns>An array of all databases on the server.</returns>
        public Database[] GetDatabases()
        {
            MergeOption tempOption = this.MergeOption;
            this.MergeOption = MergeOption.OverwriteChanges;
            Database[] allDatabases = null;

            try
            {
                allDatabases = this.Databases.ToArray();
            }
            finally
            {
                this.MergeOption = tempOption;
            }

            // Load the extra properties for all objects.
            foreach (Database database in allDatabases)
            {
                database.LoadExtraProperties(this);
            }

            return allDatabases;
        }

        /// <summary>
        /// Retrieve information on database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to retrieve.</param>
        /// <returns>An object containing the information about the specific database.</returns>
        public Database GetDatabase(string databaseName)
        {
            Database database;

            MergeOption tempOption = this.MergeOption;
            this.MergeOption = MergeOption.OverwriteChanges;

            try
            {
                // Find the database by name
                database = this.Databases.Where(db => db.Name == databaseName).SingleOrDefault();
                if (database == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.DatabaseNotFound,
                            this.ServerName,
                            databaseName));
                }
            }
            finally
            {
                this.MergeOption = tempOption;
            }

            // Load the extra properties for this object.
            database.LoadExtraProperties(this);

            return database;
        }

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
        public Database UpdateDatabase(
            string databaseName,
            string newDatabaseName,
            int? databaseMaxSize,
            DatabaseEdition? databaseEdition)
        {
            // Find the database by name
            Database database = GetDatabase(databaseName);

            // Update the name if specified
            if (newDatabaseName != null)
            {
                database.Name = newDatabaseName;
            }

            // Update the max size and edition properties
            database.MaxSizeGB = databaseMaxSize;
            database.Edition = databaseEdition == null ? null : databaseEdition.ToString();

            database.IsRecursiveTriggersOn = null;

            // Mark the database object for update and submit the changes
            this.UpdateObject(database);
            try
            {
                this.SaveChanges();
            }
            catch
            {
                this.RevertChanges(database);
                throw;
            }

            return this.GetDatabase(database.Name);
        }

        /// <summary>
        /// Removes the database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The database to remove.</param>
        public void RemoveDatabase(string databaseName)
        {
            // Find the database by name
            Database database = GetDatabase(databaseName);

            // Mark the object for delete and submit the changes
            this.DeleteObject(database);
            try
            {
                this.SaveChanges();
            }
            catch
            {
                this.RevertChanges(database);
                throw;
            }
        }

        /// <summary>
        /// Retrieve all database copy objects with matching parameters.
        /// </summary>
        /// <param name="databaseName">The name of the database to copy.</param>
        /// <param name="partnerServer">The name for the partner server.</param>
        /// <param name="partnerDatabaseName">The name of the database on the partner server.</param>
        /// <returns>All database copy objects with matching parameters.</returns>
        public DatabaseCopy[] GetDatabaseCopy(
            string databaseName,
            string partnerServer,
            string partnerDatabaseName)
        {
            // Setup the query by filtering for the source/destination server name from the context.
            IQueryable<DatabaseCopy> databaseCopyQuerySource = this.DatabaseCopies
                .Where(copy => copy.SourceServerName == this.ServerName);
            IQueryable<DatabaseCopy> databaseCopyQueryTarget = this.DatabaseCopies
                .Where(copy => copy.DestinationServerName == this.ServerName);

            // Add additional filter for database name
            if (databaseName != null)
            {
                // Append the clause to only return database of the specified name
                databaseCopyQuerySource = databaseCopyQuerySource
                    .Where(copy => copy.SourceDatabaseName == databaseName);
                databaseCopyQueryTarget = databaseCopyQueryTarget
                    .Where(copy => copy.DestinationDatabaseName == databaseName);
            }

            // Add additional filter for partner server name
            if (partnerServer != null)
            {
                databaseCopyQuerySource = databaseCopyQuerySource
                    .Where(copy => copy.DestinationServerName == partnerServer);
                databaseCopyQueryTarget = databaseCopyQueryTarget
                    .Where(copy => copy.SourceServerName == partnerServer);
            }

            // Add additional filter for partner database name
            if (partnerDatabaseName != null)
            {
                databaseCopyQuerySource = databaseCopyQuerySource
                    .Where(copy => copy.DestinationDatabaseName == partnerDatabaseName);
                databaseCopyQueryTarget = databaseCopyQueryTarget
                    .Where(copy => copy.SourceDatabaseName == partnerDatabaseName);
            }

            DatabaseCopy[] databaseCopies;

            MergeOption tempOption = this.MergeOption;
            this.MergeOption = MergeOption.OverwriteChanges;

            try
            {
                // Return all results as an array.
                DatabaseCopy[] sourceDatabaseCopies = databaseCopyQuerySource.ToArray();
                DatabaseCopy[] targetDatabaseCopies = databaseCopyQueryTarget.ToArray();
                databaseCopies = sourceDatabaseCopies.Concat(targetDatabaseCopies).ToArray();
            }
            finally
            {
                this.MergeOption = tempOption;
            }

            // Load the extra properties for all objects.
            foreach (DatabaseCopy databaseCopy in databaseCopies)
            {
                databaseCopy.LoadExtraProperties(this);
            }

            return databaseCopies;
        }

        /// <summary>
        /// Refreshes the given database copy object.
        /// </summary>
        /// <param name="databaseCopy">The object to refresh.</param>
        /// <returns>The refreshed database copy object.</returns>
        public DatabaseCopy GetDatabaseCopy(DatabaseCopy databaseCopy)
        {
            DatabaseCopy refreshedDatabaseCopy;

            MergeOption tempOption = this.MergeOption;
            this.MergeOption = MergeOption.OverwriteChanges;

            try
            {
                // Find the database copy by its keys
                refreshedDatabaseCopy = this.DatabaseCopies
                    .Where(c => c.SourceServerName == databaseCopy.SourceServerName)
                    .Where(c => c.SourceDatabaseName == databaseCopy.SourceDatabaseName)
                    .Where(c => c.DestinationServerName == databaseCopy.DestinationServerName)
                    .Where(c => c.DestinationDatabaseName == databaseCopy.DestinationDatabaseName)
                    .SingleOrDefault();
                if (refreshedDatabaseCopy == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.DatabaseCopyNotFound,
                            databaseCopy.SourceServerName,
                            databaseCopy.SourceDatabaseName,
                            databaseCopy.DestinationServerName,
                            databaseCopy.DestinationDatabaseName));
                }
            }
            finally
            {
                this.MergeOption = tempOption;
            }

            // Load the extra properties for this object.
            refreshedDatabaseCopy.LoadExtraProperties(this);

            return refreshedDatabaseCopy;
        }

        /// <summary>
        /// Start database copy on the database with the name <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName">The name of the database to copy.</param>
        /// <param name="partnerServer">The name for the partner server.</param>
        /// <param name="partnerDatabaseName">The name of the database on the partner server.</param>
        /// <param name="maxLagInMinutes">The maximum lag for the continuous copy operation.</param>
        /// <param name="continuousCopy"><c>true</c> to make this a continuous copy.</param>
        /// <returns>The new instance of database copy operation.</returns>
        public DatabaseCopy StartDatabaseCopy(
            string databaseName,
            string partnerServer,
            string partnerDatabaseName,
            int? maxLagInMinutes,
            bool continuousCopy)
        {
            // Create a new request Id for this operation
            this.clientRequestId = SqlDatabaseManagementHelper.GenerateClientTracingId();

            // Create a new database copy object with all the required properties
            DatabaseCopy databaseCopy = new DatabaseCopy();
            databaseCopy.SourceServerName = this.ServerName;
            databaseCopy.SourceDatabaseName = databaseName;
            databaseCopy.DestinationServerName = partnerServer;
            databaseCopy.DestinationDatabaseName = partnerDatabaseName;

            // Set the optional continuous copy flag
            databaseCopy.IsContinuous = continuousCopy;

            // Set the optional Maximum Lag (RPO) value
            databaseCopy.MaximumLag = maxLagInMinutes;

            this.AddToDatabaseCopies(databaseCopy);
            try
            {
                this.SaveChanges(SaveChangesOptions.None);

                // Requery for the entity to obtain updated linkid and state
                databaseCopy = this.RefreshEntity(databaseCopy);
                if (databaseCopy == null)
                {
                    throw new ApplicationException(Resources.ErrorRefreshingDatabaseCopy);
                }
            }
            catch
            {
                this.RevertChanges(databaseCopy);
                throw;
            }

            return databaseCopy;
        }

        /// <summary>
        /// Terminate an ongoing database copy operation.
        /// </summary>
        /// <param name="databaseCopy">The database copy to terminate.</param>
        /// <param name="forcedTermination"><c>true</c> to forcefully terminate the copy.</param>
        public void StopDatabaseCopy(
            DatabaseCopy databaseCopy,
            bool forcedTermination)
        {
            // Create a new request Id for this operation
            this.clientRequestId = SqlDatabaseManagementHelper.GenerateClientTracingId();

            try
            {
                // Mark Forced/Friendly flag on the databaseCopy object first
                databaseCopy.IsForcedTerminate = forcedTermination;
                this.UpdateObject(databaseCopy);
                this.SaveChanges();

                // Mark the copy operation for delete
                this.DeleteObject(databaseCopy);
                this.SaveChanges();
            }
            catch
            {
                this.RevertChanges(databaseCopy);
                throw;
            }
        }

        /// <summary>
        /// Create a new database object.
        /// </summary>
        /// <param name="databaseName">The name for the new database.</param>
        /// <param name="databaseMaxSize">The max size for the database.</param>
        /// <param name="databaseCollation">The collation for the database.</param>
        /// <param name="databaseEdition">The edition for the database.</param>
        /// <returns>The newly created database object.</returns>
        private Database CreateNewDatabaseInternal(
            string databaseName,
            int? databaseMaxSize,
            string databaseCollation,
            DatabaseEdition databaseEdition)
        {
            // Create the new entity and set its properties
            Database database = new Database();

            database.Name = databaseName;

            if (databaseMaxSize != null)
            {
                database.MaxSizeGB = (int)databaseMaxSize;
            }

            if (!string.IsNullOrEmpty(databaseCollation))
            {
                database.CollationName = databaseCollation;
            }

            if (databaseEdition != DatabaseEdition.None)
            {
                database.Edition = databaseEdition.ToString();
            }

            // Add the new database object to context
            this.AddToDatabases(database);

            return database;
        }

        #endregion

        /// <summary>
        /// Sets a supplemental property value that will be send with each request. 
        /// </summary>
        /// <param name="key">A key that uniquely identifies the property</param>
        /// <param name="value">A string representation of the property value</param>
        public void SetSessionHeader(string key, string value)
        {
            lock (this.instanceSyncObject)
            {
                this.supplementalHeaderEntries[key] = value;
            }
        }

        /// <summary>
        /// Handler to add aditional headers and properties to the request.
        /// </summary>
        /// <param name="request">The request to enhance.</param>
        protected override void OnEnhanceRequest(HttpWebRequest request)
        {
            EnhanceRequest(this, request);
        }

        /// <summary>
        /// Enhance a request with auth token.
        /// </summary>
        /// <param name="context">The data service context for the request.</param>
        /// <param name="request">The request.</param>
        private static void EnhanceRequest(ServerDataServiceSqlAuth context, HttpWebRequest request)
        {
            lock (context.instanceSyncObject)
            {
                foreach (KeyValuePair<string, string> entry in context.supplementalHeaderEntries)
                {
                    request.Headers[entry.Key] = entry.Value;
                }
            }

            // Add the UserAgent string
            request.UserAgent = Constants.WindowsAzurePowerShellUserAgent;

            // Add the access token header
            request.Headers[DataServiceConstants.AccessTokenHeader] = context.accessToken.AccessToken;

            // Add the access token cookie
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(context.accessToken.AccessCookie);

            // Add the session activity Id
            request.Headers[DataServiceConstants.SessionTraceActivityHeader] = context.sessionActivityId.ToString();

            // Add the client tracing Ids
            request.Headers[Constants.ClientSessionIdHeaderName] = context.ClientSessionId;
            request.Headers[Constants.ClientRequestIdHeaderName] = context.ClientRequestId;
        }
    }
}

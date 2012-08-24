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
    using System.Data.Services.Client;
    using System.Linq;
    using System.Net;
    using System.Xml.Linq;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;

    /// <summary>
    /// Common abstract class for the generated <see cref="ServerContextInternal"/> class.
    /// </summary>
    public abstract class ServerDataServiceContext : ServerContextInternal, IServerDataServiceContext
    {
        #region Constants

        /// <summary>
        /// The default dataservicecontext request timeout.
        /// </summary>
        private const int DefaultDataServiceContextTimeoutInSeconds = 180;

        #endregion

        /// <summary>
        /// The per request client request Id.
        /// </summary>
        private string clientRequestId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerDataServiceContext"/> class.
        /// </summary>
        /// <param name="serviceUri">The service's base <see cref="Uri"/>.</param>
        protected ServerDataServiceContext(Uri serviceUri)
            : base(serviceUri)
        {
            this.SendingRequest += new EventHandler<SendingRequestEventArgs>(this.BeforeSendingRequest);

            // Set the default timeout for the context.
            this.Timeout = DefaultDataServiceContextTimeoutInSeconds;

            // Allow this client model to talk to newer versions of server model
            this.IgnoreMissingProperties = true;
        }

        /// <summary>
        /// Gets the per session tracing Id.
        /// </summary>
        public string ClientSessionId
        {
            get
            {
                return SqlDatabaseManagementCmdletBase.clientSessionId;
            }
        }

        /// <summary>
        /// Gets or sets the per request client request Id.
        /// </summary>
        public string ClientRequestId
        {
            get
            {
                return this.clientRequestId;
            }
            set
            {
                this.clientRequestId = value;
            }
        }

        /// <summary>
        /// Retrieves the metadata for the context as a <see cref="XDocument"/>
        /// </summary>
        /// <returns>The metadata for the context as a <see cref="XDocument"/></returns>
        public abstract XDocument RetrieveMetadata();

        /// <summary>
        /// Creates a new Sql Database.
        /// </summary>
        /// <param name="databaseName">The name for the new database.</param>
        /// <param name="databaseMaxSize">The max size for the database.</param>
        /// <param name="databaseCollation">The collation for the database.</param>
        /// <param name="databaseEdition">The edition for the database.</param>
        /// <returns>The newly created Sql Database.</returns>
        public abstract Database CreateNewDatabase(
            string databaseName,
            int? databaseMaxSize,
            string databaseCollation,
            DatabaseEdition databaseEdition);

        /// <summary>
        /// Handler to add aditional headers and properties to the request.
        /// </summary>
        /// <param name="request">The request to enhance.</param>
        protected virtual void OnEnhanceRequest(HttpWebRequest request)
        {
        }

        #region Entity Refresh/Revert Helpers

        /// <summary>
        /// Refresh the object by requerying for the object and merge changes.
        /// </summary>
        /// <param name="database">The object to refresh.</param>
        protected Database RefreshEntity(Database database)
        {
            MergeOption tempOption = this.MergeOption;
            this.MergeOption = MergeOption.OverwriteChanges;
            this.Databases.Where(s => s.Id == database.Id).SingleOrDefault();
            this.MergeOption = tempOption;

            return database;
        }

        /// <summary>
        /// Revert the changes made to the given object, detach it from the context.
        /// </summary>
        /// <param name="database">The object that is being operated on.</param>
        protected void RevertChanges(Database database)
        {
            // Revert the object by requerying for the object and clean up tracking
            if (database != null)
            {
                this.RefreshEntity(database);
            }

            this.ClearTrackedEntity(database);
        }

        #endregion

        /// <summary>
        /// Handler that appends the token to every data access context request.
        /// </summary>
        /// <param name="sender">The issuer of the request.</param>
        /// <param name="e">Additional info for the request.</param>
        private void BeforeSendingRequest(object sender, SendingRequestEventArgs e)
        {
            HttpWebRequest request = e.Request as HttpWebRequest;

            if (request != null)
            {
                this.OnEnhanceRequest(request);

                // Add the tracing Ids
                request.Headers[Constants.ClientSessionIdHeaderName]= this.ClientSessionId;
                request.Headers[Constants.ClientRequestIdHeaderName] = this.ClientRequestId;
            }
        }
    }
}

// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Database.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Services.Common;
    using Services.Server;

    /// <summary>
    /// Retrieves a list of restorable dropped Windows Azure SQL Databases in the given server context.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSqlRecoverableDatabase", ConfirmImpact = ConfirmImpact.None, DefaultParameterSetName = AllDatabasesOnCurrentServer)]
    public class GetAzureSqlRecoverableDatabase : CmdletBase
    {
        #region Parameter sets

        /// <summary>
        /// The parameter set for getting all databases on the current server.
        /// </summary>
        internal const string AllDatabasesOnCurrentServer = "AllDatabasesOnCurrentServer";

        /// <summary>
        /// The parameter set for getting all databases on the given source server.
        /// </summary>
        internal const string AllDatabasesOnGivenServer = "AllDatabasesOnGivenServer";

        /// <summary>
        /// The parameter set for getting the given database on the current server.
        /// </summary>
        internal const string GivenDatabaseOnCurrentServer = "GivenDatabaseOnCurrentServer";

        /// <summary>
        /// The parameter set for getting the given database on the given source server.
        /// </summary>
        internal const string GivenDatabaseOnGivenServer = "GivenDatabaseOnGivenServer";

        /// <summary>
        /// The parameter set for refreshing the given database object.
        /// </summary>
        internal const string GivenDatabaseObject = "GivenDatabaseObject";

        #endregion

        #region Parameters

        /// <summary>
        /// Gets or sets the name of the server that will host the recovered database.
        /// </summary>
        [Parameter(Mandatory = true,
            ParameterSetName = AllDatabasesOnCurrentServer,
            HelpMessage = "The name of the server that will host the recovered database.")]
        [Parameter(Mandatory = true,
            ParameterSetName = AllDatabasesOnGivenServer,
            HelpMessage = "The name of the server that will host the recovered database.")]
        [Parameter(Mandatory = true,
            ParameterSetName = GivenDatabaseOnCurrentServer,
            HelpMessage = "The name of the server that will host the recovered database.")]
        [Parameter(Mandatory = true,
            ParameterSetName = GivenDatabaseOnGivenServer,
            HelpMessage = "The name of the server that will host the recovered database.")]
        [Parameter(Mandatory = false,
            ParameterSetName = GivenDatabaseObject,
            HelpMessage = "The name of the server that will host the recovered database.")]
        [ValidateNotNullOrEmpty]
        public string TargetServerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the server that contained the database to retrieve. If not specified, defaults to TargetServerName.
        /// </summary>
        [Parameter(Mandatory = true,
            ParameterSetName = AllDatabasesOnGivenServer,
            HelpMessage = "The name of the server that contained the database to retrieve. If not specified, defaults to TargetServerName.")]
        [Parameter(Mandatory = true,
            ParameterSetName = GivenDatabaseOnGivenServer,
            HelpMessage = "The name of the server that contained the database to retrieve. If not specified, defaults to TargetServerName.")]
        [ValidateNotNullOrEmpty]
        public string SourceServerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the database to retrieve.
        /// </summary>
        [Parameter(Mandatory = true,
            ParameterSetName = GivenDatabaseOnCurrentServer,
            HelpMessage = "The name of the database to retrieve.")]
        [Parameter(Mandatory = true,
            ParameterSetName = GivenDatabaseOnGivenServer,
            HelpMessage = "The name of the database to retrieve.")]
        [ValidateNotNullOrEmpty]
        public string SourceDatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the RecoverableDatabase object to refresh.
        /// </summary>
        [Parameter(Mandatory = true,
            ValueFromPipeline = true,
            ParameterSetName = GivenDatabaseObject,
            HelpMessage = "The RecoverableDatabase object to refresh.")]
        [ValidateNotNull]
        public RecoverableDatabase SourceDatabase { get; set; }

        #endregion

        /// <summary>
        /// Process the command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            // Obtain the source server and database name from the given parameters.
            var sourceServerName =
                this.SourceDatabase != null ? this.SourceDatabase.ServerName :
                this.SourceServerName ??
                this.TargetServerName;

            var sourceDatabaseName =
                this.SourceDatabase != null ? this.SourceDatabase.Name :
                this.SourceDatabaseName;

            IServerDataServiceContext connectionContext = null;

            // If a database object was piped in, use its connection context...
            if (this.SourceDatabase != null)
            {
                connectionContext = this.SourceDatabase.Context;
            }
            else
            {
                // ... else create a temporary context
                connectionContext = ServerDataServiceCertAuth.Create(this.TargetServerName, WindowsAzureProfile.Instance.CurrentSubscription);
            }

            string clientRequestId = connectionContext.ClientRequestId;

            try
            {
                if (sourceDatabaseName != null)
                {
                    // Retrieve the database with the specified name and deletion date
                    this.WriteObject(connectionContext.GetRecoverableDatabase(sourceServerName, sourceDatabaseName));
                }
                else
                {
                    // No name specified, retrieve all restorable dropped databases in the server
                    this.WriteObject(connectionContext.GetRecoverableDatabases(sourceServerName), true);
                }
            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this,
                    clientRequestId,
                    ex);
            }
        }
    }
}

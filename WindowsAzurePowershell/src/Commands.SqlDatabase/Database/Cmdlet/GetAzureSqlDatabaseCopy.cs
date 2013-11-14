// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Database.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Services.Common;
    using Services.Server;

    /// <summary>
    /// Retrieves a list of all ongoing Windows Azure SQL Database copy operations in the given
    /// server context.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSqlDatabaseCopy", ConfirmImpact = ConfirmImpact.None,
        DefaultParameterSetName = "ByConnectionContextOnly")]
    public class GetAzureSqlDatabaseCopy : PSCmdlet
    {
        #region Parameter Sets

        internal const string ByInputObjectWithConnectionContext =
            "ByInputObjectWithConnectionContext";

        internal const string ByInputObjectWithServerName =
            "ByInputObjectWithServerName";

        internal const string ByDatabaseWithConnectionContext =
            "ByDatabaseWithConnectionContext";

        internal const string ByDatabaseWithServerName =
            "ByDatabaseWithServerName";

        internal const string ByDatabaseNameWithConnectionContext =
            "ByDatabaseNameWithConnectionContext";

        internal const string ByDatabaseNameWithServerName =
            "ByDatabaseNameWithServerName";

        internal const string ByConnectionContextOnly =
            "ByConnectionContextOnly";

        internal const string ByServerNameOnly =
            "ByServerNameOnly";

        #endregion

        #region Parameters

        /// <summary>
        /// Gets or sets the server connection context.
        /// </summary>
        [Alias("Context")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ParameterSetName = ByInputObjectWithConnectionContext,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The connection context to the specified server.")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ParameterSetName = ByDatabaseWithConnectionContext,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The connection context to the specified server.")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ParameterSetName = ByDatabaseNameWithConnectionContext,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The connection context to the specified server.")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ParameterSetName = ByConnectionContextOnly,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The connection context to the specified server.")]
        [ValidateNotNull]
        public IServerDataServiceContext ConnectionContext { get; set; }

        /// <summary>
        /// Gets or sets the server upon which to operate
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByInputObjectWithServerName,
            HelpMessage = "The name of the server to operate on.")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByDatabaseWithServerName,
            HelpMessage = "The name of the server to operate on")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByDatabaseNameWithServerName,
            HelpMessage = "The name of the server to operate on")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByServerNameOnly,
            HelpMessage = "The name of the server to operate on")]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the sql database copy object to refresh.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ByInputObjectWithConnectionContext,
            ValueFromPipeline = true, HelpMessage = "The database copy operation to refresh.")]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ByInputObjectWithServerName,
            ValueFromPipeline = true, HelpMessage = "The database copy operation to refresh.")]
        [ValidateNotNull]
        public DatabaseCopy DatabaseCopy { get; set; }

        /// <summary>
        /// Database to filter copies by.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ByDatabaseWithConnectionContext,
            ValueFromPipeline = true, HelpMessage = "The database object for the copy operation.")]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ByDatabaseWithServerName,
            ValueFromPipeline = true, HelpMessage = "The database object for the copy operation.")]
        [ValidateNotNull]
        public Database Database { get; set; }

        /// <summary>
        /// Name of a database to filter copies by.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ByDatabaseNameWithConnectionContext,
            HelpMessage = "The name of the database for the copy operation.")]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ByDatabaseNameWithServerName,
            HelpMessage = "The name of the database for the copy operation.")]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the partner server.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ByDatabaseWithConnectionContext,
            HelpMessage = "The name of the partner server")]
        [Parameter(Mandatory = false, ParameterSetName = ByDatabaseWithServerName,
            HelpMessage = "The name of the partner server")]
        [Parameter(Mandatory = false, ParameterSetName = ByDatabaseNameWithConnectionContext,
            HelpMessage = "The name of the partner server")]
        [Parameter(Mandatory = false, ParameterSetName = ByDatabaseNameWithServerName,
            HelpMessage = "The name of the partner server")]
        [Parameter(Mandatory = false, ParameterSetName = ByConnectionContextOnly,
            HelpMessage = "The name of the partner server")]
        [Parameter(Mandatory = false, ParameterSetName = ByServerNameOnly,
            HelpMessage = "The name of the partner server")]
        [ValidateNotNullOrEmpty]
        public string PartnerServer { get; set; }

        /// <summary>
        /// Gets or sets the name of the partner database.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ByDatabaseWithConnectionContext,
            HelpMessage = "The name of the partner database")]
        [Parameter(Mandatory = false, ParameterSetName = ByDatabaseWithServerName,
            HelpMessage = "The name of the partner database")]
        [Parameter(Mandatory = false, ParameterSetName = ByDatabaseNameWithConnectionContext,
            HelpMessage = "The name of the partner database")]
        [Parameter(Mandatory = false, ParameterSetName = ByDatabaseNameWithServerName,
            HelpMessage = "The name of the partner database")]
        [Parameter(Mandatory = false, ParameterSetName = ByConnectionContextOnly,
            HelpMessage = "The name of the partner database")]
        [Parameter(Mandatory = false, ParameterSetName = ByServerNameOnly,
            HelpMessage = "The name of the partner database")]
        [ValidateNotNullOrEmpty]
        public string PartnerDatabase { get; set; }

        #endregion

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            // Obtain the database name from the given parameters.
            string databaseName = null;
            if (this.MyInvocation.BoundParameters.ContainsKey("Database"))
            {
                databaseName = this.Database.Name;
            }
            else if (this.MyInvocation.BoundParameters.ContainsKey("DatabaseName"))
            {
                databaseName = this.DatabaseName;
            }

            // Use the provided ServerDataServiceContext or create one from the
            // provided ServerName and the active subscription.
            IServerDataServiceContext context =
                this.MyInvocation.BoundParameters.ContainsKey("ConnectionContext")
                    ? this.ConnectionContext
                    : ServerDataServiceCertAuth.Create(this.ServerName,
                        WindowsAzureProfile.Instance.CurrentSubscription);

            try
            {
                if (this.MyInvocation.BoundParameters.ContainsKey("DatabaseCopy"))
                {
                    // Refresh the specified database copy object
                    this.WriteObject(context.GetDatabaseCopy(this.DatabaseCopy), true);
                }
                else
                {
                    // Retrieve all database copy object with matching parameters
                    DatabaseCopy[] copies = context.GetDatabaseCopy(
                        databaseName,
                        this.PartnerServer,
                        this.PartnerDatabase);
                    this.WriteObject(copies, true);
                }
            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this,
                    context.ClientRequestId,
                    ex);
            }
        }
    }
}

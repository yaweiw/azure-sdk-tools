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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Database.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server;

    /// <summary>
    /// Retrieves a list of all ongoing Windows Azure SQL Database copy operations in the given
    /// server context.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSqlDatabaseCopy", ConfirmImpact = ConfirmImpact.None,
        DefaultParameterSetName = "ByServerContextOnly")]
    public class GetAzureSqlDatabaseCopy : PSCmdlet
    {
        #region Parameters

        /// <summary>
        /// Gets or sets the server connection context.
        /// </summary>
        [Alias("Context")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The connection context to the specified server.")]
        [ValidateNotNull]
        public IServerDataServiceContext ConnectionContext { get; set; }

        /// <summary>
        /// Gets or sets the sql database copy object.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ByInputObject",
            ValueFromPipeline = true, HelpMessage = "The database copy operation to refresh.")]
        [ValidateNotNull]
        public DatabaseCopy DatabaseCopy { get; set; }

        /// <summary>
        /// Gets or sets the database object to refresh.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ByDatabase",
            ValueFromPipeline = true, HelpMessage = "The database object for the copy operation.")]
        [ValidateNotNull]
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the name of the database to retrieve.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "ByName",
            HelpMessage = "The name of the database for the copy operation.")]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the partner server.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "ByServerContextOnly",
            HelpMessage = "The name of the partner server")]
        [Parameter(Mandatory = false, ParameterSetName = "ByDatabase",
            HelpMessage = "The name of the partner server")]
        [Parameter(Mandatory = false, ParameterSetName = "ByName",
            HelpMessage = "The name of the partner server")]
        [ValidateNotNullOrEmpty]
        public string PartnerServer { get; set; }

        /// <summary>
        /// Gets or sets the name of the partner database.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "ByServerContextOnly",
            HelpMessage = "The name of the partner database")]
        [Parameter(Mandatory = false, ParameterSetName = "ByDatabase",
            HelpMessage = "The name of the partner database")]
        [Parameter(Mandatory = false, ParameterSetName = "ByName",
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

            try
            {
                if (this.MyInvocation.BoundParameters.ContainsKey("DatabaseCopy"))
                {
                    // Refresh the specified database copy object
                    this.WriteObject(this.ConnectionContext.GetDatabaseCopy(this.DatabaseCopy), true);
                }
                else
                {
                    // Retrieve all database copy object with matching parameters
                    DatabaseCopy[] copies = this.ConnectionContext.GetDatabaseCopy(
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
                    this.ConnectionContext.ClientRequestId,
                    ex);
            }
        }
    }
}

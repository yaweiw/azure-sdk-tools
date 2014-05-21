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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Database.Cmdlet
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server;

    /// <summary>
    /// Update settings for an existing Windows Azure SQL Database in the given server context.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureSqlDatabase", SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.Medium)]
    public class SetAzureSqlDatabase : PSCmdlet
    {
        #region Parameters

        /// <summary>
        /// Gets or sets the server connection context.
        /// </summary>
        [Alias("Context")]
        [Parameter(Mandatory = true, Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The connection context to the specified server.")]
        [ValidateNotNull]
        public IServerDataServiceContext ConnectionContext { get; set; }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ByInputObject",
            ValueFromPipeline = true)]
        [ValidateNotNull]
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the new name for the database.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The new name for the database.")]
        [ValidateNotNullOrEmpty]
        public string NewName { get; set; }

        /// <summary>
        /// Gets or sets the new Edition value for the database.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The new edition for the database.")]
        public DatabaseEdition Edition { get; set; }

        /// <summary>
        /// Gets or sets the new maximum size for the database in GB.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The new maximum size for the database in GB.")]
        public int MaxSizeGB { get; set; }

        /// <summary>
        /// Gets or sets the switch to output the target object to the pipeline.
        /// </summary>
        [Parameter(HelpMessage = "Pass through the input object to the output pipeline")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets the switch to not confirm on the altering of the database.
        /// </summary>
        [Parameter(HelpMessage = "Do not confirm on the altering of the database")]
        public SwitchParameter Force { get; set; }

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

            // Do nothing if force is not specified and user cancelled the operation
            string actionDescription = string.Format(
                CultureInfo.InvariantCulture,
                Resources.SetAzureSqlDatabaseDescription,
                this.ConnectionContext.ServerName,
                databaseName);
            string actionWarning = string.Format(
                CultureInfo.InvariantCulture,
                Resources.SetAzureSqlDatabaseWarning,
                this.ConnectionContext.ServerName,
                databaseName);
            this.WriteVerbose(actionDescription);
            if (!this.Force.IsPresent &&
                !this.ShouldProcess(
                    actionDescription,
                    actionWarning,
                    Resources.ShouldProcessCaption))
            {
                return;
            }

            try
            {
                int? maxSizeGb = this.MyInvocation.BoundParameters.ContainsKey("MaxSizeGB") ?
                    (int?)this.MaxSizeGB : null;
                DatabaseEdition? edition = this.MyInvocation.BoundParameters.ContainsKey("Edition") ?
                    (DatabaseEdition?)this.Edition : null;

                // Update the database with the specified name
                Database database = this.ConnectionContext.UpdateDatabase(
                    databaseName,
                    this.NewName,
                    maxSizeGb,
                    edition);

                // If PassThru was specified, write back the updated object to the pipeline
                if (this.PassThru.IsPresent)
                {
                    this.WriteObject(database);
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

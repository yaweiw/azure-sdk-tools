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
    using System.Globalization;
    using System.Management.Automation;
    using Properties;
    using Services.Common;
    using Services.Server;

    /// <summary>
    /// Start a copy operation for a Windows Azure SQL Database in the given server context.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start, "AzureSqlDatabaseCopy", SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.Low)]
    public class StartAzureSqlDatabaseCopy : PSCmdlet
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
        /// Gets or sets the database object to refresh.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ByInputObject",
            ValueFromPipeline = true, HelpMessage = "The database object to copy.")]
        [ValidateNotNull]
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the name of the database to retrieve.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "ByName",
            HelpMessage = "The name of the database to copy.")]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the partner server.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2,
            HelpMessage = "The name of the partner server")]
        [ValidateNotNullOrEmpty]
        public string PartnerServer { get; set; }

        /// <summary>
        /// Gets or sets the name of the partner database.
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "The name of the partner database")]
        [ValidateNotNullOrEmpty]
        public string PartnerDatabase { get; set; }

        /// <summary>
        /// Gets or sets the maximum lag for the continuous copy operation.
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "The maximum lag for the continuous copy operation")]
        public int MaxLagInMinutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to make this a continuous copy.
        /// </summary>
        [Parameter(HelpMessage = "Make this copy a continuous copy")]
        public SwitchParameter ContinuousCopy { get; set; }

        /// <summary>
        /// Gets or sets the switch to not confirm on the start of the database copy.
        /// </summary>
        [Parameter(HelpMessage = "Do not confirm on the start of the database copy")]
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

            string partnerDatabaseName = this.PartnerDatabase == null ?
                databaseName : this.PartnerDatabase;

            // Do nothing if force is not specified and user cancelled the operation
            string actionDescription = string.Format(
                CultureInfo.InvariantCulture,
                Resources.StartAzureSqlDatabaseCopyDescription,
                this.ConnectionContext.ServerName,
                databaseName,
                this.PartnerServer,
                partnerDatabaseName);
            string actionWarning = string.Format(
                CultureInfo.InvariantCulture,
                Resources.StartAzureSqlDatabaseCopyWarning,
                this.ConnectionContext.ServerName,
                databaseName,
                this.PartnerServer,
                partnerDatabaseName);
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
                int? maxLagInMinutes =
                    this.MyInvocation.BoundParameters.ContainsKey("MaxLagInMinutes") ?
                    (int?)this.MaxLagInMinutes : null;

                // Update the database with the specified name
                DatabaseCopy databaseCopy = this.ConnectionContext.StartDatabaseCopy(
                    databaseName,
                    this.PartnerServer,
                    partnerDatabaseName,
                    maxLagInMinutes,
                    this.ContinuousCopy.IsPresent);

                this.WriteObject(databaseCopy, true);
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

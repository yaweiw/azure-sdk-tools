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
    using System.Globalization;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server;
    using Microsoft.WindowsAzure.Management.Utilities.Common;

    /// <summary>
    /// Update settings for an existing Windows Azure SQL Database in the given server context.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureSqlDatabase", SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.High)]
    public class RemoveAzureSqlDatabase : PSCmdlet
    {
        #region Parameter sets

        internal const string ByNameWithConnectionContext =
            "ByNameWithConnectionContext";
        internal const string ByNameWithServerName =
            "ByNameWithServerName";
        internal const string ByObjectWithConnectionContext =
            "ByObjectWithConnectionContext";
        internal const string ByObjectWithServerName =
            "ByObjectWithServerName";

        #endregion

        #region Parameters

        /// <summary>
        /// Gets or sets the server connection context.
        /// </summary>
        [Alias("Context")]
        [Parameter(Mandatory = true, Position = 0,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByNameWithConnectionContext,
            HelpMessage = "The connection context to the specified server.")]
        [Parameter(Mandatory = true, Position = 0,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByObjectWithConnectionContext,
            HelpMessage = "The connection context to the specified server.")]
        [ValidateNotNull]
        public IServerDataServiceContext ConnectionContext { get; set; }

        /// <summary>
        /// Gets or sets the name of the server to connect to
        /// </summary>
        [Parameter(Mandatory = true, Position = 0,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByNameWithServerName,
            HelpMessage = "The name of the server to connect to")]
        [Parameter(Mandatory = true, Position = 0,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByObjectWithServerName,
            HelpMessage = "The name of the server to connect to")]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1,
            ParameterSetName = ByObjectWithConnectionContext,
            ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 1,
            ParameterSetName = ByObjectWithServerName,
            ValueFromPipeline = true)]
        [ValidateNotNull]
        [Alias("InputObject")]
        public Database Database { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1,
            ParameterSetName = ByNameWithConnectionContext)]
        [Parameter(Mandatory = true, Position = 1,
            ParameterSetName = ByNameWithServerName)]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the switch to not confirm on the removal of the database.
        /// </summary>
        [Parameter(HelpMessage = "Do not confirm on the removal of the database")]
        public SwitchParameter Force { get; set; }

        #endregion

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            //This is to enforce the mutual exclusivity of the parameters: Database
            //and DatabaseName.  This can't be done with parameter sets without changing
            //existing behaviour of the cmdlet.
            if (this.MyInvocation.BoundParameters.ContainsKey("Database") &&
                this.MyInvocation.BoundParameters.ContainsKey("DatabaseName"))
            {
                this.WriteError(new ErrorRecord(
                    new PSArgumentException("Invalid Parameter combination: "
                        + "Database and DatabaseName parameters cannot be used together"),
                    string.Empty,
                    ErrorCategory.InvalidArgument,
                    null));
            }

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
            else
            {
                this.WriteError(new ErrorRecord(
                    new PSArgumentException("Could not determine the name of the database"),
                    string.Empty,
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }


            string serverName = null;
            if (this.MyInvocation.BoundParameters.ContainsKey("ServerName"))
            {
                serverName = this.ServerName;
            }
            else
            {
                serverName = this.ConnectionContext.ServerName;
            }

            // Do nothing if force is not specified and user cancelled the operation
            string actionDescription = string.Format(
                CultureInfo.InvariantCulture,
                Resources.RemoveAzureSqlDatabaseDescription,
                serverName,
                databaseName);

            string actionWarning = string.Format(
                CultureInfo.InvariantCulture,
                Resources.RemoveAzureSqlDatabaseWarning,
                serverName,
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

            switch (ParameterSetName)
            {
                case ByNameWithConnectionContext:
                case ByObjectWithConnectionContext:
                    ProcessWithConnectionContext(databaseName);
                    break;

                case ByNameWithServerName:
                case ByObjectWithServerName:
                    ProcessWithServerName(databaseName);
                    break;
            }
        }

        private void ProcessWithServerName(string databaseName)
        {
            try
            {
                //Get the current subscription data.
                SubscriptionData subscriptionData = this.GetCurrentSubscription();

                //create a temporary context
                ServerDataServiceCertAuth context =
                    ServerDataServiceCertAuth.Create(this.ServerName, subscriptionData);

                // Remove the database with the specified name
                context.RemoveDatabase(databaseName);
            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this,
                    this.ConnectionContext.ClientRequestId,
                    ex);
            }
        }

        /// <summary>
        /// Process the request with the connection context
        /// </summary>
        /// <param name="databaseName"></param>
        private void ProcessWithConnectionContext(string databaseName)
        {
            try
            {
                // Remove the database with the specified name
                this.ConnectionContext.RemoveDatabase(databaseName);
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

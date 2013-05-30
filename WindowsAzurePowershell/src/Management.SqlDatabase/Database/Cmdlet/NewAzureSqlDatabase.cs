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
    using Microsoft.WindowsAzure.Management.Utilities.Common;

    /// <summary>
    /// Creates a new Windows Azure SQL Databases in the given server context.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSqlDatabase", SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.Low)]
    public class NewAzureSqlDatabase : PSCmdlet
    {
        #region Parameter Sets

        internal const string ByConnectionContext =
            "ByConnectionContext";
        internal const string ByServerName =
            "ByServerName";

        #endregion

        #region Parameters

        /// <summary>
        /// Gets or sets the server connection context.
        /// </summary>
        [Alias("Context")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ParameterSetName = ByConnectionContext,
            HelpMessage = "The connection context to the specified server.")]
        [ValidateNotNull]
        public IServerDataServiceContext ConnectionContext { get; set; }

        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByServerName,
            HelpMessage = "The name of the server to connect to using the current subscription")]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1,
            HelpMessage = "The name of the new database.")]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the collation for the newly created database.
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "Collation for the newly created database.")]
        [ValidateNotNullOrEmpty]
        public string Collation { get; set; }

        /// <summary>
        /// Gets or sets the edition for the newly created database.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The edition for the database.")]
        public DatabaseEdition Edition { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the newly created database in GB.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The maximum size for the database in GB.")]
        public int MaxSizeGB { get; set; }

        /// <summary>
        /// Gets or sets the switch to not confirm on the creation of the database.
        /// </summary>
        [Parameter(HelpMessage = "Do not confirm on the creation of the database")]
        public SwitchParameter Force { get; set; }

        #endregion

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            int? maxSizeGb = null;
            if(this.MyInvocation.BoundParameters.ContainsKey("MaxSizeGB"))
            {
                maxSizeGb = this.MaxSizeGB;
            }

            switch(ParameterSetName)
            {
                case ByConnectionContext:
                    ProcessWithConnectionContext(maxSizeGb);
                    break;
                case ByServerName:
                    ProcessWithServerName(maxSizeGb);
                    break;
            }

        }

        private void ProcessWithServerName(int? maxSizeGb)
        {
            try
            {
                //Get the current subscription data.
                SubscriptionData subscriptionData = this.GetCurrentSubscription();

                //create a temporary context
                ServerDataServiceCertAuth context =
                    ServerDataServiceCertAuth.Create(this.ServerName, subscriptionData);

                
                // Retrieve the database with the specified name
                this.WriteObject(context.CreateNewDatabase(
                    this.DatabaseName, 
                    maxSizeGb, 
                    this.Collation, 
                    this.Edition));
                
            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this,
                    this.ConnectionContext.ClientRequestId,
                    ex);
            }
        }

        private void ProcessWithConnectionContext(int? maxSizeGb)
        {
            // Do nothing if force is not specified and user cancelled the operation
            if (!this.Force.IsPresent &&
                !this.ShouldProcess(
                Resources.NewAzureSqlDatabaseDescription,
                Resources.NewAzureSqlDatabaseWarning,
                Resources.ShouldProcessCaption))
            {
                return;
            }

            try
            {

                Database database = this.ConnectionContext.CreateNewDatabase(
                    this.DatabaseName,
                    maxSizeGb,
                    this.Collation,
                    this.Edition);

                this.WriteObject(database, true);
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

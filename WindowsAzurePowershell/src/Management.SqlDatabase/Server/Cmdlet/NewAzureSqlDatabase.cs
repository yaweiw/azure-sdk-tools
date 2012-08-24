// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewSqlDatabase.cs" company="Microsoft Corporation">
//   (c) Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//   A cmdlet to create a new SQL database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Management.Client.DataServices.Server.Powershell
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server;

    /// <summary>
    /// This cmdlet creates a new SQL database.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSqlDatabase", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low)]
    public class NewAzureSqlDatabase : PSCmdlet
    {
        #region Parameters

        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNull]
        public IServerDataServiceContext Context { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the collation for the newly created database.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string Collation { get; set; }

        /// <summary>
        /// Gets or sets the edition for the newly created database.
        /// </summary>
        [Parameter(Mandatory = false)]
        public DatabaseEdition Edition { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the newly created database in GB.
        /// </summary>
        [Parameter(Mandatory = false)]
        public int MaxSizeGB { get; set; }

        [Parameter(HelpMessage = "Do not confirm on the creation of the server")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
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
                int? maxSizeGb = this.MyInvocation.BoundParameters.ContainsKey("MaxSizeGB") ?
                    (int?)this.MaxSizeGB : null;
                
                Database database = this.Context.CreateNewDatabase(
                    this.DatabaseName,
                    maxSizeGb,
                    this.Collation,
                    this.Edition);

                if (database != null)
                {
                    WriteObject(database, true);
                }
            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this, 
                    this.Context.ClientRequestId, 
                    ex);
            }
        }
    }
}

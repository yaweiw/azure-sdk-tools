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


namespace Microsoft.WindowsAzure.Management.SqlDatabase.Database.Cmdlet
{
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using System.Xml;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.ImportExport;
    using Microsoft.WindowsAzure.ServiceManagement;

    /// <summary>
    /// Exports a database from Sql Azure into blob storage.
    /// </summary>
    [Cmdlet("Export", "AzureSqlDatabase", ConfirmImpact = ConfirmImpact.Medium)]
    public class ExportAzureSqlDatabase : SqlDatabaseManagementCmdletBase
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="ExportAzureSqlDatabase"/> class.
        /// </summary>
        public ExportAzureSqlDatabase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="ExportAzureSqlDatabase"/> class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public ExportAzureSqlDatabase(ISqlDatabaseManagement channel)
        {
            this.Channel = channel;
        }

        #region Parameters

        /// <summary>
        /// Gets or sets the user name for connecting to the database
        /// </summary>
        [Parameter(Mandatory = true, Position = 0,
            HelpMessage = "The user name for connecting to the database")]
        [ValidateNotNullOrEmpty]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for connecting to the database
        /// </summary>
        [Parameter(Mandatory = true, Position = 1,
            HelpMessage = "The password for connecting to the database")]
        [ValidateNotNullOrEmpty]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the name of the server the database resides in
        /// </summary>
        [Parameter(Mandatory = true, Position = 2,
            HelpMessage = "The name of the server the database is in")]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the database to export
        /// </summary>
        [Parameter(Mandatory = true, Position = 3,
            HelpMessage = "The name of the database to export")]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URI for the blob storage
        /// </summary>
        [Parameter(Mandatory = true, Position = 4,
            HelpMessage = "The uri to where the blob storage is.")]
        [ValidateNotNull]
        public Uri BlobUri { get; set; }

        /// <summary>
        /// Gets or sets the storage key for accessing the blob storage
        /// </summary>
        [Parameter(Mandatory = true, Position = 5,
            HelpMessage = "The Endpoint Uri where the blob storage resides")]
        [ValidateNotNullOrEmpty]
        public string StorageKey { get; set; }

        #endregion

        /// <summary>
        /// Process the export request
        /// </summary>
        protected override void ProcessRecord()
        {
            this.WriteVerbose("Starting to process the record");
            try
            {
                this.WriteVerbose("Processing base record...");
                base.ProcessRecord();
                this.WriteVerbose("Done");


                this.WriteVerbose("Creating ExportInput object");
                //Create Web Request Inputs - Blob Storage Credentials and Server Connection Info
                ExportInput exportInput = new ExportInput
                {
                    BlobCredentials = new BlobStorageAccessKeyCredentials
                    {
                        StorageAccessKey = this.StorageKey,
                        Uri = String.Format(
                            this.BlobUri.ToString(), 
                            this.DatabaseName, 
                            DateTime.UtcNow.Ticks.ToString())
                    },
                    ConnectionInfo = new ConnectionInfo
                    {
                        ServerName = this.ServerName + DataServiceConstants.AzureSqlDatabaseDnsSuffix,
                        DatabaseName = this.DatabaseName,
                        UserName = this.Username,
                        Password = this.Password
                    }
                };
                this.WriteVerbose("Done");

                this.WriteVerbose("Calling ExportSqlAzureDatabaseProcess");
                XmlElement status = this.ExportSqlAzureDatabaseProcess(this.ServerName, exportInput);
                this.WriteVerbose("Done");

                this.WriteVerbose("Writing out result");
                this.WriteObject(status.InnerText);
                this.WriteVerbose("Done");
            }
            catch (Exception ex)
            {
                this.WriteVerbose("An error occured!: " + ex.Message);
                this.WriteWindowsAzureError(
                    new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        /// <summary>
        /// Performs the call to export database using the server data service context channel.
        /// </summary>
        /// <param name="serverName">The name of the server to connect to.</param>
        /// <param name="input">The <see cref="ExportInput"/> object that contains 
        /// all the connection information</param>
        /// <returns></returns>
        internal XmlElement ExportSqlAzureDatabaseProcess(string serverName, ExportInput input)
        {
            XmlElement result = null;

            try
            {
                this.WriteVerbose("\t\tInvokeInOperationContext");
                InvokeInOperationContext(() =>
                {
                    result = RetryCall(subscription => 
                        Channel.ExportDatabase(subscription, serverName, input));

                    Operation operation = WaitForSqlDatabaseOperation();
                });
                this.WriteVerbose("\t\tDone");
            }
            catch (CommunicationException ex)
            {
                this.WriteVerbose("\t\tThere was an error");
                this.WriteErrorDetails(ex);
                this.WriteVerbose("\t\tDone");
            }

            return result;
        }

    }
}

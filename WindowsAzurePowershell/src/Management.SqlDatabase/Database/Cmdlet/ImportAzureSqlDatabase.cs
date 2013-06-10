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
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server;
    using Microsoft.WindowsAzure.ServiceManagement;

    [Cmdlet("Import", "AzureSqlDatabase", ConfirmImpact = ConfirmImpact.Medium)]
    public class ImportAzureSqlDatabase : SqlDatabaseManagementCmdletBase
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="ImportAzureSqlDatabase"/> class.
        /// </summary>
        public ImportAzureSqlDatabase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="ImportAzureSqlDatabase"/> class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public ImportAzureSqlDatabase(ISqlDatabaseManagement channel)
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
        /// Gets or sets the edition to use for the database being imported
        /// </summary>
        [Parameter(Mandatory = true, Position = 4, 
            HelpMessage = "The edition to use for the new database")]
        [ValidateNotNull]
        public DatabaseEdition Edition { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the database being imported
        /// </summary>
        [Parameter(Mandatory = true, Position = 5,
            HelpMessage = "The maximum size for the newly imported database")]
        [ValidateNotNull]
        public int MaxSizeGb { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URI for the blob storage
        /// </summary>
        [Parameter(Mandatory = true, Position = 6,
            HelpMessage = "The uri to where the blob storage is.")]
        [ValidateNotNull]
        public Uri BlobUri { get; set; }

        /// <summary>
        /// Gets or sets the storage key for accessing the blob storage
        /// </summary>
        [Parameter(Mandatory = true, Position = 7,
            HelpMessage = "The Endpoint Uri where the blob storage resides")]
        [ValidateNotNullOrEmpty]
        public string StorageKey { get; set; }

        #endregion 

        /// <summary>
        /// Performs the call to import database using the server data service context channel.
        /// </summary>
        /// <param name="serverName">The name of the server to connect to.</param>
        /// <param name="input">The <see cref="ImportInput"/> object that contains 
        /// all the connection and database information</param>
        /// <returns>The result of the import request.  Upon success contains the GUID of the request</returns>
        internal XmlElement ImportSqlAzureDatabaseProcess(string serverName, ImportInput input)
        {
            XmlElement result = null;

            try
            {
                this.InvokeInOperationContext(() =>
                {
                    result = RetryCall(subscription =>
                        this.Channel.ImportDatabase(subscription, serverName, input));

                    Microsoft.WindowsAzure.ServiceManagement.Operation operation = WaitForSqlDatabaseOperation();
                });
            }
            catch (CommunicationException ex)
            {
                this.WriteErrorDetails(ex);
            }

            return result;
        }

        /// <summary>
        /// Process the import request
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                // Create Web Request Inputs - Blob Storage Credentials and Server Connection Info
                ImportInput exportInput = new ImportInput
                {
                    AzureEdition = this.Edition.ToString(),
                    DatabaseSizeInGB = this.MaxSizeGb,
                    BlobCredentials = new BlobStorageAccessKeyCredentials
                    {
                        StorageAccessKey = this.StorageKey,
                        Uri = string.Format(
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

                XmlElement status = this.ImportSqlAzureDatabaseProcess(this.ServerName, exportInput);

                this.WriteObject(status.InnerText);
            }
            catch (Exception ex)
            {
                this.WriteWindowsAzureError(
                    new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }


    }
}

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
    using Microsoft.WindowsAzure.Management.Storage.Model.ResourceModel;

    /// <summary>
    /// Exports a database from Sql Azure into blob storage.
    /// </summary>
    [Cmdlet("Export", "AzureSqlDatabase", ConfirmImpact = ConfirmImpact.Medium)]
    public class ExportAzureSqlDatabase : PSCmdlet
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
        /// Gets or sets the context for connecting to the server
        /// </summary>
        [Parameter(Mandatory = true, Position = 0,
            HelpMessage = "The context for connecting to the server")]
        [ValidateNotNullOrEmpty]
        public ServerDataServiceSqlAuth Context { get; set; }

        /// <summary>
        /// Gets or sets the destination storage container for the bacpac
        /// </summary>
        [Parameter(Mandatory = true, Position = 1,
            HelpMessage = "The Azure Storage Container to place the blob in.")]
        [ValidateNotNull]
        public AzureStorageContainer StorageContainer { get; set; }

        /// <summary>
        /// Gets or sets the name of the database to export
        /// </summary>
        [Parameter(Mandatory = true, Position = 2,
            HelpMessage = "The name of the database to export")]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets name of the blob to use for the export
        /// </summary>
        [Parameter(Mandatory = true, Position = 3,
            HelpMessage = "The name of the blob to use for the export")]
        [ValidateNotNullOrEmpty]
        public string BlobName { get; set; }

        #endregion

        /// <summary>
        /// Performs the call to export database using the server data service context channel.
        /// </summary>
        /// <param name="serverName">The name of the server to connect to.</param>
        /// <param name="input">The <see cref="ExportInput"/> object that contains 
        /// all the connection information</param>
        /// <returns>The result of export request.  Upon success the <see cref="ImportExportRequest"/>
        /// for the request</returns>
        internal ImportExportRequest ExportSqlAzureDatabaseProcess(string serverName, ExportInput input)
        {
            ImportExportRequest result = null;

            try
            {
                result = new ImportExportRequest();
                result.SqlCredentials = this.Context.SqlCredentials;
                result.ServerName = serverName;


            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this,
                    this.Context.ClientRequestId,
                    ex);
            }

            return result;
        }

        /// <summary>
        /// Process the export request
        /// </summary>
        protected override void ProcessRecord()
        {
            this.WriteVerbose("Starting to process the record");
            try
            {
                base.ProcessRecord();

                // Create Web Request Inputs - Blob Storage Credentials and Server Connection Info
                ExportInput exportInput = new ExportInput
                {
                    BlobCredentials = new BlobStorageAccessKeyCredentials
                    {
                        StorageAccessKey = System.Convert.ToBase64String(this.StorageContainer.CloudBlobContainer.ServiceClient.Credentials.ExportKey()),
                        Uri = string.Format(
                            this.StorageContainer.Context.BlobEndPoint, 
                            this.DatabaseName, 
                            DateTime.UtcNow.Ticks.ToString())
                    },
                    ConnectionInfo = new ConnectionInfo
                    {
                        ServerName = this.Context.ServerName + DataServiceConstants.AzureSqlDatabaseDnsSuffix,
                        DatabaseName = this.DatabaseName,
                        UserName = this.Context.SqlCredentials.UserName,
                        Password = this.Context.SqlCredentials.Password
                    }
                };

                ImportExportRequest status = this.ExportSqlAzureDatabaseProcess(this.Context.ServerName, exportInput);

                this.WriteObject(status);
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

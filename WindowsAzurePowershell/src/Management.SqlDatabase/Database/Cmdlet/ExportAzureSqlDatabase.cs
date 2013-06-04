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
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.ImportExport;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    /// <summary>
    /// Exports a database from Sql Azure into blob storage.
    /// </summary>
    [Cmdlet("Export", "AzureSqlDatabase", SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.Medium)]
    public class ExportAzureSqlDatabase : PSCmdlet
    {
        #region Parameters

        /// <summary>
        /// Gets or sets the user name for connecting to the database
        /// </summary>
        [Parameter(Mandatory = true, Position = 0,
            HelpMessage = "The user name for connecting to the database")]
        [ValidateNotNullOrEmpty]
        public string UserName { get; set; }

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
            HelpMessage = "The Endpoint Uri where the blob storage resides")]
        [ValidateNotNull]
        public Uri Endpoint { get; set; }

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
            string clientRequestId = SqlDatabaseManagementHelper.GenerateClientTracingId();

            // Do nothing if force is not specified and user cancelled the operation
            //if (!this.ShouldProcess(
            //    Resources.ExportAzureSqlDatabaseDescription,
            //    Resources.ExportAzureSqlDatabaseWarning,
            //    Resources.ShouldProcessCaption))
            //{
            //    return;
            //}

            try
            {
                //Need to connect to RDFE here... (get connection context)

                //Create request.
                ExportInput input = new ExportInput()
                {
                    BlobCredentials = new BlobStorageAccessKeyCredentials()
                    {
                        StorageAccessKey = this.StorageKey,
                        Uri = this.Endpoint.ToString(),
                    },
                    ConnectionInfo = new ConnectionInfo()
                    {
                        DatabaseName = this.DatabaseName,
                        Password = this.Password,
                        ServerName = this.ServerName,
                        UserName = this.UserName,
                    }
                };

                //make the call to "context.ExportDatabase(SubscritionId, ServerName, ExportInput);"

                //write out the result
                //this.WriteObject(result, true);
            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this,
                    clientRequestId,
                    ex);
            }

        }

    }
}

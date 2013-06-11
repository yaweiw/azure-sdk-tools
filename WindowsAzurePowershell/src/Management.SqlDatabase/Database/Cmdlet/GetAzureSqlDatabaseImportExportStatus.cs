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
    [Cmdlet(VerbsCommon.Get, "AzureSqlDatabaseImportExportStatus", ConfirmImpact = ConfirmImpact.Low)]
    public class GetAzureSqlDatabaseImportExportStatus : SqlDatabaseManagementCmdletBase
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="GetAzureSqlDatabaseImportExportStatus"/> class.
        /// </summary>
        public GetAzureSqlDatabaseImportExportStatus()
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="GetAzureSqlDatabaseImportExportStatus"/> class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public GetAzureSqlDatabaseImportExportStatus(ISqlDatabaseManagement channel)
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
        /// Gets or sets the request Id of the operation to get the status of
        /// </summary>
        [Parameter(Mandatory = true, Position = 3,
            HelpMessage = "The request Id of the operation to get the status of")]
        [ValidateNotNullOrEmpty]
        public string RequestId { get; set; }

        #endregion

        /// <summary>
        /// Performs the call to export database using the server data service context channel.
        /// </summary>
        /// <param name="serverName">The name of the server to connect to.</param>
        /// <param name="userName">The username for authentication</param>
        /// <param name="password">The password for authentication</param>
        /// <param name="requestId">The request Id of the operation to query</param>
        /// <returns>The status of the import/export operation</returns>
        internal StatusInfo GetAzureSqlDatabaseImportExportStatusProcess(
            string serverName, 
            string userName,
            string password,
            string requestId)
        {
            StatusInfo result = null;

            try
            {
                this.InvokeInOperationContext(() =>
                {
                    result = RetryCall(subscription =>
                        this.Channel.GetImportExportStatus(
                            subscription, 
                            serverName, 
                            userName, 
                            password, 
                            requestId));

                    Operation operation = WaitForSqlDatabaseOperation();
                });
            }
            catch (CommunicationException ex)
            {
                this.WriteErrorDetails(ex);
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

                StatusInfo status = 
                    this.GetAzureSqlDatabaseImportExportStatusProcess(
                        this.ServerName, 
                        this.Username, 
                        this.Password, 
                        this.RequestId);

                this.WriteObject(status);
            }
            catch (Exception ex)
            {
                this.WriteWindowsAzureError(
                    new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}

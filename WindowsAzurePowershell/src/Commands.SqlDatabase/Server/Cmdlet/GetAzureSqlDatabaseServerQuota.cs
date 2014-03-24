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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Server.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Model;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.Sql;
    using Microsoft.WindowsAzure.Management.Sql.Models;
    using Services.Common;
    using Services.Server;

    /// <summary>
    /// Retrieves a list of Windows Azure SQL Database server quotas for the selected server.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSqlDatabaseServerQuota", ConfirmImpact = ConfirmImpact.None)]
    public class GetAzureSqlDatabaseServerQuota : SqlDatabaseCmdletBase
    {
        #region Parameter Sets

        /// <summary>
        /// The name of the parameter set for connection with a connection context
        /// </summary>
        internal const string ByConnectionContext =
            "ByConnectionContext";

        /// <summary>
        /// The name of the parameter set for connecting with an azure subscription
        /// </summary>
        internal const string ByServerName =
            "ByServerName";

        #endregion

        #region Parameters

        /// <summary>
        /// Gets or sets the server connection context.
        /// </summary>
        [Alias("Context")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByConnectionContext,
            HelpMessage = "The connection context to the specified server.")]
        [ValidateNotNull]
        public IServerDataServiceContext ConnectionContext { get; set; }

        /// <summary>
        /// Gets or sets the name of the server to connect to
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true,
            ParameterSetName = ByServerName,
            HelpMessage = "The name of the server to connect to using the current subscription")]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the server quota to retrieve
        /// </summary>
        [Parameter(Position = 1, Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the quota to retrieve")]
        [ValidateNotNullOrEmpty]
        public string QuotaName { get; set; }

        #endregion

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            // Obtain the quota name from the given parameters.
            string quotaName = null;
            if (this.MyInvocation.BoundParameters.ContainsKey("QuotaName"))
            {
                quotaName = this.QuotaName;
            }

            switch(this.ParameterSetName)
            {
                case ByConnectionContext:
                    this.ProcessWithConnectionContext(quotaName);
                    break;
                case ByServerName:
                    this.ProcessWithServerName(quotaName);
                    break;
            }
        }

        private void ProcessWithConnectionContext(string quotaName)
        {
            try
            {
                if (!string.IsNullOrEmpty(quotaName))
                {
                    // Retrieve the quota with the specified name
                    this.WriteObject(this.ConnectionContext.GetQuota(quotaName));
                }
                else
                {
                    // No name specified, retrieve all quotas in the server
                    this.WriteObject(this.ConnectionContext.GetQuotas(), true);
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

        private void ProcessWithServerName(string quotaName)
        {
            try
            {
                base.ProcessRecord();

                SqlManagementClient sqlManagementClient = SqlDatabaseCmdletBase.GetCurrentSqlClient();

                // Retrieve the list of servers
                QuotaListResponse response = sqlManagementClient.Quotas.List(this.ServerName);
                IEnumerable<Quota> quotas = response.Quotas;
                if (!string.IsNullOrEmpty(quotaName))
                {
                    // Quota name is specified, find the one with the
                    // same name.
                    quotas = response.Quotas.Where(q => q.Name == quotaName);
                    if (quotas.Count() == 0)
                    {
                        throw new ItemNotFoundException(string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.GetAzureSqlDatabaseServerNotFound,
                            quotaName));
                    }
                }

                // Construct the result
                IEnumerable<SqlDatabaseServerQuotaContext> processResult = quotas.Select(
                    quota => new SqlDatabaseServerQuotaContext
                {
                    OperationStatus = Services.Constants.OperationSuccess,
                    OperationDescription = CommandRuntime.ToString(),
                    OperationId = response.RequestId,
                    ServerName = this.ServerName,
                    Name = quota.Name,
                    State = quota.State,
                    Type = quota.Type,
                    Value = quota.Value,
                });

                this.WriteObject(processResult);
            }
            catch(Exception ex)
            {
                this.WriteErrorDetails(ex);
            }
        }
    }
}

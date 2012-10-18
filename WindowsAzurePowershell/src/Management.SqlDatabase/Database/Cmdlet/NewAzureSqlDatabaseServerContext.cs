// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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
    using System.Data.Services.Client;
    using System.Linq;
    using System.Management.Automation;
    using System.Xml.Linq;
    using Microsoft.WindowsAzure.Management.Extensions;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Properties;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Common;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Services.Server;

    /// <summary>
    /// A cmdlet to Connect to a SQL server administration data service.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSqlDatabaseServerContext", ConfirmImpact = ConfirmImpact.None,
        DefaultParameterSetName = ServerNameWithSqlAuthParamSet)]
    public class NewAzureSqlDatabaseServerContext : PSCmdlet
    {
        #region ParameterSet Names

        internal const string ServerNameWithSqlAuthParamSet = "ByServerNameWithSqlAuth";
        internal const string FullyQualifiedServerNameWithSqlAuthParamSet = "ByFullyQualifiedServerNameWithSqlAuth";
        internal const string ManageUrlWithSqlAuthParamSet = "ByManageUrlWithSqlAuth";

        #endregion

        #region Parameters

        /// <summary>
        /// Gets or sets the management site data connection server name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true,
            ParameterSetName = ServerNameWithSqlAuthParamSet,
            HelpMessage = "The short server name")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true,
            ParameterSetName = ManageUrlWithSqlAuthParamSet,
            HelpMessage = "The short server name")]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the management site data connection fully qualified server name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = FullyQualifiedServerNameWithSqlAuthParamSet,
            HelpMessage = "The fully qualified server name")]
        [ValidateNotNull]
        public string FullyQualifiedServerName { get; set; }

        /// <summary>
        /// Gets or sets the management <see cref="Uri"/>.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ManageUrlWithSqlAuthParamSet,
            HelpMessage = "The full management Url for the server")]
        [ValidateNotNullOrEmpty]
        public Uri ManageUrl { get; set; }

        /// <summary>
        /// Gets or sets the server credentials
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ServerNameWithSqlAuthParamSet,
            HelpMessage = "The credentials for the server")]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = FullyQualifiedServerNameWithSqlAuthParamSet,
            HelpMessage = "The credentials for the server")]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = ManageUrlWithSqlAuthParamSet,
            HelpMessage = "The credentials for the server")]
        [ValidateNotNull]
        public PSCredential Credential { get; set; }

        #endregion

        /// <summary>
        /// Connect to a Azure Sql Server with the given ManagementService Uri using
        /// Sql Authentication credentials.
        /// </summary>
        /// <param name="serverName">The server name.</param>
        /// <param name="managementServiceUri">The server's ManagementService Uri.</param>
        /// <param name="credentials">The Sql Authentication credentials for the server.</param>
        /// <returns>A new <see cref="ServerDataServiceSqlAuth"/> context,
        /// or <c>null</c> if an error occurred.</returns>
        internal ServerDataServiceSqlAuth GetServerDataServiceBySqlAuth(
            string serverName,
            Uri managementServiceUri,
            SqlAuthenticationCredentials credentials)
        {
            ServerDataServiceSqlAuth context = null;

            Guid sessionActivityId = Guid.NewGuid();
            try
            {
                context = ServerDataServiceSqlAuth.Create(
                    managementServiceUri,
                    sessionActivityId,
                    credentials,
                    serverName);

                // Retrieve $metadata to verify model version compativility
                XDocument metadata = context.RetrieveMetadata();
                string metadataHash = DataConnectionUtility.GetDocumentHash(metadata);
                if (metadataHash != context.metadataHash)
                {
                    this.WriteWarning(Resources.WarningModelOutOfDate);
                }

                context.MergeOption = MergeOption.PreserveChanges;
            }
            catch (Exception ex)
            {
                SqlDatabaseExceptionHandler.WriteErrorDetails(
                    this,
                    sessionActivityId.ToString(),
                    ex);

                // The context is not in an valid state because of the error, set the context 
                // back to null.
                context = null;
            }

            return context;
        }

        /// <summary>
        /// Creates a new operation context based on the Cmdlet's parameter set and the manageUrl.
        /// </summary>
        /// <param name="serverName">The server name.</param>
        /// <param name="managementServiceUri">The server's ManagementService Uri.</param>
        /// <returns>A new operation context for the server.</returns>
        internal IServerDataServiceContext CreateServerDataServiceContext(
            string serverName,
            Uri managementServiceUri)
        {
            switch (this.ParameterSetName)
            {
                case ServerNameWithSqlAuthParamSet:
                case FullyQualifiedServerNameWithSqlAuthParamSet:
                case ManageUrlWithSqlAuthParamSet:
                    // Obtain the Server DataService Context by Sql Authentication
                    SqlAuthenticationCredentials credentials = this.GetSqlAuthCredentials();
                    return this.GetServerDataServiceBySqlAuth(
                        serverName,
                        managementServiceUri,
                        credentials);
                default:
                    throw new InvalidOperationException(Resources.UnknownParameterSet);
            }
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();

                // First obtain the Management Service Uri and the ServerName
                Uri manageUrl = this.GetManageUrl(this.ParameterSetName);
                Uri managementServiceUri = DataConnectionUtility.GetManagementServiceUri(manageUrl);
                string serverName = this.GetServerName(manageUrl);

                // Creates a new Server Data Service Context for the service
                IServerDataServiceContext operationContext =
                    this.CreateServerDataServiceContext(serverName, managementServiceUri);
                if (operationContext != null)
                {
                    this.WriteObject(operationContext);
                }
            }
            catch (Exception ex)
            {
                this.WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        #region Parameter Parsing Helpers

        /// <summary>
        /// Obtain the ManageUrl based on the Cmdlet's parameter set.
        /// </summary>
        /// <param name="parameterSetName">The name of the invoking parameter set.</param>
        /// <returns>The ManageUrl based on the Cmdlet's parameter set.</returns>
        private Uri GetManageUrl(string parameterSetName)
        {
            switch (parameterSetName)
            {
                case ServerNameWithSqlAuthParamSet:
                    // Only the server name was specified, eg. 'server001'. Prepend the Uri schema
                    // and append the azure database DNS suffix.
                    return new Uri(
                        Uri.UriSchemeHttps + Uri.SchemeDelimiter +
                        this.ServerName + DataServiceConstants.AzureSqlDatabaseDnsSuffix);
                case FullyQualifiedServerNameWithSqlAuthParamSet:
                    // The fully qualified server name was specified, 
                    // eg. 'server001.database.windows.net'. Prepend the Uri schema.
                    return new Uri(
                        Uri.UriSchemeHttps + Uri.SchemeDelimiter +
                        this.FullyQualifiedServerName);
                case ManageUrlWithSqlAuthParamSet:
                    // The full ManageUrl was specified, 
                    // eg. 'https://server001.database.windows.net'. Return as is.
                    return this.ManageUrl;
                default:
                    // Should never get to here, this is an invalid parameter set
                    throw new InvalidOperationException(Resources.UnknownParameterSet);
            }
        }

        /// <summary>
        /// Obtain the ServerName based on the Cmdlet's parameter set.
        /// </summary>
        /// <param name="manageUrl">The server's manageUrl.</param>
        /// <returns>The ServerName based on the Cmdlet's parameter set.</returns>
        private string GetServerName(Uri manageUrl)
        {
            if (this.MyInvocation.BoundParameters.ContainsKey("ServerName"))
            {
                // Server name is specified, return as is.
                return this.ServerName;
            }
            else
            {
                // Server name is not specified, use the first subdomain name in the manageUrl.
                return manageUrl.Host.Split('.').First();
            }
        }

        /// <summary>
        /// Obtain the SqlAuthentication Credentials based on the Cmdlet's parameter set.
        /// </summary>
        /// <returns>The Credentials based on the Cmdlet's parameter set.</returns>
        private SqlAuthenticationCredentials GetSqlAuthCredentials()
        {
            if (this.MyInvocation.BoundParameters.ContainsKey("Credential"))
            {
                return new SqlAuthenticationCredentials(
                    this.Credential.UserName,
                    this.Credential.Password);
            }
            else
            {
                throw new ArgumentException(Resources.CredentialNotSpecified);
            }
        }

        #endregion
    }
}

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

namespace Microsoft.WindowsAzure.Management.SqlDatabase
{
    using System.Globalization;
    using System.Management.Automation;
    using System.ServiceModel;
    using CloudService.Cmdlet.Common;
    using CloudService.Services;
    using Properties;
    using Services;

    /// <summary>
    /// The base class for all Windows Azure Sql Database Management Cmdlets
    /// </summary>
    public abstract class SqlDatabaseManagementCmdletBase : CloudCmdlet<ISqlDatabaseManagement>
    {
        /// <summary>
        /// Stores the session Id for all the request made in this session.
        /// </summary>
        internal static string clientSessionId;

        static SqlDatabaseManagementCmdletBase()
        {
            clientSessionId = SqlDatabaseManagementHelper.GenerateClientTracingId();
        }

        /// <summary>
        /// Stores the per request session Id for all request made in this cmdlet call.
        /// </summary>
        private string clientRequestId;

        internal SqlDatabaseManagementCmdletBase()
        {
            this.clientRequestId = SqlDatabaseManagementHelper.GenerateClientTracingId();
        }

        // Windows Azure SQL Database doesn't support async calls
        protected static Operation WaitForSqlDatabaseOperation()
        {
            string operationId = RetrieveOperationId();
            Operation operation = new Operation();
            operation.OperationTrackingId = operationId;
            operation.Status = "Success";
            return operation;
        }

        protected override void WriteErrorDetails(CommunicationException exception)
        {
            string requestId;
            ErrorRecord errorRecord;
            SqlDatabaseManagementHelper.RetrieveExceptionDetails(exception, out errorRecord, out requestId);

            // Write the request Id as a warning
            if (requestId != null)
            {
                // requestId was availiable from the server response, write that as warning to the console
                WriteWarning(string.Format(CultureInfo.InvariantCulture, Resources.ExceptionRequestId, requestId));
            }
            else
            {
                // requestId was not availiable from the server response, write the client Ids that was sent
                WriteWarning(string.Format(CultureInfo.InvariantCulture, Resources.ExceptionClientSessionId, SqlDatabaseManagementCmdletBase.clientSessionId));
                WriteWarning(string.Format(CultureInfo.InvariantCulture, Resources.ExceptionClientRequestId, this.clientRequestId));
            }

            // Write the actual errorRecord containing the exception details
            WriteError(errorRecord);
        }
    }
}
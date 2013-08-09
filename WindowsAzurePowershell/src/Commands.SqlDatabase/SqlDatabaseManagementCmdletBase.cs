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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase
{
    using System.ServiceModel;
    using Commands.Utilities.Common;
    using ServiceManagement;
    using Services;
    using Services.Common;

    /// <summary>
    /// The base class for all Windows Azure Sql Database Management Cmdlets
    /// </summary>
    public abstract class SqlDatabaseManagementCmdletBase : CloudBaseCmdlet<ISqlDatabaseManagement>
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
            // Call the handler to parse and write error details.
            SqlDatabaseExceptionHandler.WriteErrorDetails(this, this.clientRequestId, exception);
        }
    }
}
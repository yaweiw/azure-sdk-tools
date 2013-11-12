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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Database.Cmdlet
{
    using System;
    using System.Management.Automation;
    using Commands.Utilities.Common;
    using Properties;
    using Services.Common;
    using Services.Server;
    /// <summary>
    /// Retrieves a list of Windows Azure SQL Database's operations in the given server context.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSqlDatabaseOperations", ConfirmImpact = ConfirmImpact.None,
        DefaultParameterSetName = ByConnectionContext)]
    public class GetAzureSqlDatabaseOperations : GetAzureSqlDatabaseBase
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }

        protected override void OperationOnContext(IServerDataServiceContext context, string databaseName)
        {
            // ximchen Question: Do I need to add a true after this one to be able to pass to pipeline?
            this.WriteObject(context.GetDatabaseOperations(databaseName), true);
        }

        protected override void OperationOnContext(IServerDataServiceContext context)
        {
            this.WriteObject(context.GetDatabasesOperations(), true);
        }
    }
}

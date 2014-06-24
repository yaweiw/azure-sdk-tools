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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Database.Cmdlet
{
    using System;
    using System.Threading;
    using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Server;
    using System.Management.Automation;

    internal static class CmdletCommon
    {
        public static DateTime NormalizeToUtc(DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Utc:
                    return dateTime;

                case DateTimeKind.Local:
                    return dateTime.ToUniversalTime();

                case DateTimeKind.Unspecified:
                default:
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Queries the server until the database assignment succeeds or there is an error.
        /// </summary>
        /// <param name="context">The context upon which to perform the action</param>
        /// <param name="response">The database object.</param>
        /// <returns>Returns the response from the server</returns>
        public static Database WaitForSloAssignmentCompletion(PSCmdlet cmdlet, IServerDataServiceContext context, Database response, string databaseName)
        {
            // Duration in ms to sleep
            int sleepDuration = 1000;

            // Loop for 10 minutes at 60 seconds per minute at 1000/sleepDuration polls per second.
            int loopTime = (int)(60f * 10f * (1000f / sleepDuration));
            string pendingText = "Pending";
            for (int i = 0; i < loopTime; i++)
            {
                if (response == null)
                {
                    throw new Exception("An unexpected error occured.  The response from the server was null.");
                }

                // Check to see if the assignment is still pending.
                if (response.ServiceObjectiveAssignmentState != 0)
                {
                    // The SLO assignment completed so lets stop waiting.
                    break;
                }

                // Wait 1000ms before next poll.
                Thread.Sleep(sleepDuration);

                // Append a '.' so it looks like stuff is still happening 
                pendingText += '.';
                cmdlet.WriteProgress(new ProgressRecord(0, "Waiting for database creation completion.", pendingText));

                // Poll the server for the database status.
                response = context.GetDatabase(databaseName);
            }

            return response;
        }
    }
}

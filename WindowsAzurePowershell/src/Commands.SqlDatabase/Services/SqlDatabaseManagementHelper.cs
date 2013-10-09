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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Services
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    public static class SqlDatabaseManagementHelper
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposing the factory would also dispose the channel we are returning.")]
        public static ISqlDatabaseManagement CreateSqlDatabaseManagementChannel(Binding binding, Uri remoteUri, X509Certificate2 cert, string requestSessionId)
        {
            WebChannelFactory<ISqlDatabaseManagement> factory = new WebChannelFactory<ISqlDatabaseManagement>(binding, remoteUri);
            factory.Endpoint.Behaviors.Add(new ClientOutputMessageInspector(requestSessionId));
            factory.Credentials.ClientCertificate.Certificate = cert;
            return factory.CreateChannel();
        }

        /// <summary>
        /// Generates a client side tracing Id of the format:
        /// [Guid]-[Time in UTC]
        /// </summary>
        /// <returns>A string representation of the client side tracing Id.</returns>
        public static string GenerateClientTracingId()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("u"));
        }
    }
}

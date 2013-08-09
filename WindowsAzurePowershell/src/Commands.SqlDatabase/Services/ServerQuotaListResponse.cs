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

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Services
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a list of server quota objects
    /// </summary>
    [CollectionDataContract(Name = "ServiceResources", ItemName = "ServiceResource",
        Namespace = Constants.ServiceManagementNamespace)]
    public class ServerQuotaListResponse : List<ServerQuotaResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerQuotaListResponse"/> class.
        /// </summary>
        public ServerQuotaListResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerQuotaListResponse"/> class.
        /// </summary>
        /// <param name="quotas">The quotas to initialize the list with</param>
        public ServerQuotaListResponse(IEnumerable<ServerQuotaResponse> quotas)
            : base(quotas)
        {
        }
    }
}

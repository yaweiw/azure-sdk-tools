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


using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public class PSDeploymentEventData
    {
        public string EventId { get; set; }

        public string EventName { get; set; }

        public string EventSource { get; set; }

        public string Channels { get; set; }

        public string Level { get; set; }

        public string Description { get; set; }

        public DateTime Timestamp { get; set; }

        public string OperationId { get; set; }

        public string OperationName { get; set; }

        public string Status { get; set; }

        public string SubStatus { get; set; }

        public string ResourceGroup { get; set; }

        public string ResourceProvider { get; set; }

        public string ResourceUri { get; set; }

        public PSDeploymentEventDataHttpRequest HttpRequest { get; set; }

        public PSDeploymentEventDataAuthorization Authorization { get; set; }

        public Dictionary<string, string> Claims { get; set; }

        public Dictionary<string, string> Properties { get; set; }
    }
}

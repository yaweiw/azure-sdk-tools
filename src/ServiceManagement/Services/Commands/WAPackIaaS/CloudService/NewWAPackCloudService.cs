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

namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.CloudService
{
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Exceptions;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Operations;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.New, "WAPackCloudService")]
    public class NewWAPackCloudService : IaaSCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "CloudService Name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "CloudService Label.")]
        [ValidateNotNullOrEmpty]
        public string Label
        {
            get;
            set;
        }

        public override void ExecuteCmdlet()
        {
            IEnumerable<CloudService> results = null;

            Guid? cloudServiceJobId = Guid.Empty;

            var cloudService = new CloudService()
            {
                Name = this.Name,
                Label = this.Label
            };

            var cloudServiceOperations = new CloudServiceOperations(this.WebClientFactory);
            cloudServiceOperations.Create(cloudService, out cloudServiceJobId);
            WaitForJobCompletion(cloudServiceJobId);

            var createdCloudService = cloudServiceOperations.Read(this.Name);
            results = new List<CloudService>() { createdCloudService };
            this.GenerateCmdletOutput(results);
        }
    }
}

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

namespace Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Operations
{
    using DataContract;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Exceptions;
    using System.Data.Services.Client;
    using System;
    using System.Collections.Generic;
    using System.Net;

    internal class CloudServiceOperations : OperationsBase<CloudService>
    {
        private const string genericBaseUri = "/CloudServices?api-version=2013-03";
        private const string specificBaseUri = "/CloudServices/{0}?api-version=2013-03";
        private const string cloudResourceUri = "/CloudServices/{0}/Resources?api-version=2013-03";

        public CloudServiceOperations(WebClientFactory webClientFactory)
            : base(webClientFactory, genericBaseUri)
        {
        }

        public override CloudService Create(CloudService cloudServiceToCreate, out Guid? jobId)
        {
            var client = this.webClientFactory.CreateClient(genericBaseUri);
            WebHeaderCollection outHeaders;
            var resultList = client.Create<CloudService>(cloudServiceToCreate, out outHeaders);

            if (resultList.Count <= 0)
                throw new WAPackOperationException(Resources.ErrorCreatingCloudService);

            jobId = TryGetJobIdFromHeaders(outHeaders);

            return resultList[0];
        }

        public override List<CloudService> Read()
        {
            var client = this.webClientFactory.CreateClient(genericBaseUri);

            WebHeaderCollection outHeaders;
            var cloudServices = client.Get<CloudService>(out outHeaders);

            foreach (var cloudService in cloudServices)
            {
                client = this.webClientFactory.CreateClient(String.Format(cloudResourceUri, cloudService.Name));
                var cloudResource = client.Get<CloudResource>(out outHeaders);

                VMRoleOperations vmRoleOperations = new VMRoleOperations(this.webClientFactory);
                cloudResource[0].VMRoles.Load(vmRoleOperations.Read(cloudService.Name));
                cloudService.Resources.Load(cloudResource);
            }

            return cloudServices;
        }

        public CloudService Read(string cloudServiceName)
        {
            var client = this.webClientFactory.CreateClient(String.Format(specificBaseUri, cloudServiceName));

            WebHeaderCollection outHeaders;
            var cloudService = client.Get<CloudService>(out outHeaders)[0];

            client = this.webClientFactory.CreateClient(String.Format(cloudResourceUri, cloudServiceName));
            var cloudResource = client.Get<CloudResource>(out outHeaders)[0];

            VMRoleOperations vmRoleOperations = new VMRoleOperations(this.webClientFactory);
            cloudResource.VMRoles.Load(vmRoleOperations.Read(cloudServiceName));
            cloudService.Resources.Load(cloudResource);

            return cloudService;
        }

        public void Delete(string cloudServiceName, out Guid? jobId)
        {
            var client = this.webClientFactory.CreateClient(String.Format(specificBaseUri, cloudServiceName));

            WebHeaderCollection outHeaders;
            client.Delete<CloudService>(out outHeaders);

            jobId = TryGetJobIdFromHeaders(outHeaders);
        }
    }
}

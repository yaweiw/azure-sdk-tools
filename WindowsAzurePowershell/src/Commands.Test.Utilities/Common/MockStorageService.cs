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

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Commands.Utilities.Common;
    using Management.Storage;
    using Management.Storage.Models;
    using Moq;

    /// <summary>
    /// A class used to simulate the behavior of a storage account as far as existing,
    /// creation, and querying are concerned.
    /// </summary>
    public class MockStorageService
    {
        public class StorageAccountData
        {
            public string Name { get; set; }
            public string BlobEndpoint { get; set; }
            public string QueueEndpoint { get; set; }
            public string TableEndpoint { get; set; }
            public string PrimaryKey { get; set; }
            public string SecondaryKey { get; set; }
        }

        private readonly List<StorageAccountData> accounts = new List<StorageAccountData>();

        public MockStorageService Add(Action<StorageAccountData> dataSetter)
        {
            var account = new StorageAccountData();
            dataSetter(account);
            accounts.Add(account);
            return this;
        }

        public void InitializeMocks(Mock<StorageManagementClient> mock)
        {
            mock.Setup(c => c.StorageAccounts.GetAsync(It.IsAny<string>()))
                .Returns((string serviceName) => CreateGetResponse(serviceName));

            mock.Setup(c => c.StorageAccounts.GetKeysAsync(It.IsAny<string>()))
                .Returns((string serviceName) => CreateGetKeysResponse(serviceName));
        }

        private Task<StorageServiceGetResponse> CreateGetResponse(string serviceName)
        {
            Task<StorageServiceGetResponse> resultTask;
            var data = accounts.FirstOrDefault(a => a.Name == serviceName);
            if (data != null)
            {
                var storageServiceGetResponse = new StorageServiceGetResponse
                {
                    ServiceName = data.Name,
                    Properties = new StorageServiceProperties
                    {
                        Endpoints =
                        {
                            new Uri(data.BlobEndpoint),
                            new Uri(data.QueueEndpoint),
                            new Uri(data.TableEndpoint)
                        }
                    }
                };
                resultTask = Tasks.FromResult(storageServiceGetResponse);
            }
            else
            {
                resultTask = Tasks.FromException<StorageServiceGetResponse>(Make404Exception());
            }
            return resultTask;
        }

        private Task<StorageAccountGetKeysResponse> CreateGetKeysResponse(string serviceName)
        {
            Task<StorageAccountGetKeysResponse> resultTask;
            var data = accounts.FirstOrDefault(a => a.Name == serviceName);
            if (data != null)
            {
                var response = new StorageAccountGetKeysResponse
                {
                    PrimaryKey = data.PrimaryKey,
                    SecondaryKey = data.SecondaryKey,
                    StatusCode = HttpStatusCode.OK
                };
                resultTask = Tasks.FromResult(response);
            }
            else
            {
                resultTask = Tasks.FromException<StorageAccountGetKeysResponse>(Make404Exception());
            }
            return resultTask;
        }

        private CloudException Make404Exception()
        {
            return new CloudException("Not found", null, new HttpResponseMessage(HttpStatusCode.NotFound), "");
        }
    }
}

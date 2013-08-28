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
    using Management.Compute;
    using Management.Compute.Models;
    using Moq;

    /// <summary>
    /// This class simulates querying and updating hosted services.
    /// </summary>
    public class MockServicesHost
    {
        public class ServiceData
        {
            public string Name { get; set; }
            public DeploymentData ProductionDeployment { get; set; }
            public DeploymentData StagingDeployment { get; set; }

            public ServiceData AddDeployment(Action<DeploymentData> setter)
            {
                var data = new DeploymentData();
                setter(data);

                switch (data.Slot)
                {
                    case DeploymentSlot.Production:
                        ProductionDeployment = data;
                        break;
                    case DeploymentSlot.Staging:
                        StagingDeployment = data;
                        break;
                }
                return this;
            }
        }

        public class DeploymentData
        {
            public string Name { get; set; }
            public DeploymentSlot Slot { get; set; }
        }

        public IList<ServiceData> Services { get; private set; }

        public MockServicesHost()
        {
            Services = new List<ServiceData>();
        }

        public MockServicesHost Add(Action<ServiceData> setter)
        {
            var service = new ServiceData();
            setter(service);
            Services.Add(service);
            return this;
        }

        public void Clear()
        {
            Services.Clear();
        }

        public void InitializeMocks(Mock<ComputeManagementClient> mock)
        {
            mock.Setup(c => c.HostedServices.GetDetailedAsync(It.IsAny<string>()))
                .Returns((string s) => CreateGetDetailedResponse(s));
        }

        private Task<HostedServiceGetDetailedResponse> CreateGetDetailedResponse(string serviceName)
        {
            var service = Services.FirstOrDefault(s => s.Name == serviceName);
            Task<HostedServiceGetDetailedResponse> resultTask;

            if (service != null)
            {
                var response = new HostedServiceGetDetailedResponse
                {
                    ServiceName = service.Name,
                    StatusCode = HttpStatusCode.OK,
                };
                if (service.ProductionDeployment != null)
                {
                    response.Deployments.Add(CreateDeploymentResponse(service.ProductionDeployment));
                }

                if (service.StagingDeployment != null)
                {
                    response.Deployments.Add(CreateDeploymentResponse(service.StagingDeployment));
                }
                resultTask = Tasks.FromResult(response);
            }
            else
            {
                resultTask = Tasks.FromException<HostedServiceGetDetailedResponse>(Make404Exception());
            }
            return resultTask;
        }

        private HostedServiceGetDetailedResponse.Deployment CreateDeploymentResponse(DeploymentData d)
        {
            if (d != null)
            {
                return new HostedServiceGetDetailedResponse.Deployment()
                {
                    DeploymentSlot = d.Slot,
                    Name = d.Name,
                    Roles =
                    {
                        new Role
                        {
                            RoleName = "Role1"
                        }
                    }
                };
            }
            return null;
        }

        private CloudException Make404Exception()
        {
            return new CloudException("Not found", null, new HttpResponseMessage(HttpStatusCode.NotFound), "");
        }
    }
}

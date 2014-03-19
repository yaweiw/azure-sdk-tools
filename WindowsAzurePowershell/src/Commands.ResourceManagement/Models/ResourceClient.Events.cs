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

using Microsoft.Azure.Commands.ResourceManagement.Properties;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.WindowsAzure.Management.Monitoring.Events;
using Microsoft.WindowsAzure.Management.Monitoring.Events.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Commands.ResourceManagement.Models
{
    public partial class ResourcesClient
    {
        private const int EventRetentionPeriod = 89;

        /// <summary>
        /// Gets event logs.
        /// </summary>
        /// <param name="parameters">Input parameters</param>
        /// <returns>Logs.</returns>
        public virtual IEnumerable<PSDeploymentEventData> GetResourceGroupLogs(GetPSResourceGroupLogParameters parameters)
        {
            if (parameters.All)
            {
                EventDataListResponse listOfEvents =
                    EventsClient.EventData.ListEventsForResourceGroup(new ListEventsForResourceGroupParameters
                        {
                            ResourceGroupName = parameters.Name,
                            StartTime = DateTime.UtcNow - TimeSpan.FromDays(EventRetentionPeriod),
                            EndTime = DateTime.UtcNow
                        });
                return listOfEvents.EventDataCollection.Value.Select(e => e.ToPSDeploymentEventData());
            }
            else if (!string.IsNullOrEmpty(parameters.DeploymentName))
            {
                DeploymentGetResult deploymentGetResult;
                try
                {
                    deploymentGetResult = ResourceManagementClient.Deployments.Get(parameters.Name,
                                                                                   parameters.DeploymentName);
                }
                catch
                {
                    throw new ArgumentException(Resources.DeploymentNotFound);
                }

                return GetDeploymentLogs(deploymentGetResult.Deployment.Properties.TrackingId);
            }
            else
            {
                DeploymentListResult deploymentListResult;
                try
                {
                    deploymentListResult = ResourceManagementClient.Deployments.List(parameters.Name,
                                                                            new DeploymentListParameters
                                                                                {
                                                                                    Top = 1
                                                                                });
                    if (deploymentListResult.Deployments.Count == 0)
                    {
                        throw new ArgumentException(Resources.DeploymentNotFound);
                    }
                }
                catch
                {
                    throw new ArgumentException(Resources.DeploymentNotFound);
                }

                return GetDeploymentLogs(deploymentListResult.Deployments[0].Properties.TrackingId);
            }
        }

        /// <summary>
        /// Gets event logs by tracking Id.
        /// </summary>
        /// <param name="correlationId">CorrelationId Id of the deployment</param>
        /// <returns>Logs.</returns>
        public virtual IEnumerable<PSDeploymentEventData> GetDeploymentLogs(string correlationId)
        {
            EventDataListResponse listOfEvents = EventsClient.EventData.ListEventsForCorrelationId(new ListEventsForCorrelationIdParameters
                {
                    CorrelationId = correlationId,
                    StartTime = DateTime.UtcNow - TimeSpan.FromDays(EventRetentionPeriod),
                    EndTime = DateTime.UtcNow
                });
            return listOfEvents.EventDataCollection.Value.Select(e => e.ToPSDeploymentEventData());
        }
    }
}
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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.AffinityGroups
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Reflection;
    using AutoMapper;
    using Commands.Utilities.Common;
    using Management;
    using Management.Models;
    using Model;
    using WindowsAzure.ServiceManagement;


    /// <summary>
    /// List the properties for the specified affinity group.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureAffinityGroup"), OutputType(typeof(AffinityGroupContext))]
    public class GetAzureAffinityGroup : ServiceManagementBaseCmdlet
    {
        public GetAzureAffinityGroup()
        {
        }

        public GetAzureAffinityGroup(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Affinity Group name")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            OnProcessRecord();
        }

        protected override void OnProcessRecord()
        {
            Mapper.Initialize(m => m.AddProfile<ServiceManagementPofile>());

            if (this.Name != null)
            {
                ExecuteClientActionNewSM(null, 
                    CommandRuntime.ToString(), 
                    () => this.ManagementClient.AffinityGroups.Get(this.Name),
                    (s, affinityGroup) => (new int[1]).Select(i => ContextFactory<AffinityGroupGetResponse, AffinityGroupContext>(affinityGroup, s))
                );
            }
            else
            {
                ExecuteClientActionNewSM(null, 
                    CommandRuntime.ToString(), 
                    () => this.ManagementClient.AffinityGroups.List(),
                    (s, affinityGroups) => affinityGroups.AffinityGroups.Select(ag => ContextFactory<AffinityGroupListResponse.AffinityGroup, AffinityGroupContext>(ag, s))
                );
            }
        }
    }
}
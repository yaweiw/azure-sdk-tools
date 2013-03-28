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

using System.Net;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.ServiceModel;
    using Microsoft.WindowsAzure.ServiceManagement;
    using Model;

    [Cmdlet(VerbsCommon.Get, "AzureVM"), OutputType(typeof(List<PersistentVMRoleContext>), typeof(PersistentVMRoleListContext))]
    public class GetAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        public GetAzureVMCommand()
        {
        }

        public GetAzureVMCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the virtual machine to get.")]
        public virtual string Name
        {
            get;
            set;
        }

        internal override void ExecuteCommand()
        {
            base.ExecuteCommand();
            if (!string.IsNullOrEmpty(ServiceName) && CurrentDeployment == null)
            {
                return;
            }

            List<PersistentVMRoleContext> roles = new List<PersistentVMRoleContext>();
            RoleList vmRoles = null;

            if (string.IsNullOrEmpty(ServiceName))
            {
                ListAllVMs();
                return;
            }

            if (string.IsNullOrEmpty(Name))
            {
                vmRoles = CurrentDeployment.RoleList;
            }
            else
            {
                vmRoles = new RoleList(CurrentDeployment.RoleList.Where(r => r.RoleName.Equals(Name, StringComparison.InvariantCultureIgnoreCase)));
            }

            foreach (Role role in vmRoles)
            {
                string lastVM = string.Empty;

                try
                {
                    lastVM = role.RoleName;
                    PersistentVMRole vm = (PersistentVMRole)role;
                    PersistentVMRoleContext vmContext = new PersistentVMRoleContext();

                    if (CurrentDeployment != null)
                    {
                        vmContext.DNSName = CurrentDeployment.Url.AbsoluteUri;
                    }

                    vmContext.ServiceName = ServiceName;
                    vmContext.Name = vm.RoleName;
                    vmContext.DeploymentName = CurrentDeployment.Name;
                    vmContext.VM = new PersistentVM();
                    vmContext.VM.AvailabilitySetName = vm.AvailabilitySetName;
                    vmContext.AvailabilitySetName = vm.AvailabilitySetName;
                    vmContext.Label = vm.Label;
                    vmContext.VM.ConfigurationSets = vm.ConfigurationSets;
                    vmContext.VM.DataVirtualHardDisks = vm.DataVirtualHardDisks;
                    vmContext.VM.Label = vm.Label;
                    vmContext.VM.OSVirtualHardDisk = vm.OSVirtualHardDisk;
                    vmContext.VM.RoleName = vm.RoleName;
                    vmContext.Name = vm.RoleName;
                    vmContext.VM.RoleSize = vm.RoleSize;
                    vmContext.InstanceSize = vm.RoleSize;
                    vmContext.VM.RoleType = vm.RoleType;
                    vmContext.InstanceStatus = CurrentDeployment.RoleInstanceList.First(r => r.RoleName == vm.RoleName).InstanceStatus;
                    vmContext.IpAddress = CurrentDeployment.RoleInstanceList.First(r => r.RoleName == vm.RoleName).IpAddress;
                    vmContext.InstanceStateDetails = CurrentDeployment.RoleInstanceList.First(r => r.RoleName == vm.RoleName).InstanceStateDetails;
                    vmContext.PowerState = CurrentDeployment.RoleInstanceList.First(r => r.RoleName == vm.RoleName).PowerState;
                    vmContext.InstanceErrorCode = CurrentDeployment.RoleInstanceList.First(r => r.RoleName == vm.RoleName).InstanceErrorCode;
                    vmContext.InstanceName = CurrentDeployment.RoleInstanceList.First(r => r.RoleName == vm.RoleName).InstanceName;
                    vmContext.InstanceFaultDomain = CurrentDeployment.RoleInstanceList.First(r => r.RoleName == vm.RoleName).InstanceFaultDomain.Value.ToString(CultureInfo.InvariantCulture);
                    vmContext.InstanceUpgradeDomain = CurrentDeployment.RoleInstanceList.First(r => r.RoleName == vm.RoleName).InstanceUpgradeDomain.Value.ToString(CultureInfo.InvariantCulture);
                    vmContext.OperationDescription = CommandRuntime.ToString();
                    vmContext.OperationId = GetDeploymentOperation.OperationTrackingId;
                    vmContext.OperationStatus = GetDeploymentOperation.Status;
                    roles.Add(vmContext);
                }
                catch (Exception)
                {
                    WriteObject(string.Format("Could not read properties for virtual machine: {0}. It may still be provisioning.", lastVM));
                }
            }

            if (!string.IsNullOrEmpty(Name) && roles != null && roles.Count > 0)
            {
                SaveRoleState(roles[0].VM);
            }

            WriteObject(roles, true);
        }

        protected virtual void SaveRoleState(PersistentVM role)
        {
        }

        private void ListAllVMs()
        {
            using (new OperationContextScope(Channel.ToContextChannel()))
            {
                HostedServiceList services = this.RetryCall(s => this.Channel.ListHostedServices(s));
                if (services != null)
                {
                    foreach (HostedService service in services)
                    {
                        using (new OperationContextScope(Channel.ToContextChannel()))
                        {
                            try
                            {
                                var deployment = this.RetryCall(s => this.Channel.GetDeploymentBySlot(s, service.ServiceName, "Production"));
                                foreach (Role role in deployment.RoleList)
                                {
                                    if (role.RoleType == "PersistentVMRole")
                                    {
                                        RoleInstance instance = deployment.RoleInstanceList.Where(r => r.RoleName == role.RoleName).First();
                                        PersistentVMRoleListContext vmContext = new PersistentVMRoleListContext()
                                        {
                                            ServiceName = service.ServiceName,
                                            Status = instance.InstanceStatus,
                                            Name = instance.RoleName
                                        };

                                        WriteObject(vmContext, true);
                                    }
                                }
                            }
                            catch (ServiceManagementClientException exc)
                            {
                                if(exc.HttpStatus != HttpStatusCode.NotFound)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using AutoMapper;
    using Helpers;
    using Management.Compute;
    using Management.Compute.Models;
    using Model;
    using Properties;
    using DataVirtualHardDisk = Model.PersistentVMModel.DataVirtualHardDisk;
    using OSVirtualHardDisk = Model.PersistentVMModel.OSVirtualHardDisk;
    using PVM = Model.PersistentVMModel;
    using RoleInstance = Management.Compute.Models.RoleInstance;

    [Cmdlet(VerbsCommon.Get, "AzureVM"), OutputType(typeof(List<PersistentVMRoleContext>), typeof(PersistentVMRoleListContext))]
    public class GetAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
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

        protected override void ExecuteCommand()
        {
            ServiceManagementProfile.Initialize();

            base.ExecuteCommand();
            if (!string.IsNullOrEmpty(ServiceName) && CurrentDeploymentNewSM == null)
            {
                return;
            }

            var roles = new List<PersistentVMRoleContext>();
            IList<Management.Compute.Models.Role> vmRoles;

            if (string.IsNullOrEmpty(ServiceName))
            {
                ListAllVMs();
                return;
            }

            if (string.IsNullOrEmpty(Name))
            {
                vmRoles = CurrentDeploymentNewSM.Roles;
            }
            else
            {
                vmRoles = new List<Management.Compute.Models.Role>(CurrentDeploymentNewSM.Roles.Where(r => r.RoleName.Equals(Name, StringComparison.InvariantCultureIgnoreCase)));
            }

            foreach (var role in vmRoles)
            {
                string lastVM = string.Empty;

                try
                {
                    lastVM = role.RoleName;
                    var vm = role;
                    var roleInstance = CurrentDeploymentNewSM.RoleInstances.FirstOrDefault(r => r.RoleName == vm.RoleName);
                    var vmContext = new PersistentVMRoleContext
                    {
                        ServiceName = ServiceName,
                        Name = vm.RoleName,
                        DeploymentName = CurrentDeploymentNewSM.Name,
                        AvailabilitySetName = vm.AvailabilitySetName,
                        Label = vm.Label,
                        InstanceSize = vm.RoleSize.ToString(),
                        InstanceStatus = roleInstance == null ? null : roleInstance.InstanceStatus,
                        IpAddress = roleInstance == null ? null : roleInstance.IPAddress,
                        InstanceStateDetails = roleInstance == null ? null : roleInstance.InstanceStateDetails,
                        PowerState = roleInstance == null ? null : roleInstance.PowerState.ToString(),
                        InstanceErrorCode = roleInstance == null ? null : roleInstance.InstanceErrorCode,
                        InstanceName = roleInstance == null ? null : roleInstance.InstanceName,
                        InstanceFaultDomain = roleInstance == null ? null : roleInstance.InstanceFaultDomain.HasValue ? roleInstance.InstanceFaultDomain.Value.ToString(CultureInfo.InvariantCulture) : null,
                        InstanceUpgradeDomain = roleInstance == null ? null : roleInstance.InstanceUpgradeDomain.HasValue ? roleInstance.InstanceUpgradeDomain.Value.ToString(CultureInfo.InvariantCulture) : null,
                        OperationDescription = CommandRuntime.ToString(),
                        OperationId = GetDeploymentOperationNewSM.Id,
                        OperationStatus = GetDeploymentOperationNewSM.Status.ToString(),
                        VM = new PersistentVM
                        {
                            AvailabilitySetName = vm.AvailabilitySetName,
                            ConfigurationSets = PersistentVMHelper.MapConfigurationSets(vm.ConfigurationSets),
                            DataVirtualHardDisks = Mapper.Map(vm.DataVirtualHardDisks, new Collection<DataVirtualHardDisk>()),
                            Label = vm.Label,
                            OSVirtualHardDisk = Mapper.Map(vm.OSVirtualHardDisk, new OSVirtualHardDisk()),
                            RoleName = vm.RoleName,
                            RoleSize = vm.RoleSize.ToString(),
                            RoleType = vm.RoleType,
                            DefaultWinRmCertificateThumbprint = vm.DefaultWinRmCertificateThumbprint,
                            ProvisionGuestAgent = vm.ProvisionGuestAgent,
                            ResourceExtensionReferences = Mapper.Map<PVM.ResourceExtensionReferenceList>(vm.ResourceExtensionReferences)
                        }
                    };

                    if (CurrentDeploymentNewSM != null)
                    {
                        vmContext.DNSName = CurrentDeploymentNewSM.Uri.AbsoluteUri;
                    }

                    roles.Add(vmContext);
                }
                catch (Exception e)
                {
                    throw new ApplicationException(string.Format(Resources.VMPropertiesCanNotBeRead, lastVM), e);
                }
            }

            WriteObject(roles, true);
        }

        private void ListAllVMs()
        {
            var servicesList = this.ComputeClient.HostedServices.List();
            foreach (var service in servicesList.HostedServices)
            {
                try
                {
                    var deploymentGetResponse = this.ComputeClient.Deployments.GetBySlot(service.ServiceName, DeploymentSlot.Production);
                    foreach (var role in deploymentGetResponse.Roles)
                    {
                        if (role.RoleType == "PersistentVMRole")
                        {
                            RoleInstance instance = deploymentGetResponse.RoleInstances.FirstOrDefault(r => r.RoleName == role.RoleName);
                            var vmContext = new PersistentVMRoleListContext
                                            {
                                                ServiceName = service.ServiceName,
                                                Status = instance == null ? null : instance.InstanceStatus,
                                                Name = instance == null ? null : instance.RoleName
                                            };

                            WriteObject(vmContext, true);
                        }
                    }
                }
                catch (CloudException e)
                {
                    if (e.Response.StatusCode != HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
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

using Microsoft.WindowsAzure.Commands.Utilities.Common;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Net;
    using AutoMapper;
    using Helpers;
    using Management.Compute.Models;
    using Model;
    using Properties;
    using DataVirtualHardDisk = Model.PersistentVMModel.DataVirtualHardDisk;
    using OSVirtualHardDisk = Model.PersistentVMModel.OSVirtualHardDisk;
    using PVM = Model.PersistentVMModel;
    using RoleInstance = Management.Compute.Models.RoleInstance;

    [Cmdlet(VerbsCommon.Get, "AzureVM"), OutputType(typeof(PersistentVMRoleContext))]
    public class GetAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        protected const string PersistentVMRoleStr = "PersistentVMRole";

        [Parameter(
            Position = 0,
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName { get; set; }

        [Parameter(
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The name of the virtual machine to get.")]
        public virtual string Name { get; set; }

        protected override void ExecuteCommand()
        {
            ServiceManagementProfile.Initialize(this);
            base.ExecuteCommand();

            if (!string.IsNullOrEmpty(ServiceName) && CurrentDeploymentNewSM == null)
            {
                WriteWarning(string.Format(Resources.NoDeploymentFoundInService, ServiceName));
                return;
            }

            if (string.IsNullOrEmpty(ServiceName))
            {
                var roleContexts = new List<PersistentVMRoleListContext>();
                var servicesList = this.ComputeClient.HostedServices.List();
                foreach (var service in servicesList.HostedServices)
                {
                    try
                    {
                        var deployment = this.ComputeClient.Deployments.GetBySlot(
                            service.ServiceName,
                            DeploymentSlot.Production);

                        foreach (var vm in deployment.Roles)
                        {
                            if (string.Equals(vm.RoleType, PersistentVMRoleStr, StringComparison.OrdinalIgnoreCase))
                            {
                                var roleInstance = deployment.RoleInstances.FirstOrDefault(
                                    r => r.RoleName == vm.RoleName);

                                if (roleInstance == null)
                                {
                                    WriteWarning(string.Format(Resources.RoleInstanceCanNotBeFoundWithName, vm.RoleName));
                                    roleInstance = new RoleInstance();
                                }

                                var vmContext = GetContext<PersistentVMRoleListContext>(
                                    service.ServiceName,
                                    vm,
                                    roleInstance,
                                    deployment);

                                roleContexts.Add(vmContext);
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

                WriteObject(roleContexts, true);
            }
            else
            {
                var roleContexts = new List<PersistentVMRoleContext>();

                var vmRoles = new List<Role>(CurrentDeploymentNewSM.Roles.Where(
                    r => string.IsNullOrEmpty(Name) || r.RoleName.Equals(Name, StringComparison.InvariantCultureIgnoreCase)));

                foreach (var vm in vmRoles)
                {
                    string lastVM = string.Empty;

                    try
                    {
                        lastVM = vm.RoleName;
                        var roleInstance = CurrentDeploymentNewSM.RoleInstances.FirstOrDefault(
                            r => r.RoleName == vm.RoleName);

                        if (roleInstance == null)
                        {
                            WriteWarning(string.Format(Resources.RoleInstanceCanNotBeFoundWithName, vm.RoleName));
                            roleInstance = new RoleInstance();
                        }

                        var vmContext = GetContext<PersistentVMRoleContext>(ServiceName, vm, roleInstance, CurrentDeploymentNewSM);

                        roleContexts.Add(vmContext);
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException(string.Format(Resources.VMPropertiesCanNotBeRead, lastVM), e);
                    }
                }

                WriteObject(roleContexts, true);
            }
        }

        private T GetContext<T>(
            string serviceName,
            Role vmRole,
            RoleInstance roleInstance,
            DeploymentGetResponse deployment)
            where T : PersistentVMRoleContext, new()
        {
            var vmContext = new T
            {
                ServiceName = serviceName,
                Name = vmRole.RoleName,
                DeploymentName = deployment.Name,
                AvailabilitySetName = vmRole.AvailabilitySetName,
                Label = vmRole.Label,
                InstanceSize = vmRole.RoleSize.ToString(),
                InstanceStatus = roleInstance.InstanceStatus,
                IpAddress = roleInstance.IPAddress,
                InstanceStateDetails = roleInstance.InstanceStateDetails,
                PowerState = roleInstance.PowerState.ToString(),
                InstanceErrorCode = roleInstance.InstanceErrorCode,
                InstanceName = roleInstance.InstanceName,
                InstanceFaultDomain = roleInstance.InstanceFaultDomain.HasValue ? roleInstance.InstanceFaultDomain.Value.ToString(CultureInfo.InvariantCulture) : null,
                InstanceUpgradeDomain = roleInstance.InstanceUpgradeDomain.HasValue ? roleInstance.InstanceUpgradeDomain.Value.ToString(CultureInfo.InvariantCulture) : null,
                Status = roleInstance.InstanceStatus,
                OperationDescription = CommandRuntime.ToString(),
                OperationId = deployment.RequestId,
                OperationStatus = deployment.StatusCode.ToString(),
                GuestAgentStatus = Mapper.Map<PVM.GuestAgentStatus>(roleInstance.GuestAgentStatus),
                ResourceExtensionStatusList = Mapper.Map<List<PVM.ResourceExtensionStatus>>(roleInstance.ResourceExtensionStatusList),
                VM = new PersistentVM
                {
                    AvailabilitySetName = vmRole.AvailabilitySetName,
                    ConfigurationSets = PersistentVMHelper.MapConfigurationSets(vmRole.ConfigurationSets),
                    DataVirtualHardDisks = Mapper.Map(vmRole.DataVirtualHardDisks, new Collection<DataVirtualHardDisk>()),
                    Label = vmRole.Label,
                    OSVirtualHardDisk = Mapper.Map(vmRole.OSVirtualHardDisk, new OSVirtualHardDisk()),
                    RoleName = vmRole.RoleName,
                    RoleSize = vmRole.RoleSize.ToString(),
                    RoleType = vmRole.RoleType,
                    DefaultWinRmCertificateThumbprint = vmRole.DefaultWinRmCertificateThumbprint,
                    ProvisionGuestAgent = vmRole.ProvisionGuestAgent,
                    ResourceExtensionReferences = Mapper.Map<PVM.ResourceExtensionReferenceList>(vmRole.ResourceExtensionReferences)
                }
            };

            if (deployment != null)
            {
                vmContext.DNSName = deployment.Uri.AbsoluteUri;
            }

            return vmContext;
        }
    }
}
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
    using System.Linq;
    using System.Globalization;
    using System.Management.Automation;
    using Commands.ServiceManagement.Helpers;
    using WindowsAzure.ServiceManagement;
    using Properties;
    using Model;

    [Cmdlet(VerbsData.Export, "AzureVM")]
    public class ExportAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        public ExportAzureVMCommand()
        {
        }

        public ExportAzureVMCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the virtual machine to get.")]
        public virtual string Name
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, HelpMessage = "The file path in which serialize the persistent VM role state.")]
        [ValidateNotNullOrEmpty]
        public string Path
        {
            get;
            set;
        }

        internal override void ExecuteCommand()
        {
            base.ExecuteCommand();
            if (CurrentDeployment == null)
            {
                return;
            }

            var role = CurrentDeployment.RoleList.FirstOrDefault(r => r.RoleName.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
            if(role == null)
            {
                throw new ApplicationException(string.Format(Resources.NoCorrespondingRoleCanBeFoundInDeployment, Name));
            }
            try
            {
                var vm = (PersistentVMRole)role;
                var roleInstance = CurrentDeployment.RoleInstanceList.First(r => r.RoleName == vm.RoleName);
                var vmContext = new PersistentVMRoleContext
                {
                    ServiceName = ServiceName,
                    Name = vm.RoleName,
                    DeploymentName = CurrentDeployment.Name,
                    AvailabilitySetName = vm.AvailabilitySetName,
                    Label = vm.Label,
                    InstanceSize = vm.RoleSize,
                    InstanceStatus = roleInstance.InstanceStatus,
                    IpAddress = roleInstance.IpAddress,
                    InstanceStateDetails = roleInstance.InstanceStateDetails,
                    PowerState = roleInstance.PowerState,
                    InstanceErrorCode = roleInstance.InstanceErrorCode,
                    InstanceName = roleInstance.InstanceName,
                    InstanceFaultDomain = roleInstance.InstanceFaultDomain.HasValue ? roleInstance.InstanceFaultDomain.Value.ToString(CultureInfo.InvariantCulture) : null,
                    InstanceUpgradeDomain = roleInstance.InstanceUpgradeDomain.HasValue ? roleInstance.InstanceUpgradeDomain.Value.ToString(CultureInfo.InvariantCulture) : null,
                    OperationDescription = CommandRuntime.ToString(),
                    OperationId = GetDeploymentOperation.OperationTrackingId,
                    OperationStatus = GetDeploymentOperation.Status,
                    VM = new PersistentVM
                    {
                        AvailabilitySetName = vm.AvailabilitySetName,
                        ConfigurationSets = vm.ConfigurationSets,
                        DataVirtualHardDisks = vm.DataVirtualHardDisks,
                        Label = vm.Label,
                        OSVirtualHardDisk = vm.OSVirtualHardDisk,
                        RoleName = vm.RoleName,
                        RoleSize = vm.RoleSize,
                        RoleType = vm.RoleType,
                        DefaultWinRmCertificateThumbprint = vm.DefaultWinRmCertificateThumbprint
                    }
                };
                PersistentVMHelper.SaveStateToFile(vmContext.VM, Path);
                WriteObject(vmContext, true);
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format(Resources.VMPropertiesCanNotBeRead, role.RoleName), e);
            }
        }
    }
}

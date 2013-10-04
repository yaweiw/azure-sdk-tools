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
    using System.Management.Automation;
    using AutoMapper;
    using Commands.Utilities.Common;
    using Management.Compute;
    using Management.Compute.Models;
    using Storage;
    using Model;
    using Properties;
    // TODO: Wait for fix
    // https://github.com/WindowsAzure/azure-sdk-for-net-pr/issues/187
    using Microsoft.WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsData.Update, "AzureVM"), OutputType(typeof(ManagementOperationContext))]
    public class UpdateAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Virtual Machine to update.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Virtual Machine to update.")]
        [ValidateNotNullOrEmpty]
        [Alias("InputObject")]
        public PersistentVM VM
        {
            get;
            set;
        }

        internal void ExecuteCommandNewSM()
        {
            ServiceManagementProfile.Initialize();

            base.ExecuteCommand();

            WindowsAzureSubscription currentSubscription = CurrentSubscription;
            if (CurrentDeploymentNewSM == null)
            {
                return;
            }

            // Auto generate disk names based off of default storage account 
            foreach (var datadisk in this.VM.DataVirtualHardDisks)
            {
                if (datadisk.MediaLink == null && string.IsNullOrEmpty(datadisk.DiskName))
                {
                    CloudStorageAccount currentStorage = currentSubscription.GetCloudStorageAccount();
                    if (currentStorage == null)
                    {
                        throw new ArgumentException(Resources.CurrentStorageAccountIsNotAccessible);
                    }

                    DateTime dateTimeCreated = DateTime.Now;
                    string diskPartName = VM.RoleName;

                    if (datadisk.DiskLabel != null)
                    {
                        diskPartName += "-" + datadisk.DiskLabel;
                    }

                    string vhdname = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", ServiceName, diskPartName, dateTimeCreated.Year, dateTimeCreated.Month, dateTimeCreated.Day, dateTimeCreated.Millisecond);
                    string blobEndpoint = currentStorage.BlobEndpoint.AbsoluteUri;

                    if (blobEndpoint.EndsWith("/") == false)
                    {
                        blobEndpoint += "/";
                    }

                    datadisk.MediaLink = new Uri(blobEndpoint + "vhds/" + vhdname);
                }

                if (VM.DataVirtualHardDisks.Count > 1)
                {
                    // To avoid duplicate disk names
                    System.Threading.Thread.Sleep(1);
                }
            }

            VirtualMachineRoleSize roleSizeResult;
            if (string.IsNullOrEmpty(VM.RoleSize))
            {
                roleSizeResult = VirtualMachineRoleSize.Small;
            }
            else if (!Enum.TryParse(VM.RoleSize, true, out roleSizeResult))
            {
                throw new ArgumentOutOfRangeException("RoleSize:" + VM.RoleSize);
            }

            //TODO: https://github.com/WindowsAzure/azure-sdk-for-net-pr/issues/130
            var parameters = new VirtualMachineUpdateParameters
            {
                AvailabilitySetName = VM.AvailabilitySetName,
                Label = VM.Label,
                OSVirtualHardDisk = Mapper.Map(VM.OSVirtualHardDisk, new Microsoft.WindowsAzure.Management.Compute.Models.OSVirtualHardDisk()),
                RoleName = VM.RoleName,
                RoleSize = roleSizeResult,
//                RoleType = VM.RoleType
            };
            VM.DataVirtualHardDisks.ForEach(c => parameters.DataVirtualHardDisks.Add(Mapper.Map(c, new Microsoft.WindowsAzure.Management.Compute.Models.DataVirtualHardDisk())));
            //VM.ConfigurationSets.ForEach(c => parameters.ConfigurationSets.Add(Mapper.Map(c, new Microsoft.WindowsAzure.Management.Compute.Models.ConfigurationSet())));
            Microsoft.WindowsAzure.Commands.ServiceManagement.Helpers.PersistentVMHelper.MapConfigurationSets(VM.ConfigurationSets).ForEach(c => parameters.ConfigurationSets.Add(c));

            ExecuteClientActionNewSM(
                parameters,
                CommandRuntime.ToString(),
                () => this.ComputeClient.VirtualMachines.Update(this.ServiceName, CurrentDeploymentNewSM.Name, this.Name, parameters));
        }
        
        internal override void ExecuteCommand()
        {
            this.ExecuteCommandOldSM();
        }

        internal void ExecuteCommandOldSM()
        {
            base.ExecuteCommand();

            var currentSubscription = this.CurrentSubscription;
            if (CurrentDeploymentNewSM == null)
            {
                return;
            }

            // Auto generate disk names based off of default storage account 
            foreach (var datadisk in this.VM.DataVirtualHardDisks)
            {
                if (datadisk.MediaLink == null && string.IsNullOrEmpty(datadisk.DiskName))
                {
                    CloudStorageAccount currentStorage = currentSubscription.CurrentCloudStorageAccount;
                    if (currentStorage == null)
                    {
                        throw new ArgumentException(Resources.CurrentStorageAccountIsNotAccessible);
                    }

                    DateTime dateTimeCreated = DateTime.Now;
                    string diskPartName = VM.RoleName;

                    if (datadisk.DiskLabel != null)
                    {
                        diskPartName += "-" + datadisk.DiskLabel;
                    }

                    string vhdname = string.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", ServiceName, diskPartName, dateTimeCreated.Year, dateTimeCreated.Month, dateTimeCreated.Day, dateTimeCreated.Millisecond);
                    string blobEndpoint = currentStorage.BlobEndpoint.AbsoluteUri;

                    if (blobEndpoint.EndsWith("/") == false)
                    {
                        blobEndpoint += "/";
                    }

                    datadisk.MediaLink = new Uri(blobEndpoint + "vhds/" + vhdname);
                }

                if (VM.DataVirtualHardDisks.Count > 1)
                {
                    // To avoid duplicate disk names
                    System.Threading.Thread.Sleep(1);
                }
            }

            var role = new Microsoft.WindowsAzure.ServiceManagement.PersistentVMRole
            {
                AvailabilitySetName = VM.AvailabilitySetName,
                ConfigurationSets = new System.Collections.ObjectModel.Collection<WindowsAzure.ServiceManagement.ConfigurationSet>(),
                DataVirtualHardDisks = new System.Collections.ObjectModel.Collection<WindowsAzure.ServiceManagement.DataVirtualHardDisk>(),
                Label = VM.Label,
                OSVirtualHardDisk = VM.OSVirtualHardDisk == null ? null : new Microsoft.WindowsAzure.ServiceManagement.OSVirtualHardDisk
                {
                    DiskLabel = VM.OSVirtualHardDisk.DiskLabel,
                    DiskName = VM.OSVirtualHardDisk.DiskName,
                    HostCaching = VM.OSVirtualHardDisk.HostCaching,
                    MediaLink = VM.OSVirtualHardDisk.MediaLink,
                    OS = VM.OSVirtualHardDisk.OS,
                    SourceImageName = VM.OSVirtualHardDisk.SourceImageName
                },
                RoleName = VM.RoleName,
                RoleSize = VM.RoleSize,
                RoleType = VM.RoleType
            };

            VM.DataVirtualHardDisks.ForEach(c => role.DataVirtualHardDisks.Add(new Microsoft.WindowsAzure.ServiceManagement.DataVirtualHardDisk
            {
                DiskLabel = c.DiskLabel,
                DiskName = c.DiskName,
                HostCaching = c.HostCaching,
                LogicalDiskSizeInGB = c.LogicalDiskSizeInGB,
                Lun = c.Lun,
                MediaLink = c.MediaLink,
                SourceMediaLink = c.SourceMediaLink
            }));
            
            VM.ConfigurationSets.ForEach(c =>
            {
                Microsoft.WindowsAzure.ServiceManagement.ConfigurationSet cs = null;
                if (c is Model.PersistentVMModel.NetworkConfigurationSet)
                {
                    var ncs = (Model.PersistentVMModel.NetworkConfigurationSet)c;
                    cs = new WindowsAzure.ServiceManagement.NetworkConfigurationSet
                    {
                        ConfigurationSetType = ncs.ConfigurationSetType,
                        VirtualIPGroups = ncs.VirtualIPGroups == null ? null : new WindowsAzure.ServiceManagement.VirtualIPGroups(from vg in ncs.VirtualIPGroups
                                                                                             select new WindowsAzure.ServiceManagement.VirtualIPGroup
                                                                                             {
                                                                                                 Name = vg.Name,
                                                                                                 VirtualIPs = vg.VirtualIPs == null ? null : new WindowsAzure.ServiceManagement.VirtualIPList(from vip in vg.VirtualIPs
                                                                                                                                                               select new WindowsAzure.ServiceManagement.VirtualIP
                                                                                                                                                               {
                                                                                                                                                                   Address = vip.Address,
                                                                                                                                                                   IsDnsProgrammed = vip.IsDnsProgrammed,
                                                                                                                                                                   Name = vip.Name
                                                                                                                                                               }),
                                                                                                 EndpointContracts = vg.EndpointContracts == null ? null : new WindowsAzure.ServiceManagement.EndpointContractList(from ec in vg.EndpointContracts
                                                                                                                                                                             select new WindowsAzure.ServiceManagement.EndpointContract
                                                                                                                                                                             {
                                                                                                                                                                                 Name = ec.Name,
                                                                                                                                                                                 Port = ec.Port,
                                                                                                                                                                                 Protocol = ec.Protocol
                                                                                                                                                                             })
                                                                                             }),
                        InputEndpoints = ncs.InputEndpoints == null ? null : new System.Collections.ObjectModel.Collection<WindowsAzure.ServiceManagement.InputEndpoint>((from ep in ncs.InputEndpoints
                                                                                                                                      select new WindowsAzure.ServiceManagement.InputEndpoint
                                                                                                                                      {
                                                                                                                                          EnableDirectServerReturn = ep.EnableDirectServerReturn,
                                                                                                                                          EndpointAccessControlList = ep.EndpointAccessControlList == null ? null : new EndpointAccessControlList
                                                                                                                                          {
                                                                                                                                              Rules = ep.EndpointAccessControlList.Rules == null ? null : new System.Collections.ObjectModel.Collection<WindowsAzure.ServiceManagement.AccessControlListRule>(
                                                                                                                                                     (from t in ep.EndpointAccessControlList.Rules
                                                                                                                                                      select new Microsoft.WindowsAzure.ServiceManagement.AccessControlListRule
                                                                                                                                                      {
                                                                                                                                                          Action = t.Action,
                                                                                                                                                          Description = t.Description,
                                                                                                                                                          Order = t.Order,
                                                                                                                                                          RemoteSubnet = t.RemoteSubnet
                                                                                                                                                      }).ToList())
                                                                                                                                          },
                                                                                                                                          LoadBalancedEndpointSetName = ep.LoadBalancedEndpointSetName,
                                                                                                                                          LoadBalancerProbe = ep.LoadBalancerProbe == null ? null : new WindowsAzure.ServiceManagement.LoadBalancerProbe
                                                                                                                                          {
                                                                                                                                              IntervalInSeconds = ep.LoadBalancerProbe.IntervalInSeconds,
                                                                                                                                              Path = ep.LoadBalancerProbe.Path,
                                                                                                                                              Port = ep.LoadBalancerProbe.Port,
                                                                                                                                              Protocol = ep.LoadBalancerProbe.Protocol,
                                                                                                                                              TimeoutInSeconds = ep.LoadBalancerProbe.TimeoutInSeconds
                                                                                                                                          },
                                                                                                                                          LocalPort = ep.LocalPort,
                                                                                                                                          Name = ep.Name,
                                                                                                                                          Port = ep.Port,
                                                                                                                                          Protocol = ep.Protocol,
                                                                                                                                          Vip = ep.Vip
                                                                                                                                      }).ToList()),
                        SubnetNames = new SubnetNamesCollection()
                    };
                    ncs.SubnetNames.ForEach(s => (cs as WindowsAzure.ServiceManagement.NetworkConfigurationSet).SubnetNames.Add(s));
                }
                else if (c is Model.PersistentVMModel.WindowsProvisioningConfigurationSet)
                {
                    var ncs = (Model.PersistentVMModel.WindowsProvisioningConfigurationSet)c;
                    cs = new WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet
                    {
                        AdminPassword = ncs.AdminPassword,
                        AdminUsername = ncs.AdminUsername,
                        ComputerName = ncs.ComputerName,
                        ConfigurationSetType = ncs.ConfigurationSetType,
                        DomainJoin = ncs.DomainJoin == null ? null : new WindowsProvisioningConfigurationSet.DomainJoinSettings
                        {
                            Credentials = ncs.DomainJoin.Credentials == null ? null : new WindowsProvisioningConfigurationSet.DomainJoinCredentials
                            {
                                Domain = ncs.DomainJoin.Credentials.Domain,
                                Password = ncs.DomainJoin.Credentials.Password,
                                Username = ncs.DomainJoin.Credentials.Username
                            }
                        },
                        EnableAutomaticUpdates = ncs.EnableAutomaticUpdates,
                        ResetPasswordOnFirstLogon = ncs.ResetPasswordOnFirstLogon,
                        StoredCertificateSettings = new CertificateSettingList(),
                        TimeZone = ncs.TimeZone,
                        WinRM = ncs.WinRM == null ? null : new WindowsProvisioningConfigurationSet.WinRmConfiguration
                        {
                            Listeners = new WindowsProvisioningConfigurationSet.WinRmListenerCollection()
                        }
                    };

                    if (ncs.StoredCertificateSettings != null)
                    {
                        ncs.StoredCertificateSettings.ForEach(cc => (cs as WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet).StoredCertificateSettings.Add(
                            new CertificateSetting
                            {
                                StoreLocation = cc.StoreLocation,
                                StoreName = cc.StoreName,
                                Thumbprint = cc.Thumbprint
                            }));
                    }

                    if (ncs.WinRM != null && ncs.WinRM.Listeners != null)
                    {
                        ncs.WinRM.Listeners.ForEach(n => (cs as WindowsAzure.ServiceManagement.WindowsProvisioningConfigurationSet).WinRM.Listeners.Add(
                            new WindowsProvisioningConfigurationSet.WinRmListenerProperties
                            {
                                CertificateThumbprint = n.CertificateThumbprint,
                                Protocol = n.Protocol
                            }));
                    }
                }
                else if (c is Model.PersistentVMModel.LinuxProvisioningConfigurationSet)
                {
                    var ncs = (Model.PersistentVMModel.LinuxProvisioningConfigurationSet)c;
                    cs = new WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet
                    {
                        ConfigurationSetType = ncs.ConfigurationSetType,
                        DisableSshPasswordAuthentication = ncs.DisableSshPasswordAuthentication,
                        HostName = ncs.HostName,
                        SSH = new LinuxProvisioningConfigurationSet.SSHSettings
                        {
                            KeyPairs = new LinuxProvisioningConfigurationSet.SSHKeyPairList(),
                            PublicKeys = new LinuxProvisioningConfigurationSet.SSHPublicKeyList()
                        },
                        UserName = ncs.UserName,
                        UserPassword = ncs.UserPassword
                    };
                    if (ncs.SSH != null)
                    {
                        if (ncs.SSH.KeyPairs != null)
                        {
                            ncs.SSH.KeyPairs.ForEach(p => (cs as WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet).SSH.KeyPairs.Add(new LinuxProvisioningConfigurationSet.SSHKeyPair
                            {
                                Fingerprint = p.Fingerprint,
                                Path = p.Path
                            }));
                        }

                        if (ncs.SSH.PublicKeys != null)
                        {
                            ncs.SSH.PublicKeys.ForEach(p => (cs as WindowsAzure.ServiceManagement.LinuxProvisioningConfigurationSet).SSH.PublicKeys.Add(new LinuxProvisioningConfigurationSet.SSHPublicKey
                            {
                                Fingerprint = p.Fingerprint,
                                Path = p.Path
                            }));
                        }
                    }
                }

                role.ConfigurationSets.Add(cs);
            });

            ExecuteClientActionInOCS(role, CommandRuntime.ToString(), s => this.Channel.UpdateRole(s, this.ServiceName, this.CurrentDeploymentNewSM.Name, this.Name, role));
        }
    }
}

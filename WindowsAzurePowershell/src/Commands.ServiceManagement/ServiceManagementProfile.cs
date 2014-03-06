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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using AutoMapper;
    using Extensions;
    using IaaS.Extensions;
    using Management.Compute.Models;
    using Management.Models;
    using Management.Storage.Models;
    using Model;
    using Utilities.Common;
    using NSM = Management.Compute.Models;
    using NVM = Management.VirtualNetworks.Models;
    using PVM = Model.PersistentVMModel;
    using WSM = WindowsAzure.ServiceManagement;

    public class ServiceManagementProfile : Profile
    {
        private static readonly Lazy<bool> initialize;

        static ServiceManagementProfile()
        {
            initialize = new Lazy<bool>(() =>
            {
                Mapper.AddProfile<ServiceManagementProfile>();
                return true;
            });
        }

        public override string ProfileName
        {
            get { return "ServiceManagementProfile"; }
        }

        public static bool Initialize()
        {
            return initialize.Value;
        }

        public static bool Initialize(GetAzureServiceAvailableExtensionCommand command)
        {
            Mapper.CreateMap<OperationStatusResponse, ExtensionImageContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<HostedServiceListAvailableExtensionsResponse.ExtensionImage, ExtensionImageContext>()
                  .ForMember(c => c.ExtensionName, o => o.MapFrom(r => r.Type));

            return Initialize();
        }

        public static bool Initialize(GetAzureVMAvailableExtensionCommand command)
        {
            Mapper.CreateMap<OperationStatusResponse, VirtualMachineExtensionImageContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<VirtualMachineExtensionListResponse.ResourceExtension, VirtualMachineExtensionImageContext>()
                  .ForMember(c => c.ExtensionName, o => o.MapFrom(r => r.Name));

            return Initialize();
        }

        protected override void Configure()
        {
            //SM to NewSM mapping
            Mapper.CreateMap<PVM.LoadBalancerProbe, NSM.LoadBalancerProbe>()
                  .ForMember(c => c.Protocol, o => o.MapFrom(r => r.Protocol));
            Mapper.CreateMap<PVM.AccessControlListRule, NSM.AccessControlListRule>();
            Mapper.CreateMap<PVM.EndpointAccessControlList, NSM.EndpointAcl>()
                  .ForMember(c => c.Rules, o => o.MapFrom(r => r.Rules.ToList()));
            Mapper.CreateMap<PVM.InputEndpoint, NSM.InputEndpoint>()
                  .ForMember(c => c.VirtualIPAddress, o => o.MapFrom(r => r.Vip != null ? IPAddress.Parse(r.Vip) : null))
                  .ForMember(c => c.EndpointAcl, o => o.MapFrom(r => r.EndpointAccessControlList));
            Mapper.CreateMap<PVM.DataVirtualHardDisk, NSM.DataVirtualHardDisk>()
                  .ForMember(c => c.LogicalUnitNumber, o => o.MapFrom(r => r.Lun));
            Mapper.CreateMap<PVM.OSVirtualHardDisk, NSM.OSVirtualHardDisk>()
                  .ForMember(c => c.OperatingSystem, o => o.MapFrom(r => r.OS));
            Mapper.CreateMap<PVM.NetworkConfigurationSet, NSM.ConfigurationSet>()
                  .ForMember(c => c.InputEndpoints, o => o.MapFrom(r => r.InputEndpoints != null ? r.InputEndpoints.ToList() : null))
                  .ForMember(c => c.SubnetNames, o => o.MapFrom(r => r.SubnetNames != null ? r.SubnetNames.ToList() : null));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.ProvisioningConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.ConfigurationSet, NSM.ConfigurationSet>();
            Mapper.CreateMap<PVM.InstanceEndpoint, NSM.InstanceEndpoint>()
                  .ForMember(c => c.VirtualIPAddress, o => o.MapFrom(r => r.Vip != null ? IPAddress.Parse(r.Vip) : null))
                  .ForMember(c => c.Port, o => o.MapFrom(r => r.PublicPort));
            Mapper.CreateMap<PVM.InstanceEndpointList, IList<NSM.InstanceEndpoint>>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<NSM.InstanceEndpoint>(t));
                          }
                      }
                  });
            Mapper.CreateMap<PVM.InstanceEndpointList, List<NSM.InstanceEndpoint>>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<NSM.InstanceEndpoint>(t));
                          }
                      }
                  });
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration, NSM.WindowsRemoteManagementSettings>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties, NSM.WindowsRemoteManagementListener>()
                  .ForMember(c => c.ListenerType, o => o.MapFrom(r => r.Protocol));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection, IList<NSM.WindowsRemoteManagementListener>>();

            //NewSM to SM mapping
            Mapper.CreateMap<NSM.LoadBalancerProbe, PVM.LoadBalancerProbe>()
                  .ForMember(c => c.Protocol, o => o.MapFrom(r => r.Protocol.ToString().ToLower()));
            Mapper.CreateMap<NSM.AccessControlListRule, PVM.AccessControlListRule>();
            Mapper.CreateMap<NSM.EndpointAcl, PVM.EndpointAccessControlList>()
                  .ForMember(c => c.Rules, o => o.MapFrom(r => r.Rules));
            Mapper.CreateMap<NSM.InputEndpoint, PVM.InputEndpoint>()
                  .ForMember(c => c.Vip, o => o.MapFrom(r => r.VirtualIPAddress != null ? r.VirtualIPAddress.ToString() : null))
                  .ForMember(c => c.EndpointAccessControlList, o => o.MapFrom(r => r.EndpointAcl));
            Mapper.CreateMap<NSM.DataVirtualHardDisk, PVM.DataVirtualHardDisk>()
                  .ForMember(c => c.Lun, o => o.MapFrom(r => r.LogicalUnitNumber));
            Mapper.CreateMap<NSM.OSVirtualHardDisk, PVM.OSVirtualHardDisk>()
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystem));
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.ConfigurationSet>();
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.NetworkConfigurationSet>();
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.WindowsProvisioningConfigurationSet>();
            Mapper.CreateMap<NSM.ConfigurationSet, PVM.LinuxProvisioningConfigurationSet>();
            Mapper.CreateMap<NSM.InstanceEndpoint, PVM.InstanceEndpoint>()
                  .ForMember(c => c.Vip, o => o.MapFrom(r => r.VirtualIPAddress != null ? r.VirtualIPAddress.ToString() : null))
                  .ForMember(c => c.PublicPort, o => o.MapFrom(r => r.Port));
            Mapper.CreateMap<IList<NSM.InstanceEndpoint>, PVM.InstanceEndpointList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<PVM.InstanceEndpoint>(t));
                          }
                      }
                  });
            Mapper.CreateMap<IEnumerable<NSM.InstanceEndpoint>, PVM.InstanceEndpointList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<PVM.InstanceEndpoint>(t));
                          }
                      }
                  });
            Mapper.CreateMap<List<NSM.InstanceEndpoint>, PVM.InstanceEndpointList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<PVM.InstanceEndpoint>(t));
                          }
                      }
                  });
            Mapper.CreateMap<NSM.WindowsRemoteManagementSettings, PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration>();
            Mapper.CreateMap<NSM.WindowsRemoteManagementListener, PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties>()
                  .ForMember(c => c.Protocol, o => o.MapFrom(r => r.ListenerType.ToString()));
            Mapper.CreateMap<IList<NSM.WindowsRemoteManagementListener>, PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection>();

            // LoadBalancedEndpointList mapping
            Mapper.CreateMap<PVM.AccessControlListRule, NSM.AccessControlListRule>();
            Mapper.CreateMap<PVM.EndpointAccessControlList, NSM.EndpointAcl>();
            Mapper.CreateMap<PVM.InputEndpoint, VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>()
                  .ForMember(c => c.Rules, o => o.MapFrom(r => r.EndpointAccessControlList == null ? null : r.EndpointAccessControlList.Rules))
                  .ForMember(c => c.VirtualIPAddress, o => o.MapFrom(r => r.Vip));
            Mapper.CreateMap<PVM.LoadBalancedEndpointList, IList<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>>();
            Mapper.CreateMap<PVM.LoadBalancedEndpointList, List<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>>();

            Mapper.CreateMap<NSM.AccessControlListRule, PVM.AccessControlListRule>();
            Mapper.CreateMap<NSM.EndpointAcl, PVM.EndpointAccessControlList>();
            Mapper.CreateMap<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint, PVM.InputEndpoint>()
                  .ForMember(c => c.EndpointAccessControlList, o => o.MapFrom(r => r.Rules == null ? null : r.Rules))
                  .ForMember(c => c.Vip, o => o.MapFrom(r => r.VirtualIPAddress));
            Mapper.CreateMap<IList<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>, PVM.LoadBalancedEndpointList>();
            Mapper.CreateMap<List<NSM.VirtualMachineUpdateLoadBalancedSetParameters.InputEndpoint>, PVM.LoadBalancedEndpointList>();

            //Common mapping
            Mapper.CreateMap<OperationResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.RequestId))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.StatusCode.ToString()));

            Mapper.CreateMap<OperationStatusResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //AffinityGroup mapping
            Mapper.CreateMap<AffinityGroupGetResponse, AffinityGroupContext>();
            Mapper.CreateMap<AffinityGroupListResponse.AffinityGroup, AffinityGroupContext>();
            Mapper.CreateMap<AffinityGroupGetResponse.HostedServiceReference, AffinityGroupContext.Service>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<AffinityGroupGetResponse.StorageServiceReference, AffinityGroupContext.Service>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<OperationStatusResponse, AffinityGroupContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Location mapping
            Mapper.CreateMap<LocationsListResponse.Location, LocationsContext>();
            Mapper.CreateMap<OperationStatusResponse, LocationsContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Role sizes mapping
            Mapper.CreateMap<RoleSizeListResponse.RoleSize, RoleSizeContext>()
                  .ForMember(c => c.InstanceSize, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.RoleSizeLabel, o => o.MapFrom(r => r.Label));
            Mapper.CreateMap<OperationStatusResponse, RoleSizeContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));
            
            //ServiceCertificate mapping
            Mapper.CreateMap<ServiceCertificateGetResponse, CertificateContext>()
                  .ForMember(c => c.Data, o => o.MapFrom(r => r.Data != null ? Convert.ToBase64String(r.Data) : null));
            Mapper.CreateMap<ServiceCertificateListResponse.Certificate, CertificateContext>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.CertificateUri))
                  .ForMember(c => c.Data, o => o.MapFrom(r => r.Data != null ? Convert.ToBase64String(r.Data) : null));
            Mapper.CreateMap<OperationStatusResponse, CertificateContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));
            Mapper.CreateMap<ComputeOperationStatusResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //OperatingSystems mapping
            Mapper.CreateMap<OperatingSystemListResponse.OperatingSystem, OSVersionsContext>();
            Mapper.CreateMap<OperationStatusResponse, OSVersionsContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Service mapping
            Mapper.CreateMap<HostedServiceProperties, HostedServiceDetailedContext>()
                  .ForMember(c => c.Description, o => o.MapFrom(r => string.IsNullOrEmpty(r.Description) ? null : r.Description))
                  .ForMember(c => c.DateModified, o => o.MapFrom(r => r.DateLastModified));
            Mapper.CreateMap<HostedServiceGetResponse, HostedServiceDetailedContext>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<HostedServiceListResponse.HostedService, HostedServiceDetailedContext>()
                  .ForMember(c => c.Url, o => o.MapFrom(r => r.Uri));
            Mapper.CreateMap<OperationStatusResponse, HostedServiceDetailedContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Disk mapping
            Mapper.CreateMap<VirtualMachineDiskListResponse.VirtualMachineDisk, DiskContext>()
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.DiskSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.DiskName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.AttachedTo, o => o.MapFrom(r => r.UsageDetails));
            Mapper.CreateMap<VirtualMachineDiskListResponse.VirtualMachineDiskUsageDetails, DiskContext.RoleReference>();

            Mapper.CreateMap<VirtualMachineDiskGetDiskResponse, DiskContext>()
                  .ForMember(c => c.AttachedTo, o => o.MapFrom(r => r.UsageDetails))
                  .ForMember(c => c.DiskName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.DiskSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB))
                  .ForMember(c => c.IsCorrupted, o => o.MapFrom(r => r.IsCorrupted))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType));
            Mapper.CreateMap<VirtualMachineDiskGetDiskResponse.VirtualMachineDiskUsageDetails, DiskContext.RoleReference>();

            Mapper.CreateMap<VirtualMachineDiskCreateDiskResponse, DiskContext>()
                  .ForMember(c => c.DiskName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystem))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.DiskSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB))
                  .ForMember(c => c.AttachedTo, o => o.MapFrom(r => r.UsageDetails));
            Mapper.CreateMap<VirtualMachineDiskCreateDiskResponse.VirtualMachineDiskUsageDetails, DiskContext.RoleReference>();

            Mapper.CreateMap<VirtualMachineDiskUpdateDiskResponse, DiskContext>()
                  .ForMember(c => c.DiskName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.DiskSizeInGB, o => o.MapFrom(r => r.LogicalSizeInGB));

            Mapper.CreateMap<OperationStatusResponse, DiskContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            //Image mapping
            Mapper.CreateMap<VirtualMachineImageListResponse.VirtualMachineImage, OSImageContext>()
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.PublishedDate, o => o.MapFrom(r => new DateTime?(r.PublishedDate)))
                  .ForMember(c => c.IconUri, o => o.MapFrom(r => r.SmallIconUri))
                  .ForMember(c => c.LogicalSizeInGB, o => o.MapFrom(r => (int)r.LogicalSizeInGB));

            Mapper.CreateMap<VirtualMachineImageGetResponse, OSImageContext>()
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.PublishedDate, o => o.MapFrom(r => new DateTime?(r.PublishedDate)))
                  .ForMember(c => c.LogicalSizeInGB, o => o.MapFrom(r => (int)r.LogicalSizeInGB));

            Mapper.CreateMap<VirtualMachineImageCreateResponse, OSImageContext>()
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.IconUri, o => o.MapFrom(r => r.SmallIconUri))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.PublishedDate, o => o.MapFrom(r => r.PublishedDate))
                  .ForMember(c => c.LogicalSizeInGB, o => o.MapFrom(r => (int)r.LogicalSizeInGB));
            
            Mapper.CreateMap<VirtualMachineImageUpdateResponse, OSImageContext>()
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.IconUri, o => o.MapFrom(r => r.SmallIconUri))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystemType))
                  .ForMember(c => c.PublishedDate, o => o.MapFrom(r => r.PublishedDate))
                  .ForMember(c => c.LogicalSizeInGB, o => o.MapFrom(r => (int)r.LogicalSizeInGB));

            Mapper.CreateMap<OperationStatusResponse, OSImageContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<VirtualMachineDiskCreateDiskResponse, OSImageContext>()
                  .ForMember(c => c.MediaLink, o => o.MapFrom(r => r.MediaLinkUri))
                  .ForMember(c => c.ImageName, o => o.MapFrom(r => r.Name))
                  .ForMember(c => c.OS, o => o.MapFrom(r => r.OperatingSystem));

            //Storage mapping
            Mapper.CreateMap<StorageServiceGetResponse, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.StorageAccountDescription, o => o.MapFrom(r => r.Properties == null ? null : r.Properties.Description))
                  .ForMember(c => c.StorageAccountName, o => o.MapFrom(r => r.ServiceName));
            Mapper.CreateMap<StorageServiceProperties, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.StorageAccountDescription, o => o.MapFrom(r => r.Description))
                  .ForMember(c => c.GeoPrimaryLocation, o => o.MapFrom(r => r.GeoPrimaryRegion))
                  .ForMember(c => c.GeoSecondaryLocation, o => o.MapFrom(r => r.GeoSecondaryRegion))
                  .ForMember(c => c.StorageAccountStatus, o => o.MapFrom(r => r.Status))
                  .ForMember(c => c.StatusOfPrimary, o => o.MapFrom(r => r.StatusOfGeoPrimaryRegion))
                  .ForMember(c => c.StatusOfSecondary, o => o.MapFrom(r => r.StatusOfGeoSecondaryRegion));
            Mapper.CreateMap<StorageServiceListResponse.StorageService, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.StorageAccountDescription, o => o.MapFrom(r => r.Properties == null ? null : r.Properties.Description))
                  .ForMember(c => c.StorageAccountName, o => o.MapFrom(r => r.ServiceName));
            Mapper.CreateMap<OperationStatusResponse, StorageServicePropertiesOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<StorageAccountGetKeysResponse, StorageServiceKeyOperationContext>()
                  .ForMember(c => c.Primary, o => o.MapFrom(r => r.PrimaryKey))
                  .ForMember(c => c.Secondary, o => o.MapFrom(r => r.SecondaryKey));
            Mapper.CreateMap<OperationStatusResponse, StorageServiceKeyOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            Mapper.CreateMap<StorageOperationStatusResponse, ManagementOperationContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            // DomainJoinSettings mapping for IaaS
            Mapper.CreateMap<NSM.DomainJoinCredentials, PVM.WindowsProvisioningConfigurationSet.DomainJoinCredentials>()
                  .ForMember(c => c.Domain, o => o.MapFrom(r => r.Domain))
                  .ForMember(c => c.Username, o => o.MapFrom(r => r.UserName))
                  .ForMember(c => c.Password, o => o.MapFrom(r => r.Password));
            Mapper.CreateMap<NSM.DomainJoinProvisioning, PVM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning>()
                  .ForMember(c => c.AccountData, o => o.MapFrom(r => r.AccountData));
            Mapper.CreateMap<NSM.DomainJoinSettings, PVM.WindowsProvisioningConfigurationSet.DomainJoinSettings>()
                  .ForMember(c => c.Credentials, o => o.MapFrom(r => r.Credentials))
                  .ForMember(c => c.JoinDomain, o => o.MapFrom(r => r.DomainToJoin))
                  .ForMember(c => c.MachineObjectOU, o => o.MapFrom(r => r.LdapMachineObjectOU))
                  .ForMember(c => c.Provisioning, o => o.MapFrom(r => r.Provisioning));

            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinCredentials, NSM.DomainJoinCredentials>()
                  .ForMember(c => c.Domain, o => o.MapFrom(r => r.Domain))
                  .ForMember(c => c.UserName, o => o.MapFrom(r => r.Username))
                  .ForMember(c => c.Password, o => o.MapFrom(r => r.Password));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning, NSM.DomainJoinProvisioning>()
                  .ForMember(c => c.AccountData, o => o.MapFrom(r => r.AccountData));
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinSettings, NSM.DomainJoinSettings>()
                  .ForMember(c => c.Credentials, o => o.MapFrom(r => r.Credentials))
                  .ForMember(c => c.DomainToJoin, o => o.MapFrom(r => r.JoinDomain))
                  .ForMember(c => c.LdapMachineObjectOU, o => o.MapFrom(r => r.MachineObjectOU))
                  .ForMember(c => c.Provisioning, o => o.MapFrom(r => r.Provisioning));

            // Networks mapping
            Mapper.CreateMap<IList<string>, PVM.AddressPrefixList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(t);
                          }
                      }
                  });
            Mapper.CreateMap<NVM.NetworkListResponse.AddressSpace, PVM.AddressSpace>();
            Mapper.CreateMap<NVM.NetworkListResponse.Connection, PVM.Connection>();
            Mapper.CreateMap<NVM.NetworkListResponse.LocalNetworkSite, PVM.LocalNetworkSite>();
            Mapper.CreateMap<IList<NVM.NetworkListResponse.LocalNetworkSite>, PVM.LocalNetworkSiteList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<PVM.LocalNetworkSite>(t));
                          }
                      }
                  });
            Mapper.CreateMap<NVM.NetworkListResponse.DnsServer, PVM.DnsServer>();
            Mapper.CreateMap<IList<NVM.NetworkListResponse.DnsServer>, PVM.DnsServerList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<PVM.DnsServer>(t));
                          }
                      }
                  });
            Mapper.CreateMap<NVM.NetworkListResponse.Subnet, PVM.Subnet>();
            Mapper.CreateMap<IList<NVM.NetworkListResponse.Subnet>, PVM.SubnetList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<PVM.Subnet>(t));
                          }
                      }
                  });
            Mapper.CreateMap<IList<NVM.NetworkListResponse.DnsServer>, PVM.DnsSettings>()
                  .ForMember(c => c.DnsServers, o => o.MapFrom(r => r));
            Mapper.CreateMap<IList<NVM.NetworkListResponse.Gateway>, PVM.Gateway>();
            Mapper.CreateMap<NVM.NetworkListResponse.VirtualNetworkSite, PVM.VirtualNetworkSite>();
            Mapper.CreateMap<IList<NVM.NetworkListResponse.VirtualNetworkSite>, PVM.VirtualNetworkSiteList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<PVM.VirtualNetworkSite>(t));
                          }
                      }
                  });
            Mapper.CreateMap<NVM.NetworkListResponse.VirtualNetworkSite, VirtualNetworkSiteContext>()
                  .ForMember(c => c.AddressSpacePrefixes, o => o.MapFrom(r => r.AddressSpace == null ? null : r.AddressSpace.AddressPrefixes == null ? null :
                                                                              r.AddressSpace.AddressPrefixes.Select(p => p)))
                  .ForMember(c => c.DnsServers, o => o.MapFrom(r => r.DnsServers.AsEnumerable()))
                  .ForMember(c => c.GatewayProfile, o => o.MapFrom(r => r.Gateway.Profile))
                  .ForMember(c => c.GatewaySites, o => o.MapFrom(r => r.Gateway.Sites));

            Mapper.CreateMap<OperationStatusResponse, VirtualNetworkSiteContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()))
                  .ForMember(c => c.Id, o => o.Ignore());

            // Check Static IP Availability Response Mapping
            Mapper.CreateMap<NVM.NetworkStaticIPAvailabilityResponse, VirtualNetworkStaticIPAvailabilityContext>();
            Mapper.CreateMap<OperationStatusResponse, VirtualNetworkStaticIPAvailabilityContext>()
                  .ForMember(c => c.OperationId, o => o.MapFrom(r => r.Id))
                  .ForMember(c => c.OperationStatus, o => o.MapFrom(r => r.Status.ToString()));

            // New SM to Model
            Mapper.CreateMap<NSM.StoredCertificateSettings, PVM.CertificateSetting>();
            Mapper.CreateMap<IList<NSM.StoredCertificateSettings>, PVM.CertificateSettingList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<PVM.CertificateSetting>(t));
                          }
                      }
                  });

            // Model to New SM
            Mapper.CreateMap<PVM.CertificateSetting, NSM.StoredCertificateSettings>();
            Mapper.CreateMap<PVM.CertificateSettingList, IList<NSM.StoredCertificateSettings>>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<NSM.StoredCertificateSettings>(t));
                          }
                      }
                  });

            // Resource Reference Mapping - NSM to PVM
            Mapper.CreateMap<NSM.ResourceExtensionParameterValue, PVM.ResourceExtensionParameterValue>();
            Mapper.CreateMap<IList<NSM.ResourceExtensionParameterValue>, PVM.ResourceExtensionParameterValueList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<PVM.ResourceExtensionParameterValue>(r)));
                      }
                  });
            Mapper.CreateMap<IEnumerable<NSM.ResourceExtensionParameterValue>, PVM.ResourceExtensionParameterValueList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<PVM.ResourceExtensionParameterValue>(r)));
                      }
                  });
            Mapper.CreateMap<List<NSM.ResourceExtensionParameterValue>, PVM.ResourceExtensionParameterValueList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<PVM.ResourceExtensionParameterValue>(r)));
                      }
                  });
            Mapper.CreateMap<NSM.ResourceExtensionReference, PVM.ResourceExtensionReference>();
            Mapper.CreateMap<IList<NSM.ResourceExtensionReference>, List<PVM.ResourceExtensionReference>>()
                  .Include<IList<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>();
            Mapper.CreateMap<IEnumerable<NSM.ResourceExtensionReference>, List<PVM.ResourceExtensionReference>>()
                  .Include<IEnumerable<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>();
            Mapper.CreateMap<List<NSM.ResourceExtensionReference>, List<PVM.ResourceExtensionReference>>()
                  .Include<List<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>();
            Mapper.CreateMap<IList<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<PVM.ResourceExtensionReference>(r)));
                      }
                  });
            Mapper.CreateMap<IEnumerable<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<PVM.ResourceExtensionReference>(r)));
                      }
                  });
            Mapper.CreateMap<List<NSM.ResourceExtensionReference>, PVM.ResourceExtensionReferenceList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<PVM.ResourceExtensionReference>(r)));
                      }
                  });
            // Resource Reference Mapping - PVM to NSM
            Mapper.CreateMap<PVM.ResourceExtensionParameterValue, NSM.ResourceExtensionParameterValue>();
            Mapper.CreateMap<PVM.ResourceExtensionParameterValueList, IList<NSM.ResourceExtensionParameterValue>>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<NSM.ResourceExtensionParameterValue>(r)));
                      }
                  });
            Mapper.CreateMap<PVM.ResourceExtensionParameterValueList, IEnumerable<NSM.ResourceExtensionParameterValue>>();
            Mapper.CreateMap<PVM.ResourceExtensionParameterValueList, List<NSM.ResourceExtensionParameterValue>>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<NSM.ResourceExtensionParameterValue>(r)));
                      }
                  });
            Mapper.CreateMap<PVM.ResourceExtensionReference, NSM.ResourceExtensionReference>();
            Mapper.CreateMap<PVM.ResourceExtensionReference, NSM.ResourceExtensionReference>();
            Mapper.CreateMap<List<PVM.ResourceExtensionReference>, IList<NSM.ResourceExtensionReference>>()
                  .Include<PVM.ResourceExtensionReferenceList, IList<NSM.ResourceExtensionReference>>();
            Mapper.CreateMap<List<PVM.ResourceExtensionReference>, IEnumerable<NSM.ResourceExtensionReference>>()
                  .Include<PVM.ResourceExtensionReferenceList, IEnumerable<NSM.ResourceExtensionReference>>();
            Mapper.CreateMap<List<PVM.ResourceExtensionReference>, List<NSM.ResourceExtensionReference>>()
                  .Include<PVM.ResourceExtensionReferenceList, List<NSM.ResourceExtensionReference>>();
            Mapper.CreateMap<PVM.ResourceExtensionReferenceList, IList<NSM.ResourceExtensionReference>>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<NSM.ResourceExtensionReference>(r)));
                      }
                  });
            Mapper.CreateMap<PVM.ResourceExtensionReferenceList, IEnumerable<NSM.ResourceExtensionReference>>();
            Mapper.CreateMap<PVM.ResourceExtensionReferenceList, List<NSM.ResourceExtensionReference>>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null)
                      {
                          c.ForEach(r => s.Add(Mapper.Map<NSM.ResourceExtensionReference>(r)));
                      }
                  });


            // WSM to PVM
            Mapper.CreateMap<WSM.LoadBalancerProbe,                                           PVM.LoadBalancerProbe>();
            Mapper.CreateMap<WSM.LoadBalancedEndpointList,                                    PVM.EndpointAccessControlList>();
            Mapper.CreateMap<WSM.InputEndpoint,                                               PVM.InputEndpoint>();
            Mapper.CreateMap<WSM.InstanceEndpoint,                                            PVM.InstanceEndpoint>();
            Mapper.CreateMap<WSM.InstanceEndpointList,                                        PVM.InstanceEndpointList>();
            Mapper.CreateMap<WSM.DataVirtualHardDisk,                                         PVM.DataVirtualHardDisk>();
            Mapper.CreateMap<WSM.OSVirtualHardDisk,                                           PVM.OSVirtualHardDisk>();
            Mapper.CreateMap<WSM.CertificateFile,                                             PVM.CertificateFile>();
            Mapper.CreateMap<WSM.CertificateSetting,                                          PVM.CertificateSetting>();
            Mapper.CreateMap<WSM.CertificateSettingList,                                      PVM.CertificateSettingList>();
            Mapper.CreateMap<WSM.WindowsProvisioningConfigurationSet.DomainJoinCredentials,   PVM.WindowsProvisioningConfigurationSet.DomainJoinCredentials>();
            Mapper.CreateMap<WSM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning,  PVM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning>();
            Mapper.CreateMap<WSM.WindowsProvisioningConfigurationSet.DomainJoinSettings,      PVM.WindowsProvisioningConfigurationSet.DomainJoinSettings>();
            Mapper.CreateMap<WSM.WindowsProvisioningConfigurationSet.WinRmProtocol,           PVM.WindowsProvisioningConfigurationSet.WinRmProtocol>();
            Mapper.CreateMap<WSM.WindowsProvisioningConfigurationSet.WinRmListenerProperties, PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties>();
            Mapper.CreateMap<WSM.WindowsProvisioningConfigurationSet.WinRmListenerCollection, PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection>();
            Mapper.CreateMap<WSM.WindowsProvisioningConfigurationSet.WinRmConfiguration,      PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration>();
            Mapper.CreateMap<WSM.LinuxProvisioningConfigurationSet.SSHKeyPair,                PVM.LinuxProvisioningConfigurationSet.SSHKeyPair>();
            Mapper.CreateMap<WSM.LinuxProvisioningConfigurationSet.SSHKeyPairList,            PVM.LinuxProvisioningConfigurationSet.SSHKeyPairList>();
            Mapper.CreateMap<WSM.LinuxProvisioningConfigurationSet.SSHPublicKey,              PVM.LinuxProvisioningConfigurationSet.SSHPublicKey>();
            Mapper.CreateMap<WSM.LinuxProvisioningConfigurationSet.SSHPublicKeyList,          PVM.LinuxProvisioningConfigurationSet.SSHPublicKeyList>();
            Mapper.CreateMap<WSM.LinuxProvisioningConfigurationSet.SSHSettings,               PVM.LinuxProvisioningConfigurationSet.SSHSettings>();
            Mapper.CreateMap<WSM.NetworkConfigurationSet,                                     PVM.NetworkConfigurationSet>();
            Mapper.CreateMap<WSM.WindowsProvisioningConfigurationSet,                         PVM.WindowsProvisioningConfigurationSet>();
            Mapper.CreateMap<WSM.LinuxProvisioningConfigurationSet,                           PVM.LinuxProvisioningConfigurationSet>();
            Mapper.CreateMap<WSM.ProvisioningConfigurationSet,                                PVM.ProvisioningConfigurationSet>();
            Mapper.CreateMap<WSM.ConfigurationSet,                                            PVM.ConfigurationSet>();

            // PVM to WSM
            Mapper.CreateMap<PVM.LoadBalancerProbe,                                           WSM.LoadBalancerProbe>();
            Mapper.CreateMap<PVM.LoadBalancedEndpointList,                                    WSM.EndpointAccessControlList>();
            Mapper.CreateMap<PVM.InputEndpoint,                                               WSM.InputEndpoint>();
            Mapper.CreateMap<PVM.InstanceEndpoint,                                            WSM.InstanceEndpoint>();
            Mapper.CreateMap<PVM.InstanceEndpointList,                                        WSM.InstanceEndpointList>();
            Mapper.CreateMap<PVM.DataVirtualHardDisk,                                         WSM.DataVirtualHardDisk>();
            Mapper.CreateMap<PVM.OSVirtualHardDisk,                                           WSM.OSVirtualHardDisk>();
            Mapper.CreateMap<PVM.CertificateFile,                                             WSM.CertificateFile>();
            Mapper.CreateMap<PVM.CertificateSetting,                                          WSM.CertificateSetting>();
            Mapper.CreateMap<PVM.CertificateSettingList,                                      WSM.CertificateSettingList>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinCredentials,   WSM.WindowsProvisioningConfigurationSet.DomainJoinCredentials>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning,  WSM.WindowsProvisioningConfigurationSet.DomainJoinProvisioning>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.DomainJoinSettings,      WSM.WindowsProvisioningConfigurationSet.DomainJoinSettings>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmProtocol,           WSM.WindowsProvisioningConfigurationSet.WinRmProtocol>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerProperties, WSM.WindowsProvisioningConfigurationSet.WinRmListenerProperties>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmListenerCollection, WSM.WindowsProvisioningConfigurationSet.WinRmListenerCollection>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet.WinRmConfiguration,      WSM.WindowsProvisioningConfigurationSet.WinRmConfiguration>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHKeyPair,                WSM.LinuxProvisioningConfigurationSet.SSHKeyPair>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHKeyPairList,            WSM.LinuxProvisioningConfigurationSet.SSHKeyPairList>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHPublicKey,              WSM.LinuxProvisioningConfigurationSet.SSHPublicKey>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHPublicKeyList,          WSM.LinuxProvisioningConfigurationSet.SSHPublicKeyList>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet.SSHSettings,               WSM.LinuxProvisioningConfigurationSet.SSHSettings>();
            Mapper.CreateMap<PVM.NetworkConfigurationSet,                                     WSM.NetworkConfigurationSet>();
            Mapper.CreateMap<PVM.WindowsProvisioningConfigurationSet,                         WSM.WindowsProvisioningConfigurationSet>();
            Mapper.CreateMap<PVM.LinuxProvisioningConfigurationSet,                           WSM.LinuxProvisioningConfigurationSet>();
            Mapper.CreateMap<PVM.ProvisioningConfigurationSet,                                WSM.ProvisioningConfigurationSet>();
            Mapper.CreateMap<PVM.ConfigurationSet,                                            WSM.ConfigurationSet>();

            // WSM to NSM
            Mapper.CreateMap<WSM.RoleInstanceList, IList<NSM.RoleInstance>>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<NSM.RoleInstance>(t));
                          }
                      }
                  });
            Mapper.CreateMap<WSM.RoleInstanceList, List<NSM.RoleInstance>>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<NSM.RoleInstance>(t));
                          }
                      }
                  });

            // NSM to WSM
            Mapper.CreateMap<IList<NSM.RoleInstance>, WSM.RoleInstanceList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<WSM.RoleInstance>(t));
                          }
                      }
                  });
            Mapper.CreateMap<List<NSM.RoleInstance>, WSM.RoleInstanceList>()
                  .AfterMap((c, s) =>
                  {
                      if (c != null && s != null)
                      {
                          foreach (var t in c)
                          {
                              s.Add(Mapper.Map<WSM.RoleInstance>(t));
                          }
                      }
                  });
        }
    }
}
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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using ConfigDataInfo;
    using IaasCmdletInfo;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.ServiceManagement;
    using Model;

    public class ServiceManagementCmdletTestHelper 
    {

        public Collection <PSObject> RunPSScript(string script)
        {
            List<string> st = new List<string>();
            st.Add(script);

            WindowsAzurePowershellScript azurePowershellCmdlet = new WindowsAzurePowershellScript(st);
            return azurePowershellCmdlet.Run();
        }

        public bool TestAzureServiceName(string serviceName)
        {
            TestAzureNameCmdletInfo testAzureNameCmdlet = new TestAzureNameCmdletInfo("Service", serviceName);
            WindowsAzurePowershellCmdlet testAzureName = new WindowsAzurePowershellCmdlet(testAzureNameCmdlet);
            Collection<bool> response = new Collection<bool>();
            foreach (PSObject result in testAzureName.Run())
            {
                response.Add((bool)result.BaseObject);
            }
            return response[0];
        }

        public Collection<LocationsContext> GetAzureLocation()
        {
            GetAzureLocationCmdletInfo getAzureLocationCmdlet = new GetAzureLocationCmdletInfo();
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureLocationCmdlet);

            Collection<LocationsContext> locationsContext = new Collection<LocationsContext>();
            foreach (PSObject result in azurePowershellCmdlet.Run())
            {
                locationsContext.Add((LocationsContext)result.BaseObject);
            }
            return locationsContext;
        }

        public string GetAzureLocationName(string[] keywords, bool exactMatch = true)
        {
            Collection<LocationsContext> locations = GetAzureLocation();
            if (keywords != null)
            {
                foreach (LocationsContext location in locations)
                {
                    if (Utilities.MatchKeywords(location.Name, keywords, exactMatch) >= 0)
                    {
                        return location.Name;
                    }
                }
            }
            else
            {
                if (locations.Count == 1)
                {
                    return locations[0].Name;
                }
            }
            return null;
        }

        public Collection<OSVersionsContext> GetAzureOSVersion()
        {
            GetAzureOSVersionCmdletInfo getAzureOSVersionCmdletInfo = new GetAzureOSVersionCmdletInfo();
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureOSVersionCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            Collection<OSVersionsContext> osVersions = new Collection<OSVersionsContext>();
            foreach (PSObject re in result)
            {
                osVersions.Add((OSVersionsContext)re.BaseObject);
            }
            return osVersions;
        }

        #region CertificateSetting, VMConifig, ProvisioningConfig

        public CertificateSetting NewAzureCertificateSetting(string thumbprint, string store)
        {
            NewAzureCertificateSettingCmdletInfo newAzureCertificateSettingCmdletInfo = new NewAzureCertificateSettingCmdletInfo(thumbprint, store);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(newAzureCertificateSettingCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();

            if (result.Count == 1)
            {
                return (CertificateSetting)result[0].BaseObject;

            }
            return null;
        }


        public PersistentVM NewAzureVMConfig(AzureVMConfigInfo vmConfig)
        {
            NewAzureVMConfigCmdletInfo newAzureVMConfigCmdletInfo = new NewAzureVMConfigCmdletInfo(vmConfig);
            WindowsAzurePowershellCmdlet newAzureServiceCmdlet = new WindowsAzurePowershellCmdlet(newAzureVMConfigCmdletInfo);

            Collection<PSObject> result = newAzureServiceCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;

        }

        public PersistentVM AddAzureProvisioningConfig(AzureProvisioningConfigInfo provConfig)
        {
            AddAzureProvisioningConfigCmdletInfo addAzureProvisioningConfigCmdletInfog = new AddAzureProvisioningConfigCmdletInfo(provConfig);
            WindowsAzurePowershellCmdlet addAzureProvisioningConfigCmdlet = new WindowsAzurePowershellCmdlet(addAzureProvisioningConfigCmdletInfog);

            Collection<PSObject> result = addAzureProvisioningConfigCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }


        #endregion


        #region AzureAffinityGroup

        public ManagementOperationContext NewAzureAffinityGroup(string name, string location, string label, string description)
        {
            NewAzureAffinityGroupCmdletInfo newAzureAffinityGroupCmdletInfo = new NewAzureAffinityGroupCmdletInfo(name, location, label, description);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(newAzureAffinityGroupCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        public Collection<AffinityGroupContext> GetAzureAffinityGroup(string name)
        {
            GetAzureAffinityGroupCmdletInfo getAzureAffinityGroupCmdletInfo = new GetAzureAffinityGroupCmdletInfo(name);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureAffinityGroupCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            Collection<AffinityGroupContext> certCtxts = new Collection<AffinityGroupContext>();
            foreach (PSObject re in result)
            {
                certCtxts.Add((AffinityGroupContext)re.BaseObject);
            }
            return certCtxts;
        }

        public ManagementOperationContext SetAzureAffinityGroup(string name, string label, string description)
        {
            SetAzureAffinityGroupCmdletInfo setAzureAffinityGroupCmdletInfo = new SetAzureAffinityGroupCmdletInfo(name, label, description);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureAffinityGroupCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }



        public ManagementOperationContext RemoveAzureAffinityGroup(string name)
        {
            RemoveAzureAffinityGroupCmdletInfo removeAzureAffinityGroupCmdletInfo = new RemoveAzureAffinityGroupCmdletInfo(name);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureAffinityGroupCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }



        #endregion


        #region AzureCertificate

        public ManagementOperationContext AddAzureCertificate(string serviceName, PSObject cert, string password)
        {
            AddAzureCertificateCmdletInfo addAzureCertificateCmdletInfo = new AddAzureCertificateCmdletInfo(serviceName, cert, password);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(addAzureCertificateCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        public ManagementOperationContext AddAzureCertificate(string serviceName, PSObject cert)
        {
            return AddAzureCertificate(serviceName, cert, null);

        }


        public Collection <CertificateContext> GetAzureCertificate(string serviceName, string thumbprint, string algorithm)
        {
            GetAzureCertificateCmdletInfo getAzureCertificateCmdletInfo = new GetAzureCertificateCmdletInfo(serviceName, thumbprint, algorithm);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureCertificateCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            Collection<CertificateContext> certCtxts = new Collection<CertificateContext>();
            foreach (PSObject re in result)
            {
                certCtxts.Add((CertificateContext)re.BaseObject);
            }            
            return certCtxts;
        }

        public Collection<CertificateContext> GetAzureCertificate(string serviceName)
        {
            return GetAzureCertificate(serviceName, null, null);
        }

        public ManagementOperationContext RemoveAzureCertificate(string serviceName, string thumbprint, string algorithm)
        {
            RemoveAzureCertificateCmdletInfo removeAzureCertificateCmdletInfo = new RemoveAzureCertificateCmdletInfo(serviceName, thumbprint, algorithm);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureCertificateCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        #endregion


        #region AzureDataDisk

        public PersistentVM AddAzureDataDisk(AddAzureDataDiskConfig diskConfig)
        {
            AddAzureDataDiskCmdletInfo addAzureDataDiskCmdletInfo = new AddAzureDataDiskCmdletInfo(diskConfig);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(addAzureDataDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }

        public void AddDataDisk(string vmName, string serviceName, AddAzureDataDiskConfig [] diskConfigs)
        {

            PersistentVM vm = GetAzureVM(vmName, serviceName).VM;

            foreach (AddAzureDataDiskConfig config in diskConfigs)
            {
                config.Vm = vm;
                vm = AddAzureDataDisk(config);
            }
            UpdateAzureVM(vmName, serviceName, vm);
        }

        public PersistentVM SetAzureDataDisk(SetAzureDataDiskConfig discCfg)
        {
            SetAzureDataDiskCmdletInfo setAzureDataDiskCmdletInfo = new SetAzureDataDiskCmdletInfo(discCfg);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureDataDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }

        public void SetDataDisk(string vmName, string serviceName, HostCaching hc, int lun)
        {
            
            SetAzureDataDiskConfig config = new SetAzureDataDiskConfig(hc, lun);
            config.Vm = GetAzureVM(vmName, serviceName).VM;
            UpdateAzureVM(vmName, serviceName, SetAzureDataDisk(config));
        }

        public Collection<DataVirtualHardDisk> GetAzureDataDisk(string vmName, string serviceName)
        {
            PersistentVMRoleContext vmRolectx = GetAzureVM(vmName, serviceName);

            GetAzureDataDiskCmdletInfo getAzureDataDiskCmdlet = new GetAzureDataDiskCmdletInfo(vmRolectx.VM);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureDataDiskCmdlet);

            Collection<DataVirtualHardDisk> hardDisks = new Collection<DataVirtualHardDisk>();
            foreach (PSObject disk in azurePowershellCmdlet.Run())
            {
                hardDisks.Add((DataVirtualHardDisk)disk.BaseObject);
            }
            return hardDisks;
        }

        private PersistentVM RemoveAzureDataDisk(RemoveAzureDataDiskConfig discCfg)
        {
            RemoveAzureDataDiskCmdletInfo removeAzureDataDiskCmdletInfo = new RemoveAzureDataDiskCmdletInfo(discCfg);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureDataDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }

        public void RemoveDataDisk(string vmName, string serviceName, int [] lunSlots)
        {

            PersistentVM vm = GetAzureVM(vmName, serviceName).VM;

            foreach (int lun in lunSlots)
            {
                RemoveAzureDataDiskConfig config = new RemoveAzureDataDiskConfig(lun, vm);                
                RemoveAzureDataDisk(config);
            }
            UpdateAzureVM(vmName, serviceName, vm);
        }

        #endregion


        #region AzureDeployment

        public ManagementOperationContext NewAzureDeployment(string serviceName, string packagePath, string configPath, string slot, string label, string name, bool doNotStart, bool warning)
        {
            NewAzureDeploymentCmdletInfo newAzureDeploymentCmdletInfo = new NewAzureDeploymentCmdletInfo(serviceName, packagePath, configPath, slot, label, name, doNotStart, warning);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(newAzureDeploymentCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;


        }

        public DeploymentInfoContext GetAzureDeployment(string serviceName, string slot)
        {
            GetAzureDeploymentCmdletInfo getAzureDeploymentCmdletInfo = new GetAzureDeploymentCmdletInfo(serviceName, slot);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureDeploymentCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (DeploymentInfoContext)result[0].BaseObject;
            }
            return null;


        }

        private ManagementOperationContext SetAzureDeployment(SetAzureDeploymentCmdletInfo cmdletInfo)
        {
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(cmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        public ManagementOperationContext SetAzureDeploymentStatus(string serviceName, string slot, string newStatus)
        {
            return SetAzureDeployment(SetAzureDeploymentCmdletInfo.SetAzureDeploymentStatusCmdletInfo(serviceName, slot, newStatus));
        }

        public ManagementOperationContext SetAzureDeploymentConfig(string serviceName, string slot, string configPath)
        {
            return SetAzureDeployment(SetAzureDeploymentCmdletInfo.SetAzureDeploymentConfigCmdletInfo(serviceName, slot, configPath));
        }

        public ManagementOperationContext SetAzureDeploymentUpgrade(string serviceName, string slot, string mode, string packagePath, string configPath)
        {
            return SetAzureDeployment(SetAzureDeploymentCmdletInfo.SetAzureDeploymentUpgradeCmdletInfo(serviceName, slot, mode, packagePath, configPath));
        }

        public ManagementOperationContext SetAzureDeployment(string option, string serviceName, string packagePath, string newStatus, string configName, string slot, string mode, string label, string roleName, bool force)
        {
            return SetAzureDeployment(new SetAzureDeploymentCmdletInfo(option, serviceName, packagePath, newStatus, configName, slot, mode, label, roleName, force));            
        }

        public ManagementOperationContext RemoveAzureDeployment(string serviceName, string slot, bool force)
        {
            RemoveAzureDeploymentCmdletInfo removeAzureDeploymentCmdletInfo = new RemoveAzureDeploymentCmdletInfo(serviceName, slot, force);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureDeploymentCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;

        }

        public ManagementOperationContext MoveAzureDeployment(string serviceName)
        {
            MoveAzureDeploymentCmdletInfo moveAzureDeploymentCmdletInfo = new MoveAzureDeploymentCmdletInfo(serviceName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(moveAzureDeploymentCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;




        }




        
        #endregion


        #region AzureDisk

        // Add-AzureDisk
        public DiskContext AddAzureDisk(string diskName, string mediaPath, string label, string os)
        {
            AddAzureDiskCmdletInfo addAzureDiskCmdletInfo = new AddAzureDiskCmdletInfo(diskName, mediaPath, label, os);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(addAzureDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (DiskContext)result[0].BaseObject;
            }
            return null;
        }


        // Get-AzureDisk
        public Collection<DiskContext> GetAzureDisk(string diskName)
        {
            return GetAzureDisk(new GetAzureDiskCmdletInfo(diskName));
        }

        public Collection<DiskContext> GetAzureDisk()
        {
            return GetAzureDisk(new GetAzureDiskCmdletInfo((string)null));
        }

        private Collection<DiskContext> GetAzureDisk(GetAzureDiskCmdletInfo getAzureDiskCmdletInfo)
        {
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            Collection<DiskContext> disks = new Collection<DiskContext>();
            foreach (PSObject re in result)
            {
                disks.Add((DiskContext)re.BaseObject);
            }
            return disks;
        }

        public Collection<DiskContext> GetAzureDiskAttachedToRoleName(string[] roleName, bool exactMatch = true)
        {
            Collection<DiskContext> retDisks = new Collection<DiskContext>();
            Collection<DiskContext> disks = GetAzureDisk();
            foreach (DiskContext disk in disks)
            {
                if (disk.AttachedTo != null && disk.AttachedTo.RoleName != null)
                {
                    if (Utilities.MatchKeywords(disk.AttachedTo.RoleName, roleName, exactMatch) >= 0)
                        retDisks.Add(disk);
                }
            }
            return retDisks;
        }


        // Remove-AzureDisk
        public ManagementOperationContext RemoveAzureDisk(string diskName, bool deleteVhd)
        {
            RemoveAzureDiskCmdletInfo removeAzureDiskCmdletInfo = new RemoveAzureDiskCmdletInfo(diskName, deleteVhd);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        // Update-AzureDisk
        public DiskContext UpdateAzureDisk(string diskName, string label)
        {
            UpdateAzureDiskCmdletInfo updateAzureDiskCmdletInfo = new UpdateAzureDiskCmdletInfo(diskName, label);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(updateAzureDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (DiskContext)result[0].BaseObject;
            }
            return null;
        }



        #endregion


        #region AzureDns


        public DnsServer NewAzureDns(string name, string ipAddress)
        {
            NewAzureDnsCmdletInfo newAzureDnsCmdletInfo = new NewAzureDnsCmdletInfo(name, ipAddress);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(newAzureDnsCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (DnsServer)result[0].BaseObject;
            }
            return null;
        }

        
        public DnsServerList GetAzureDns(DnsSettings settings)
        {
            GetAzureDnsCmdletInfo getAzureDnsCmdletInfo = new GetAzureDnsCmdletInfo(settings);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureDnsCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            DnsServerList dnsList = new DnsServerList();

            foreach (PSObject re in result)
            {
                dnsList.Add((DnsServer)re.BaseObject);
            }
            return dnsList;
        }

        #endregion



        #region AzureEndpoint

        public PersistentVM AddAzureEndPoint(AzureEndPointConfigInfo endPointConfig)
        {
            AddAzureEndpointCmdletInfo addAzureEndPointCmdletInfo = new AddAzureEndpointCmdletInfo(endPointConfig);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(addAzureEndPointCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }

        public PersistentVM AddAzureEndPointNoLB(AzureEndPointConfigInfo endPointConfig)
        {
            AddAzureEndpointCmdletInfo addAzureEndPointCmdletInfo = AddAzureEndpointCmdletInfo.BuildNoLoadBalancedCmdletInfo(endPointConfig);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(addAzureEndPointCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }

        public void AddEndPoint(string vmName, string serviceName, AzureEndPointConfigInfo [] endPointConfigs)
        {
            
            PersistentVM vm = GetAzureVM(vmName, serviceName).VM;

            foreach (AzureEndPointConfigInfo config in endPointConfigs)
            {
                config.Vm = vm;
                vm = AddAzureEndPointNoLB(config);
            }
            UpdateAzureVM(vmName, serviceName, vm);
        }

        

        public Collection <InputEndpointContext> GetAzureEndPoint(PersistentVMRoleContext vmRoleCtxt)
        {
            GetAzureEndpointCmdletInfo getAzureEndpointCmdletInfo = new GetAzureEndpointCmdletInfo(vmRoleCtxt);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureEndpointCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            Collection<InputEndpointContext> epCtxts = new Collection<InputEndpointContext>();

            foreach(PSObject re in result)
            {            
                epCtxts.Add((InputEndpointContext)re.BaseObject);                
            }
            return epCtxts;
        }

        public PersistentVM SetAzureEndPoint(AzureEndPointConfigInfo endPointConfig)
        {
            if (null != endPointConfig)
            {
                SetAzureEndpointCmdletInfo setAzureEndpointCmdletInfo = SetAzureEndpointCmdletInfo.BuildNoLoadBalancedCmdletInfo(endPointConfig);
                WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureEndpointCmdletInfo);

                Collection<PSObject> result = azurePowershellCmdlet.Run();
                if (result.Count == 1)
                {
                    return (PersistentVM)result[0].BaseObject;
                }
            }
            return null;
        }

        public void SetEndPoint(string vmName, string serviceName, AzureEndPointConfigInfo endPointConfig)
        {

            endPointConfig.Vm = GetAzureVM(vmName, serviceName).VM;

            UpdateAzureVM(vmName, serviceName, SetAzureEndPoint(endPointConfig));
        }

        public PersistentVMRoleContext RemoveAzureEndPoint(string epName, PersistentVMRoleContext vmRoleCtxt)
        {
            RemoveAzureEndpointCmdletInfo removeAzureEndPointCmdletInfo = new RemoveAzureEndpointCmdletInfo(epName, vmRoleCtxt);            
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureEndPointCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVMRoleContext)result[0].BaseObject;
            }
            return null;
        }

        public void RemoveEndPoint(string vmName, string serviceName, string [] epNames)
        {

            PersistentVMRoleContext vmRoleCtxt = GetAzureVM(vmName, serviceName);

            foreach (string ep in epNames)
            {                
                vmRoleCtxt.VM = RemoveAzureEndPoint(ep, vmRoleCtxt).VM;
            }
            UpdateAzureVM(vmName, serviceName, vmRoleCtxt.VM);
        }
        #endregion


        #region AzureOSDisk

        public PersistentVM SetAzureOSDisk(HostCaching hc, PersistentVM vm)
        {
            SetAzureOSDiskCmdletInfo setAzureOSDiskCmdletInfo = new SetAzureOSDiskCmdletInfo(hc, vm);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureOSDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }


        public OSVirtualHardDisk GetAzureOSDisk(PersistentVM vm)
        {
            GetAzureOSDiskCmdletInfo getAzureOSDiskCmdletInfo = new GetAzureOSDiskCmdletInfo(vm);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureOSDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (OSVirtualHardDisk)result[0].BaseObject;
            }
            return null;
        }

        


        #endregion


        #region AzureRole

        public ManagementOperationContext SetAzureRole(string serviceName, string slot, string roleName, int count)
        {
            SetAzureRoleCmdletInfo setAzureRoleCmdletInfo = new SetAzureRoleCmdletInfo(serviceName, slot, roleName, count);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureRoleCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }


        public Collection<RoleContext> GetAzureRole(string serviceName, string slot, string roleName, bool details)
        {
            GetAzureRoleCmdletInfo getAzureRoleCmdletInfo = new GetAzureRoleCmdletInfo(serviceName, slot, roleName, details);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureRoleCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            Collection<RoleContext> roles = new Collection<RoleContext>();
            foreach (PSObject re in result)
            {
                roles.Add((RoleContext)re.BaseObject);                
            }
            return roles;
        }


        #endregion


        #region AzureQuickVM

        public PersistentVMRoleContext NewAzureQuickVM(OS os, string name, string serviceName, string imageName, string password, string locationName)
        {
            NewAzureQuickVMCmdletInfo newAzureQuickVMCmdlet = new NewAzureQuickVMCmdletInfo(os, name, serviceName, imageName, password, locationName);
            WindowsAzurePowershellCmdletSequence sequence = new WindowsAzurePowershellCmdletSequence();

            SubscriptionData currentSubscription;
            if ((currentSubscription = GetCurrentAzureSubscription()) == null)
            {
                ImportAzurePublishSettingsFile();
                currentSubscription = GetCurrentAzureSubscription();
            }
            if (string.IsNullOrEmpty(currentSubscription.CurrentStorageAccount))
            {
                StorageServicePropertiesOperationContext storageAccount = NewAzureStorageAccount(Utilities.GetUniqueShortName("storage"), locationName);
                if (storageAccount != null)
                {
                    SetAzureSubscription(currentSubscription.SubscriptionName, storageAccount.StorageAccountName);
                    currentSubscription = GetCurrentAzureSubscription();
                }
            }
            if (!string.IsNullOrEmpty(currentSubscription.CurrentStorageAccount))
            {
                sequence.Add(newAzureQuickVMCmdlet);
                sequence.Run();
                return GetAzureVM(name, serviceName);
            }
            return null;
        }

        public PersistentVMRoleContext NewAzureQuickLinuxVM(OS os, string name, string serviceName, string imageName, string userName, string password, string locationName)
        {
            NewAzureQuickVMCmdletInfo newAzureQuickVMLinuxCmdlet = new NewAzureQuickVMCmdletInfo(os, name, serviceName, imageName, userName, password, locationName);
            WindowsAzurePowershellCmdletSequence sequence = new WindowsAzurePowershellCmdletSequence();

            SubscriptionData currentSubscription;
            if ((currentSubscription = GetCurrentAzureSubscription()) == null)
            {
                currentSubscription = GetCurrentAzureSubscription();
            }
            if (string.IsNullOrEmpty(currentSubscription.CurrentStorageAccount))
            {
                StorageServicePropertiesOperationContext storageAccount = NewAzureStorageAccount(Utilities.GetUniqueShortName("storage"), locationName);
                if (storageAccount != null)
                {
                    SetAzureSubscription(currentSubscription.SubscriptionName, storageAccount.StorageAccountName);
                    currentSubscription = GetCurrentAzureSubscription();
                }
            }

            if (!string.IsNullOrEmpty(currentSubscription.CurrentStorageAccount))
            {
                sequence.Add(newAzureQuickVMLinuxCmdlet);
                sequence.Run();
                return GetAzureVM(name, serviceName);
            }
            return null;
        }



        #endregion 

        
        #region AzurePublishSettingsFile

        public void ImportAzurePublishSettingsFile()
        {
            this.ImportAzurePublishSettingsFile(Utilities.publishSettingsFile);
        }

        internal void ImportAzurePublishSettingsFile(string publishSettingsFile)
        {
            ImportAzurePublishSettingsFileCmdletInfo importAzurePublishSettingsFileCmdlet = new ImportAzurePublishSettingsFileCmdletInfo(publishSettingsFile);

            WindowsAzurePowershellCmdlet importAzurePublishSettingsFile = new WindowsAzurePowershellCmdlet(importAzurePublishSettingsFileCmdlet);
            importAzurePublishSettingsFile.Run();
        }


        #endregion


        #region AzureSubscription

        public Collection<SubscriptionData> GetAzureSubscription()
        {
            GetAzureSubscriptionCmdletInfo getAzureSubscriptionCmdlet = new GetAzureSubscriptionCmdletInfo();
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureSubscriptionCmdlet);
            Collection<SubscriptionData> subscriptions = new Collection<SubscriptionData>();
            foreach (PSObject result in azurePowershellCmdlet.Run())
            {
                subscriptions.Add((SubscriptionData)result.BaseObject);
            }

            return subscriptions;
        }


        public SubscriptionData GetCurrentAzureSubscription()
        {
            Collection<SubscriptionData> subscriptions = GetAzureSubscription();
            foreach (SubscriptionData subscription in subscriptions)
            {
                if (subscription.IsDefault)
                {
                    return subscription;
                }
            }
            return null;
        }

        public SubscriptionData SetAzureSubscription(string subscriptionName, string currentStorageAccount)
        {
            SetAzureSubscriptionCmdletInfo setAzureSubscriptionCmdlet = new SetAzureSubscriptionCmdletInfo(subscriptionName, currentStorageAccount);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureSubscriptionCmdlet);
            azurePowershellCmdlet.Run();

            Collection<SubscriptionData> subscriptions = GetAzureSubscription();
            foreach (SubscriptionData subscription in subscriptions)
            {
                if (subscription.SubscriptionName == subscriptionName)
                {
                    return subscription;
                }
            }
            return null;
        }

        public SubscriptionData SetDefaultAzureSubscription(string subscriptionName)
        {
            SetAzureSubscriptionCmdletInfo setAzureSubscriptionCmdlet = new SetAzureSubscriptionCmdletInfo(subscriptionName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureSubscriptionCmdlet);
            azurePowershellCmdlet.Run();

            Collection<SubscriptionData> subscriptions = GetAzureSubscription();
            foreach (SubscriptionData subscription in subscriptions)
            {
                if (subscription.SubscriptionName == subscriptionName)
                {
                    return subscription;
                }
            }
            return null;
        }


        #endregion

        #region AzureSubnet


        public SubnetNamesCollection GetAzureSubnet(PersistentVM vm)
        {
            GetAzureSubnetCmdletInfo getAzureSubnetCmdlet = new GetAzureSubnetCmdletInfo(vm);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureSubnetCmdlet);
            Collection <PSObject> result = azurePowershellCmdlet.Run();
            
            SubnetNamesCollection subnets = new SubnetNamesCollection();
            foreach (PSObject re in result)            
            {
                subnets.Add((string)re.BaseObject);
            }
            return subnets;
        }

        public PersistentVM SetAzureSubnet(PersistentVM vm, string [] subnetNames)
        {
            SetAzureSubnetCmdletInfo setAzureSubnetCmdlet = new SetAzureSubnetCmdletInfo(vm, subnetNames);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureSubnetCmdlet);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
            
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }

            return null;
        }



        #endregion


        #region AzureStorageAccount

        public ManagementOperationContext NewAzureStorageAccount(string storageName, string locationName, string affinity, string label, string description)
        {
            NewAzureStorageAccountCmdletInfo newAzureStorageAccountCmdletInfo = new NewAzureStorageAccountCmdletInfo(storageName, locationName, affinity, label, description);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(newAzureStorageAccountCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        public StorageServicePropertiesOperationContext NewAzureStorageAccount(string storageName, string locationName)
        {
            NewAzureStorageAccount(storageName, locationName, null, null, null);

            Collection<StorageServicePropertiesOperationContext> storageAccounts = GetAzureStorageAccount(null);
            foreach (StorageServicePropertiesOperationContext storageAccount in storageAccounts)
            {
                if (storageAccount.StorageAccountName == storageName)
                    return storageAccount;
            }
            return null;
        }

        public Collection<StorageServicePropertiesOperationContext> GetAzureStorageAccount(string accountName)
        {
            GetAzureStorageAccountCmdletInfo getAzureStorageAccountCmdlet = new GetAzureStorageAccountCmdletInfo(accountName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureStorageAccountCmdlet);

            Collection<StorageServicePropertiesOperationContext> storageAccounts = new Collection<StorageServicePropertiesOperationContext>();
            foreach (PSObject result in azurePowershellCmdlet.Run())
            {
                storageAccounts.Add((StorageServicePropertiesOperationContext)result.BaseObject);
            }
            return storageAccounts;
        }

        public ManagementOperationContext SetAzureStorageAccount(string accountName, string label, string description, bool geoReplication)
        {
            SetAzureStorageAccountCmdletInfo setAzureStorageAccountCmdletInfo = new SetAzureStorageAccountCmdletInfo(accountName, label, description, geoReplication);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureStorageAccountCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        public void RemoveAzureStorageAccount(string storageAccountName)
        {
            var removeAzureStorageAccountCmdletInfo = new RemoveAzureStorageAccountCmdletInfo(storageAccountName);
            var azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureStorageAccountCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
        }


        #endregion


        #region AzureStorageKey

        public StorageServiceKeyOperationContext GetAzureStorageAccountKey(string stroageAccountName)
        {
            GetAzureStorageKeyCmdletInfo getAzureStorageKeyCmdletInfo = new GetAzureStorageKeyCmdletInfo(stroageAccountName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureStorageKeyCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (StorageServiceKeyOperationContext)result[0].BaseObject;
            }
            return null;
        }

        public StorageServiceKeyOperationContext NewAzureStorageAccountKey(string stroageAccountName, string keyType)
        {
            NewAzureStorageKeyCmdletInfo newAzureStorageKeyCmdletInfo = new NewAzureStorageKeyCmdletInfo(stroageAccountName, keyType);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(newAzureStorageKeyCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (StorageServiceKeyOperationContext)result[0].BaseObject;
            }
            return null;
        }





        #endregion


        #region AzureService

        public ManagementOperationContext NewAzureService(string serviceName, string location)
        {
            var newAzureServiceCmdletInfo = new NewAzureServiceCmdletInfo(serviceName, location);
            var newAzureServiceCmdlet = new WindowsAzurePowershellCmdlet(newAzureServiceCmdletInfo);

            var result = newAzureServiceCmdlet.Run();

            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        internal void NewAzureService(string serviceName, string serviceLabel, string locationName)
        {
            NewAzureServiceCmdletInfo newAzureServiceCmdletInfo = new NewAzureServiceCmdletInfo(serviceName, serviceLabel, locationName);
            WindowsAzurePowershellCmdlet newAzureServiceCmdlet = new WindowsAzurePowershellCmdlet(newAzureServiceCmdletInfo);

            Collection<PSObject> result = newAzureServiceCmdlet.Run();
        }

        public void RemoveAzureService(string serviceName)
        {
            RemoveAzureServiceCmdletInfo removeAzureServiceCmdletInfo = new RemoveAzureServiceCmdletInfo(serviceName);
            WindowsAzurePowershellCmdlet removeAzureServiceCmdlet = new WindowsAzurePowershellCmdlet(removeAzureServiceCmdletInfo);

            var result = removeAzureServiceCmdlet.Run();
        }

        public HostedServiceDetailedContext GetAzureService(string serviceName)
        {
            GetAzureServiceCmdletInfo getAzureServiceCmdletInfo = new GetAzureServiceCmdletInfo(serviceName);
            WindowsAzurePowershellCmdlet getAzureServiceCmdlet = new WindowsAzurePowershellCmdlet(getAzureServiceCmdletInfo);

            Collection<PSObject> result = getAzureServiceCmdlet.Run();
            if (result.Count == 1)
            {
                return (HostedServiceDetailedContext)result[0].BaseObject;
            }
            return null;
        }



        #endregion


        #region AzureVM
        
        internal Collection<ManagementOperationContext> NewAzureVM(string serviceName, PersistentVM[] VMs)
        {
            return NewAzureVM(serviceName, VMs, null, null, null, null, null, null, null, null);                        
        }

        internal Collection<ManagementOperationContext> NewAzureVM(string serviceName, PersistentVM[] vms, string vnetName, DnsServer[] dnsSettings, string affinityGroup,
            string serviceLabel, string serviceDescription, string deploymentLabel, string deploymentDescription, string location)
        {
            NewAzureVMCmdletInfo newAzureVMCmdletInfo = 
                new NewAzureVMCmdletInfo(serviceName, vms, vnetName, dnsSettings, affinityGroup, serviceLabel, serviceDescription, deploymentLabel, deploymentDescription, location);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(newAzureVMCmdletInfo);

            Collection<ManagementOperationContext> newAzureVMs = new Collection<ManagementOperationContext>();
            foreach (PSObject result in azurePowershellCmdlet.Run())
            {
                newAzureVMs.Add((ManagementOperationContext)result.BaseObject);
            }
            return newAzureVMs;
        }

        public PersistentVMRoleContext GetAzureVM(string vmName, string serviceName)
        {
            GetAzureVMCmdletInfo getAzureVMCmdletInfo = new GetAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureVMCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVMRoleContext)result[0].BaseObject;
            }
            return null;
        }

        public void RemoveAzureVM(string vmName, string serviceName)
        {
            RemoveAzureVMCmdletInfo removeAzureVMCmdletInfo = new RemoveAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureVMCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
        }



        public void StartAzureVM(string vmName, string serviceName)
        {
            StartAzureVMCmdletInfo startAzureVMCmdlet = new StartAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(startAzureVMCmdlet);
            azurePowershellCmdlet.Run();
        }

        public void StopAzureVM(string vmName, string serviceName)
        {
            StopAzureVMCmdletInfo stopAzureVMCmdlet = new StopAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(stopAzureVMCmdlet);
            azurePowershellCmdlet.Run();
        }

        public void RestartAzureVM(string vmName, string serviceName)
        {
            RestartAzureVMCmdletInfo restartAzureVMCmdlet = new RestartAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(restartAzureVMCmdlet);
            azurePowershellCmdlet.Run();
        }

        


        public PersistentVMRoleContext ExportAzureVM(string vmName, string serviceName, string path)
        {
            //PersistentVMRoleContext result = new PersistentVMRoleContext
            ExportAzureVMCmdletInfo exportAzureVMCmdletInfo = new ExportAzureVMCmdletInfo(vmName, serviceName, path);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(exportAzureVMCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();

            if (result.Count == 1)
            {
                return (PersistentVMRoleContext)result[0].BaseObject;
            }

            return null;
        }

        public Collection<PersistentVM> ImportAzureVM(string path)
        {
            Collection<PersistentVM> result = new Collection<PersistentVM>();
            ImportAzureVMCmdletInfo importAzureVMCmdletInfo = new ImportAzureVMCmdletInfo(path);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(importAzureVMCmdletInfo);

            foreach (var vm in azurePowershellCmdlet.Run())
            {
                result.Add((PersistentVM)vm.BaseObject);
            }
            return result;
        }

        private ManagementOperationContext UpdateAzureVM(string vmName, string serviceName, PersistentVM persistentVM)
        {
            UpdateAzureVMCmdletInfo updateAzureVMCmdletInfo = new UpdateAzureVMCmdletInfo(vmName, serviceName, persistentVM);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(updateAzureVMCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }



        

        #endregion

        

        #region AzureVMImage

        private OSImageContext OSImageContextRun(WindowsAzurePowershellCmdlet azurePSCmdlet)
        {
            Collection<PSObject> result = azurePSCmdlet.Run();
            if (result.Count == 1)
            {
                return (OSImageContext)result[0].BaseObject;
            }
            return null;
        }
        

        public OSImageContext AddAzureVMImage(string imageName, string mediaLocation, OSType os, string label = null)
        {
            AddAzureVMImageCmdletInfo addAzureVMImageCmdlet = new AddAzureVMImageCmdletInfo(imageName, mediaLocation, os, label);
            return OSImageContextRun(new WindowsAzurePowershellCmdlet(addAzureVMImageCmdlet));            
        }

        public OSImageContext UpdateAzureVMImage(string imageName, string label)
        {
            UpdateAzureVMImageCmdletInfo updateAzureVMImageCmdlet = new UpdateAzureVMImageCmdletInfo(imageName, label);
            return OSImageContextRun(new WindowsAzurePowershellCmdlet(updateAzureVMImageCmdlet));
        }

        public ManagementOperationContext RemoveAzureVMImage(string imageName, bool deleteVhd = false)
        {
            RemoveAzureVMImageCmdletInfo removeAzureVMImageCmdlet = new RemoveAzureVMImageCmdletInfo(imageName, deleteVhd);
            return ManagementOperationContextRun(new WindowsAzurePowershellCmdlet(removeAzureVMImageCmdlet));
        }

        public void SaveAzureVMImage(string serviceName, string vmName, string newVmName, string newImageName = null)
        {
            SaveAzureVMImageCmdletInfo saveAzureVMImageCmdlet = new SaveAzureVMImageCmdletInfo(serviceName, vmName, newVmName, newImageName);
            ManagementOperationContextRun(new WindowsAzurePowershellCmdlet(saveAzureVMImageCmdlet));
        }

        public Collection<OSImageContext> GetAzureVMImage(string imageName = null)
        {
            GetAzureVMImageCmdletInfo getAzureVMImageCmdlet = new GetAzureVMImageCmdletInfo(imageName);
            WindowsAzurePowershellCmdletSequence azurePowershellCmdlet = new WindowsAzurePowershellCmdletSequence();
            azurePowershellCmdlet.Add(getAzureVMImageCmdlet);
            Collection<OSImageContext> osImageContext = new Collection<OSImageContext>();
            foreach (PSObject result in azurePowershellCmdlet.Run())
            {
                osImageContext.Add((OSImageContext)result.BaseObject);
            }
            return osImageContext;
        }
        
        public string GetAzureVMImageName(string[] keywords, bool exactMatch = true)
        {
            Collection<OSImageContext> vmImages = GetAzureVMImage();
            foreach (OSImageContext image in vmImages)
            {
                if (Utilities.MatchKeywords(image.ImageName, keywords, exactMatch) >= 0)
                    return image.ImageName;
            }
            return null;
        }


        #endregion


        #region AzureVhd

        public VhdUploadContext AddAzureVhd(AddAzureVhdCmdletInfo cmdletInfo)
        {
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(cmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (VhdUploadContext)result[0].BaseObject;
            }
            return null;
        }

        public string AddAzureVhdStop(AddAzureVhdCmdletInfo cmdletInfo, int ms)
        {
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(cmdletInfo);
            return azurePowershellCmdlet.RunAndStop(ms).ToString();            
        }

        public VhdUploadContext AddAzureVhd(FileInfo localFile, string destination)
        {
            var addAzureVhdCmdletInfo = new AddAzureVhdCmdletInfo(destination, localFile.FullName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(addAzureVhdCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (VhdUploadContext)result[0].BaseObject;
            }
            return null;
        }

        public VhdUploadContext AddAzureVhd(FileInfo localFile, string destination, int numberOfUploaderThreads, bool overWrite)
        {
            var addAzureVhdCmdletInfo = new AddAzureVhdCmdletInfo(destination, localFile.FullName, numberOfUploaderThreads, overWrite);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(addAzureVhdCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (VhdUploadContext)result[0].BaseObject;
            }
            return null;
        }

        

        public VhdDownloadContext SaveAzureVhd(Uri source, FileInfo localFilePath, int? numThreads, string storageKey, bool overwrite)
        {
            SaveAzureVhdCmdletInfo saveAzureVhdCmdletInfo = new SaveAzureVhdCmdletInfo(source, localFilePath, numThreads, storageKey, overwrite);            
            return runSaveAzureVhd(saveAzureVhdCmdletInfo);
        }

        public string SaveAzureVhdStop(Uri source, FileInfo localFilePath, int? numThreads, string storageKey, bool overwrite, int ms)
        {
            SaveAzureVhdCmdletInfo saveAzureVhdCmdletInfo = new SaveAzureVhdCmdletInfo(source, localFilePath, numThreads, storageKey, overwrite);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(saveAzureVhdCmdletInfo);
            return azurePowershellCmdlet.RunAndStop(ms).ToString();            
        }       

        private VhdDownloadContext runSaveAzureVhd(SaveAzureVhdCmdletInfo saveAzureVhdCmdletInfo)
        {
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(saveAzureVhdCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (VhdDownloadContext)result[0].BaseObject;
            }
            return null;
        }



        #endregion

      

        #region AzureVnetConfig

        public Collection<VirtualNetworkConfigContext> GetAzureVNetConfig(string filePath)
        {
            GetAzureVNetConfigCmdletInfo getAzureVNetConfigCmdletInfo = new GetAzureVNetConfigCmdletInfo(filePath);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureVNetConfigCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            Collection<VirtualNetworkConfigContext> vnetGateways = new Collection<VirtualNetworkConfigContext>();
            foreach (PSObject re in result)
            {
                vnetGateways.Add((VirtualNetworkConfigContext)re.BaseObject);
            }
            return vnetGateways;
        }
     

        public ManagementOperationContext SetAzureVNetConfig(string filePath)
        {
            SetAzureVNetConfigCmdletInfo setAzureVNetConfigCmdletInfo = new SetAzureVNetConfigCmdletInfo(filePath);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureVNetConfigCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            
            if (result.Count == 1)
            {
                return (ManagementOperationContext) result[0].BaseObject;
            }
            return null;
        }

        public ManagementOperationContext RemoveAzureVNetConfig()
        {
            RemoveAzureVNetConfigCmdletInfo removeAzureVNetConfigCmdletInfo = new RemoveAzureVNetConfigCmdletInfo();
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureVNetConfigCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();


            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }


        #endregion


        #region AzureVNetGateway


        public ManagementOperationContext NewAzureVNetGateway(string vnetName)
        {
            NewAzureVNetGatewayCmdletInfo newAzureVNetGatewayCmdletInfo = new NewAzureVNetGatewayCmdletInfo(vnetName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(newAzureVNetGatewayCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }


        public Collection <VirtualNetworkGatewayContext> GetAzureVNetGateway(string vnetName)
        {
            GetAzureVNetGatewayCmdletInfo getAzureVNetGatewayCmdletInfo = new GetAzureVNetGatewayCmdletInfo(vnetName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureVNetGatewayCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            Collection<VirtualNetworkGatewayContext> vnetGateways = new Collection<VirtualNetworkGatewayContext>();
            foreach (PSObject re in result)
            {
                vnetGateways.Add ((VirtualNetworkGatewayContext) re.BaseObject);
            }
            return vnetGateways;
        }

        public ManagementOperationContext SetAzureVNetGateway(string option, string vnetName, string localNetwork)
        {
            SetAzureVNetGatewayCmdletInfo setAzureVNetGatewayCmdletInfo = new SetAzureVNetGatewayCmdletInfo(option, vnetName, localNetwork);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(setAzureVNetGatewayCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        public ManagementOperationContext RemoveAzureVNetGateway(string vnetName)
        {
            GetAzureVNetGatewayKeyCmdletInfo a = new GetAzureVNetGatewayKeyCmdletInfo("aaa", "vvv");

            RemoveAzureVNetGatewayCmdletInfo removeAzureVNetGatewayCmdletInfo = new RemoveAzureVNetGatewayCmdletInfo(vnetName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureVNetGatewayCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }


        public SharedKeyContext GetAzureVNetGatewayKey(string vnetName, string localnet)
        {
            GetAzureVNetGatewayKeyCmdletInfo getAzureVNetGatewayKeyCmdletInfo = new GetAzureVNetGatewayKeyCmdletInfo(vnetName, localnet);            
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureVNetGatewayKeyCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
            
            if (result.Count == 1)
            {
                return (SharedKeyContext) result[0].BaseObject;
            }
            return null;
        }



        #endregion


        #region AzureVNet

        public Collection<GatewayConnectionContext> GetAzureVNetConnection(string vnetName)
        {
            GetAzureVNetConnectionCmdletInfo getAzureVNetConnectionCmdletInfo = new GetAzureVNetConnectionCmdletInfo(vnetName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureVNetConnectionCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            Collection<GatewayConnectionContext> connections = new Collection<GatewayConnectionContext>();
            foreach (PSObject re in result)
            {
                connections.Add((GatewayConnectionContext)re.BaseObject);
            }
            return connections;
        }


        public Collection<VirtualNetworkSiteContext> GetAzureVNetSite(string vnetName)
        {
            GetAzureVNetSiteCmdletInfo getAzureVNetSiteCmdletInfo = new GetAzureVNetSiteCmdletInfo(vnetName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureVNetSiteCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();

            Collection<VirtualNetworkSiteContext> connections = new Collection<VirtualNetworkSiteContext>();
            foreach (PSObject re in result)
            {
                connections.Add((VirtualNetworkSiteContext)re.BaseObject);
            }
            return connections;
        }


        #endregion


        public void GetAzureRemoteDesktopFile(string vmName, string serviceName, string localPath, bool launch)
        {
            GetAzureRemoteDesktopFileCmdletInfo getAzureRemoteDesktopFileCmdletInfo = new GetAzureRemoteDesktopFileCmdletInfo(vmName, serviceName, localPath, launch);
            WindowsAzurePowershellCmdlet getAzureRemoteDesktopFileCmdlet = new WindowsAzurePowershellCmdlet(getAzureRemoteDesktopFileCmdletInfo);

            Collection<PSObject> result = getAzureRemoteDesktopFileCmdlet.Run();            
        }


        
        

       

        internal PersistentVM GetPersistentVM(PersistentVMConfigInfo configInfo)
        {
            PersistentVM vm = null;

            if (null != configInfo)
            {
                if (configInfo.VmConfig != null)
                {
                    vm = NewAzureVMConfig(configInfo.VmConfig);
                }

                if (configInfo.ProvConfig != null)
                {
                    configInfo.ProvConfig.Vm = vm;
                    vm = AddAzureProvisioningConfig(configInfo.ProvConfig);
                }

                if (configInfo.DiskConfig != null)
                {
                    configInfo.DiskConfig.Vm = vm;
                    vm = AddAzureDataDisk(configInfo.DiskConfig);
                }

                if (configInfo.EndPointConfig != null)
                {
                    configInfo.EndPointConfig.Vm = vm;
                    vm = AddAzureEndPoint(configInfo.EndPointConfig);
                }
            }

            return vm;
        }

       

        

        


        
        

        

        internal void AddVMDataDisks(string vmName, string serviceName, AddAzureDataDiskConfig[] diskConfig)
        {
            PersistentVMRoleContext vmRolectx = GetAzureVM(vmName, serviceName);

            foreach (AddAzureDataDiskConfig discCfg in diskConfig)
            {
                discCfg.Vm = vmRolectx.VM;
                vmRolectx.VM = AddAzureDataDisk(discCfg);
            }

            UpdateAzureVM(vmName, serviceName, vmRolectx.VM);
        }

        
        internal void SetVMDataDisks(string vmName, string serviceName, SetAzureDataDiskConfig[] diskConfig)
        {
            PersistentVMRoleContext vmRolectx = GetAzureVM(vmName, serviceName);

            foreach (SetAzureDataDiskConfig discCfg in diskConfig)
            {
                discCfg.Vm = vmRolectx.VM;
                vmRolectx.VM = SetAzureDataDisk(discCfg);
            }

            UpdateAzureVM(vmName, serviceName, vmRolectx.VM);
        }

        

        internal void SetVMSize(string vmName, string serviceName, SetAzureVMSizeConfig vmSizeConfig)
        {
            PersistentVMRoleContext vmRolectx = GetAzureVM(vmName, serviceName);

            vmSizeConfig.Vm = vmRolectx.VM;
            vmRolectx.VM = SetAzureVMSize(vmSizeConfig);

            UpdateAzureVM(vmName, serviceName, vmRolectx.VM);
        }

        private PersistentVM SetAzureVMSize(SetAzureVMSizeConfig sizeCfg)
        {
            SetAzureVMSizeCmdletInfo setAzureVMSizeCmdletInfo = new SetAzureVMSizeCmdletInfo(sizeCfg);
            WindowsAzurePowershellCmdlet setAzureDataDiskCmdlet = new WindowsAzurePowershellCmdlet(setAzureVMSizeCmdletInfo);

            Collection<PSObject> result = setAzureDataDiskCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }

        internal void AddVMDataDisksAndEndPoint(string vmName, string serviceName, AddAzureDataDiskConfig[] dataDiskConfig, AzureEndPointConfigInfo endPointConfig)
        {
            AddVMDataDisks(vmName, serviceName, dataDiskConfig);

            AddEndPoint(vmName, serviceName, new [] {endPointConfig});


        }

        private ManagementOperationContext ManagementOperationContextRun(WindowsAzurePowershellCmdlet azurePSCmdlet)
        {
            Collection<PSObject> result = azurePSCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
        }

        private PersistentVM PersistentVMRun(WindowsAzurePowershellCmdlet azurePSCmdlet)
        {
            Collection<PSObject> result = azurePSCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }

        

    }

}

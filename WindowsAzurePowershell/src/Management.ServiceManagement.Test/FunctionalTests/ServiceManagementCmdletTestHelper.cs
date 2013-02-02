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
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.IaasCmdletInfo;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests.ConfigDataInfo;

    using System.Collections.Generic;

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


        #region AzureDisk


        public Collection<DiskContext> GetAzureDisk()
        {
            GetAzureDiskCmdletInfo getAzureDiskCmdlet = new GetAzureDiskCmdletInfo((string)null);
            WindowsAzurePowershellCmdletSequence azurePowershellCmdlet = new WindowsAzurePowershellCmdletSequence();
            azurePowershellCmdlet.Add(getAzureDiskCmdlet);

            Collection<DiskContext> diskContext = new Collection<DiskContext>();
            foreach (PSObject result in azurePowershellCmdlet.Run())
            {
                diskContext.Add((DiskContext)result.BaseObject);
            }
            return diskContext;
        }

        public Collection<DiskContext> GetAzureDisk(string diskName)
        {
            GetAzureDiskCmdletInfo getAzureDiskCmdlet = new GetAzureDiskCmdletInfo(diskName);
            WindowsAzurePowershellCmdletSequence azurePowershellCmdlet = new WindowsAzurePowershellCmdletSequence();
            azurePowershellCmdlet.Add(getAzureDiskCmdlet);

            Collection<DiskContext> diskContext = new Collection<DiskContext>();
            foreach (PSObject result in azurePowershellCmdlet.Run())
            {
                diskContext.Add((DiskContext)result.BaseObject);
            }
            return diskContext;
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

        public void RemoveAzureDisk(string diskName, bool deleteVHD)
        {
            RemoveAzureDiskCmdletInfo removeAzureDiskCmdletInfo = new RemoveAzureDiskCmdletInfo(diskName, deleteVHD);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
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
            UpdateAzureVM(serviceName, vmName, vm);
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
            UpdateAzureVM(serviceName, vmName, SetAzureDataDisk(config));
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

        private ManagementOperationContext RemoveAzureDataDisk(RemoveAzureDataDiskConfig discCfg)
        {
            RemoveAzureDataDiskCmdletInfo removeAzureDataDiskCmdletInfo = new RemoveAzureDataDiskCmdletInfo(discCfg);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureDataDiskCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
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
            UpdateAzureVM(serviceName, vmName, vm);
        }

        #endregion


        #region AzureEndpoint

        public PersistentVM AddAzureEndPoint(AzureEndPointConfigInfo endPointConfig)
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
                vm = AddAzureEndPoint(config);
            }            
            UpdateAzureVM(serviceName, vmName, vm);
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
                SetAzureEndpointCmdletInfo setAzureEndpointCmdletInfo = new SetAzureEndpointCmdletInfo(endPointConfig);
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
            
            UpdateAzureVM(serviceName, vmName, SetAzureEndPoint(endPointConfig));
        }

        public PersistentVM RemoveAzureEndPoint(string vmName, PersistentVMRoleContext vmRoleCtxt)
        {
            RemoveAzureEndpointCmdletInfo removeAzureEndPointCmdletInfo = new RemoveAzureEndpointCmdletInfo(vmName, vmRoleCtxt);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureEndPointCmdletInfo);

            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }

        public void RemoveEndPoint(string vmName, string serviceName, string [] epNames)
        {

            PersistentVMRoleContext vmRoleCtxt = GetAzureVM(vmName, serviceName);

            foreach (string ep in epNames)
            {                
                vmRoleCtxt.VM = RemoveAzureEndPoint(ep, vmRoleCtxt);
            }
            UpdateAzureVM(serviceName, vmName, vmRoleCtxt.VM);
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


        #region AzureVM
        
        internal Collection<ManagementOperationContext> NewAzureVM(string serviceName, PersistentVM[] VMs)
        {
            NewAzureVMCmdletInfo newAzureVMCmdletInfo = new NewAzureVMCmdletInfo(serviceName, VMs);
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

        private ManagementOperationContext UpdateAzureVM(string serviceName, string vmName, PersistentVM persistentVM)
        {
            UpdateAzureVMCmdletInfo updateAzureVMCmdletInfo = new UpdateAzureVMCmdletInfo(serviceName, vmName, persistentVM);
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

        public Collection<OSImageContext> GetAzureVMImage()
        {
            GetAzureVMImageCmdletInfo getAzureVMImageCmdlet = new GetAzureVMImageCmdletInfo();
            WindowsAzurePowershellCmdletSequence azurePowershellCmdlet = new WindowsAzurePowershellCmdletSequence();
            azurePowershellCmdlet.Add(getAzureVMImageCmdlet);
            Collection<OSImageContext> osImageContext = new Collection<OSImageContext>();
            foreach (PSObject result in azurePowershellCmdlet.Run())
            {
                osImageContext.Add((OSImageContext)result.BaseObject);
            }
            return osImageContext;
        }

        public void SaveAzureVMImage(string serviceName, string vmName, string newVmName, string newImageName, string postCaptureAction)
        {

            SaveAzureVMImageCmdletInfo saveAzureVMImageCmdlet = new SaveAzureVMImageCmdletInfo(serviceName, vmName, newVmName, newImageName, postCaptureAction);
            WindowsAzurePowershellCmdletSequence azurePowershellCmdlet = new WindowsAzurePowershellCmdletSequence();
            azurePowershellCmdlet.Add(saveAzureVMImageCmdlet);
            Collection<OSImageContext> osImageContext = new Collection<OSImageContext>();
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


        #region AzureStorageAccount

        public ManagementOperationContext NewAzureStorageAccount(string storageName, string locationName, string affinity, string label, string description)
        {
            NewAzureStorageAccountCmdletInfo newAzureStorageAccountCmdletInfo = new NewAzureStorageAccountCmdletInfo(storageName, locationName, affinity, label, description);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(newAzureStorageAccountCmdletInfo);
            Collection <PSObject> result = azurePowershellCmdlet.Run();

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


        #endregion



        public void GetAzureRemoteDesktopFile(string vmName, string serviceName, string localPath, bool launch)
        {
            GetAzureRemoteDesktopFileCmdletInfo getAzureRemoteDesktopFileCmdletInfo = new GetAzureRemoteDesktopFileCmdletInfo(vmName, serviceName, localPath, launch);
            WindowsAzurePowershellCmdlet getAzureRemoteDesktopFileCmdlet = new WindowsAzurePowershellCmdlet(getAzureRemoteDesktopFileCmdletInfo);

            Collection<PSObject> result = getAzureRemoteDesktopFileCmdlet.Run();            
        }


        
        public StorageServiceKeyOperationContext GetAzureStorageAccountKey(string stroageAccountName)
        {
            var getAzureStorageKeyCmdletInfo = new GetAzureStorageKeyCmdletInfo(stroageAccountName);
            WindowsAzurePowershellCmdlet azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(getAzureStorageKeyCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
            if (result.Count == 1)
            {
                return (StorageServiceKeyOperationContext)result[0].BaseObject;
            }
            return null;
        }

       

        internal PersistentVM GetPersistentVM(PersistentVMConfigInfo configInfo)
        {
            PersistentVM vm = null;

            if (null != configInfo)
            {
                if (configInfo.VmConfig != null)
                {
                    vm = SetNewAzureVMConfig(configInfo.VmConfig);
                }

                if (configInfo.ProvConfig != null)
                {
                    configInfo.ProvConfig.Vm = vm;
                    vm = SetProvisioningConfig(configInfo.ProvConfig);
                }

                if (configInfo.DiskConfig != null)
                {
                    configInfo.DiskConfig.Vm = vm;
                    vm = AddAzureDataDisk(configInfo.DiskConfig);
                }

                if (configInfo.EndPointConfig != null)
                {
                    configInfo.EndPointConfig.Vm = vm;
                    vm = SetAzureEndPoint(configInfo.EndPointConfig);
                }
            }

            return vm;
        }

       

        

        private PersistentVM SetProvisioningConfig(AzureProvisioningConfigInfo provConfig)
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



        private PersistentVM SetNewAzureVMConfig(AzureVMConfigInfo vmConfig)
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

        

        

        internal void AddVMDataDisks(string vmName, string serviceName, AddAzureDataDiskConfig[] diskConfig)
        {
            PersistentVMRoleContext vmRolectx = GetAzureVM(vmName, serviceName);

            foreach (AddAzureDataDiskConfig discCfg in diskConfig)
            {
                discCfg.Vm = vmRolectx.VM;
                vmRolectx.VM = AddAzureDataDisk(discCfg);
            }

            UpdateAzureVM(serviceName, vmName, vmRolectx.VM);
        }

        
        internal void SetVMDataDisks(string vmName, string serviceName, SetAzureDataDiskConfig[] diskConfig)
        {
            PersistentVMRoleContext vmRolectx = GetAzureVM(vmName, serviceName);

            foreach (SetAzureDataDiskConfig discCfg in diskConfig)
            {
                discCfg.Vm = vmRolectx.VM;
                vmRolectx.VM = SetAzureDataDisk(discCfg);
            }

            UpdateAzureVM(serviceName, vmName, vmRolectx.VM);
        }

        

        internal void SetVMSize(string vmName, string serviceName, SetAzureVMSizeConfig vmSizeConfig)
        {
            PersistentVMRoleContext vmRolectx = GetAzureVM(vmName, serviceName);

            vmSizeConfig.Vm = vmRolectx.VM;
            vmRolectx.VM = SetAzureVMSize(vmSizeConfig);

            UpdateAzureVM(serviceName, vmName, vmRolectx.VM);
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

        

    }

}

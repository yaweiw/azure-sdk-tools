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
    using WindowsAzure.ServiceManagement;
    using Management.Model;
    using Model;
    using IaasCmdletInfo;
    using ConfigDataInfo;

    public class ServiceManagementCmdletTestHelper 
    {
        public Collection<DataVirtualHardDisk> GetAzureDataDisk(string vmName, string serviceName)
        {
            PersistentVMRoleContext vmRolectx = GetAzureVM(vmName, serviceName);

            GetAzureDataDiskCmdletInfo getAzureDataDiskCmdlet = new GetAzureDataDiskCmdletInfo(vmRolectx.VM);
            WindowsAzurePowershellCmdlet getAzureDataDisk = new WindowsAzurePowershellCmdlet(getAzureDataDiskCmdlet);

            Collection<DataVirtualHardDisk> hardDisks = new Collection<DataVirtualHardDisk>();
            foreach (PSObject disk in getAzureDataDisk.Run())
            {
                hardDisks.Add((DataVirtualHardDisk)disk.BaseObject);
            }
            return hardDisks;
        }

        public void StartAzureVM(string vmName, string serviceName)
        {
            StartAzureVMCmdletInfo startAzureVMCmdlet = new StartAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet startAzureVM = new WindowsAzurePowershellCmdlet(startAzureVMCmdlet);
            startAzureVM.Run();
        }

        public void StopAzureVM(string vmName, string serviceName)
        {
            StopAzureVMCmdletInfo stopAzureVMCmdlet = new StopAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet stopAzureVM = new WindowsAzurePowershellCmdlet(stopAzureVMCmdlet);
            stopAzureVM.Run();
        }

        public void RestartAzureVM(string vmName, string serviceName)
        {
            RestartAzureVMCmdletInfo restartAzureVMCmdlet = new RestartAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet restartAzureVM = new WindowsAzurePowershellCmdlet(restartAzureVMCmdlet);
            restartAzureVM.Run();
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

        public Collection<OSImageContext> GetAzureVMImage()
        {
            GetAzureVMImageCmdletInfo getAzureVMImageCmdlet = new GetAzureVMImageCmdletInfo();
            WindowsAzurePowershellCmdletSequence getAzureVMImage = new WindowsAzurePowershellCmdletSequence();
            getAzureVMImage.Add(getAzureVMImageCmdlet);
            Collection<OSImageContext> osImageContext = new Collection<OSImageContext>();
            foreach (PSObject result in getAzureVMImage.Run())
            {
                osImageContext.Add((OSImageContext)result.BaseObject);
            }
            return osImageContext;
        }

        public void SaveAzureVMImage(string serviceName, string vmName, string newVmName, string newImageName, string postCaptureAction)
        {

            SaveAzureVMImageCmdletInfo saveAzureVMImageCmdlet = new SaveAzureVMImageCmdletInfo(serviceName, vmName, newVmName, newImageName, postCaptureAction);
            WindowsAzurePowershellCmdletSequence saveAzureVMImage = new WindowsAzurePowershellCmdletSequence();
            saveAzureVMImage.Add(saveAzureVMImageCmdlet);
            Collection<OSImageContext> osImageContext = new Collection<OSImageContext>();            
        }



        public Collection<DiskContext> GetAzureDisk()
        {
            GetAzureDiskCmdletInfo getAzureDiskCmdlet = new GetAzureDiskCmdletInfo((string) null);
            WindowsAzurePowershellCmdletSequence getAzureDisk = new WindowsAzurePowershellCmdletSequence();
            getAzureDisk.Add(getAzureDiskCmdlet);
            Collection<DiskContext> diskContext = new Collection<DiskContext>();
            foreach (PSObject result in getAzureDisk.Run())
            {
                diskContext.Add((DiskContext)result.BaseObject);
            }
            return diskContext;
        }

        public Collection<DiskContext> GetAzureDisk(string diskName)
        {
            GetAzureDiskCmdletInfo getAzureDiskCmdlet = new GetAzureDiskCmdletInfo(diskName);
            WindowsAzurePowershellCmdletSequence getAzureDisk = new WindowsAzurePowershellCmdletSequence();
            getAzureDisk.Add(getAzureDiskCmdlet);
            Collection<DiskContext> diskContext = new Collection<DiskContext>();
            foreach (PSObject result in getAzureDisk.Run())
            {
                diskContext.Add((DiskContext)result.BaseObject);
            }
            return diskContext;
        }

        public Collection<DiskContext> GetAzureDiskAttachedToRoleName(string[] roleName, bool exactMatch = true)
        {
            Collection <DiskContext> retDisks = new Collection <DiskContext>();
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

        public Collection<LocationsContext> GetAzureLocation()
        {
            GetAzureLocationCmdletInfo getAzureLocationCmdlet = new GetAzureLocationCmdletInfo();
            WindowsAzurePowershellCmdlet getAzureLocation = new WindowsAzurePowershellCmdlet(getAzureLocationCmdlet);
            Collection<LocationsContext> locationsContext = new Collection<LocationsContext>();
            foreach (PSObject result in getAzureLocation.Run())
            {
                locationsContext.Add((LocationsContext)result.BaseObject);
            }
            return locationsContext;
        }

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

        public Collection<SubscriptionData> GetAzureSubscription()
        {
            GetAzureSubscriptionCmdletInfo getAzureSubscriptionCmdlet = new GetAzureSubscriptionCmdletInfo();
            WindowsAzurePowershellCmdlet getAzureSubscription = new WindowsAzurePowershellCmdlet(getAzureSubscriptionCmdlet);
            Collection<SubscriptionData> subscriptions = new Collection<SubscriptionData>();
            foreach (PSObject result in getAzureSubscription.Run())
            {
                subscriptions.Add((SubscriptionData)result.BaseObject);
            }

            return subscriptions;
        }

        public Collection<StorageServicePropertiesOperationContext> GetAzureStorageAccount()
        {
            GetAzureStorageAccountCmdletInfo getAzureStorageAccountCmdlet = new GetAzureStorageAccountCmdletInfo();
            WindowsAzurePowershellCmdlet getAzureStorageAccount = new WindowsAzurePowershellCmdlet(getAzureStorageAccountCmdlet);
            Collection<StorageServicePropertiesOperationContext> storageAccounts = new Collection<StorageServicePropertiesOperationContext>();
            foreach (PSObject result in getAzureStorageAccount.Run())
            {
                storageAccounts.Add((StorageServicePropertiesOperationContext)result.BaseObject);
            }
            return storageAccounts;
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
            WindowsAzurePowershellCmdlet setAzureSubscription = new WindowsAzurePowershellCmdlet(setAzureSubscriptionCmdlet);
            setAzureSubscription.Run();

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
            WindowsAzurePowershellCmdlet setAzureSubscription = new WindowsAzurePowershellCmdlet(setAzureSubscriptionCmdlet);
            setAzureSubscription.Run();

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

        public StorageServicePropertiesOperationContext NewAzureStorageAccount(string storageName, string locationName)
        {
            NewAzureStorageAccountCmdletInfo newAzureStorageAccountCmdletInfo = new NewAzureStorageAccountCmdletInfo(storageName, locationName);
            WindowsAzurePowershellCmdlet newAzureStorageCmdlet = new WindowsAzurePowershellCmdlet(newAzureStorageAccountCmdletInfo);
            newAzureStorageCmdlet.Run();
            Collection<StorageServicePropertiesOperationContext> storageAccounts = GetAzureStorageAccount();
            foreach (StorageServicePropertiesOperationContext storageAccount in storageAccounts)
            {
                if (storageAccount.StorageAccountName == storageName)
                    return storageAccount;
            }
            return null;
        }

        public PersistentVMRoleContext GetAzureVM(string vmName, string serviceName)
        {
            GetAzureVMCmdletInfo getAzureVMCmdletInfo = new GetAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet getAzureVMCmdlet = new WindowsAzurePowershellCmdlet(getAzureVMCmdletInfo);

            Collection<PSObject> result = getAzureVMCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVMRoleContext)result[0].BaseObject;
            }
            return null;
        }

        public void GetAzureRemoteDesktopFile(string vmName, string serviceName, string localPath, bool launch)
        {
            GetAzureRemoteDesktopFileCmdletInfo getAzureRemoteDesktopFileCmdletInfo = new GetAzureRemoteDesktopFileCmdletInfo(vmName, serviceName, localPath, launch);
            WindowsAzurePowershellCmdlet getAzureRemoteDesktopFileCmdlet = new WindowsAzurePowershellCmdlet(getAzureRemoteDesktopFileCmdletInfo);

            Collection<PSObject> result = getAzureRemoteDesktopFileCmdlet.Run();            
        }


        public InputEndpointContext GetAzureEndpoint(PersistentVMRoleContext vmRoleCtxt)
        {
            GetAzureEndpointCmdletInfo getAzureEndpointCmdletInfo = new GetAzureEndpointCmdletInfo(vmRoleCtxt);
            WindowsAzurePowershellCmdlet getAzureEndpointCmdlet = new WindowsAzurePowershellCmdlet(getAzureEndpointCmdletInfo);

            Collection<PSObject> result = getAzureEndpointCmdlet.Run();
            if (result.Count == 1)
            {
                return (InputEndpointContext)result[0].BaseObject;
            }
            return null;
        }

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

        public PersistentVMRoleContext ExportAzureVM(string vmName, string serviceName, string path)
        {
            //PersistentVMRoleContext result = new PersistentVMRoleContext
            ExportAzureVMCmdletInfo exportAzureVMCmdletInfo = new ExportAzureVMCmdletInfo(vmName, serviceName, path);
            WindowsAzurePowershellCmdlet exportAzureVMCmdlet = new WindowsAzurePowershellCmdlet(exportAzureVMCmdletInfo);

            Collection<PSObject> result = exportAzureVMCmdlet.Run();

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
            WindowsAzurePowershellCmdlet importAzureVMCmdlet = new WindowsAzurePowershellCmdlet(importAzureVMCmdletInfo);

            foreach (var vm in importAzureVMCmdlet.Run())
            {
                result.Add((PersistentVM)vm.BaseObject);
            }
            return result;
        }


        public void RemoveAzureVM(string vmName, string serviceName)
        {
            RemoveAzureVMCmdletInfo removeAzureVMCmdletInfo = new RemoveAzureVMCmdletInfo(vmName, serviceName);
            WindowsAzurePowershellCmdlet removeAzureVMCmdlet = new WindowsAzurePowershellCmdlet(removeAzureVMCmdletInfo);

            Collection<PSObject> result = removeAzureVMCmdlet.Run();
        }

        public void RemoveAzureDisk(string diskName, bool deleteVHD)
        {
            RemoveAzureDiskCmdletInfo removeAzureDiskCmdletInfo = new RemoveAzureDiskCmdletInfo(diskName, deleteVHD);
            WindowsAzurePowershellCmdlet removeAzureDiskCmdlet = new WindowsAzurePowershellCmdlet(removeAzureDiskCmdletInfo);

            Collection<PSObject> result = removeAzureDiskCmdlet.Run();
        }

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

        public void RemoveAzureStorageAccount(string storageAccountName)
        {
            var removeAzureStorageAccountCmdletInfo = new RemoveAzureStorageAccountCmdletInfo(storageAccountName);
            var azurePowershellCmdlet = new WindowsAzurePowershellCmdlet(removeAzureStorageAccountCmdletInfo);
            Collection<PSObject> result = azurePowershellCmdlet.Run();
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

        internal void NewAzureService(string serviceName, string serviceLabel, string locationName)
        {
            NewAzureServiceCmdletInfo newAzureServiceCmdletInfo = new NewAzureServiceCmdletInfo(serviceName, serviceLabel, locationName);
            WindowsAzurePowershellCmdlet newAzureServiceCmdlet = new WindowsAzurePowershellCmdlet(newAzureServiceCmdletInfo);

            Collection<PSObject> result = newAzureServiceCmdlet.Run();
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

        private PersistentVM SetAzureEndPoint(AzureEndPointConfigInfo endPointConfig)
        {
            if (null != endPointConfig)
            {
                AddAzureEndpointCmdletInfo addAzureEndpointCmdletInfo = new AddAzureEndpointCmdletInfo(endPointConfig);
                WindowsAzurePowershellCmdlet addAzureEndpointCmdlet = new WindowsAzurePowershellCmdlet(addAzureEndpointCmdletInfo);

                Collection<PSObject> result = addAzureEndpointCmdlet.Run();
                if (result.Count == 1)
                {
                    return (PersistentVM)result[0].BaseObject;
                }
            }
            return null;
        }

        private PersistentVM AddAzureDataDisk(AddAzureDataDiskConfig diskConfig)
        {
            AddAzureDataDiskCmdletInfo addAzureDataDiskCmdletInfo = new AddAzureDataDiskCmdletInfo(diskConfig);
            WindowsAzurePowershellCmdlet addAzureDataDiskCmdlet = new WindowsAzurePowershellCmdlet(addAzureDataDiskCmdletInfo);

            Collection<PSObject> result = addAzureDataDiskCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
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

        internal Collection<ManagementOperationContext> NewAzureVM(string serviceName, PersistentVM[] VMs)
        {
            NewAzureVMCmdletInfo newAzureVMCmdletInfo = new NewAzureVMCmdletInfo(serviceName, VMs);
            WindowsAzurePowershellCmdlet newAzureVMCmdlet = new WindowsAzurePowershellCmdlet(newAzureVMCmdletInfo);

            Collection<ManagementOperationContext> newAzureVMs = new Collection<ManagementOperationContext>();
            foreach (PSObject result in newAzureVMCmdlet.Run())
            {
                newAzureVMs.Add((ManagementOperationContext)result.BaseObject);
            }
            return newAzureVMs;
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

        private ManagementOperationContext UpdateAzureVM(string serviceName, string vmName, PersistentVM persistentVM)
        {
            UpdateAzureVMCmdletInfo updateAzureVMCmdletInfo = new UpdateAzureVMCmdletInfo(serviceName, vmName, persistentVM);
            WindowsAzurePowershellCmdlet updateAzureVMCmdlet = new WindowsAzurePowershellCmdlet(updateAzureVMCmdletInfo);

            Collection<PSObject> result = updateAzureVMCmdlet.Run();
            if (result.Count == 1)
            {
                return (ManagementOperationContext)result[0].BaseObject;
            }
            return null;
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

        private PersistentVM SetAzureDataDisk(SetAzureDataDiskConfig discCfg)
        {
            SetAzureDataDiskCmdletInfo setAzureDataDiskCmdletInfo = new SetAzureDataDiskCmdletInfo(discCfg);
            WindowsAzurePowershellCmdlet setAzureDataDiskCmdlet = new WindowsAzurePowershellCmdlet(setAzureDataDiskCmdletInfo);

            Collection<PSObject> result = setAzureDataDiskCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
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

            AddEndPoint(vmName, serviceName, endPointConfig);


        }

        private void AddEndPoint(string vmName, string serviceName, AzureEndPointConfigInfo endPointConfig)
        {
            PersistentVMRoleContext vmRolectx = GetAzureVM(vmName, serviceName);

            endPointConfig.Vm = vmRolectx.VM;
            vmRolectx.VM = AddAzureEndPoint(endPointConfig);

            UpdateAzureVM(serviceName, vmName, vmRolectx.VM);
        }

        private PersistentVM AddAzureEndPoint(AzureEndPointConfigInfo endPointConfig)
        {
            AddAzureEndpointCmdletInfo addAzureEndPointCmdletInfo = AddAzureEndpointCmdletInfo.BuildNoLoadBalancedCmdletInfo(endPointConfig);
            WindowsAzurePowershellCmdlet addAzureEndPointCmdlet = new WindowsAzurePowershellCmdlet(addAzureEndPointCmdletInfo);

            Collection<PSObject> result = addAzureEndPointCmdlet.Run();
            if (result.Count == 1)
            {
                return (PersistentVM)result[0].BaseObject;
            }
            return null;
        }

    }

}

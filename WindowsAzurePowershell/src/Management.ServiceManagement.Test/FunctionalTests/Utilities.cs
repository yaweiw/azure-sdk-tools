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
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;

    internal class Utilities 
    {
        public static string windowsAzurePowershellPath = Path.Combine(Environment.CurrentDirectory);
        public const string windowsAzurePowershellModuleCloudService = "Microsoft.WindowsAzure.Management.CloudService.dll";
        public const string windowsAzurePowershellModuleManagement = "Microsoft.WindowsAzure.Management.dll";
        public const string windowsAzurePowershellModuleService = "Microsoft.WindowsAzure.Management.Service.dll";
        public const string windowsAzurePowershellModuleServiceManagement = "Microsoft.WindowsAzure.Management.ServiceManagement.dll";

        public static string publishSettingsFile = Resource.PublishSettingsFile;

        public enum OS
        {
            Windows,
            Linux
        };

        // AzureAffinityGroup
        public const string NewAzureAffinityGroupCmdletName = "New-AzureAffinityGroup";
        public const string GetAzureAffinityGroupCmdletName = "Get-AzureAffinityGroup";
        public const string SetAzureAffinityGroupCmdletName = "Set-AzureAffinityGroup";
        public const string RemoveAzureAffinityGroupCmdletName = "Remove-AzureAffinityGroup";

        // AzureCertificate & AzureCertificateSetting
        public const string AddAzureCertificateCmdletName = "Add-AzureCertificate";
        public const string GetAzureCertificateCmdletName = "Get-AzureCertificate";
        public const string RemoveAzureCertificateCmdletName = "Remove-AzureCertificate";
        public const string NewAzureCertificateSettingCmdletName = "New-AzureCertificateSetting";


        // AzureDataDisk
        public const string AddAzureDataDiskCmdletName = "Add-AzureDataDisk";
        public const string GetAzureDataDiskCmdletName = "Get-AzureDataDisk";
        public const string SetAzureDataDiskCmdletName = "Set-AzureDataDisk";        
        public const string RemoveAzureDataDiskCmdletName = "Remove-AzureDataDisk";

        // AzureDeployment
        public const string NewAzureDeploymentCmdletName = "New-AzureDeployment";
        public const string GetAzureDeploymentCmdletName = "Get-AzureDeployment";
        public const string SetAzureDeploymentCmdletName = "Set-AzureDeployment";
        public const string RemoveAzureDeploymentCmdletName = "Remove-AzureDeployment";
        public const string MoveAzureDeploymentCmdletName = "Move-AzureDeployment";

        // AzureDisk        
        public const string AddAzureDiskCmdletName = "Add-AzureDisk";
        public const string GetAzureDiskCmdletName = "Get-AzureDisk";
        public const string UpdateAzureDiskCmdletName = "Update-AzureDisk";
        public const string RemoveAzureDiskCmdletName = "Remove-AzureDisk";


        // AzureDns
        public const string NewAzureDnsCmdletName = "New-AzureDns";
        public const string GetAzureDnsCmdletName = "Get-AzureDns";

        // AzureEndpoint
        public const string AddAzureEndpointCmdletName = "Add-AzureEndpoint";        
        public const string GetAzureEndpointCmdletName = "Get-AzureEndpoint";
        public const string SetAzureEndpointCmdletName = "Set-AzureEndpoint";
        public const string RemoveAzureEndpointCmdletName = "Remove-AzureEndpoint";



        // AzureLocation
        public const string GetAzureLocationCmdletName = "Get-AzureLocation";
        

        // AzureOSDisk & AzureOSVersion
        public const string GetAzureOSDiskCmdletName = "Get-AzureOSDisk";
        public const string SetAzureOSDiskCmdletName = "Set-AzureOSDisk";

        public const string GetAzureOSVersionCmdletName = "Get-AzureOSVersion";

        // AzureProvisioningConfig
        public const string AddAzureProvisioningConfigCmdletName = "Add-AzureProvisioningConfig";

        // AzurePublishSettingsFile
        public const string ImportAzurePublishSettingsFileCmdletName = "Import-AzurePublishSettingsFile";
        public const string GetAzurePublishSettingsFileCmdletName = "Get-AzurePublishSettingsFile";

        // AzureQuickVM
        public const string NewAzureQuickVMCmdletName = "New-AzureQuickVM";

        // AzureRemoteDesktopFile
        public const string GetAzureRemoteDesktopFileCmdletName = "Get-AzureRemoteDesktopFile";
        


        // AzureRole & AzureRoleInstnace
        public const string GetAzureRoleCmdletName = "Get-AzureRole";
        public const string SetAzureRoleCmdletName = "Set-AzureRole";

        public const string GetAzureRoleInstanceCmdletName = "Get-AzureRoleInstance";

        // AzureService
        public const string NewAzureServiceCmdletName = "New-AzureService";
        public const string GetAzureServiceCmdletName = "Get-AzureService";
        public const string SetAzureServiceCmdletName = "Set-AzureService";
        public const string RemoveAzureServiceCmdletName = "Remove-AzureService";
        
        // AzureSSHKey
        public const string NewAzureSSHKeyCmdletName = "New-AzureSSHKey";

        // AzureStorageAccount
        public const string NewAzureStorageAccountCmdletName = "New-AzureStorageAccount";
        public const string GetAzureStorageAccountCmdletName = "Get-AzureStorageAccount";        
        public const string SetAzureStorageAccountCmdletName = "Set-AzureStorageAccount";        
        public static string RemoveAzureStorageAccountCmdletName = "Remove-AzureStorageAccount";


        // AzureStorageKey
        public static string NewAzureStorageKeyCmdletName = "New-AzureStorageKey";
        public static string GetAzureStorageKeyCmdletName = "Get-AzureStorageKey";

        // AzureSubnet
        public static string GetAzureSubnetCmdletName = "Get-AzureSubnet";
        public static string SetAzureSubnetCmdletName = "Set-AzureSubnet";


        // AzureSubscription
        public const string GetAzureSubscriptionCmdletName = "Get-AzureSubscription";
        public const string SetAzureSubscriptionCmdletName = "Set-AzureSubscription";
        public const string SelectAzureSubscriptionCmdletName = "Select-AzureSubscription";
        public const string RemoveAzureSubscriptionCmdletName = "Remove-AzureSubscription";        


        // AzureVhd
        public static string AddAzureVhdCmdletName = "Add-AzureVhd";

        // AzureVM
        public const string NewAzureVMCmdletName = "New-AzureVM";
        public const string GetAzureVMCmdletName = "Get-AzureVM";
        public const string UpdateAzureVMCmdletName = "Update-AzureVM";                
        public const string RemoveAzureVMCmdletName = "Remove-AzureVM";
        
        public const string ExportAzureVMCmdletName = "Export-AzureVM";
        public const string ImportAzureVMCmdletName = "Import-AzureVM";
        
        public const string StartAzureVMCmdletName = "Start-AzureVM";
        public const string StopAzureVMCmdletName = "Stop-AzureVM";
        public const string RestartAzureVMCmdletName = "Restart-AzureVM";
        
        
        

        // AzureVMConfig
        public const string NewAzureVMConfigCmdletName = "New-AzureVMConfig";

        // AzureVMImage
        
        public const string AddAzureVMImageCmdletName = "Add-AzureVMImage";
        public const string GetAzureVMImageCmdletName = "Get-AzureVMImage";
        public const string RemoveAzureVMImageCmdletName = "Remove-AzureVMImage";
        public const string SaveAzureVMImageCmdletName = "Save-AzureVMImage";
        public const string UpdateAzureVMImageCmdletName = "Update-AzureVMImage";
        
        // AzureVMSize
        public const string SetAzureVMSizeCmdletName = "Set-AzureVMSize";

        // AzureVNetConfig & AzureVNetConnection
        public const string GetAzureVNetConfigCmdletName = "Get-AzureVNetConfig";
        public const string SetAzureVNetConfigCmdletName = "Set-AzureVNetConfig";
        public const string RemoveAzureVNetConfigCmdletName = "Remove-AzureVNetConfig";
        
        public const string GetAzureVNetConnectionCmdletName = "Get-AzureVNetConnection";

        // AzureVnetGateway & AzureVnetGatewayKey
        public const string NewAzureVNetGatewayCmdletName = "New-AzureVNetGateway";
        public const string GetAzureVNetGatewayCmdletName = "Get-AzureVNetGateway";
        public const string SetAzureVNetGatewayCmdletName = "Set-AzureVNetGateway";
        public const string RemoveAzureVNetGatewayCmdletName = "Remove-AzureVNetGateway";

        public const string GetAzureVNetGatewayKeyCmdletName = "Get-AzureVNetGatewayKey";

        // AzureVNetSite
        public const string GetAzureVNetSiteCmdletName = "Get-AzureVNetSite";

        // AzureWalkUpgradeDomain
        public const string SetAzureUpgradeDomainCmdletName = "Set-AzureUpgradeDomain";


        public const string GetModuleCmdletName = "Get-Module";       
        public const string TestAzureNameCmdletName = "Test-AzureName";
        
        public const string CopyAzureStorageBlobCmdletName = "Copy-AzureStorageBlob";
        

        public static string GetUniqueShortName(string prefix = "", int length = 6, string suffix = "")
        {
            return string.Format("{0}{1}{2}", prefix, Guid.NewGuid().ToString("N").Substring(0, length), suffix);
        }

        public static int MatchKeywords(string input, string[] keywords, bool exactMatch = true)
        { //returns -1 for no match, 0 for exact match, and a positive number for how many keywords are matched.
            int result = 0;
            if (string.IsNullOrEmpty(input) || keywords.Length == 0)
                return -1;
            foreach (string keyword in keywords)
            {
                //For whole word match, modify pattern to be "\b{0}\b"
                if (!string.IsNullOrEmpty(keyword) && Regex.IsMatch(input, string.Format(@"{0}", Regex.Escape(keyword)), RegexOptions.IgnoreCase))
                {
                    result++;
                }
            }
            if (result == keywords.Length)
            {
                return 0;
            }
            else if (result == 0)
            {
                return -1;
            }
            else
            {
                if (exactMatch)
                    return -1;
                else
                    return result;
            }
        }
    }
}

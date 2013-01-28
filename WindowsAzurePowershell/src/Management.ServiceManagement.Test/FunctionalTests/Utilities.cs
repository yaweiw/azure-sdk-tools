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

        public const string GetModuleCmdletName = "Get-Module";
        public const string NewAzureQuickVMCmdletName = "New-AzureQuickVM";
        public const string SaveAzureVMImageCmdletName = "Save-AzureVMImage";
        public const string GetAzureVMImageCmdletName = "Get-AzureVMImage";
        public const string GetAzureVMCmdletName = "Get-AzureVM";
        public const string ExportAzureVMCmdletName = "Export-AzureVM";
        public const string ImportAzureVMCmdletName = "Import-AzureVM";
        public const string RemoveAzureVMCmdletName = "Remove-AzureVM";
        public const string RemoveAzureDiskCmdletName = "Remove-AzureDisk";
        public const string GetAzureLocationCmdletName = "Get-AzureLocation";
        public const string GetAzureEndpointCmdletName = "Get-AzureEndpoint";
        public const string GetAzureRemoteDesktopFileCmdletName = "Get-AzureRemoteDesktopFile";
        public const string ImportAzurePublishSettingsFileCmdletName = "Import-AzurePublishSettingsFile";
        public const string NewAzureAffinityGroupCmdletName = "New-AzureAffinityGroup";
        public const string SetAzureStorageAccountCmdletName = "Set-AzureStorageAccount";
        public const string GetAzureSubscriptionCmdletName = "Get-AzureSubscription";
        public const string GetAzureStorageAccountCmdletName = "Get-AzureStorageAccount";
        public const string NewAzureStorageAccountCmdletName = "New-AzureStorageAccount";
        public const string SetAzureSubscriptionCmdletName = "Set-AzureSubscription";
         
        public const string AddAzureDataDiskCmdletName = "Add-AzureDataDisk";
        public const string AddAzureEndpointCmdletName = "Add-AzureEndpoint";
        public const string UpdateAzureVMCmdletName = "Update-AzureVM";
        public const string SetAzureDataCmdletName = "Set-AzureDataDisk";

        public const string NewAzureServiceCmdletName = "New-AzureService";
        public const string RemoveAzureServiceCmdletName = "Remove-AzureService";
        public const string GetAzureServiceCmdletName = "Get-AzureService";

        //Basic Provisioning a Virtual Machine
        public static string AddAzureVhdCmdletName = "Add-AzureVhd";
        public const string TestAzureNameCmdletName = "Test-AzureName";
        public const string RestartAzureCmdletName = "Restart-AzureVM";
        public const string StopAzureCmdletName = "Stop-AzureVM";
        public const string StartAzureCmdletName = "Start-AzureVM";
        
        public const string NewAzureVMConfigCmdletName = "New-AzureVMConfig";
        public const string AddAzureProvisioningConfigCmdletName = "Add-AzureProvisioningConfig";
        public const string NewAzureVMCmdletName = "New-AzureVM";
        public const string SetAzureDataDiskCmdletName = "Set-AzureDataDisk";
        public const string SetAzureVMSizeCmdletName = "Set-AzureVMSize";
        public static string GetAzureDataDiskCmdletName = "Get-AzureDataDisk";
        public const string GetAzureDiskCmdletName = "Get-AzureDisk";
        public static string GetAzureStorageKeyCmdletName = "Get-AzureStorageKey";

        public const string CopyAzureStorageBlobCmdletName = "Copy-AzureStorageBlob";
        public static string RemoveAzureStorageAccountCmdletName = "Remove-AzureStorageAccount";

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

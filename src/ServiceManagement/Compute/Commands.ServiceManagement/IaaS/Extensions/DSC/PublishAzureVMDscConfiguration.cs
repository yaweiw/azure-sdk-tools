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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions.DSC;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Model;
    using Microsoft.WindowsAzure.Commands.ServiceManagement.Properties;
    using Microsoft.WindowsAzure.Commands.Storage.Model.ResourceModel;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Utilities.Common;

    /// <summary>
    /// Uploads a Desired State Configuration script to Azure blob storage, which 
    /// later can be applied to Azure Virtual Machines using the 
    /// Set-AzureVMDscExtension cmdlet.
    /// </summary>
    [Cmdlet("Publish", "AzureVMDscConfiguration",
        SupportsShouldProcess = true)]
    public class PublishAzureVMDscConfigurationCommand : ServiceManagementBaseCmdlet
    {
        protected const string DefaultContainerName = "windows-powershell-dsc";

        /// <summary>
        /// Path to a file containing one or more configurations; the file can be a 
        /// PowerShell script (*.ps1) or MOF interface (*.mof).
        /// </summary>
        [Parameter(Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Path to a file containing one or more configurations")]
        public string ConfigurationPath { get; set; }

        /// <summary>
        /// Name of the Azure Storage Container the configuration is uploaded to.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true,
            HelpMessage = "Name of the Azure Storage Container the configuration is uploaded to")]
        public string ContainerName { get; set; }

        /// <summary>
        /// By default Publish-AzureVMDscConfiguration will not overwrite any existing blobs. 
        /// Use -Force to overwrite them.
        /// </summary>
        [Parameter(HelpMessage = "By default Publish-AzureVMDscConfiguration will not overwrite any existing blobs")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// The Azure Storage Context that provides the security settings used to upload 
        /// the configuration script to the container specified by ContainerName. This 
        /// context should provide write access to the container.
        ///  If not given, $ENV:azure_storage_connection_string is used instead.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Azure Storage Context that provides the security settings used to upload " +
                          "the configuration script to the container specified by ContainerName")]
        public AzureStorageContext StorageContext { get; set; }

        /// <summary>
        /// Credentials used to access Azure Storage
        /// </summary>
        private StorageCredentials _storageCredentials;

        private const string Ps1FileExtension = ".ps1";
        //private const string MofFileExtension = ".mof";
        private static readonly HashSet<String> AllowedFileExtensions =
            new HashSet<String>(StringComparer.OrdinalIgnoreCase) { Ps1FileExtension, };

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ExecuteCommand();
        }

        internal void ExecuteCommand()
        {
            ValidateParameters();
            PublishConfiguration();
        }

        protected void ValidateParameters()
        {
			// Resolve PowerShell path to a system path.
			ProviderInfo provider;
			this.ConfigurationPath = this.GetResolvedProviderPathFromPSPath(this.ConfigurationPath, out provider).FirstOrDefault();

			// Check that ConfigurationPath points to a valid file
			if (!File.Exists(this.ConfigurationPath))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                    Resources.PublishVMDscExtensionConfigFileNotFound, this.ConfigurationPath));
            }
            if (!AllowedFileExtensions.Contains(GetConfigurationFileExtension()))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                    Resources.PublishVMDscExtensionConfigFileInvalidExtension, this.ConfigurationPath));
            }

            // Ensure we have an storage account
            this._storageCredentials = this.StorageContext != null ? this.StorageContext.StorageAccount.Credentials : GetStorageCredentials();
            if (string.IsNullOrEmpty(this._storageCredentials.AccountName))
            {
                ThrowTerminatingError(
				new ErrorRecord(
					new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Resources.AzureVMDscStorageContextMustIncludeAccountName)),
					string.Empty,
					ErrorCategory.InvalidArgument,
					null));
            }

			if (this.ContainerName == null)
			{
                this.ContainerName = DefaultContainerName;
			}
        }

        private string GetConfigurationFileExtension()
        {
            return Path.GetExtension(this.ConfigurationPath);
        }

        /// <summary>
        /// Publish the configuration and its modules
        /// </summary>
        protected void PublishConfiguration()
        {
            string extenstion = GetConfigurationFileExtension();
            List<string> requiredModules;
            if (String.Equals(extenstion, Ps1FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                WriteVerbose(String.Format(CultureInfo.CurrentCulture, "Parsing configuration script: {0}", this.ConfigurationPath));
                ConfigurationParseResult parseResult = ConfigurationNameHelper.ExtractConfigurationNames(this.ConfigurationPath);
                if (parseResult.Errors.Any())
                {
                    throw new ParseException(String.Format(CultureInfo.CurrentCulture,
                        Resources.PublishVMDscExtensionStorageParserErrors,
                        this.ConfigurationPath,
                        String.Join("\n", parseResult.Errors.Select(error => error.ToString()))));
                }
                requiredModules = parseResult.RequiredModules;
            }
            else
            {
                // TODO: Need parse for MOF files, use empty array for now
                requiredModules = new List<string>();
            }

            // Copy configuration
            CloudBlobContainer cloudBlobContainer = GetStorageContainier();
            CopyConfigurationAndRequiredModules(cloudBlobContainer, requiredModules);
        }

        private static readonly Regex AlphaNumericRegexp = new Regex(@"^[a-zA-Z0-9\s,]*$");
        private static Boolean IsAlphaNumeric(string str)
        {
            return AlphaNumericRegexp.IsMatch(str);
        }

        private void CopyConfigurationAndRequiredModules(CloudBlobContainer cloudBlobContainer, List<string> requiredModules)
        {
            // Create a temporary directory for uploaded zip file
            string tempZipFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempZipFolder);

            // CopyConfiguration
            // TODO: Consider computing a checksum and publish only if there is anything new
            string configurationName = Path.GetFileName(this.ConfigurationPath);
            File.Copy(this.ConfigurationPath, Path.Combine(tempZipFolder, configurationName));

            // CopyRequiredModules
            foreach (var module in requiredModules)
            {
                // Check that module name is alpha-numeric to prevent script-injection.
                // Drop it, in case if it's not.
                if (!IsAlphaNumeric(module))
                {
                    WriteWarning(String.Format(CultureInfo.InvariantCulture, "Module name '{0}' contains illegal characters", module));
                    continue;    
                }
                using (var powershell = System.Management.Automation.PowerShell.Create())
                {
                    powershell.AddScript(
                        @"$mi = Get-Module -List -Name " + module + ";" +
                        @"$moduleFolder = Split-Path -Parent $mi.Path;" +
                        @"Copy-Item -Recurse -Path $moduleFolder -Destination " + tempZipFolder + ";"
                        );
                    powershell.Invoke();
                }
            }
			// Zip the directory
			string packageName = configurationName + ".zip";
			string tempModuleArchive = Path.Combine(Path.GetTempPath(), packageName);
            if (File.Exists(tempModuleArchive))
            {
                File.Delete(tempModuleArchive);
            }
            // azure-sdk-tools uses .net framework 4.0
            // System.IO.Compression.ZipFile was added in .net 4.5
            // Since support for DSC require powershell 4.0+, which require .net 4.5+
            // we assume that created powershell session will have access to System.IO.Compression.FileSystem assembly
            // from version 4.5. We load it to create a zip archive from a directory.
            using (var powershell = System.Management.Automation.PowerShell.Create())
            {
				var script = 
					@"Add-Type -AssemblyName System.IO.Compression.FileSystem > $null;" +
                    @"[void] [System.IO.Compression.ZipFile]::CreateFromDirectory('" + tempZipFolder + "', '" + tempModuleArchive + "');";

                powershell.AddScript(script);
                powershell.Invoke();
            }
			CloudBlockBlob modulesBlob = cloudBlobContainer.GetBlockBlobReference(packageName);
            modulesBlob.UploadFromFile(tempModuleArchive, FileMode.Open);
        }

        private CloudBlobContainer GetStorageContainier()
        {
            var storageAccount = new CloudStorageAccount(this._storageCredentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer containerReference = blobClient.GetContainerReference(this.ContainerName);
            containerReference.CreateIfNotExists();
            return containerReference;
        }
    }
}


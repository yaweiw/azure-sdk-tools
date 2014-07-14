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
	using System.Linq;
	using System.Management.Automation;
    using Model;
    using Properties;
    using Utilities.Common;
	using System.Collections;
	using System.Globalization;
	using System.IO;
	using Newtonsoft.Json;
	using Microsoft.WindowsAzure.Storage;
	using Microsoft.WindowsAzure.Storage.Auth;
	using Microsoft.WindowsAzure.Storage.Blob;
	using Microsoft.WindowsAzure.Commands.Common.Storage;


	[Cmdlet(VerbsCommon.Set, VirtualMachineDscExtensionCmdletNoun),
    OutputType(typeof(IPersistentVM))]
    public class SetAzureVMDscExtensionCommand : VirtualMachineDscExtensionCmdletBase
    {
		/// <summary>
		/// The Extension Reference Name
		/// </summary>
        [Parameter(
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Reference Name")]
        [ValidateNotNullOrEmpty]
        public override string ReferenceName { get; set; }

		/// <summary>
		/// A hashtable specifying the arguments to the ConfigurationFunction. The keys 
		/// correspond to the parameter names and the values to the parameter values.
		/// </summary>
		[Parameter(
			ValueFromPipelineByPropertyName = true,
			HelpMessage = "A hashtable specifying the arguments to the ConfigurationFunction")]
		[ValidateNotNullOrEmpty]
		public Hashtable ConfigurationArgument { get; set; }

		/// <summary>
		/// Path to a .psd1 file that specifies the data for the Configuration. This 
		/// file must contain a hashtable with the items described in 
		/// http://technet.microsoft.com/en-us/library/dn249925.aspx.
		/// </summary>
		[Parameter(
			ValueFromPipelineByPropertyName = true,
			HelpMessage = "Path to a .psd1 file that specifies the data for the Configuration")]
		[ValidateNotNullOrEmpty]
		public string ConfigurationDataPath { get; set; }

		/// <summary>
		/// The name of the configuration file that was previously uploaded by 
		/// Publish-AzureVMDSCConfiguration. This parameter must specify only the name 
		/// of the file, without any path.
		/// </summary>
		[Parameter(
			Mandatory = true,
			Position = 1,
			ValueFromPipelineByPropertyName = true,
			HelpMessage = "The name of the configuration file that was previously uploaded by Publish-AzureVMDSCConfiguration")]
		[ValidateNotNullOrEmpty]
		public string ConfigurationFileName { get; set; }

		/// <summary>
		/// Name of the configuration that will be invoked by the DSC Extension. The value of this parameter should be the name of one of the configurations 
		/// contained within the file specified by ConfigurationFileName.
		/// 
		/// If omitted, this parameter will default to the name of the file given by the ConfigurationFileName parameter, excluding any extension, for example if 
		/// ConfigurationFileName is "SalesWebSite.ps1", the default value for ConfigurationName will be "SalesWebSite".
		/// </summary>
		[Parameter(
			ValueFromPipelineByPropertyName = true,
			HelpMessage = "Name of the configuration that will be invoked by the DSC Extension")]
		[ValidateNotNullOrEmpty]
		public string ConfigurationName { get; set; }

		/// <summary>
		/// Name of the Azure Storage Container where the configuration script is located.
		/// </summary>
		[Parameter(
			ValueFromPipelineByPropertyName = true,
			HelpMessage = " Name of the Azure Storage Container where the configuration script is located")]
		[ValidateNotNullOrEmpty]
		public string ContainerName { get; set; }

		/// <summary>
		/// By default Set-AzureVMDscExtension will not overwrite any existing blobs. Use -Force to overwrite them.
		/// </summary>
		[Parameter(HelpMessage = "Use this parameter to overwrite any existing blobs")]
		public SwitchParameter Force { get; set; }

		/// <summary>
		/// The Azure Storage Context that provides the security settings used to access the configuration script. This context should provide read access to the 
		/// container specified by ContainerName.
		/// </summary>
        [Parameter(
			ValueFromPipelineByPropertyName = true,
			HelpMessage = "The Azure Storage Context that provides the security settings used to access the configuration script")]
		[ValidateNotNullOrEmpty]
		public AzureStorageContext StorageContext { get; set; }

		/// <summary>
		/// The specific version of the DSC extension that Set-AzureVMDSCExtension will 
		/// apply the settings to. If not given, it will default to "1.*"
		/// </summary>
		[Parameter(
			ValueFromPipelineByPropertyName = true,
			HelpMessage = "The version of the DSC extension that Set-AzureVMDSCExtension will apply the settings to")]
		[ValidateNotNullOrEmpty]
		public override string Version { get; set; }

		/// <summary>
		/// The DNS endpoint suffix for all storage services, e.g. "core.windows.net".
		/// </summary>
        [Parameter(
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Storage Endpoint Suffix.")]
        [ValidateNotNullOrEmpty]
        public string StorageEndpointSuffix { get; set; }

		/// <summary>
		/// Credentials used to access Azure Storage
		/// </summary>
		private StorageCredentials _storageCredentials;
		
		protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ExecuteCommand();
        }

		internal void ExecuteCommand()
        {
            ValidateParameters();
			CreateConfiguration();
			RemovePredicateExtensions();
			AddResourceExtension();
            WriteObject(VM);
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();

			//
			// Validate parameters
			//
			if (this.ConfigurationDataPath != null)
			{
				ProviderInfo provider;

				this.ConfigurationDataPath = this.GetResolvedProviderPathFromPSPath(this.ConfigurationDataPath, out provider).FirstOrDefault();

				if (!File.Exists(this.ConfigurationDataPath))
				{
					ThrowArgumentError(Resources.AzureVMDscCannotFindConfigurationDataFile, this.ConfigurationDataPath);
				}
				if (string.Compare(Path.GetExtension(this.ConfigurationDataPath), ".psd1", StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					ThrowArgumentError(Resources.AzureVMDscInvalidConfigurationDataFile);
				}
			}
			if (string.Compare(Path.GetFileName(this.ConfigurationFileName), this.ConfigurationFileName, StringComparison.InvariantCultureIgnoreCase) != 0)
			{
				ThrowArgumentError(Resources.AzureVMDscConfigurationDataFileShouldNotIncludePath);
			}

            this._storageCredentials = this.StorageContext != null ? this.StorageContext.StorageAccount.Credentials : GetStorageCredentials();
            
	        if (string.IsNullOrEmpty(this._storageCredentials.AccountName))
	        {
		        ThrowArgumentError(Resources.AzureVMDscStorageContextMustIncludeAccountName);
	        }

			//
			// Set defaults for parameters that were not provided by the caller
			//
	        if (this.ConfigurationName == null)
	        {
		        this.ConfigurationName = Path.GetFileNameWithoutExtension(this.ConfigurationFileName);
	        }

			if (this.ContainerName == null)
			{
				this.ContainerName = DefaultContainerName;
			}

			if (this.Version == null)
			{
				this.Version = DefaultExtensionVersion;
			}

			if (this.ReferenceName == null)
			{
				this.ReferenceName = ExtensionPublishedName;
			}
		}

		private void CreateConfiguration()
		{
			//
			// Get a reference to the container in blob storage
			//
			var storageAccount = string.IsNullOrEmpty(this.StorageEndpointSuffix)
							   ? new CloudStorageAccount(this._storageCredentials, true)
							   : new CloudStorageAccount(this._storageCredentials, this.StorageEndpointSuffix, true);

			var blobClient = storageAccount.CreateCloudBlobClient();

			var containerReference = blobClient.GetContainerReference(this.ContainerName);
			
			//
			// Get a reference to the configuration blob and create a SAS token to access it
			//
			var blobAccessPolicy = new SharedAccessBlobPolicy()
			{
				SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
				Permissions = SharedAccessBlobPermissions.Read
			};

			var configurationBlobName = this.ConfigurationFileName + ".zip";

			var configurationBlobReference = containerReference.GetBlockBlobReference(configurationBlobName);

            var configurationBlobSasToken = configurationBlobReference.GetSharedAccessSignature(blobAccessPolicy);

			//
			// Upload the configuration data to blob storage and get a SAS token
			//
			string configurationDataBlobUri = null;

			if (this.ConfigurationDataPath != null)
			{
				var guid = Guid.NewGuid(); // there may be multiple VMs using the same configuration

				var configurationDataBlobName = string.Format(CultureInfo.InvariantCulture, "{0}-{1}.psd1", this.ConfigurationName, guid);

				var configurationDataBlobReference = containerReference.GetBlockBlobReference(configurationDataBlobName);

				configurationDataBlobReference.UploadFromFile(this.ConfigurationDataPath, FileMode.Open);

				var configurationDataBlobSasToken = configurationDataBlobReference.GetSharedAccessSignature(blobAccessPolicy);

				configurationDataBlobUri = configurationDataBlobReference.StorageUri.PrimaryUri.AbsoluteUri + configurationDataBlobSasToken;
			}

			//
			// Define the public and private property bags that will be passed to the extension.
			//
			this.PublicConfiguration = JsonUtilities.TryFormatJson(
				JsonConvert.SerializeObject(
				   new DscPublicSettings()
				   {
					   SasToken                       = configurationBlobSasToken,
					   ModulesUrl                     = configurationBlobReference.StorageUri.PrimaryUri.AbsoluteUri,
					   ConfigurationFunction          = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", this.ConfigurationFileName, this.ConfigurationName),
					   DeploymentMetadataSectionOrUrl = null, // this will be used by Visual Studio
					   Properties                     = this.ConfigurationArgument,
				   }));

			this.PrivateConfiguration = JsonUtilities.TryFormatJson(
				JsonConvert.SerializeObject(
				   new DscPrivateSettings()
				   {
					   DataBlobUri = configurationDataBlobUri
				   }));
		}
    }
}

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
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Management.Automation;
    using Model;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Storage;

    [Cmdlet(
        VerbsCommon.Set,
        VirtualMachineDiagnosticsExtensionNoun,
        DefaultParameterSetName = SetExtParamSetName),
    OutputType(
        typeof(IPersistentVM))]
    public class SetAzureVMDiagnosticsExtensionCommand : VirtualMachineDiagnosticsExtensionCmdletBase
    {
        protected const string SetExtParamSetName = "SetDiagnosticsExtension";
        protected const string SetExtRefParamSetName = "SetDiagnosticsWithReferenceExtension";

        [Parameter(
            ParameterSetName = SetExtParamSetName,
            Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = false,
            HelpMessage = "XML Diagnostics Configuration")]
        [Parameter(
            ParameterSetName = SetExtRefParamSetName,
            Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = false,
            HelpMessage = "XML Diagnostics Configuration")]
        [ValidateNotNullOrEmpty]
        public string DiagnosticsConfigurationPath
        {
            get;
            set;
        }

        [Parameter(ParameterSetName = SetExtParamSetName,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            Mandatory = true,
            HelpMessage = "Diagnostics Storage Account Name")]
        [Parameter(ParameterSetName = SetExtRefParamSetName,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            Mandatory = true,
            HelpMessage = "Diagnostics Storage Account Name")]
        [ValidateNotNullOrEmpty]
        public override string StorageAccountName
        {
            get;
            set;
        }

        [Parameter(
            ParameterSetName = SetExtParamSetName,
            Position = 2,
            ValueFromPipelineByPropertyName = false,
            HelpMessage = "Local directory where to store the agent logs")]
        [Parameter(
           ParameterSetName = SetExtRefParamSetName,
           Position = 2,
           ValueFromPipelineByPropertyName = false,
           HelpMessage = "Local directory where to store the agent logs")]
        public string LocalDirectory { get; set; }

        [Parameter(
        ParameterSetName = SetExtParamSetName,
        Position = 3,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "WAD Version")]
        [Parameter(
        ParameterSetName = SetExtRefParamSetName,
        Position = 3,
        ValueFromPipelineByPropertyName = false,
        HelpMessage = "WAD Version")]
        public override string Version { get; set; }

        [Parameter(
            ParameterSetName = SetExtParamSetName,
            Position = 4,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "To Set the Extension State to 'Disable'.")]
        [Parameter(
            ParameterSetName = SetExtRefParamSetName,
            Position = 4,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "To Set the Extension State to 'Disable'.")]
        public override SwitchParameter Disable { get; set; }

        [Parameter(
            ParameterSetName = SetExtRefParamSetName,
            Position = 5,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "To specify the reference name.")]
        public override string ReferenceName { get; set; }

        internal void ExecuteCommand()
        {
            ValidateParameters();
            RemovePredicateExtensions();
            AddResourceExtension();
            WriteObject(VM);
            UpdateAzureVMCommand cmd = new UpdateAzureVMCommand();
        }

        protected override void ValidateParameters()
        {     
            base.ValidateParameters();
            ValidateStorageAccount();
            ValidateConfiguration();
            ExtensionName = DiagnosticsExtensionType;
            Publisher = DiagnosticsExtensionNamespace;
        }

        private void ValidateStorageAccount()
        {
            StorageKey = GetStorageKey();
            // We need the suffix, NOT the full account endpoint.
            Endpoint = "https://" +  WindowsAzureProfile.Instance.CurrentEnvironment.StorageEndpointSuffix;
        }

        private void ValidateConfiguration()
        {
            // Public configuration must look like:
            // { "xmlCfg":"base-64 encoded string", "StorageAccount":"account_name", "localResourceDirectory":{ "path":"some_path", "expandResourceDirectory":<true|false> }}
            //
            // localResourceDirectory is optional
            using (StreamReader sr = new StreamReader(DiagnosticsConfigurationPath))
            {
                string config = string.Format("<WadCfg>{0}</WadCfg>", sr.ReadToEnd());
                config = Convert.ToBase64String(Encoding.UTF8.GetBytes(config.ToCharArray()));
                PublicConfiguration = "{ \"xmlCfg\":\"" + config + "\", \"StorageAccount\":\"" + StorageAccountName + "\"";

                if (!string.IsNullOrEmpty(LocalDirectory))
                {
                    PublicConfiguration += ", \"localResourceDirectory\":{ \"path\":\"" + LocalDirectory + "\", \"expandResourceDirectory\":false}";
                }

                PublicConfiguration += "}";   
            }

            // Private configuration must look like:
            // { "storageAccountName":"your_account_name", "storageAccountKey":"your_key", "storageAccountEndPoint":"end_point" }
            PrivateConfiguration =  "{ \"storageAccountName\":\"" + StorageAccountName + 
                                    "\", \"storageAccountKey\":\"" + StorageKey + 
                                    "\", \"storageAccountEndPoint\":\"" + Endpoint + "\"}";
        }

        protected string GetStorageKey()
        {
            string storageKey = string.Empty;

            if (!string.IsNullOrEmpty(StorageAccountName))
            {
                var storageAccount = this.StorageClient.StorageAccounts.Get(StorageAccountName);
                if (storageAccount != null)
                {
                    var keys = this.StorageClient.StorageAccounts.GetKeys(StorageAccountName);
                    if (keys != null)
                    {
                        storageKey = !string.IsNullOrEmpty(keys.PrimaryKey) ? keys.PrimaryKey : keys.SecondaryKey;
                    }
                }
            }

            return storageKey;
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ExecuteCommand();
        }
    }
}
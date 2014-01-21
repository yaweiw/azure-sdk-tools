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
    using System.Xml;
    using Model;
    using Model.PersistentVMModel;
    using Properties;
    using Utilities.Common;

    [Cmdlet(
        VerbsCommon.Add,
        VirtualMachineDiagnosticsExtensionNoun,
        DefaultParameterSetName = EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext),
    OutputType(
        typeof(IPersistentVM))]
    public class AddAzureVMDiagnosticsExtensionCommand : VirtualMachineDiagnosticsExtensionCmdletBase
    {
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSet, HelpMessage = "Diagnostics Configuration")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext, HelpMessage = "Diagnostics Configuration")]
        [ValidateNotNullOrEmpty]
        public override XmlDocument DiagnosticsConfiguration
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSet, HelpMessage = "Diagnostics Configuration File")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext, HelpMessage = "Diagnostics Configuration File")]
        [ValidateNotNullOrEmpty]
        public override string DiagnosticsConfigurationFile
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext, HelpMessage = "Storage Account Context")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext, HelpMessage = "Storage Account Context")]
        [ValidateNotNullOrEmpty]
        public override StorageServicePropertiesOperationContext StorageAccountContext
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSet, HelpMessage = "Storage Account Name")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSet, HelpMessage = "Storage Account Name")]
        [ValidateNotNullOrEmpty]
        public override string StorageAccountName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSet, HelpMessage = "Storage Account Key")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSet, HelpMessage = "Storage Account Key")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSetUsingStorageContext, HelpMessage = "Storage Account Key")]
        [Parameter(Mandatory = true, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSetUsingStorageContext, HelpMessage = "Storage Account Key")]
        [ValidateNotNullOrEmpty]
        public override string StorageAccountKey
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = DisableExtensionParameterSet, HelpMessage = "Disable Diagnostics Extension")]
        public override SwitchParameter Disable
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSet, HelpMessage = "Endpoint Uri Suffix for storage services, e.g. \"core.windows.net\".")]
        [Parameter(Mandatory = false, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSet, HelpMessage = "Endpoint Uri Suffix for storage services, e.g. \"core.windows.net\".")]
        public override string EndpointSuffix
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = EnableExtensionUsingXmlDocumentParameterSet, HelpMessage = "Blob/Queue/Table Endpoint Uri for storage services, e.g. {\"http://foo.blob.core.windows.net\", \"http://foo.queue.core.windows.net\", \"http://foo.table.core.windows.net\"}.")]
        [Parameter(Mandatory = false, ParameterSetName = EnableExtensionUsingXmlFilePathParameterSet, HelpMessage = "Blob/Queue/Table Endpoint Uri for storage services, e.g. {\"http://foo.blob.core.windows.net\", \"http://foo.queue.core.windows.net\", \"http://foo.table.core.windows.net\"}.")]
        [ValidateNotNullOrEmpty]
        public override Uri[] Endpoints
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            ValidateParameters();
            this.PublicConfiguration = GetDiagnosticsAgentConfig();
            AddResourceExtension();
            WriteObject(VM);
        }

        protected override void ProcessRecord()
        {
            ServiceManagementProfile.Initialize();
            try
            {
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}

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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Set Windows Azure Service Diagnostics Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureServiceDiagnosticsExtension"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureServiceDiagnosticsExtensionCommand : BaseAzureServiceDiagnosticsExtensionCmdlet
    {
        public SetAzureServiceDiagnosticsExtensionCommand()
            : base()
        {
        }

        public SetAzureServiceDiagnosticsExtensionCommand(IServiceManagement channel)
            : base(channel)
        {
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Cloud Service Name")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Production (default) or Staging.")]
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Production (default) or Staging.")]
        [ValidateSet(DeploymentSlotType.Production, DeploymentSlotType.Staging, IgnoreCase = true)]
        public override string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [ValidateNotNullOrEmpty]
        public override string[] Roles
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "X509Certificate used to encrypt password.")]
        [ValidateNotNullOrEmpty]
        public override X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Thumbprint of a certificate used for encryption.")]
        [ValidateNotNullOrEmpty]
        public override string Thumbprint
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "ThumbprintAlgorithm associated with the Thumbprint.")]
        [ValidateNotNullOrEmpty]
        public override string ThumbprintAlgorithm
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = true, ParameterSetName = "NewExtension", HelpMessage = "Diagnostics ConnectionQualifiers")]
        [Parameter(Position = 5, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Diagnostics ConnectionQualifiers")]
        [ValidateNotNullOrEmpty]
        public string ConnectionQualifiers
        {
            get;
            set;
        }

        [Parameter(Position = 5, Mandatory = true, ParameterSetName = "NewExtension", HelpMessage = "Diagnostics DefaultEndpointsProtocol")]
        [Parameter(Position = 6, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Diagnostics DefaultEndpointsProtocol")]
        [ValidateNotNullOrEmpty]
        public string DefaultEndpointsProtocol
        {
            get;
            set;
        }

        [Parameter(Position = 6, Mandatory = true, ParameterSetName = "NewExtension", HelpMessage = "Diagnostics Storage Name")]
        [Parameter(Position = 7, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Diagnostics Storage Name")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Position = 7, Mandatory = true, ParameterSetName = "NewExtension", HelpMessage = "Diagnostics WadCfg")]
        [Parameter(Position = 8, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Diagnostics WadCfg")]
        [ValidateNotNullOrEmpty]
        public string WadCfg
        {
            get;
            set;
        }

        [Parameter(Position = 9, Mandatory = true, ParameterSetName = "NewExtension", HelpMessage = "Diagnostics StorageKey")]
        [Parameter(Position = 10, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Diagnostics StorageKey")]
        [ValidateNotNullOrEmpty]
        public string StorageKey
        {
            get;
            set;
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            ValidateDeployment();
            ValidateRoles();
            ValidateThumbprint();
        }

        public void ExecuteCommand()
        {
            ValidateParameters();
            ExtensionConfiguration extConfig = ExtensionManager.NewExtensionConfig(Deployment.ExtensionConfiguration);
            ExtensionConfigurationContext configContext = new ExtensionConfigurationContext
            {
                ProviderNameSpace = ExtensionNameSpace,
                Type = ExtensionType,
                Thumbprint = Thumbprint,
                ThumbprintAlgorithm = ThumbprintAlgorithm,
                X509Certificate = X509Certificate,
                PublicConfiguration = string.Format(PublicConfigurationXmlTemplate.ToString(), ConnectionQualifiers, DefaultEndpointsProtocol, Name, WadCfg),
                PrivateConfiguration = string.Format(PrivateConfigurationXmlTemplate.ToString(), StorageKey),
                Roles = Roles != null ? Roles.Select(r => new ExtensionRole(r)).ToList() : new List<ExtensionRole>(new ExtensionRole[] { new ExtensionRole() }),
            };
            ExtensionManager.InstallExtension(configContext, Slot, ref extConfig);
            ChangeDeployment(extConfig);
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}

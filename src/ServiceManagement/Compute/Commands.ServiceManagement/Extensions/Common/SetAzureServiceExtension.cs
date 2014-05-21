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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.Extensions
{
    using Model.PersistentVMModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using Utilities.Common;

    /// <summary>
    /// Set Windows Azure Service Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureServiceExtension", DefaultParameterSetName = "SetExtension"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureServiceExtensionCommand : BaseAzureServiceExtensionCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtension", HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Cloud Service Name")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtension", HelpMessage = "Production (default) or Staging.")]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Production (default) or Staging.")]
        [ValidateSet(DeploymentSlotType.Production, DeploymentSlotType.Staging, IgnoreCase = true)]
        public override string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtension", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [ValidateNotNullOrEmpty]
        public override string[] Role
        {
            get;
            set;
        }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtension", HelpMessage = "X509Certificate used to encrypt password.")]
        [ValidateNotNullOrEmpty]
        public override X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Thumbprint of a certificate used for encryption.")]
        [ValidateNotNullOrEmpty]
        public override string CertificateThumbprint
        {
            get;
            set;
        }

        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtension", HelpMessage = "Algorithm associated with the Thumbprint.")]
        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Algorithm associated with the Thumbprint.")]
        [ValidateNotNullOrEmpty]
        public override string ThumbprintAlgorithm
        {
            get;
            set;
        }

        [Parameter(Position = 5, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtension", HelpMessage = "Extension Name")]
        [Parameter(Position = 5, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Extension Name")]
        [ValidateNotNullOrEmptyAttribute]
        public override string ExtensionName
        {
            get;
            set;
        }

        [Parameter(Position = 6, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtension", HelpMessage = "Extension Provider Namespace")]
        [Parameter(Position = 6, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Extension Provider Namespace")]
        [ValidateNotNullOrEmptyAttribute]
        public override string ProviderNamespace
        {
            get;
            set;
        }

        [Parameter(Position = 7, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtension", HelpMessage = "Extension Public Configuration.")]
        [Parameter(Position = 7, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Extension Public Configuration.")]
        [ValidateNotNullOrEmpty]
        public override string PublicConfiguration
        {
            get;
            set;
        }


        [Parameter(Position = 8, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtension", HelpMessage = "Extension Private Configuration.")]
        [Parameter(Position = 8, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Extension Private Configuration.")]
        [ValidateNotNullOrEmpty]
        public override string PrivateConfiguration
        {
            get;
            set;
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            ValidateService();
            ValidateDeployment();
            ValidateRoles();
            ValidateThumbprint(true);
            ValidateConfiguration();
        }

        public void ExecuteCommand()
        {
            ValidateParameters();
            ExtensionConfigurationInput context = new ExtensionConfigurationInput
            {
                ProviderNameSpace = ProviderNamespace,
                Type = ExtensionName,
                CertificateThumbprint = CertificateThumbprint,
                ThumbprintAlgorithm = ThumbprintAlgorithm,
                X509Certificate = X509Certificate,
                PublicConfiguration = PublicConfiguration,
                PrivateConfiguration = PrivateConfiguration,
                Roles = new ExtensionRoleList(Role != null && Role.Any() ? Role.Select(r => new ExtensionRole(r)) : Enumerable.Repeat(new ExtensionRole(), 1))
            };
            var extConfig = ExtensionManager.InstallExtension(context, Slot, Deployment.ExtensionConfiguration);
            ChangeDeployment(extConfig);
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}

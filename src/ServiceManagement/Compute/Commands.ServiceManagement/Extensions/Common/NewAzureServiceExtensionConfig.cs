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
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// New Microsoft Azure Service Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureServiceExtensionConfig", DefaultParameterSetName = "NewExtension"), OutputType(typeof(ExtensionConfigurationInput))]
    public class NewAzureServiceExtensionConfigCommand : BaseAzureServiceExtensionCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtension", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [ValidateNotNullOrEmpty]
        public override string[] Role
        {
            get;
            set;
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtension", HelpMessage = "X509Certificate used to encrypt password.")]
        [ValidateNotNullOrEmpty]
        public override X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Thumbprint of a certificate used for encryption.")]
        [ValidateNotNullOrEmpty]
        public override string CertificateThumbprint
        {
            get;
            set;
        }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtension", HelpMessage = "Algorithm associated with the Thumbprint.")]
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Algorithm associated with the Thumbprint.")]
        [ValidateNotNullOrEmpty]
        public override string ThumbprintAlgorithm
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtension", HelpMessage = "Extension Name")]
        [Parameter(Position = 3, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Extension Name")]
        [ValidateNotNullOrEmptyAttribute]
        public override string ExtensionName
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtension", HelpMessage = "Extension Provider Namespace")]
        [Parameter(Position = 4, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Extension Provider Namespace")]
        [ValidateNotNullOrEmptyAttribute]
        public override string ProviderNamespace
        {
            get;
            set;
        }

        [Parameter(Position = 5, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtension", HelpMessage = "Extension Public Configuration.")]
        [Parameter(Position = 5, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Extension Public Configuration.")]
        [ValidateNotNullOrEmpty]
        public override string PublicConfiguration
        {
            get;
            set;
        }


        [Parameter(Position = 6, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtension", HelpMessage = "Extension Private Configuration.")]
        [Parameter(Position = 6, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Extension Private Configuration.")]
        [ValidateNotNullOrEmpty]
        public override string PrivateConfiguration
        {
            get;
            set;
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            ValidateThumbprint(false);
            ValidateConfiguration();
        }

        public void ExecuteCommand()
        {
            ValidateParameters();
            WriteObject(new ExtensionConfigurationInput
            {
                CertificateThumbprint = CertificateThumbprint,
                ThumbprintAlgorithm = ThumbprintAlgorithm,
                ProviderNameSpace = ProviderNamespace,
                Type = ExtensionName,
                PublicConfiguration = PublicConfiguration,
                PrivateConfiguration = PrivateConfiguration,
                X509Certificate = X509Certificate,
                Roles = new ExtensionRoleList(Role != null && Role.Any() ? Role.Select(r => new ExtensionRole(r)) : Enumerable.Repeat(new ExtensionRole(), 1))
            });
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}

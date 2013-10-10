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
    using System;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using ADDomain;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Set Windows Azure Service AD Domain Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, ADDomainExtensionNoun, DefaultParameterSetName = DomainParameterSet), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureServiceADDomainExtensionCommand : BaseAzureServiceADDomainExtensionCmdlet
    {
        public SetAzureServiceADDomainExtensionCommand()
            : base()
        {
        }

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet, HelpMessage = ServiceNameHelpMessage)]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet, HelpMessage = ServiceNameHelpMessage)]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupParameterSet, HelpMessage = ServiceNameHelpMessage)]
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupThumbprintParameterSet, HelpMessage = ServiceNameHelpMessage)]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet, HelpMessage = SlotHelpMessage)]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet, HelpMessage = SlotHelpMessage)]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupParameterSet, HelpMessage = SlotHelpMessage)]
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupThumbprintParameterSet, HelpMessage = SlotHelpMessage)]
        [ValidateSet(DeploymentSlotType.Production, DeploymentSlotType.Staging, IgnoreCase = true)]
        public override string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet, HelpMessage = RoleHelpMessage)]
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet, HelpMessage = RoleHelpMessage)]
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupParameterSet, HelpMessage = RoleHelpMessage)]
        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupThumbprintParameterSet, HelpMessage = RoleHelpMessage)]
        [ValidateNotNullOrEmpty]
        public override string[] Role
        {
            get;
            set;
        }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet, HelpMessage = X509CertificateHelpMessage)]
        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupParameterSet, HelpMessage = X509CertificateHelpMessage)]
        [ValidateNotNullOrEmpty]
        public override X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = DomainThumbprintParameterSet, HelpMessage = CertificateThumbprintHelpMessage)]
        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = WorkgroupThumbprintParameterSet, HelpMessage = CertificateThumbprintHelpMessage)]
        [ValidateNotNullOrEmpty]
        public override string CertificateThumbprint
        {
            get;
            set;
        }

        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet, HelpMessage = ThumbprintAlgorithmHelpMessage)]
        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet, HelpMessage = ThumbprintAlgorithmHelpMessage)]
        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupParameterSet, HelpMessage = ThumbprintAlgorithmHelpMessage)]
        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupThumbprintParameterSet, HelpMessage = ThumbprintAlgorithmHelpMessage)]
        [ValidateNotNullOrEmpty]
        public override string ThumbprintAlgorithm
        {
            get;
            set;
        }

        [Parameter(Position = 5, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = DomainParameterSet)]
        [Parameter(Position = 5, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = DomainThumbprintParameterSet)]
        [ValidateNotNullOrEmpty]
        public override string DomainName
        {
            get
            {
                return base.DomainName;
            }
            set
            {
                base.DomainName = value;
            }
        }

        [Parameter(Position = 5, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = WorkgroupParameterSet)]
        [Parameter(Position = 5, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = WorkgroupThumbprintParameterSet)]
        [ValidateNotNullOrEmpty]
        public override string WorkgroupName
        {
            get
            {
                return base.WorkgroupName;
            }
            set
            {
                base.WorkgroupName = value;
            }
        }

        [Parameter(Position = 6, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet)]
        [Parameter(Position = 6, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet)]
        [Parameter(Position = 6, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupParameterSet)]
        [Parameter(Position = 6, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupThumbprintParameterSet)]
        public override SwitchParameter Restart
        {
            get
            {
                return base.Restart;
            }
            set
            {
                base.Restart = value;
            }
        }

        [Parameter(Position = 7, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet)]
        [Parameter(Position = 7, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet)]
        [Parameter(Position = 7, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupParameterSet)]
        [Parameter(Position = 7, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = WorkgroupThumbprintParameterSet)]
        [ValidateNotNullOrEmpty]
        public override PSCredential Credential
        {
            get
            {
                return base.Credential;
            }
            set
            {
                base.Credential = value;
            }
        }

        [Parameter(Position = 8, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet)]
        [Parameter(Position = 8, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet)]
        [ValidateNotNullOrEmpty]
        public override PSCredential UnjoinDomainCredential
        {
            get
            {
                return base.UnjoinDomainCredential;
            }
            set
            {
                base.UnjoinDomainCredential = value;
            }
        }

        [Parameter(Position = 9, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet)]
        [Parameter(Position = 9, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet)]
        [ValidateNotNullOrEmpty]
        public override JoinOptions Options
        {
            get
            {
                return base.Options;
            }
            set
            {
                base.Options = value;
            }
        }

        [Parameter(Position = 10, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet)]
        [Parameter(Position = 10, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet)]
        [ValidateNotNullOrEmpty]
        public override uint AdditionalOptions
        {
            set
            {
                base.AdditionalOptions = value;
            }
        }

        [Parameter(Position = 11, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainParameterSet)]
        [Parameter(Position = 11, ValueFromPipelineByPropertyName = true, Mandatory = false, ParameterSetName = DomainThumbprintParameterSet)]
        [ValidateNotNullOrEmpty]
        public override string OUPath
        {
            get
            {
                return base.OUPath;
            }
            set
            {
                base.OUPath = value;
            }
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
                ProviderNameSpace = ExtensionNameSpace,
                Type = ExtensionType,
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

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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
    using System.Management.Automation;
    using Samples.WindowsAzure.ServiceManagement;
    
    public class ProvisioningConfigurationCmdletBase : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "Linux", HelpMessage = "Set configuration to Linux.")]
        public SwitchParameter Linux
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "Linux", HelpMessage = "User to Create")]
        [ValidateNotNullOrEmpty]
        public string LinuxUser
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Linux", HelpMessage = "Disable SSH Password Authentication.")]
        public SwitchParameter DisableSSH
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Linux", HelpMessage = "Do not create an SSH Endpoint.")]
        public SwitchParameter NoSSHEndpoint
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Linux", HelpMessage = "SSH Public Key List")]
        public LinuxProvisioningConfigurationSet.SSHPublicKeyList SSHPublicKeys
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Linux", HelpMessage = "SSH Key Pairs")]
        public LinuxProvisioningConfigurationSet.SSHKeyPairList SSHKeyPairs
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "Windows", HelpMessage = "Set configuration to Windows.")]
        public SwitchParameter Windows
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "WindowsDomain", HelpMessage = "Set configuration to Windows with Domain Join.")]
        public SwitchParameter WindowsDomain
        {
            get;
            set;
        }
        
        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Administrator password to use for the role.")]
        [Parameter(Mandatory = false, ParameterSetName = "WindowsDomain", HelpMessage = "Administrator password to use for the role.")]
        [Parameter(Mandatory = true, ParameterSetName = "Linux", HelpMessage = "Default password for linux user created.")]
        [ValidateNotNullOrEmpty]
        public string Password
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Specify to force the user to change the password on first logon.")]
        [Parameter(Mandatory = false, ParameterSetName = "WindowsDomain", HelpMessage = "Specify to force the user to change the password on first logon.")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter ResetPasswordOnFirstLogon
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Disable Automatic Updates.")]
        [Parameter(Mandatory = false, ParameterSetName = "WindowsDomain", HelpMessage = "Disable Automatic Updates.")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter DisableAutomaticUpdates
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Do No Create an RDP Endpoint.")]
        [Parameter(Mandatory = false, ParameterSetName = "WindowsDomain", HelpMessage = "Do Not Create an RDP Endpoint.")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter NoRDPEndpoint
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Specify the time zone for the virtual machine.")]
        [Parameter(Mandatory = false, ParameterSetName = "WindowsDomain", HelpMessage = "Specify the time zone for the virtual machine.")]
        [ValidateNotNullOrEmpty]
        public string TimeZone
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Set of certificates to install in the VM.")]
        [Parameter(Mandatory = false, ParameterSetName = "WindowsDomain", HelpMessage = "Set of certificates to install in the VM.")]
        [ValidateNotNullOrEmpty]
        public CertificateSettingList Certificates
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "WindowsDomain", HelpMessage = "Domain to join (FQDN).")]
        [ValidateNotNullOrEmpty]
        public string JoinDomain
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "WindowsDomain", HelpMessage = "Domain name.")]
        [ValidateNotNullOrEmpty]
        public string Domain
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "WindowsDomain", HelpMessage = "Domain user name.")]
        [ValidateNotNullOrEmpty]
        public string DomainUserName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "WindowsDomain", HelpMessage = "Domain password.")]
        [ValidateNotNullOrEmpty]
        public string DomainPassword
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = "WindowsDomain", HelpMessage = "Machine object organization unit.")]
        [ValidateNotNullOrEmpty]
        public string MachineObjectOU
        {
            get;
            set;
        }

        protected void SetProvisioningConfiguration(LinuxProvisioningConfigurationSet provisioningConfiguration)
        {
            provisioningConfiguration.UserName = LinuxUser;
            provisioningConfiguration.UserPassword = Password;

            if (DisableSSH.IsPresent)
            {
                provisioningConfiguration.DisableSshPasswordAuthentication = true;
            }
            else
            {
                provisioningConfiguration.DisableSshPasswordAuthentication = false;
            }

            if (SSHKeyPairs != null && SSHKeyPairs.Count > 0 || SSHPublicKeys != null && SSHPublicKeys.Count > 0)
            {
                provisioningConfiguration.SSH = new LinuxProvisioningConfigurationSet.SSHSettings { PublicKeys = SSHPublicKeys, KeyPairs = SSHKeyPairs };
            }
        }

        protected void SetProvisioningConfiguration(WindowsProvisioningConfigurationSet provisioningConfiguration)
        {            
            provisioningConfiguration.AdminPassword = Password;            
            provisioningConfiguration.ResetPasswordOnFirstLogon = ResetPasswordOnFirstLogon.IsPresent;
            provisioningConfiguration.StoredCertificateSettings = Certificates;
            provisioningConfiguration.EnableAutomaticUpdates = !DisableAutomaticUpdates.IsPresent;

            if (!string.IsNullOrEmpty(TimeZone))
            {
                provisioningConfiguration.TimeZone = TimeZone;
            }

            if (ParameterSetName == "WindowsDomain")
            {
                provisioningConfiguration.DomainJoin = new WindowsProvisioningConfigurationSet.DomainJoinSettings
                {
                    Credentials = new WindowsProvisioningConfigurationSet.DomainJoinCredentials
                    {
                        Username = DomainUserName,
                        Password = DomainPassword,
                        Domain = Domain
                    },
                    MachineObjectOU = MachineObjectOU,
                    JoinDomain = JoinDomain
                };
            }
        }
    }
}

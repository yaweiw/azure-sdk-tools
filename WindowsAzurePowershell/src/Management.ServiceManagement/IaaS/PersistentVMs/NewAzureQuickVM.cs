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

using System.Net;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.PersistentVMs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.ServiceModel;
    using Management.Model;
    using Common;
    using IaaS;
    using Extensions;
    using Storage;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Creates a VM without advanced provisioning configuration options
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureQuickVM", DefaultParameterSetName = "Windows"), OutputType(typeof(ManagementOperationContext))]
    public class NewQuickVM : IaaSDeploymentManagementCmdletBase
    {
        private bool _createdDeployment = false;

        public NewQuickVM()
        {

        }

        public NewQuickVM(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Mandatory = true, ParameterSetName = "Windows", HelpMessage = "Create a Windows VM")]
        public SwitchParameter Windows
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = "Linux", HelpMessage = "Create a Linux VM")]
        public SwitchParameter Linux
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Service Name")]
        [ValidateNotNullOrEmpty]
        override public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Virtual Machine Name")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Reference to a platform stock image or a user image from the image repository.")]
        [ValidateNotNullOrEmpty]
        public string ImageName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, HelpMessage = "Administrator password to use for the role.")]
        [ValidateNotNullOrEmpty]
        public string Password
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Use when creating the first virtual machine in a cloud service (or specify affinity group).  The data center region where the cloud service will be created.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Use when creating the first virtual machine in a cloud service (or specify location). The name of an existing affinity group associated with this subscription.")]
        [ValidateNotNullOrEmpty]
        public string AffinityGroup
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

        [Parameter(Mandatory = false, ParameterSetName = "Windows", HelpMessage = "Set of certificates to install in the VM.")]
        [ValidateNotNullOrEmpty]
        public CertificateSettingList Certificates
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

        [Parameter(HelpMessage = "Virtual network name.")]
        public string VNetName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "The list of subnet names.")]
        [AllowEmptyCollection]
        [AllowNull]
        public string[] SubnetNames
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "DNS Settings for Deployment.")]
        [ValidateNotNullOrEmpty]
        public DnsServer[] DnsSettings
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Controls the platform caching behavior of the OS disk.")]
        [ValidateSet("ReadWrite", "ReadOnly", IgnoreCase = true)]
        public String HostCaching
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "The name of the availability set.")]
        [ValidateNotNullOrEmpty]
        public string AvailabilitySetName
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Represents the size of the machine.")]
        [ValidateSet("ExtraSmall", "Small", "Medium", "Large", "ExtraLarge", IgnoreCase = true)]
        [ValidateNotNullOrEmpty]
        public string InstanceSize
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Location of the where the VHD should be created. This link refers to a blob in a storage account. If not specified the VHD will be created in the current storage account in the vhds container.")]
        [ValidateNotNullOrEmpty]
        public string MediaLocation
        {
            get;
            set;
        }

        public void NewAzureVMProcess()
        {
            SubscriptionData currentSubscription = this.GetCurrentSubscription();
            CloudStorageAccount currentStorage = null;
            try
            {
                currentStorage = CloudStorageAccountFactory.GetCurrentCloudStorageAccount(Channel, currentSubscription);
            }
            catch (ServiceManagementClientException) // couldn't access
            {
                throw new ArgumentException("CurrentStorageAccount is not accessible. Ensure the current storage account is accessible and in the same location or affinity group as your cloud service.");
            }
            if (currentStorage == null) // not set
            {
                throw new ArgumentException("CurrentStorageAccount is not set. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");
            }

            var vm = new PersistentVMRole
            {
                AvailabilitySetName = AvailabilitySetName,
                ConfigurationSets = new Collection<ConfigurationSet>(),
                DataVirtualHardDisks = new Collection<DataVirtualHardDisk>(),
                RoleName = String.IsNullOrEmpty(Name) ? ServiceName : Name, // default like the portal
                RoleSize = String.IsNullOrEmpty(InstanceSize) ? null : InstanceSize,
                RoleType = "PersistentVMRole",
                Label = ServiceManagementHelper.EncodeToBase64String(ServiceName)
            };

            vm.OSVirtualHardDisk = new OSVirtualHardDisk()
            {
                DiskName = null,
                SourceImageName = ImageName,
                MediaLink = string.IsNullOrEmpty(MediaLocation) ? null : new Uri(MediaLocation),
                HostCaching = HostCaching
            };

            if (vm.OSVirtualHardDisk.MediaLink == null && String.IsNullOrEmpty(vm.OSVirtualHardDisk.DiskName))
            {
                DateTime dtCreated = DateTime.Now;
                string vhdname = String.Format("{0}-{1}-{2}-{3}-{4}-{5}.vhd", this.ServiceName, vm.RoleName, dtCreated.Year, dtCreated.Month, dtCreated.Day, dtCreated.Millisecond);
                string blobEndpoint = currentStorage.BlobEndpoint.AbsoluteUri;
                if (blobEndpoint.EndsWith("/") == false)
                    blobEndpoint += "/";
                vm.OSVirtualHardDisk.MediaLink = new Uri(blobEndpoint + "vhds/" + vhdname);
            }

            NetworkConfigurationSet netConfig = new NetworkConfigurationSet();
            netConfig.InputEndpoints = new Collection<InputEndpoint>();
            if (SubnetNames != null)
            {
                netConfig.SubnetNames = new SubnetNamesCollection();
                foreach (string subnet in SubnetNames)
                {
                    netConfig.SubnetNames.Add(subnet);
                }
            }

            if (ParameterSetName.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                WindowsProvisioningConfigurationSet windowsConfig = new WindowsProvisioningConfigurationSet
                {
                    AdminPassword = Password,
                    ComputerName = string.IsNullOrEmpty(Name) ? ServiceName : Name,
                    EnableAutomaticUpdates = true,
                    ResetPasswordOnFirstLogon = false,
                    StoredCertificateSettings = Certificates
                };

                InputEndpoint rdpEndpoint = new InputEndpoint { LocalPort = 3389, Protocol = "tcp", Name = "RemoteDesktop" };

                netConfig.InputEndpoints.Add(rdpEndpoint);
                vm.ConfigurationSets.Add(windowsConfig);
                vm.ConfigurationSets.Add(netConfig);
            }
            else
            {
                LinuxProvisioningConfigurationSet linuxConfig = new LinuxProvisioningConfigurationSet();
                linuxConfig.HostName = string.IsNullOrEmpty(this.Name) ? this.ServiceName : this.Name;
                linuxConfig.UserName = this.LinuxUser;
                linuxConfig.UserPassword = this.Password;
                linuxConfig.DisableSshPasswordAuthentication = false;

                if (this.SSHKeyPairs != null && this.SSHKeyPairs.Count > 0 || this.SSHPublicKeys != null && this.SSHPublicKeys.Count > 0)
                {
                    linuxConfig.SSH = new LinuxProvisioningConfigurationSet.SSHSettings();
                    linuxConfig.SSH.PublicKeys = this.SSHPublicKeys;
                    linuxConfig.SSH.KeyPairs = this.SSHKeyPairs;
                }

                InputEndpoint rdpEndpoint = new InputEndpoint();
                rdpEndpoint.LocalPort = 22;
                rdpEndpoint.Protocol = "tcp";
                rdpEndpoint.Name = "SSH";
                netConfig.InputEndpoints.Add(rdpEndpoint);
                vm.ConfigurationSets.Add(linuxConfig);
                vm.ConfigurationSets.Add(netConfig);
            }

            string CreateCloudServiceOperationID = String.Empty;
            string CreateDeploymentOperationID = String.Empty;
            List<String> CreateVMOperationIDs = new List<String>();
            Operation lastOperation = null;

            bool ServiceExists = DoesCloudServiceExist(this.ServiceName);

            if (string.IsNullOrEmpty(this.Location) == false || string.IsNullOrEmpty(AffinityGroup) == false || (!String.IsNullOrEmpty(VNetName) && ServiceExists == false))
            {
                using (new OperationContextScope(Channel.ToContextChannel()))
                {
                    try
                    {
                        //Implicitly created hosted service2012-05-07 23:12 

                        // Create the Cloud Service when
                        // Location or Affinity Group is Specified
                        // or VNET is specified and the service doesn't exist
                        var chsi = new CreateHostedServiceInput
                        {
                            AffinityGroup = this.AffinityGroup,
                            Location = this.Location,
                            ServiceName = this.ServiceName,
                            Description = String.Format("Implicitly created hosted service{0}", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm")),
                            Label = ServiceManagementHelper.EncodeToBase64String(this.ServiceName)
                        };

                        ExecuteClientAction(chsi, CommandRuntime + " - Create Cloud Service", s => this.Channel.CreateHostedService(s, chsi), WaitForOperation);
                    }

                    catch (ServiceManagementClientException ex)
                    {
                        this.WriteErrorDetails(ex);
                        return;
                    }
                }
            }
            if (lastOperation != null && string.Compare(lastOperation.Status, OperationState.Failed, StringComparison.OrdinalIgnoreCase) == 0)
                return;

            // If the current deployment doesn't exist set it create it
            if (CurrentDeployment == null)
            {
                using (new OperationContextScope(Channel.ToContextChannel()))
                {
                    try
                    {
                        var deployment = new Deployment
                        {
                            DeploymentSlot = "Production",
                            Name = this.ServiceName,
                            Label = this.ServiceName,
                            RoleList = new RoleList(new List<Role> { vm }),
                            VirtualNetworkName = this.VNetName

                        };

                        if (this.DnsSettings != null)
                        {
                            deployment.Dns = new DnsSettings();
                            deployment.Dns.DnsServers = new DnsServerList();
                            foreach (DnsServer dns in this.DnsSettings)
                                deployment.Dns.DnsServers.Add(dns);
                        }

                        ExecuteClientAction(deployment, CommandRuntime + " - Create Deployment with VM " + vm.RoleName, s => this.Channel.CreateDeployment(s, this.ServiceName, deployment), WaitForOperation);
                    }

                    catch (ServiceManagementClientException ex)
                    {
                        if (ex.HttpStatus == HttpStatusCode.NotFound)
                        {
                            throw new Exception("Cloud Service does not exist. Specify -Location or -Affinity group to create one.");
                        }
                        else
                        {
                            this.WriteErrorDetails(ex);
                        }
                        return;
                    }

                    _createdDeployment = true;
                }
            }
            else
            {
                if (VNetName != null || DnsSettings != null)
                {
                    WriteWarning("VNetName or DnsSettings can only be specified on new deployments.");
                }
            }


            if (lastOperation != null && string.Compare(lastOperation.Status, OperationState.Failed, StringComparison.OrdinalIgnoreCase) == 0)
                return;

            // Only create the VM when a new VM was added and it was not created during the deployment phase.
            if ((_createdDeployment == false))
            {
                using (new OperationContextScope(Channel.ToContextChannel()))
                {
                    try
                    {
                        ExecuteClientAction(vm, CommandRuntime + " - Create VM " + vm.RoleName, s => this.Channel.AddRole(s, this.ServiceName, this.ServiceName, vm), WaitForOperation);
                    }
                    catch (ServiceManagementClientException ex)
                    {
                        this.WriteErrorDetails(ex);
                        return;
                    }
                }
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                this.ValidateParameters();
                base.ProcessRecord();
                this.NewAzureVMProcess();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        protected bool DoesCloudServiceExist(string serviceName)
        {
            bool IsPresent = false;
            using (new OperationContextScope(Channel.ToContextChannel()))
            {
                try
                {
                    AvailabilityResponse response = this.RetryCall(s => this.Channel.IsDNSAvailable(s, serviceName));
                    WaitForOperation(CommandRuntime.ToString(), true);
                    IsPresent = !response.Result;
                }
                catch (ServiceManagementClientException ex)
                {
                    if (ex.HttpStatus == HttpStatusCode.NotFound)
                        IsPresent = false;
                    else
                        this.WriteErrorDetails(ex);
                }
            }
            return IsPresent;
        }

        protected void ValidateParameters()
        {
            if (this.DnsSettings != null && string.IsNullOrEmpty(this.VNetName))
            {
                throw new ArgumentException("VNetName is required when specifying DNS Settings.");
            }

            if (this.ParameterSetName.Contains("Linux") && string.IsNullOrEmpty(this.LinuxUser))
            {
                throw new ArgumentException("Specify -LinuxUser when creating Linux Virtual Machines");
            }

            if (this.ParameterSetName.Contains("Linux") && !ValidationHelpers.IsLinuxPasswordValid(this.Password))
            {
                throw new ArgumentException("Password does not meet complexity requirements.");
            }

            if (this.ParameterSetName.Contains("Windows") && !ValidationHelpers.IsWindowsPasswordValid(this.Password))
            {
                throw new ArgumentException("Password does not meet complexity requirements.");
            }

            if (this.ParameterSetName.Contains("Linux"))
            {
                bool valid = false;
                if (string.IsNullOrEmpty(this.Name))
                {
                    valid = ValidationHelpers.IsLinuxHostNameValid(this.ServiceName); // uses servicename if name not specified
                }
                else
                {
                    valid = ValidationHelpers.IsLinuxHostNameValid(this.Name); 
                }
                if (valid == false)
                {
                    throw new ArgumentException("Hostname is invalid.");
                }
            }

            if (string.IsNullOrEmpty(this.Name) == false)
            {
                if (this.ParameterSetName.Contains("Windows") && !ValidationHelpers.IsWindowsComputerNameValid(this.Name))
                {
                    throw new ArgumentException("Computer Name is invalid.");
                }
            }
            else
            {
                if (this.ParameterSetName.Contains("Windows") && !ValidationHelpers.IsWindowsComputerNameValid(this.ServiceName))
                {
                    throw new ArgumentException("Computer Name is invalid.");
                }
            }

            if(String.IsNullOrEmpty(this.VNetName) == false && (String.IsNullOrEmpty(this.Location) && String.IsNullOrEmpty(this.AffinityGroup)))
            {
                throw new ArgumentException("Virtual Network Name may only be specified on the initial deployment. Specify Location or Affinity Group to create a new cloud service and deployment.");
            }

        }
    }
}

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

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.WAPackIaaS.FunctionalTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;

    public class CmdletTestCloudServiceBase : CmdletTestBase
    {
        // CloudService
        protected const string GetCloudServiceCmdletName = "Get-WAPackCloudService";

        protected const string NewCloudServiceCmdletName = "New-WAPackCloudService";

        protected const string RemoveCloudServiceCmdletName = "Remove-WAPackCloudService";

        protected string CloudServiceName = "TestCloudService";

        protected string CloudServiceLabel = "Label - TestCloudService";

        protected List<PSObject> CreatedCloudServices;

        // VMRole
        protected const string GetVMRoleCmdletName = "Get-WAPackVMRole";

        protected const string NewVMRoleCmdletName = "New-WAPackVMRole";

        protected const string RemoveVMRoleCmdletName = "Remove-WAPackVMRole";

        protected string VMRoleNameFromCloudService = "TestVMRoleFromCloudService";

        protected string VMRoleNameFromQuickCreate = "TestVMRoleFromQuickCreate";

        protected string VMRoleLabelToCreate = "Label - TestVMRole";

        protected List<PSObject> CreatedVMRolesFromQuickCreate;

        protected List<PSObject> CreatedVMRolesFromCloudService;

        // Error handling
        protected const string NonExistantResourceExceptionMessage = "The remote server returned an error: (404) Not Found.";

        protected const string AssertFailedNonExistantRessourceExceptionMessage = "Assert.IsFalse failed. " + NonExistantResourceExceptionMessage;

        protected CmdletTestCloudServiceBase()
        {
            CreatedCloudServices = new List<PSObject>();
            CreatedVMRolesFromQuickCreate = new List<PSObject>();
            CreatedVMRolesFromCloudService = new List<PSObject>();
        }

        protected void CreateCloudService()
        {
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", CloudServiceName},
                {"Label", CloudServiceLabel}
            };
            var createdCloudService = this.InvokeCmdlet(NewCloudServiceCmdletName, inputParams);
            Assert.AreEqual(1, createdCloudService.Count, string.Format("Actual CloudServices found - {0}, Expected CloudServices - {1}", createdCloudService.Count, 1));

            var readCloudServiceName = createdCloudService.First().Properties["Name"].Value;
            Assert.AreEqual(CloudServiceName, readCloudServiceName, string.Format("Actual CloudService name - {0}, Expected CloudService name- {1}", readCloudServiceName, CloudServiceName));

            var readCloudServiceLabel = createdCloudService.First().Properties["Label"].Value;
            Assert.AreEqual(CloudServiceLabel, readCloudServiceLabel, string.Format("Actual CloudService Label - {0}, Expected CloudService Label- {1}", readCloudServiceLabel, CloudServiceLabel));

            var readCloudServiceProvisioningState = createdCloudService.First().Properties["ProvisioningState"].Value;
            Assert.AreEqual("Provisioned", readCloudServiceProvisioningState, string.Format("Actual CloudService Provisionning State - {0}, Expected CloudService name- {1}", readCloudServiceProvisioningState, "Provisioned"));

            this.CreatedCloudServices.AddRange(createdCloudService);
        }

        protected void CloudServicePreTestCleanup()
        {
            try
            {
                var inputParams = new Dictionary<string, object>()
                {
                    {"Name", this.CloudServiceName}
                };
                var existingCloudServices = this.InvokeCmdlet(GetCloudServiceCmdletName, inputParams);

                if (existingCloudServices != null && existingCloudServices.Any())
                {
                    this.CreatedCloudServices.AddRange(existingCloudServices);
                    this.RemoveCloudServices();
                }
            }
            catch (AssertFailedException e)
            {
                Assert.AreEqual(AssertFailedNonExistantRessourceExceptionMessage, e.Message);
            }
        }

        protected void RemoveCloudServices()
        {
            foreach (var cloudService in this.CreatedCloudServices)
            {
                var inputParams = new Dictionary<string, object>()
                {
                    {"CloudService", cloudService},
                    {"Force", null},
                    {"PassThru", null}
                };
                var isDeleted = this.InvokeCmdlet(RemoveCloudServiceCmdletName, inputParams, null);
                Assert.AreEqual(1, isDeleted.Count);
                Assert.AreEqual(true, isDeleted.First());

                inputParams = new Dictionary<string, object>()
                {
                    {"Name", this.CloudServiceName}
                };
                var deletedCloudService = this.InvokeCmdlet(GetCloudServiceCmdletName, inputParams, NonExistantResourceExceptionMessage);
                Assert.AreEqual(0, deletedCloudService.Count);
            }

            this.CreatedCloudServices.Clear();
        }

        protected void CreateVMRoleFromQuickCreate()
        {
            Dictionary<string, object> inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VMRoleNameFromQuickCreate},
                {"Label", this.VMRoleLabelToCreate},
                {"ResourceDefinition", GetBasicResDef()}
            };
            var createdVMRole = this.InvokeCmdlet(NewVMRoleCmdletName, inputParams, null);

            Assert.AreEqual(1, createdVMRole.Count, string.Format("Actual VMRoles found - {0}, Expected VMRoles - {1}", createdVMRole.Count, 1));
            var createdVMRoleName = createdVMRole.First().Properties["Name"].Value;

            Assert.AreEqual(this.VMRoleNameFromQuickCreate, createdVMRoleName, string.Format("Actual VMRoles Name - {0}, Expected VMRoles Name- {1}", createdVMRoleName, this.VMRoleNameFromQuickCreate));
            this.CreatedVMRolesFromQuickCreate.AddRange(createdVMRole);
        }

        protected void CreateVMRoleFromCloudService()
        {
            this.CreateCloudService();

            Dictionary<string, object> inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VMRoleNameFromCloudService},
                {"Label", this.VMRoleLabelToCreate},
                {"CloudService", this.CreatedCloudServices.First()},
                {"ResourceDefinition", GetBasicResDef()}
            };
            var createdVMRole = this.InvokeCmdlet(NewVMRoleCmdletName, inputParams, null);

            Assert.AreEqual(1, createdVMRole.Count, string.Format("Actual VMRoles found - {0}, Expected VMRoles - {1}", createdVMRole.Count, 1));
            var createdVMRoleName = createdVMRole.First().Properties["Name"].Value;

            Assert.AreEqual(this.VMRoleNameFromCloudService, createdVMRoleName, string.Format("Actual VMRoles Name - {0}, Expected VMRoles Name- {1}", createdVMRoleName, this.VMRoleNameFromCloudService));
            this.CreatedVMRolesFromCloudService.AddRange(createdVMRole);
        }

        protected void VMRolePreTestCleanup()
        {
            // Cleaning up VMRole on cloudservice having the same name as the VMRole (QuickCreateVMRole)
            try
            {
                var inputParams = new Dictionary<string, object>()
                {
                    {"Name", this.VMRoleNameFromQuickCreate}
                };
                var existingVMRoles = this.InvokeCmdlet(GetVMRoleCmdletName, inputParams, null);

                if (existingVMRoles != null && existingVMRoles.Any())
                {
                    this.CreatedVMRolesFromQuickCreate.AddRange(existingVMRoles);
                }

                this.RemoveVMRoles();
            }
            catch (AssertFailedException e)
            {
                Assert.AreEqual(AssertFailedNonExistantRessourceExceptionMessage, e.Message);
            }

            // Cleaning up VMRole created on existing CloudServices
            try
            {
                if (this.CreatedCloudServices.Any())
                {
                    var inputParams = new Dictionary<string, object>()
                    {
                        {"Name", this.VMRoleNameFromCloudService},
                        {"CloudService", this.CreatedCloudServices.First()}
                    };
                    var existingVMRoles = this.InvokeCmdlet(GetVMRoleCmdletName, inputParams, null);

                    if (existingVMRoles != null && existingVMRoles.Any())
                    {
                        this.CreatedVMRolesFromCloudService.AddRange(existingVMRoles);
                    }

                    this.RemoveVMRoles();
                }
            }
            catch (AssertFailedException e)
            {
                Assert.AreEqual(AssertFailedNonExistantRessourceExceptionMessage, e.Message);
            }
        }

        protected void RemoveVMRoles()
        {
            foreach (var vmRole in this.CreatedVMRolesFromQuickCreate)
            {
                var inputParams = new Dictionary<string, object>()
                {
                    {"VMRole", vmRole},
                    {"Force", null},
                    {"PassThru", null}
                };
                var isDeleted = this.InvokeCmdlet(RemoveVMRoleCmdletName, inputParams, null);
                Assert.AreEqual(1, isDeleted.Count);
                Assert.AreEqual(true, isDeleted.First());

                inputParams = new Dictionary<string, object>()
                {
                    {"Name", this.VMRoleNameFromQuickCreate}
                };
                var deletedVMRole = this.InvokeCmdlet(GetVMRoleCmdletName, inputParams, NonExistantResourceExceptionMessage);
                Assert.AreEqual(0, deletedVMRole.Count);                
            }

            foreach (var vmRole in this.CreatedVMRolesFromCloudService)
            {
                var inputParams = new Dictionary<string, object>()
                {
                    {"VMRole", vmRole},
                    {"CloudServiceName", this.CloudServiceName},
                    {"Force", null},
                    {"PassThru", null}
                };
                var isDeleted = this.InvokeCmdlet(RemoveVMRoleCmdletName, inputParams, null);
                Assert.AreEqual(1, isDeleted.Count);
                Assert.AreEqual(true, isDeleted.First());

                inputParams = new Dictionary<string, object>()
                {
                    {"Name", this.VMRoleNameFromCloudService},
                    {"CloudServiceName", this.CloudServiceName}
                };
                var deletedVMRole = this.InvokeCmdlet(GetVMRoleCmdletName, inputParams, NonExistantResourceExceptionMessage);
                Assert.AreEqual(0, deletedVMRole.Count);          
            }

            this.CreatedVMRolesFromQuickCreate.Clear();
        }

        protected VMRoleResourceDefinition GetBasicResDef()
        {
            var resdef = new VMRoleResourceDefinition();

            #region Resdef
            resdef.Name = "NoAppIPv6";
            resdef.Publisher = "Microsoft";
            resdef.SchemaVersion = "1.0";
            resdef.Version = "1.0.0.0";
            resdef.Type = "Microsoft.Compute/VMRole/1.0";

            #region IntrinsicSettings

            #region Hardware Profile
            resdef.IntrinsicSettings.HardwareProfile.VMSize = "ExtraSmall";
            #endregion

            #region Network Profile
            var ip1 = new IPAddress();
            ip1.AllocationMethod = "Dynamic";
            ip1.Type = "IPV4";
            ip1.ConfigurationName = "SampleIPV4Config";

            var networkAdapter = new NetworkAdapter();
            networkAdapter.Name = "Nic1";
            networkAdapter.IPAddresses.Add(ip1);

            resdef.IntrinsicSettings.NetworkProfile.NetworkAdapters.Add(networkAdapter);
            #endregion

            #region Operating System Profile
            resdef.IntrinsicSettings.OperatingSystemProfile = null;
            #endregion

            #region Scaleout Settings
            resdef.IntrinsicSettings.ScaleOutSettings.InitialInstanceCount = "1";
            resdef.IntrinsicSettings.ScaleOutSettings.MaximumInstanceCount = "5";
            resdef.IntrinsicSettings.ScaleOutSettings.MinimumInstanceCount = "1";
            resdef.IntrinsicSettings.ScaleOutSettings.UpgradeDomainCount = "1";
            #endregion

            #region Storage Profile
            resdef.IntrinsicSettings.StorageProfile.OSVirtualHardDiskImage = WAPackConfigurationFactory.LinuxOSVirtualHardDiskImage;
            #endregion

            #endregion

            #endregion

            return resdef;
        }

    }
}

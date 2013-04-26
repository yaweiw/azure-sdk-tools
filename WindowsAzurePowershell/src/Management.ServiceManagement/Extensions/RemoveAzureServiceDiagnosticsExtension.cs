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
    using System.Xml;
    using WindowsAzure.Management.Utilities.CloudService;
    using WindowsAzure.Management.Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Remove Windows Azure Service Diagnostics Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureServiceDiagnosticsExtension"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureServiceDiagnosticsExtensionCommand : BaseAzureServiceDiagnosticsExtensionCmdlet
    {
        public RemoveAzureServiceDiagnosticsExtensionCommand()
            : base()
        {
        }

        public RemoveAzureServiceDiagnosticsExtensionCommand(IServiceManagement channel)
            : base(channel)
        {
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "RemoveExtension", HelpMessage = "Cloud Service Name")]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "RemoveExtension", HelpMessage = "Deployment Slot: Production | Staging. Default Production.")]
        [ValidateSet(DeploymentSlotType.Production, DeploymentSlotType.Staging, IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "RemoveExtension", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        public string[] Roles
        {
            get;
            set;
        }

        private Deployment Deployment
        {
            get;
            set;
        }

        private bool ValidateParameters()
        {
            string serviceName;
            ServiceSettings settings = General.GetDefaultSettings(General.TryGetServiceRootPath(CurrentPath()), ServiceName, null, null, null, null, CurrentSubscription.SubscriptionId, out serviceName);

            if (string.IsNullOrEmpty(serviceName))
            {
                WriteExceptionError(new Exception("Invalid service name"));
                return false;
            }
            else
            {
                ServiceName = serviceName;
            }

            if (!IsServiceAvailable(ServiceName))
            {
                WriteExceptionError(new Exception("Service not found: " + ServiceName));
                return false;
            }

            Slot = string.IsNullOrEmpty(Slot) ? DeploymentSlotType.Production : Slot;

            Deployment = Channel.GetDeploymentBySlot(CurrentSubscription.SubscriptionId, ServiceName, Slot);
            if (Deployment == null)
            {
                WriteExceptionError(new Exception(string.Format("Deployment not found in service: {0} and slot: {1}", ServiceName, Slot)));
                return false;
            }

            if (Deployment.ExtensionConfiguration == null)
            {
                Deployment.ExtensionConfiguration = new ExtensionConfiguration
                {
                    AllRoles = new AllRoles(),
                    NamedRoles = new NamedRoles()
                };
            }

            if (Roles != null)
            {
                foreach (string roleName in Roles)
                {
                    if (Deployment.RoleList == null || !Deployment.RoleList.Any(r => r.RoleName == roleName))
                    {
                        WriteExceptionError(new Exception(string.Format("Role{0} not found in deployment {1} of service {2}.", roleName, Slot, ServiceName)));
                        return false;
                    }
                }
            }

            ExtensionManager = new HostedServiceExtensionManager(Channel, CurrentSubscription.SubscriptionId, ServiceName);

            return true;
        }

        private void ChangeDeployment(ExtensionConfiguration extConfig)
        {
            ChangeConfigurationInput changeConfigInput = new ChangeConfigurationInput
            {
                Configuration = Deployment.Configuration,
                ExtendedProperties = Deployment.ExtendedProperties,
                ExtensionConfiguration = Deployment.ExtensionConfiguration = extConfig,
                Mode = "Auto",
                TreatWarningsAsError = false
            };
            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => Channel.ChangeConfigurationBySlot(s, ServiceName, Slot, changeConfigInput));
        }

        private void ExecuteCommand()
        {
            ValidateParameters();
            ExtensionConfiguration extConfig = ExtensionManager.NewExtensionConfig(Deployment);
            if (ExtensionManager.ExistExtension(extConfig, Roles, ExtensionNameSpace, ExtensionType))
            {
                extConfig = ExtensionManager.RemoveExtension(extConfig, Roles, ExtensionNameSpace, ExtensionType);
                ChangeDeployment(extConfig);
            }
            else
            {
                WriteWarning("No existing extensions enabled on role(s). This cmdlet will end.");
            }
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}

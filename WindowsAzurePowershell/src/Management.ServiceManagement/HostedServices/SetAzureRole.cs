// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
    using System.ServiceModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Xml.Linq;
    using Samples.WindowsAzure.ServiceManagement;
    using Cmdlets.Common;

    /// <summary>
    /// Sets the instance count for the selected role.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureRole", DefaultParameterSetName = "ParameterSetDeploymentSlot")]
    public class SetAzureRoleCommand : CloudBaseCmdlet<IServiceManagement>
    {
        public SetAzureRoleCommand()
        {
        }

        public SetAzureRoleCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Slot of the deployment.")]
        [ValidateNotNullOrEmpty]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string RoleName
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, HelpMessage = "Instance count.")]
        [ValidateNotNullOrEmpty]
        public int Count
        {
            get;
            set;
        }

        public void SetRoleInstanceCountProcess()
        {
            Operation operation;
            var currentDeployment = this.GetCurrentDeployment(out operation);
            if (currentDeployment == null)
            {
                return;
            }

            using (new OperationContextScope((IContextChannel)this.Channel))
            {
                try
                {
                    XNamespace ns = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration";
                    var configuration = XDocument.Parse(ServiceManagementHelper.DecodeFromBase64String(currentDeployment.Configuration));
                    var role = configuration.Root.Elements(ns + "Role")
                                    .Where(p => string.Compare(p.Attribute("name").Value, this.RoleName, true) == 0)
                                    .SingleOrDefault();

                    if (role != null)
                    {
                        role.Element(ns + "Instances").SetAttributeValue("count", this.Count);
                    }

                    using (new OperationContextScope((IContextChannel)Channel))
                    {
                        var updatedConfiguration = new ChangeConfigurationInput
                        {
                            Configuration = ServiceManagementHelper.EncodeToBase64String(configuration.ToString())
                        };

                        ExecuteClientAction(configuration, CommandRuntime.ToString(), s => this.Channel.ChangeConfigurationBySlot(s, this.ServiceName, this.Slot, updatedConfiguration), WaitForOperation);
                    }
                }
                catch (EndpointNotFoundException ex)
                {
                    this.WriteErrorDetails(ex);
                }
                catch (CommunicationException ex)
                {
                    this.WriteErrorDetails(ex);
                }
            }
        }

        protected override void OnProcessRecord()
        {
            this.SetRoleInstanceCountProcess();
        }

        private Deployment GetCurrentDeployment(out Operation operation)
        {
            using (new OperationContextScope((IContextChannel)Channel))
            {
                try
                {
                    var deployment = this.RetryCall(s => this.Channel.GetDeploymentBySlot(s, this.ServiceName, this.Slot));
                    operation = WaitForOperation("Get Deployment");
                    return deployment;
                }
                catch (CommunicationException)
                {
                    throw;
                }
            }
        }
    }
}
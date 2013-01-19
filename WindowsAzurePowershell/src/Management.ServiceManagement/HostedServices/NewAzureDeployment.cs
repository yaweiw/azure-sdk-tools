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
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using System.Management.Automation;
    using Helpers;
    using Samples.WindowsAzure.ServiceManagement;
    using Cmdlets.Common;
    using Management.Model;
    using Extensions;

    /// <summary>
    /// Create a new deployment. Note that there shouldn't be a deployment 
    /// of the same name or in the same slot when executing this command.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureDeployment", DefaultParameterSetName = "PaaS")]
    public class NewAzureDeploymentCommand : CloudBaseCmdlet<IServiceManagement>
    {
        public NewAzureDeploymentCommand()
        {
        }

        public NewAzureDeploymentCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Cloud service name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "Package location. This parameter specifies the path or URI to a .cspkg in blob storage. The storage account must belong to the same subscription as the deployment.")]
        [ValidateNotNullOrEmpty]
        public string Package
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true,  HelpMessage = "Configuration file path. This parameter should specifiy a .cscfg file on disk.")]
        [ValidateNotNullOrEmpty]
        public string Configuration
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot [Staging | Production].")]
        [ValidateSet("Staging", "Production", IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = false, HelpMessage = "Label for the new deployment.")]
        [ValidateNotNullOrEmpty]
        public string Label
        {
            get;
            set;
        }

        [Parameter(Position = 5, HelpMessage = "Deployment name.")]
        [Alias("DeploymentName")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }
        
        [Parameter(Mandatory = false, HelpMessage = "Do not start deployment upon creation.")]
        public SwitchParameter DoNotStart
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Indicates whether to treat package validation warnings as errors.")]
        public SwitchParameter TreatWarningsAsError
        {
            get;
            set;
        }

        public void NewPaaSDeploymentProcess()
        {
            bool removePackage = false;

            AssertNoPersistenVmRoleExistsInDeployment("Production");
            AssertNoPersistenVmRoleExistsInDeployment("Slot");

            var storageName = CurrentSubscription.CurrentStorageAccount;

            Uri packageUrl;
            if (this.Package.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                this.Package.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                packageUrl = new Uri(this.Package);
            }
            else
            {
                var progress = new ProgressRecord(0, "Please wait...", "Uploading package to blob storage");
                WriteProgress(progress);
                removePackage = true;
                packageUrl = this.RetryCall(s =>
                    AzureBlob.UploadPackageToBlob(
                    this.Channel,
                    storageName,
                    s,
                    this.Package));
            }
            
            var deploymentInput = new CreateDeploymentInput
            {
                PackageUrl = packageUrl,
                Configuration = Utility.GetConfiguration(this.Configuration),
                Label = ServiceManagementHelper.EncodeToBase64String(this.Label),
                Name = this.Name,
                StartDeployment = !this.DoNotStart.IsPresent,
                TreatWarningsAsError = this.TreatWarningsAsError.IsPresent
            };
            
            using (new OperationContextScope((IContextChannel)Channel))
            {
                try
                {
                    var progress = new ProgressRecord(0, "Please wait...", "Creating the new deployment");
                    WriteProgress(progress);

                    ExecuteClientAction(deploymentInput, CommandRuntime.ToString(), s => this.Channel.CreateOrUpdateDeployment(s, this.ServiceName, this.Slot, deploymentInput), WaitForOperation);

                    if (removePackage == true)
                    {
                        this.RetryCall(s =>
                        AzureBlob.DeletePackageFromBlob(
                                this.Channel,
                                storageName,
                                s,
                                packageUrl));
                    }
                }
                catch (CommunicationException ex)
                {
                    this.WriteErrorDetails(ex);
                }
            }
        }

        private void AssertNoPersistenVmRoleExistsInDeployment(string slot)
        {
            using (new OperationContextScope((IContextChannel) Channel))
            {
                try
                {
                    Deployment currentDeployment = this.RetryCall(s => this.Channel.GetDeploymentBySlot(s, this.ServiceName, slot));
                    if (currentDeployment.RoleList != null)
                    {
                        if (string.Compare(currentDeployment.RoleList[0].RoleType, "PersistentVMRole", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            throw new ArgumentException(String.Format("Cannot Create New Deployment with Virtual Machines Present in {0} Slot", slot));
                        }
                    }
                }
                catch (CommunicationException ex)
                {
                    if ((ex is EndpointNotFoundException) == false && IsVerbose() == false)
                    {
                        this.WriteErrorDetails(ex);
                    }
                }
            }
        }

        protected override void OnProcessRecord()
        {
            this.ValidateParameters();
            this.NewPaaSDeploymentProcess();
        }

        private void ValidateParameters()
        {
            if (string.IsNullOrEmpty(this.Slot))
            {
                this.Slot = "Production";
            }

            if (string.IsNullOrEmpty(this.Name))
            {
                 this.Name = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(this.Label))
            {
                this.Label = this.Name;
            }

            if (string.IsNullOrEmpty(this.CurrentSubscription.CurrentStorageAccount))
            {
                throw new ArgumentException("CurrentStorageAccount is not set. Use Set-AzureSubscription subname -CurrentStorageAccount storageaccount to set it.");                
            }
        }
    }
}

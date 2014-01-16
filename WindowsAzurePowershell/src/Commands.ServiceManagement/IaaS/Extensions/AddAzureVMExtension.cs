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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS.Extensions
{
    using Model;
    using Model.PersistentVMModel;
    using System;
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Add, "AzureVMExtension"), OutputType(typeof(IPersistentVM))]
    public class AddAzureVMExtensionCommand : VirtualMachineConfigurationCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0, HelpMessage = "The Extension Image Name.")]
        [ValidateNotNullOrEmpty]
        public string ExtensionImageName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 1, HelpMessage = "The Extension Publisher.")]
        [ValidateNotNullOrEmpty]
        public string Publisher
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 2, HelpMessage = "The Extension Version.")]
        [ValidateNotNullOrEmpty]
        public string Version
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 3, HelpMessage = "The Extension Reference Name.")]
        [ValidateNotNullOrEmpty]
        public string ReferenceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 4, HelpMessage = "The Extension Public Configuration.")]
        [ValidateNotNullOrEmpty]
        public string PublicConfiguration
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 5, HelpMessage = "The Extension Private Configuration.")]
        [ValidateNotNullOrEmpty]
        public string PrivateConfiguration
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, Position = 6, HelpMessage = "The Extension State, 'Enable' or 'Disable'.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet("Enable", "Disable")]
        public string State
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            if (VM.GetInstance().ResourceExtensionReferences == null)
            {
                VM.GetInstance().ResourceExtensionReferences = new ResourceExtensionReferenceList();
            }

            var newExtensionRef = new ResourceExtensionReference
            {
                Name = this.ExtensionImageName,
                Publisher = this.Publisher,
                Version = this.Version,
                State = this.State,
                ResourceExtensionParameterValues = new ResourceExtensionParameterValueList()
            };

            if (!string.IsNullOrEmpty(this.ReferenceName))
            {
                newExtensionRef.ReferenceName = string.Format(
                    "{0}-{1}-{2}",
                    newExtensionRef.Publisher,
                    newExtensionRef.Name,
                    newExtensionRef.Version);
            }
            else
            {
                newExtensionRef.ReferenceName = this.ReferenceName;
            }

            if (!string.IsNullOrEmpty(this.PublicConfiguration))
            {
                newExtensionRef.ResourceExtensionParameterValues.Add(
                    new ResourceExtensionParameterValue
                    {
                        Key = "PublicConfiguration",
                        Type = "Public",
                        Value = PublicConfiguration
                    });
            }

            if (!string.IsNullOrEmpty(this.PrivateConfiguration))
            {
                newExtensionRef.ResourceExtensionParameterValues.Add(
                    new ResourceExtensionParameterValue
                    {
                        Key = "PrivateConfiguration",
                        Type = "Private",
                        Value = PrivateConfiguration
                    });
            }

            VM.GetInstance().ResourceExtensionReferences.Add(newExtensionRef);
            WriteObject(VM);
        }

        protected override void ProcessRecord()
        {
            ServiceManagementProfile.Initialize();
            try
            {
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}

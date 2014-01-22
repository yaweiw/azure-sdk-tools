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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Management.Compute;
    using Management.Compute.Models;
    using Utilities.Common;

    /// <summary>
    /// Get Windows Azure VM Extension Image.
    /// </summary>
    [Cmdlet(
        VerbsCommon.Get,
        AzureVMExtensionConfigTemplateCommandNoun),
    OutputType(
        typeof(VirtualMachineExtensionConfigContext))]
    public class GetAzureVMExtensionConfigTemplateCommand : ServiceManagementBaseCmdlet
    {
        protected const string AzureVMExtensionConfigTemplateCommandNoun = "AzureVMExtensionConfigTemplate";

        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Image Name.")]
        [ValidateNotNullOrEmpty]
        public string ExtensionName
        {
            get;
            set;
        }

        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Publisher.")]
        [ValidateNotNullOrEmpty]
        public string Publisher
        {
            get;
            set;
        }

        [Parameter(
            Position = 2,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Extension Version.")]
        [ValidateNotNullOrEmpty]
        public string Version
        {
            get;
            set;
        }

        [Parameter(
            Mandatory = true,
            Position = 3,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The File Path to Save the Sample Config Template.")]
        [ValidateNotNullOrEmpty]
        public string SampleConfigPath
        {
            get;
            set;
        }

        protected string sampleConfig;

        public void ExecuteCommand()
        {
            ServiceManagementProfile.Initialize(this);

            ExecuteClientActionNewSM(null,
                CommandRuntime.ToString(),
                () =>
                {
                    if (!string.IsNullOrEmpty(this.Version))
                    {
                        return this.ComputeClient.VirtualMachineExtensions.List();
                    }
                    else
                    {
                        return this.ComputeClient.VirtualMachineExtensions.ListVersions(this.Publisher, this.ExtensionName);
                    }
                },
                (op, response) => GetVersionedExtensionImage(response, this.Version, out sampleConfig).Select(
                     extension => new VirtualMachineExtensionConfigContext
                     {
                         OperationDescription = CommandRuntime.ToString(),
                         OperationStatus = op.Status.ToString(),
                         OperationId = op.Id,
                         ExtensionName = extension.Name,
                         Publisher = extension.Publisher,
                         Version = extension.Version,
                         SampleConfig = extension.SampleConfig
                     }));

            if (!string.IsNullOrEmpty(this.SampleConfigPath))
            {
                File.WriteAllText(this.SampleConfigPath, sampleConfig);
            }
        }

        protected IEnumerable<VirtualMachineExtensionListResponse.ResourceExtension> GetVersionedExtensionImage(
            VirtualMachineExtensionListResponse response,
            string version,
            out string sampleConfig)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            Func<VirtualMachineExtensionListResponse.ResourceExtension, bool> truePred = s => true;

            Func<string, Func<VirtualMachineExtensionListResponse.ResourceExtension, string>,
                 Func<VirtualMachineExtensionListResponse.ResourceExtension, bool>> predFunc =
                 (x, f) => string.IsNullOrEmpty(x) ? truePred : s => string.Equals(x, f(s), StringComparison.OrdinalIgnoreCase);

            var result = response.Where(predFunc(version, s => s.Version));
            sampleConfig = result.Select(r => r.SampleConfig).FirstOrDefault();
            return result;
        }

        protected override void OnProcessRecord()
        {
            try
            {
                this.ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}

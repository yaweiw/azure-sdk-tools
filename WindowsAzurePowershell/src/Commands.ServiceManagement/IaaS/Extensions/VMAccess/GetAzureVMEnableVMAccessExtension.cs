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
    using System.Linq;
    using System.Management.Automation;
    using Model.PersistentVMModel;

    [Cmdlet(VerbsCommon.Get, "AzureVMEnableVMAccessExtension"), OutputType(typeof(IEnumerable<VMEnableVMAccessExtensionContext>))]
    public class GetAzureVMEnableVMAccessExtensionCommand : VirtualMachineConfigurationCmdletBase
    {
        internal void ExecuteCommand()
        {
            List<ResourceExtensionReference> extensionRefs = null;

            if (VM.GetInstance().ResourceExtensionReferences != null)
            {
                extensionRefs = VM.GetInstance().ResourceExtensionReferences.FindAll(
                    r => r.Name == VMEnableVMAccessExtensionBuilder.ExtensionDefaultName && r.Publisher == VMEnableVMAccessExtensionBuilder.ExtensionDefaultPublisher);
            }

            IEnumerable<VMEnableVMAccessExtensionContext> extensionContexts = extensionRefs == null ? null : extensionRefs.Select(
                r =>
                {
                    var extensionKeyValPair = r.ResourceExtensionParameterValues.Find(p => p.Key == VMEnableVMAccessExtensionBuilder.ExtensionReferenceKeyStr);
                    var daExtensionBuilder = extensionKeyValPair == null ? null : new VMEnableVMAccessExtensionBuilder(extensionKeyValPair.Value);
                    return new VMEnableVMAccessExtensionContext
                    {
                        Name = r.Name,
                        Publisher = r.Publisher,
                        ReferenceName = r.ReferenceName,
                        Version = r.Version,
                        Enabled = daExtensionBuilder == null ? false : daExtensionBuilder.Enabled,
                        UserName = daExtensionBuilder == null ? string.Empty : daExtensionBuilder.UserName,
                        Password = daExtensionBuilder == null ? null : daExtensionBuilder.Password
                    };
                });

            WriteObject(extensionContexts);
        }

        protected override void ProcessRecord()
        {
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

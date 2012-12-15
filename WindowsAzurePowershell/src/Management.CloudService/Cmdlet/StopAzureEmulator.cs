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

namespace Microsoft.WindowsAzure.Management.CloudService.Cmdlet
{
    using System;
    using System.Management.Automation;
    using System.Security.Permissions;
    using AzureTools;
    using Common;
    using Model;
    using Properties;
    using Services;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;

    /// <summary>
    /// Runs the service in the emulator
    /// </summary>
    [Cmdlet(VerbsLifecycle.Stop, "AzureEmulator")]
    public class StopAzureEmulatorCommand : CloudCmdlet<IServiceManagement>
    {
        [Parameter(Mandatory = false)]
        [Alias("ln")]
        public SwitchParameter Launch { get; set; }

        public StopAzureEmulatorCommand()
        {
            SkipChannelInit = true;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public void StopAzureEmulatorProcess()
        {
            string standardOutput;
            string standardError;

            AzureService service = new AzureService();
            SafeWriteVerbose(Resources.StopEmulatorMessage);
            service.StopEmulator(out standardOutput, out standardError);
            
            SafeWriteVerbose(Resources.StoppedEmulatorMessage);
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            AzureTool.Validate();
            base.ExecuteCmdlet();
            StopAzureEmulatorProcess();
        }
    }
}
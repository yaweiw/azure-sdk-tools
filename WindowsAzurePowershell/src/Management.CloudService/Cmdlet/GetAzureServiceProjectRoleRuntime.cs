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
    using Model;
    using Services;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Configure the number of instances for a web/worker role. Updates the cscfg with the number of instances
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureServiceProjectRoleRuntime")]
    public class GetAzureServiceProjectRoleRuntimeCommand : DeploymentServiceManagementCmdletBase
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string Runtime { get; set; }


        public void GetAzureRuntimesProcess(string runtimeType, string rootPath, string manifest = null)
        {
            AzureService service = new AzureService(rootPath, null);
            CloudRuntimeCollection runtimes = service.GetCloudRuntimes(service.Paths, manifest);
            WriteObject(runtimes.Where<CloudRuntimePackage>(p => string.IsNullOrEmpty(runtimeType) || p.Runtime == CloudRuntime.GetRuntimeByType(runtimeType)), true);
        }

        protected override void ProcessRecord()
        {
            try
            {
                SkipChannelInit = true;
                base.ProcessRecord();
                
                    this.GetAzureRuntimesProcess(Runtime, base.GetServiceRootPath());
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
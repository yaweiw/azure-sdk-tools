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
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Common;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Model;

    /// <summary>
    /// Retrieve a list of role runtimes available in the cloud
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureServiceProjectRoleRuntime")]
    public class GetAzureServiceProjectRoleRuntimeCommand : CloudCmdlet<IServiceManagement>
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string Runtime { get; set; }

        /// <summary>
        /// Retrieve the runtimes from the given manifest, or from the default cloud location, if none given.
        /// The manifest parameter is mainly a testing hook.
        /// </summary>
        /// <param name="runtimeType">The runtime type to filter by</param>
        /// <param name="rootPath">The path to the service in question</param>
        /// <param name="manifest">The path to the manifest file, if null, the default cloud manifest is used (test hook)</param>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void GetAzureRuntimesProcess(string runtimeType, string rootPath, string manifest = null)
        {
            AzureService service = new AzureService(rootPath, null);
            CloudRuntimeCollection runtimes = service.GetCloudRuntimes(service.Paths, manifest);
            WriteOutputObject(runtimes.Where<CloudRuntimePackage>(p => string.IsNullOrEmpty(runtimeType) ||
                p.Runtime == CloudRuntime.GetRuntimeByType(runtimeType)).ToList<CloudRuntimePackage>());
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();
            SkipChannelInit = true;
            this.GetAzureRuntimesProcess(Runtime, base.GetServiceRootPath());
        }        
    }
}
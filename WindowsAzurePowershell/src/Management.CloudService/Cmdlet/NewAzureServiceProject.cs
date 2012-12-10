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
    using System.IO;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Common;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Model;
    using Properties;

    /// <summary>
    /// Create scaffolding for a new hosted service. Generates a basic folder structure, 
    /// default cscfg file which wires up node/iisnode at startup in Azure as well as startup.js. 
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureServiceProject")]
    public class NewAzureServiceProjectCommand : CloudCmdlet<IServiceManagement>
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the cloud project")]
        [ValidateNotNullOrEmpty]
        public string ServiceName { get; set; }

        internal AzureService NewAzureServiceProcess(string parentDirectory, string serviceName)
        {
            // Create scaffolding structure
            //
            AzureService newService = new AzureService(parentDirectory, serviceName, null);

            SafeWriteOutputPSObject(
                newService.GetType().FullName,
                Parameters.ServiceName, newService.ServiceName,
                Parameters.RootPath, newService.Paths.RootPath
                );

            SafeWriteVerbose(string.Format(Resources.NewServiceCreatedMessage, newService.Paths.RootPath));

            return newService;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            SkipChannelInit = true;
            NewAzureServiceProcess(CurrentPath(), ServiceName);
            SessionState.Path.SetLocation(Path.Combine(CurrentPath(), ServiceName));
        }
    }
}
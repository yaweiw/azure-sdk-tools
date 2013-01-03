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
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using AzureTools;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Model;
    using ServiceConfigurationSchema;
    using ServiceDefinitionSchema;
    using Services;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using System.IO;

    /// <summary>
    /// Packages the service project into *.cspkg
    /// </summary>
    [Cmdlet(VerbsData.Save, "AzureServiceProjectPackage")]
    public class SaveAzureServiceProjectPackageCommand : CmdletBase
    {
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            AzureTool.Validate();
            string unused;
            string rootPath = base.GetServiceRootPath();
            AzureService service = new AzureService(base.GetServiceRootPath(), null);
            service.CreatePackage(DevEnv.Cloud, out unused, out unused);
            WriteVerbose(string.Format(Resources.PackageCreated, Path.Combine(rootPath, Resources.LocalPackageFileName)));
        }
    }
}

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

namespace Microsoft.WindowsAzure.Management.CloudService.Cmdlet.Common
{
    using Model;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;

    public abstract class CloudCmdlet<T> : CloudBaseCmdlet<T>
        where T : class
    {
        protected ServiceSettings GetDefaultSettings(string rootPath, string inServiceName, string slot, string location, string storageName, string subscription, out string serviceName)
        {
            ServiceSettings serviceSettings;

            if (string.IsNullOrEmpty(rootPath))
            {
                serviceSettings = ServiceSettings.LoadDefault(null, slot, location, subscription, storageName, inServiceName, null, out serviceName);
            }
            else
            {
                serviceSettings = ServiceSettings.LoadDefault(new AzureService(rootPath, null).Paths.Settings,
                slot, location, subscription, storageName, inServiceName, new AzureService(rootPath, null).ServiceName, out serviceName);
            }

            return serviceSettings;
        }
    }
}
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

namespace Microsoft.Azure.Commands.ManagedCache
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;

    using Microsoft.Azure.Management.ManagedCache;
    using Microsoft.Azure.Management.ManagedCache.Models;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;

    [Cmdlet(VerbsCommon.Get, "AzureManagedCache", ConfirmImpact = ConfirmImpact.None)]
    public class GetAzureManagedCache : ManagedCacheCmdletBase
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true,
            HelpMessage = "azure cache service name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        internal IntrinsicSettings.CacheServiceInput GetCacheService(string cacheService)
        {
            CloudServiceListResponse listResp = CacheClient.CloudServices.List();
            IntrinsicSettings.CacheServiceInput matchedCacheService = null;
            foreach(var cloudService in listResp)
            {
                //TODO: use const for caching type and provider namespace
                CloudServiceListResponse.CloudService.AddOnResource matched = cloudService.Resources.FirstOrDefault(
                    p => { return p.Type == "Caching" && cacheService.Equals(p.Name, StringComparison.OrdinalIgnoreCase); }
                    );

                if (matched != null)
                {
                    matchedCacheService = matched.IntrinsicSettingsSection.CacheServiceInputSection;
                }
            }
            return matchedCacheService;
        }

        public override void ExecuteCmdlet()
        {
            var cacheService = this.GetCacheService(Name);
            if (cacheService == null)
            {
                throw new ArgumentException("Invalid name"); //TODO: using resource string
            }
            WriteObject(cacheService);
        }      
    }
}
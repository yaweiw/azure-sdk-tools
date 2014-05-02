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

namespace Microsoft.Azure.Commands.ManagedCache.Models
{
    using Microsoft.Azure.Management.ManagedCache.Models;
    public class PSCacheService
    {
        public PSCacheService(CloudServiceGetResponse.Resource resource)
        {
            Name = resource.Name;
            State = resource.State;
            SubState = resource.SubState;
            Offering = resource.IntrinsicSettingsSection.CacheServiceInputSection.SkuType;
            int skuCount = resource.IntrinsicSettingsSection.CacheServiceInputSection.SkuCount;
            if (Offering == "Basic")
            {
                Memory = (skuCount * 128).ToString() + "MB";
            }
            else if (Offering == "Standard")
            {
                Memory = skuCount.ToString() + "GB";
            }
            else
            {
                Memory = (skuCount * 5).ToString() + "GB";
            }
        }

        public string Location { get; private set; }

        public string Name { get; private set; }

        public string State { get; private set; }

        public string SubState { get; private set; }

        public string Offering { get; private set; }

        public string Memory { get; private set; }

    }
}

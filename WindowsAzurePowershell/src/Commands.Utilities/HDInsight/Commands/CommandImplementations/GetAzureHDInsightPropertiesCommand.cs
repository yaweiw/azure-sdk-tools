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
namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Commands.CommandImplementations
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandInterfaces;
    using DataObjects;
    using GetAzureHDInsightClusters;

    internal class GetAzureHDInsightPropertiesCommand : AzureHDInsightClusterCommand<AzureHDInsightCapabilities>, IGetAzureHDInsightPropertiesCommand
    {
        private const string ContainersCountKey = "CONTAINERS_Count";
        private const string CoresUsedKey = "CONTAINERS_CoresUsed";
        private const string MaxCoresAllowedKey = "CONTAINERS_MaxCoresAllowed";

        public override async Task EndProcessing()
        {
            IHDInsightClient client = this.GetClient();
            var capabilities = await client.ListResourceProviderPropertiesAsync();
            capabilities = capabilities.ToList();
            var azureCapabilities = new AzureHDInsightCapabilities(capabilities);
            azureCapabilities.Versions = await client.ListAvailableVersionsAsync();
            azureCapabilities.Locations = await client.ListAvailableLocationsAsync();
            azureCapabilities.ClusterCount = this.GetIntCapability(capabilities, ContainersCountKey);
            azureCapabilities.CoresUsed = this.GetIntCapability(capabilities, CoresUsedKey);
            azureCapabilities.MaxCoresAllowed = this.GetIntCapability(capabilities, MaxCoresAllowedKey);
            this.Output.Add(azureCapabilities);
        }

        private int GetIntCapability(IEnumerable<KeyValuePair<string, string>> capabilities, string capabilityName)
        {
            int capabilityValue = 0;
            KeyValuePair<string, string> capablity = capabilities.FirstOrDefault(cap => cap.Key == capabilityName);
            if (int.TryParse(capablity.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out capabilityValue))
            {
                return capabilityValue;
            }

            return 0;
        }
    }
}

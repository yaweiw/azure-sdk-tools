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
namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters
{
    using BaseInterfaces;
    using Extensions;
    using System.Linq;
    using WindowsAzure.Commands.Utilities.Common;

    internal class AzureHDInsightSubscriptionResolver : IAzureHDInsightSubscriptionResolver
    {
        private readonly WindowsAzureProfile profile;

        public AzureHDInsightSubscriptionResolver(WindowsAzureProfile profile)
        {
            this.profile = profile;
        }

        public WindowsAzureSubscription ResolveSubscription(string subscription)
        {
            var resolvedSubscription = this.profile.Subscriptions.FirstOrDefault(s => s.SubscriptionId == subscription);
            if (resolvedSubscription.IsNull())
            {
                resolvedSubscription = this.profile.Subscriptions.FirstOrDefault(s => s.SubscriptionName == subscription);
            }

            return resolvedSubscription;
        }
    }
}
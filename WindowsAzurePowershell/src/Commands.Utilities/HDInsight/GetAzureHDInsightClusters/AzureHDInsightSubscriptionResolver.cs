// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License.  You may obtain a copy
// of the License at http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
// WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

namespace Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters
{
    using System.Linq;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.BaseInterfaces;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.Extensions;

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
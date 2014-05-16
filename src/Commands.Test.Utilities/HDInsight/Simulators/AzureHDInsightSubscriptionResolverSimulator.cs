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

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.HDInsight.Simulators
{
    using Commands.Utilities.Common;
    using Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.BaseInterfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Utilities;

    internal class AzureHDInsightSubscriptionResolverSimulator : IAzureHDInsightSubscriptionResolver
    {
        private IEnumerable<WindowsAzureSubscription> knownSubscriptions;

        internal AzureHDInsightSubscriptionResolverSimulator()
        {
            this.knownSubscriptions = new WindowsAzureSubscription[]
                {
                    new WindowsAzureSubscription()
                        {
                            Certificate = new X509Certificate2(Convert.FromBase64String(IntegrationTestBase.TestCredentials.Certificate), string.Empty),
                            SubscriptionId = IntegrationTestBase.TestCredentials.SubscriptionId.ToString()
                        }, 
                };
        }

        public WindowsAzureSubscription ResolveSubscription(string subscription)
        {
            return this.knownSubscriptions.FirstOrDefault(s => s.SubscriptionId == subscription)
                   ?? this.knownSubscriptions.FirstOrDefault(s => s.SubscriptionName == subscription);
        }
    }
}
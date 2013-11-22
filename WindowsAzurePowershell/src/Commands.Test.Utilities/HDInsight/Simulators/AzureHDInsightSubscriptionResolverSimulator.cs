using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.Commands.Test.Utilities.HDInsight.Utilities;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.BaseInterfaces;

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.HDInsight.Simulators
{
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
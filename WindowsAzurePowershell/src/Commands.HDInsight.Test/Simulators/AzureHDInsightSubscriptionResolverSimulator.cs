namespace Microsoft.WindowsAzure.Management.HDInsight.Test.Simulators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.BaseInterfaces;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.Tests.Utilities;

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
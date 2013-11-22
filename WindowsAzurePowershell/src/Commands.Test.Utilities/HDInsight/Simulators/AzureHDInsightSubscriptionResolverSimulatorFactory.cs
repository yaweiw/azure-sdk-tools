using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.BaseInterfaces;

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.HDInsight.Simulators
{
    internal class AzureHDInsightSubscriptionResolverSimulatorFactory : IAzureHDInsightSubscriptionResolverFactory
    {
        public IAzureHDInsightSubscriptionResolver Create(WindowsAzureProfile profile)
        {
            return new AzureHDInsightSubscriptionResolverSimulator();
        }
    }
}

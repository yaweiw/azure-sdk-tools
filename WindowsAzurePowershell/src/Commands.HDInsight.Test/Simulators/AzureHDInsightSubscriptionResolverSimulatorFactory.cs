namespace Microsoft.WindowsAzure.Management.HDInsight.Test.Simulators
{
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.GetAzureHDInsightClusters.BaseInterfaces;

    internal class AzureHDInsightSubscriptionResolverSimulatorFactory : IAzureHDInsightSubscriptionResolverFactory
    {
        public IAzureHDInsightSubscriptionResolver Create(WindowsAzureProfile profile)
        {
            return new AzureHDInsightSubscriptionResolverSimulator();
        }
    }
}

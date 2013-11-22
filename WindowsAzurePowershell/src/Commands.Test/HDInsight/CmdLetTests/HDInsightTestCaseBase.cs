using Microsoft.WindowsAzure.Commands.Test.Utilities.HDInsight.PowerShellTestAbstraction.Interfaces;

namespace Microsoft.WindowsAzure.Commands.Test.HDInsight.CmdLetTests
{
    using Microsoft.WindowsAzure.Commands.Test.Utilities.HDInsight.Utilities;
    using Microsoft.WindowsAzure.Management.HDInsight.Cmdlet.PSCmdlets;

    public class HDInsightTestCaseBase : IntegrationTestBase
    {
        public override void Initialize()
        {
            base.Initialize();
            AzureHDInsightCmdlet.testSubscription = GetCurrentSubscription();
        }

        public override void TestCleanup()
        {
            base.TestCleanup();
            AzureHDInsightCmdlet.testSubscription = null;
        }

        protected IRunspace GetPowerShellRunspace()
        {
            string location = typeof(GetAzureHDInsightClusterCmdlet).Assembly.Location;
            return base.GetPowerShellRunspace(location);
        }
    }
}

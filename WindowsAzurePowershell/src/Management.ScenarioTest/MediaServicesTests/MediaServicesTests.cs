using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.ScenarioTest.Common;

namespace Microsoft.WindowsAzure.Management.ScenarioTest.MediaServicesTests
{
    [TestClass]
    public class MediaServicesTests : WindowsAzurePowerShellTest
    {
        public MediaServicesTests()
            : base("MediaServices\\MediaServices.ps1")
        {

        }

        [TestInitialize]
        public override void TestSetup()
        {
            base.TestSetup();
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
        }

        [TestMethod]
        public void TestNewAzureMediaServicesKey()
        {
            RunPowerShellTest("Test-NewAzureMediaServiceKey");
        }

        [TestMethod]
        public void TestRemoveAzureMediaServicesAccount()
        {
            RunPowerShellTest("Test-RemoveAzureMediaServiceAccount");
        }
    }
}

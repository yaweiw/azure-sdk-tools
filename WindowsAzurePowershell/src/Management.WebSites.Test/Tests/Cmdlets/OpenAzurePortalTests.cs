namespace Microsoft.WindowsAzure.Management.WebSites.Test.Tests.Cmdlets
{
    using System;
    using Properties;
    using VisualStudio.TestTools.UnitTesting;
    using WebSites.Cmdlets;

    [TestClass]
    public class OpenAzurePortalTests
    {
        [TestMethod]
        public void GetAzurePublishSettingsProcessTest()
        {
            new OpenAzurePortalCommand().OpenAzurePortalProcess(Resources.AzurePortalUrl, null);
        }

        /// <summary>
        /// Happy case, user has internet connection and uri specified is valid.
        /// </summary>
        [TestMethod]
        public void OpenAzurePortalProcessTestFail()
        {
            Assert.IsFalse(string.IsNullOrEmpty(Resources.AzurePortalUrl));
        }

        /// <summary>
        /// The url doesn't exist.
        /// </summary>
        [TestMethod]
        public void OpenAzurePortalProcessTestEmptyDnsFail()
        {
            string emptyDns = string.Empty;
            string expectedMsg = string.Format(Resources.InvalidOrEmptyArgumentMessage, "azure portal url");

            try
            {
                new OpenAzurePortalCommand().OpenAzurePortalProcess(emptyDns, null);
                Assert.Fail("No exception was thrown");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
                Assert.IsTrue(string.Compare(expectedMsg, ex.Message, StringComparison.OrdinalIgnoreCase) == 0);
            }
        }
    }
}
